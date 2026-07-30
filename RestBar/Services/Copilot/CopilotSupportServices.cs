using Microsoft.EntityFrameworkCore;
using RestBar.Domain.Copilot;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services.Copilot;

public class CopilotMemoryService : ICopilotMemoryService
{
    private readonly RestBarContext _db;

    public CopilotMemoryService(RestBarContext db) => _db = db;

    public async Task<CopilotConversation> GetOrCreateConversationAsync(
        CopilotRuntimeContext ctx, Guid? conversationId, CancellationToken ct = default)
    {
        if (conversationId.HasValue)
        {
            var existing = await _db.CopilotConversations
                .FirstOrDefaultAsync(c => c.Id == conversationId.Value
                    && c.CompanyId == ctx.CompanyId
                    && c.UserId == ctx.UserId, ct);
            if (existing != null) return existing;
        }

        var conv = new CopilotConversation
        {
            Id = Guid.NewGuid(),
            CompanyId = ctx.CompanyId,
            BranchId = ctx.BranchId,
            UserId = ctx.UserId,
            Title = "Dirección operativa",
            StartedAtUtc = DateTime.UtcNow,
            LastMessageAtUtc = DateTime.UtcNow
        };
        _db.CopilotConversations.Add(conv);
        await _db.SaveChangesAsync(ct);
        return conv;
    }

    public async Task AppendMessageAsync(
        Guid conversationId, CopilotMessageRole role, CopilotIntent intent,
        string content, string? toolsJson, int durationMs, CancellationToken ct = default)
    {
        var conv = await _db.CopilotConversations.FirstAsync(c => c.Id == conversationId, ct);
        conv.LastMessageAtUtc = DateTime.UtcNow;
        _db.CopilotMessages.Add(new CopilotMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            Role = role,
            Intent = intent,
            Content = content,
            ToolsJson = toolsJson,
            DurationMs = durationMs,
            CreatedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpsertPreferenceAsync(Guid companyId, Guid userId, string key, string value, CancellationToken ct = default)
    {
        var item = await _db.CopilotMemoryItems
            .FirstOrDefaultAsync(m => m.CompanyId == companyId && m.UserId == userId && m.Key == key, ct);
        if (item == null)
        {
            _db.CopilotMemoryItems.Add(new CopilotMemoryItem
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                UserId = userId,
                Key = key,
                Value = value,
                UpdatedAtUtc = DateTime.UtcNow
            });
        }
        else
        {
            item.Value = value;
            item.UpdatedAtUtc = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(ct);
    }
}

public class CopilotAuditService : ICopilotAuditService
{
    private readonly RestBarContext _db;
    public CopilotAuditService(RestBarContext db) => _db = db;

    public async Task LogAsync(CopilotAuditEvent evt, CancellationToken ct = default)
    {
        evt.Id = evt.Id == Guid.Empty ? Guid.NewGuid() : evt.Id;
        evt.CreatedAtUtc = DateTime.UtcNow;
        if (string.IsNullOrEmpty(evt.ContentHash))
            evt.ContentHash = CopilotHash.Sha256($"{evt.UserId}|{evt.Question}|{evt.AnswerDigest}|{evt.ToolsJson}|{evt.DurationMs}");
        _db.CopilotAuditEvents.Add(evt);
        await _db.SaveChangesAsync(ct);
    }
}

public class CopilotDecisionService : ICopilotDecisionService
{
    public IReadOnlyList<CopilotRecommendationCard> RankFromSnapshot(ExecutiveCommandCenterDto snapshot)
    {
        var pool = new List<(BiSeverity, string, string, string)>();

        foreach (var a in snapshot.Alerts)
            pool.Add((a.Severity, a.Message, $"Atender alerta {a.Code}", a.SourceModule));

        foreach (var i in snapshot.Insights)
            pool.Add((i.Severity, i.Title, i.Action, i.Type.ToString()));

        foreach (var t in snapshot.TopActions)
            pool.Add((BiSeverity.Medium, t, t, "TopAction"));

        var ranked = CopilotDecisionMath.Rank(pool, snapshot.RevenueToday);
        return ranked.Select(d => new CopilotRecommendationCard(
            d.Title, d.Action, d.Action, d.Severity, d.EstimatedImpact, d.Source)).ToList();
    }
}

public class CopilotActionService : ICopilotActionService
{
    private readonly RestBarContext _db;
    private readonly IPurchaseRequestService _prs;

    public CopilotActionService(RestBarContext db, IPurchaseRequestService prs)
    {
        _db = db;
        _prs = prs;
    }

    public async Task<(IReadOnlyList<CopilotActionCard> Cards, string? WriteResult)> ExecuteIntentActionsAsync(
        CopilotIntent intent, CopilotRuntimeContext ctx, CancellationToken ct = default)
    {
        var cards = new List<CopilotActionCard>
        {
            new("cc", "Command Center", "/ExecutiveCommandCenter"),
            new("fc", "Food Cost", "/FoodCostDashboard"),
            new("po", "Compras", "/ProcurementDashboard"),
            new("cash", "Caja", "/CashSession/Dashboard")
        };

        string? write = null;
        if (intent == CopilotIntent.DraftPurchaseRequest)
        {
            if (!CopilotPolicyMap.HasPolicy(ctx.Role, "PurchasingAccess"))
            {
                await LogAction(ctx, "draft_pr", false, "RBAC denied", ct);
                return (cards, "No autorizado para crear solicitudes de compra.");
            }

            try
            {
                var pr = await _prs.CreateDraftAsync(new PurchaseRequest
                {
                    CompanyId = ctx.CompanyId,
                    BranchId = ctx.BranchId,
                    RequestedByUserId = ctx.UserId,
                    Notes = "Borrador creado por Copilot Director Operativo"
                }, ct);
                await LogAction(ctx, "draft_pr", true, pr.Id.ToString(), ct);
                cards.Insert(0, new CopilotActionCard("pr", $"Abrir PR {pr.RequestNumber}", $"/PurchaseOrder", "Borrador creado"));
                write = $"Solicitud de compra **{pr.RequestNumber}** creada en borrador. Completa líneas en el módulo de Compras.";
            }
            catch (Exception ex)
            {
                await LogAction(ctx, "draft_pr", false, ex.Message, ct);
                write = $"No pude crear el borrador: {ex.Message}";
            }
        }

        return (cards, write);
    }

    private async Task LogAction(CopilotRuntimeContext ctx, string code, bool ok, string payload, CancellationToken ct)
    {
        _db.CopilotActionLogs.Add(new CopilotActionLog
        {
            Id = Guid.NewGuid(),
            CompanyId = ctx.CompanyId,
            BranchId = ctx.BranchId,
            UserId = ctx.UserId,
            ActionCode = code,
            PayloadJson = payload,
            Succeeded = ok,
            Error = ok ? null : payload,
            CreatedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
    }
}
