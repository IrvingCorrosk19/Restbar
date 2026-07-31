using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RestBar.Domain.BusinessRules;
using RestBar.Domain.DecisionIntelligence;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services.BusinessRules;

public interface IBusinessRulesEngine
{
    Task EnsureTemplatesAsync(CancellationToken ct = default);
    Task<BrRule> CreateFromTemplateAsync(Guid companyId, Guid? branchId, Guid userId, string templateCode, CancellationToken ct = default);
    Task<BrRuleVersion> SaveDraftAsync(Guid companyId, Guid ruleId, Guid userId, string flowJson, string? notes, CancellationToken ct = default);
    Task<BrRule> PublishAsync(Guid companyId, Guid ruleId, Guid userId, CancellationToken ct = default);
    Task<BrRule> DisableAsync(Guid companyId, Guid ruleId, Guid userId, CancellationToken ct = default);
    Task<BrRule?> RollbackAsync(Guid companyId, Guid ruleId, int versionNumber, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<BrRule>> ListAsync(Guid companyId, Guid? branchId, CancellationToken ct = default);
    Task<BrRule?> GetAsync(Guid companyId, Guid ruleId, CancellationToken ct = default);
    Task<RuleRunResult> SimulateAsync(Guid companyId, Guid ruleId, IReadOnlyDictionary<string, object?> facts, Guid? userId, CancellationToken ct = default);
    Task<IReadOnlyList<RuleRunResult>> EvaluatePublishedAsync(Guid companyId, Guid? branchId, IReadOnlyDictionary<string, object?> facts, Guid? userId, bool live, CancellationToken ct = default);
    Task<IReadOnlyList<BrRuleExecution>> ListExecutionsAsync(Guid companyId, Guid? ruleId, int take = 50, CancellationToken ct = default);
    IReadOnlyList<BusinessRuleTemplates.TemplateDef> GetTemplates();
    Dictionary<string, object?> BuildOperationalFacts(Guid companyId, Guid? branchId);
}

public sealed record RuleRunResult(
    Guid RuleId,
    string RuleName,
    Guid VersionId,
    int VersionNumber,
    BrExecutionResult Result,
    IReadOnlyList<string> Trace,
    IReadOnlyList<string> ActionsPlannedOrExecuted,
    int DurationMs,
    Guid? ExecutionId);

public sealed class BusinessRulesEngine : IBusinessRulesEngine
{
    private readonly RestBarContext _db;

    public BusinessRulesEngine(RestBarContext db) => _db = db;

    public IReadOnlyList<BusinessRuleTemplates.TemplateDef> GetTemplates() => BusinessRuleTemplates.All;

    public async Task EnsureTemplatesAsync(CancellationToken ct = default)
    {
        foreach (var t in BusinessRuleTemplates.All)
        {
            var exists = await _db.BrRuleTemplates.AnyAsync(x => x.Code == t.Code, ct);
            if (exists) continue;
            _db.BrRuleTemplates.Add(new BrRuleTemplate
            {
                Id = Guid.NewGuid(),
                Code = t.Code,
                Name = t.Name,
                Category = t.Category,
                Description = t.Description,
                FlowJson = t.FlowJson,
                IsSystem = true
            });
        }
        await _db.SaveChangesAsync(ct);
    }

    public async Task<BrRule> CreateFromTemplateAsync(Guid companyId, Guid? branchId, Guid userId, string templateCode, CancellationToken ct = default)
    {
        await EnsureTemplatesAsync(ct);
        var tpl = BusinessRuleTemplates.All.FirstOrDefault(t => t.Code.Equals(templateCode, StringComparison.OrdinalIgnoreCase))
                  ?? throw new ArgumentException($"Unknown template '{templateCode}'");

        var rule = new BrRule
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            BranchId = branchId,
            Name = tpl.Name,
            Description = tpl.Description,
            Category = tpl.Category,
            Status = BrRuleStatus.Draft,
            CurrentVersionNumber = 1,
            CreatedByUserId = userId,
            TemplateCode = tpl.Code,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        var version = CompileVersion(rule.Id, 1, userId, tpl.FlowJson, "Created from template");
        rule.Versions.Add(version);
        _db.BrRules.Add(rule);
        await _db.SaveChangesAsync(ct);
        return rule;
    }

    public async Task<BrRuleVersion> SaveDraftAsync(Guid companyId, Guid ruleId, Guid userId, string flowJson, string? notes, CancellationToken ct = default)
    {
        var rule = await _db.BrRules.Include(r => r.Versions).FirstOrDefaultAsync(r => r.Id == ruleId && r.CompanyId == companyId, ct)
                   ?? throw new KeyNotFoundException("Rule not found");
        ValidateFlow(flowJson);
        var next = rule.Versions.Count == 0 ? 1 : rule.Versions.Max(v => v.VersionNumber) + 1;
        var version = CompileVersion(rule.Id, next, userId, flowJson, notes);
        rule.CurrentVersionNumber = next;
        rule.Status = BrRuleStatus.Draft;
        rule.UpdatedAtUtc = DateTime.UtcNow;
        _db.BrRuleVersions.Add(version);
        await _db.SaveChangesAsync(ct);
        return version;
    }

    public async Task<BrRule> PublishAsync(Guid companyId, Guid ruleId, Guid userId, CancellationToken ct = default)
    {
        var rule = await _db.BrRules.Include(r => r.Versions).ThenInclude(v => v.Conditions)
            .Include(r => r.Versions).ThenInclude(v => v.Actions)
            .FirstOrDefaultAsync(r => r.Id == ruleId && r.CompanyId == companyId, ct)
            ?? throw new KeyNotFoundException("Rule not found");

        var version = rule.Versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault()
                      ?? throw new InvalidOperationException("No version to publish");
        ValidateFlow(version.FlowJson);
        if (version.Conditions.Count == 0)
            throw new InvalidOperationException("Cannot publish rule without conditions (fail-closed).");
        if (version.Actions.Count == 0)
            throw new InvalidOperationException("Cannot publish rule without actions.");

        // Infinite-loop guard: reject self-triggering WriteAudit-only loops is N/A; reject > 20 actions
        if (version.Actions.Count > 20)
            throw new InvalidOperationException("Too many actions (max 20).");

        foreach (var v in rule.Versions) v.IsPublished = false;
        version.IsPublished = true;
        rule.Status = BrRuleStatus.Published;
        rule.ApprovedByUserId = userId;
        rule.EffectiveFromUtc ??= DateTime.UtcNow;
        rule.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return rule;
    }

    public async Task<BrRule> DisableAsync(Guid companyId, Guid ruleId, Guid userId, CancellationToken ct = default)
    {
        var rule = await _db.BrRules.FirstOrDefaultAsync(r => r.Id == ruleId && r.CompanyId == companyId, ct)
                   ?? throw new KeyNotFoundException("Rule not found");
        rule.Status = BrRuleStatus.Disabled;
        rule.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return rule;
    }

    public async Task<BrRule?> RollbackAsync(Guid companyId, Guid ruleId, int versionNumber, Guid userId, CancellationToken ct = default)
    {
        var rule = await _db.BrRules.Include(r => r.Versions).ThenInclude(v => v.Conditions)
            .Include(r => r.Versions).ThenInclude(v => v.Actions)
            .FirstOrDefaultAsync(r => r.Id == ruleId && r.CompanyId == companyId, ct);
        if (rule == null) return null;
        var target = rule.Versions.FirstOrDefault(v => v.VersionNumber == versionNumber)
                     ?? throw new KeyNotFoundException("Version not found");
        // Clone as new draft version then publish
        var next = rule.Versions.Max(v => v.VersionNumber) + 1;
        var clone = CompileVersion(rule.Id, next, userId, target.FlowJson, $"Rollback to v{versionNumber}");
        _db.BrRuleVersions.Add(clone);
        rule.CurrentVersionNumber = next;
        await _db.SaveChangesAsync(ct);
        return await PublishAsync(companyId, ruleId, userId, ct);
    }

    public async Task<IReadOnlyList<BrRule>> ListAsync(Guid companyId, Guid? branchId, CancellationToken ct = default)
    {
        var q = _db.BrRules.AsNoTracking().Where(r => r.CompanyId == companyId);
        if (branchId.HasValue)
            q = q.Where(r => r.BranchId == null || r.BranchId == branchId);
        return await q.OrderBy(r => r.Priority).ThenBy(r => r.Name).ToListAsync(ct);
    }

    public Task<BrRule?> GetAsync(Guid companyId, Guid ruleId, CancellationToken ct = default) =>
        _db.BrRules.AsNoTracking()
            .Include(r => r.Versions).ThenInclude(v => v.Conditions)
            .Include(r => r.Versions).ThenInclude(v => v.Actions)
            .FirstOrDefaultAsync(r => r.Id == ruleId && r.CompanyId == companyId, ct);

    public Task<RuleRunResult> SimulateAsync(Guid companyId, Guid ruleId, IReadOnlyDictionary<string, object?> facts, Guid? userId, CancellationToken ct = default)
        => RunOneAsync(companyId, ruleId, facts, userId, live: false, ct);

    public async Task<IReadOnlyList<RuleRunResult>> EvaluatePublishedAsync(
        Guid companyId, Guid? branchId, IReadOnlyDictionary<string, object?> facts, Guid? userId, bool live, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var rules = await _db.BrRules.AsNoTracking()
            .Where(r => r.CompanyId == companyId && r.Status == BrRuleStatus.Published
                        && (r.BranchId == null || r.BranchId == branchId)
                        && (r.EffectiveFromUtc == null || r.EffectiveFromUtc <= now)
                        && (r.EffectiveToUtc == null || r.EffectiveToUtc >= now))
            .OrderBy(r => r.Priority)
            .Select(r => r.Id)
            .ToListAsync(ct);

        var results = new List<RuleRunResult>();
        foreach (var id in rules)
            results.Add(await RunOneAsync(companyId, id, facts, userId, live, ct));
        return results;
    }

    public async Task<IReadOnlyList<BrRuleExecution>> ListExecutionsAsync(Guid companyId, Guid? ruleId, int take = 50, CancellationToken ct = default)
    {
        var q = _db.BrRuleExecutions.AsNoTracking().Where(e => e.CompanyId == companyId);
        if (ruleId.HasValue) q = q.Where(e => e.RuleId == ruleId);
        return await q.OrderByDescending(e => e.CreatedAtUtc).Take(take).Include(e => e.Logs).ToListAsync(ct);
    }

    public Dictionary<string, object?> BuildOperationalFacts(Guid companyId, Guid? branchId)
    {
        // Sync snapshot for on-demand evaluation — lightweight counts.
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var salesToday = _db.Orders.AsNoTracking()
            .Where(o => o.CompanyId == companyId && (branchId == null || o.BranchId == branchId)
                        && o.Status == OrderStatus.Completed && o.ClosedAt >= today && o.ClosedAt < tomorrow)
            .Sum(o => (decimal?)o.TotalAmount ?? 0m);
        var salesYesterday = _db.Orders.AsNoTracking()
            .Where(o => o.CompanyId == companyId && (branchId == null || o.BranchId == branchId)
                        && o.Status == OrderStatus.Completed && o.ClosedAt >= today.AddDays(-1) && o.ClosedAt < today)
            .Sum(o => (decimal?)o.TotalAmount ?? 0m);

        var drop = salesYesterday <= 0 ? 0m : Math.Round((salesYesterday - salesToday) / salesYesterday * 100m, 2);
        var lowStock = _db.Products.AsNoTracking()
            .Count(p => p.CompanyId == companyId && p.TrackInventory && p.IsActive && p.MinStock != null && p.Stock <= p.MinStock);
        var zeroStock = _db.Products.AsNoTracking()
            .Count(p => p.CompanyId == companyId && p.TrackInventory && p.IsActive && p.Stock <= 0);
        var openOrders = _db.Orders.AsNoTracking()
            .Count(o => o.CompanyId == companyId && (branchId == null || o.BranchId == branchId)
                        && o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled);
        var cutoff = DateTime.UtcNow.AddMinutes(-20);
        var delayed = (
            from oi in _db.OrderItems.AsNoTracking()
            join o in _db.Orders.AsNoTracking() on oi.OrderId equals o.Id
            where o.CompanyId == companyId && (branchId == null || o.BranchId == branchId)
                  && oi.SentAt != null && oi.PreparedAt == null && oi.SentAt < cutoff
                  && o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled
            select oi.Id).Count();
        var overduePo = _db.PurchaseOrders.AsNoTracking()
            .Count(po => po.CompanyId == companyId && (branchId == null || po.BranchId == branchId)
                         && po.ExpectedDelivery != null && po.ExpectedDelivery < DateTime.UtcNow
                         && po.Status != PurchaseOrderStatus.Closed && po.Status != PurchaseOrderStatus.Cancelled
                         && po.Status != PurchaseOrderStatus.FullyReceived);
        var varianceAbs = _db.CashSessions.AsNoTracking()
            .Where(s => s.CompanyId == companyId && (branchId == null || s.BranchId == branchId) && s.OpenedAt >= today)
            .Sum(s => (decimal?)Math.Abs(s.Variance) ?? 0m);

        var fc = _db.FoodCostSnapshots.AsNoTracking()
            .Where(s => s.CompanyId == companyId && (branchId == null || s.BranchId == branchId))
            .OrderByDescending(s => s.GeneratedAt)
            .Select(s => (decimal?)s.FoodCostPercentActual)
            .FirstOrDefault() ?? 0m;

        // dead sku approx: track inventory with no sale movements in 30d and stock>0 — simplified via zero sales products not available quickly; use no-move heuristic 0 unless we query movements
        var dead = 0;
        var needsReorder = lowStock > 0 || zeroStock > 0;

        return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["sales.today"] = salesToday,
            ["sales.yesterday"] = salesYesterday,
            ["sales.dropPercent"] = drop,
            ["sales.vsForecastPct"] = drop, // until forecast fact injected by caller
            ["inventory.lowStockCount"] = lowStock,
            ["inventory.zeroStockCount"] = zeroStock,
            ["inventory.needsReorder"] = needsReorder,
            ["inventory.maxCoverageDays"] = 0m,
            ["inventory.deadSkuCount"] = dead,
            ["cash.varianceAbs"] = varianceAbs,
            ["foodcost.actualPct"] = fc,
            ["kitchen.delayedOrders"] = delayed,
            ["procurement.overduePoCount"] = overduePo,
            ["ops.openOrders"] = openOrders,
            ["customers.vipCount"] = 0,
            ["customers.inactiveCount"] = 0,
        };
    }

    async Task<RuleRunResult> RunOneAsync(Guid companyId, Guid ruleId, IReadOnlyDictionary<string, object?> facts, Guid? userId, bool live, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var rule = await _db.BrRules
            .Include(r => r.Versions).ThenInclude(v => v.Conditions)
            .Include(r => r.Versions).ThenInclude(v => v.Actions)
            .FirstOrDefaultAsync(r => r.Id == ruleId && r.CompanyId == companyId, ct)
            ?? throw new KeyNotFoundException("Rule not found");

        var version = rule.Versions.Where(v => v.IsPublished).OrderByDescending(v => v.VersionNumber).FirstOrDefault()
                      ?? rule.Versions.OrderByDescending(v => v.VersionNumber).First();

        var (logic, compiledConds, compiledActs) = RuleFlowCompiler.Compile(version.FlowJson);
        // Prefer persisted conditions if present
        var conditions = version.Conditions.Count > 0
            ? version.Conditions.Select(c => new RuleConditionSpec(c.SortOrder, c.Negate, c.FieldKey, MapOp(c.Operator), c.ValueJson)).ToList()
            : compiledConds;
        var actions = version.Actions.Count > 0
            ? version.Actions.Select(a => new RuleActionSpec(a.SortOrder, a.ActionType.ToString(), a.ParametersJson)).ToList()
            : compiledActs;

        var rootLogic = version.Conditions.Count > 0
            ? (version.RootLogic == BrLogicGate.Or ? BrLogicGateRoot.Or : BrLogicGateRoot.And)
            : logic;

        var trace = new List<string>();
        var matched = RuleConditionEvaluator.EvaluateAll(conditions, rootLogic, facts, trace);
        var planned = new List<string>();

        var dedupe = BuildDedupeKey(companyId, rule.Id, version.Id, facts, live);
        if (live)
        {
            var dup = await _db.BrRuleExecutions.AsNoTracking()
                .AnyAsync(e => e.CompanyId == companyId && e.DedupeKey == dedupe
                               && e.CreatedAtUtc >= DateTime.UtcNow.AddHours(-6), ct);
            if (dup)
            {
                sw.Stop();
                var dupExec = await PersistExecution(rule, version, companyId, facts, userId, live, BrExecutionResult.Duplicate, trace, planned, "Duplicate suppressed", (int)sw.ElapsedMilliseconds, dedupe, ct);
                return new RuleRunResult(rule.Id, rule.Name, version.Id, version.VersionNumber, BrExecutionResult.Duplicate, trace, planned, (int)sw.ElapsedMilliseconds, dupExec.Id);
            }
        }

        BrExecutionResult result;
        if (!matched)
        {
            result = BrExecutionResult.NotMatched;
            trace.Add("Rule did not match.");
        }
        else
        {
            result = BrExecutionResult.Matched;
            foreach (var act in actions.OrderBy(a => a.SortOrder))
            {
                var summary = $"{act.ActionType}: {act.ParametersJson}";
                planned.Add(summary);
                trace.Add((live ? "EXECUTE " : "SIMULATE ") + summary);
                if (live)
                    await DispatchActionAsync(companyId, rule.BranchId, act, ct);
            }
        }

        sw.Stop();
        var exec = await PersistExecution(rule, version, companyId, facts, userId, live, result, trace, planned, null, (int)sw.ElapsedMilliseconds, dedupe, ct);
        return new RuleRunResult(rule.Id, rule.Name, version.Id, version.VersionNumber, result, trace, planned, (int)sw.ElapsedMilliseconds, exec.Id);
    }

    async Task DispatchActionAsync(Guid companyId, Guid? branchId, RuleActionSpec act, CancellationToken ct)
    {
        using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(act.ParametersJson) ? "{}" : act.ParametersJson);
        var p = doc.RootElement;
        switch (act.ActionType)
        {
            case nameof(BrActionType.CreateAlert):
                _db.BiAlerts.Add(new BiAlert
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    BranchId = branchId ?? Guid.Empty,
                    AlertCode = Str(p, "code") ?? "BR_ALERT",
                    Severity = ParseSeverity(Str(p, "severity")),
                    Message = Str(p, "message") ?? "Business rule alert",
                    SourceModule = "BusinessRules",
                    CreatedAt = DateTime.UtcNow
                });
                break;
            case nameof(BrActionType.CreateRecommendation):
                _db.DiDecisionRecords.Add(new DiDecisionRecord
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    BranchId = branchId,
                    RecommendationCode = Str(p, "code") ?? "BR.REC",
                    Category = "BusinessRule",
                    Observation = "Generado por motor de reglas",
                    Evidence = act.ParametersJson,
                    RecommendedAction = Str(p, "action") ?? "Revisar",
                    ExpectedImpact = "Según regla publicada",
                    Status = DiDecisionStatus.New,
                    CreatedByUserId = Guid.Empty,
                    CreatedAtUtc = DateTime.UtcNow
                });
                break;
            case nameof(BrActionType.CreateNotification):
                _db.Notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    BranchId = branchId,
                    Message = $"[BR→{Str(p, "role") ?? "manager"}] {Str(p, "message") ?? "Notificación de regla"}",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                });
                break;
            case nameof(BrActionType.CreateTask):
                _db.Notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    BranchId = branchId,
                    Message = $"[TASK:{Str(p, "ownerRole") ?? "manager"}] {Str(p, "title") ?? "Business Rule"}",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                });
                break;
            case nameof(BrActionType.WriteAudit):
                _db.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    BranchId = branchId,
                    Action = Str(p, "event") ?? "business_rule",
                    Module = "BusinessRules",
                    TableName = "br_rules",
                    Description = act.ParametersJson,
                    Timestamp = DateTime.UtcNow,
                    LogLevel = "INFO"
                });
                break;
            default:
                throw new InvalidOperationException($"Action '{act.ActionType}' not allowed in v1.");
        }
        await _db.SaveChangesAsync(ct);
    }

    async Task<BrRuleExecution> PersistExecution(
        BrRule rule, BrRuleVersion version, Guid companyId, IReadOnlyDictionary<string, object?> facts,
        Guid? userId, bool live, BrExecutionResult result, List<string> trace, List<string> planned,
        string? error, int ms, string dedupe, CancellationToken ct)
    {
        var exec = new BrRuleExecution
        {
            Id = Guid.NewGuid(),
            RuleId = rule.Id,
            RuleVersionId = version.Id,
            CompanyId = companyId,
            BranchId = rule.BranchId,
            Mode = live ? BrExecutionMode.Live : BrExecutionMode.Simulation,
            Result = result,
            DedupeKey = dedupe,
            FactsJson = JsonSerializer.Serialize(facts),
            ErrorMessage = error,
            DurationMs = ms,
            TriggeredByUserId = userId,
            CreatedAtUtc = DateTime.UtcNow
        };
        foreach (var t in trace.Take(50))
            exec.Logs.Add(new BrRuleExecutionLog { Id = Guid.NewGuid(), ExecutionId = exec.Id, StepType = "trace", Message = t.Length > 200 ? t[..200] : t, CreatedAtUtc = DateTime.UtcNow });
        foreach (var a in planned.Take(20))
            exec.Logs.Add(new BrRuleExecutionLog { Id = Guid.NewGuid(), ExecutionId = exec.Id, StepType = "action", Message = a.Length > 200 ? a[..200] : a, CreatedAtUtc = DateTime.UtcNow });
        _db.BrRuleExecutions.Add(exec);
        await _db.SaveChangesAsync(ct);
        return exec;
    }

    static BrRuleVersion CompileVersion(Guid ruleId, int number, Guid userId, string flowJson, string? notes)
    {
        ValidateFlow(flowJson);
        var (logic, conditions, actions) = RuleFlowCompiler.Compile(flowJson);
        var version = new BrRuleVersion
        {
            Id = Guid.NewGuid(),
            RuleId = ruleId,
            VersionNumber = number,
            RootLogic = logic == BrLogicGateRoot.Or ? BrLogicGate.Or : BrLogicGate.And,
            FlowJson = flowJson,
            Notes = notes,
            CreatedByUserId = userId,
            CreatedAtUtc = DateTime.UtcNow,
            IsPublished = false
        };
        foreach (var c in conditions)
        {
            version.Conditions.Add(new BrRuleCondition
            {
                Id = Guid.NewGuid(),
                RuleVersionId = version.Id,
                SortOrder = c.SortOrder,
                Negate = c.Negate,
                FieldKey = c.FieldKey,
                Operator = MapOpToModel(c.Operator),
                ValueJson = c.ValueJson
            });
        }
        foreach (var a in actions)
        {
            if (!Enum.TryParse<BrActionType>(a.ActionType, true, out var at))
                throw new InvalidOperationException($"Unsupported action '{a.ActionType}'. Destructive actions are blocked.");
            version.Actions.Add(new BrRuleAction
            {
                Id = Guid.NewGuid(),
                RuleVersionId = version.Id,
                SortOrder = a.SortOrder,
                ActionType = at,
                ParametersJson = a.ParametersJson
            });
        }
        return version;
    }

    static void ValidateFlow(string flowJson)
    {
        var (logic, conditions, actions) = RuleFlowCompiler.Compile(flowJson);
        _ = logic;
        if (conditions.Any(c => string.IsNullOrWhiteSpace(c.FieldKey)))
            throw new ArgumentException("Condition field required");
        // Cycle / recursion: flow is declarative flat list — no loops possible in v1 schema.
        foreach (var a in actions)
        {
            if (!Enum.TryParse<BrActionType>(a.ActionType, true, out _))
                throw new ArgumentException($"Action '{a.ActionType}' not allowed");
        }
    }

    static string BuildDedupeKey(Guid companyId, Guid ruleId, Guid versionId, IReadOnlyDictionary<string, object?> facts, bool live)
    {
        var day = DateTime.UtcNow.ToString("yyyyMMddHH"); // hourly bucket
        var payload = $"{companyId}|{ruleId}|{versionId}|{day}|{live}|{JsonSerializer.Serialize(facts.Keys.OrderBy(k => k))}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash)[..40];
    }

    static Domain.BusinessRules.BrConditionOp MapOp(Models.BrConditionOp op) => op switch
    {
        Models.BrConditionOp.Eq => Domain.BusinessRules.BrConditionOp.Eq,
        Models.BrConditionOp.Neq => Domain.BusinessRules.BrConditionOp.Neq,
        Models.BrConditionOp.Gt => Domain.BusinessRules.BrConditionOp.Gt,
        Models.BrConditionOp.Gte => Domain.BusinessRules.BrConditionOp.Gte,
        Models.BrConditionOp.Lt => Domain.BusinessRules.BrConditionOp.Lt,
        Models.BrConditionOp.Lte => Domain.BusinessRules.BrConditionOp.Lte,
        Models.BrConditionOp.Contains => Domain.BusinessRules.BrConditionOp.Contains,
        Models.BrConditionOp.NotContains => Domain.BusinessRules.BrConditionOp.NotContains,
        Models.BrConditionOp.Between => Domain.BusinessRules.BrConditionOp.Between,
        Models.BrConditionOp.In => Domain.BusinessRules.BrConditionOp.In,
        Models.BrConditionOp.NotIn => Domain.BusinessRules.BrConditionOp.NotIn,
        _ => Domain.BusinessRules.BrConditionOp.Eq
    };

    static Models.BrConditionOp MapOpToModel(Domain.BusinessRules.BrConditionOp op) =>
        Enum.Parse<Models.BrConditionOp>(op.ToString());

    static string? Str(JsonElement p, string name) =>
        p.TryGetProperty(name, out var e) ? e.ValueKind == JsonValueKind.String ? e.GetString() : e.ToString() : null;

    static BiSeverity ParseSeverity(string? s) => s?.ToLowerInvariant() switch
    {
        "critical" => BiSeverity.Critical,
        "high" => BiSeverity.High,
        "medium" => BiSeverity.Medium,
        "low" => BiSeverity.Low,
        _ => BiSeverity.Info
    };
}
