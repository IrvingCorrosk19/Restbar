using System.Diagnostics;
using System.Text.Json;
using RestBar.Domain.Copilot;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services.Copilot;

public class CopilotOrchestratorService : ICopilotOrchestrator
{
    private readonly ICopilotIntentClassifier _classifier;
    private readonly ICopilotToolRegistry _tools;
    private readonly IAiProvider _ai;
    private readonly ICopilotMemoryService _memory;
    private readonly ICopilotAuditService _audit;
    private readonly ICopilotDecisionService _decisions;
    private readonly ICopilotActionService _actions;

    public CopilotOrchestratorService(
        ICopilotIntentClassifier classifier,
        ICopilotToolRegistry tools,
        IAiProvider ai,
        ICopilotMemoryService memory,
        ICopilotAuditService audit,
        ICopilotDecisionService decisions,
        ICopilotActionService actions)
    {
        _classifier = classifier;
        _tools = tools;
        _ai = ai;
        _memory = memory;
        _audit = audit;
        _decisions = decisions;
        _actions = actions;
    }

    public async Task<CopilotAskResponse> AskAsync(
        CopilotRuntimeContext ctx, CopilotAskRequest request, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var message = CopilotGuardrails.Sanitize(request.Message);

        if (string.IsNullOrWhiteSpace(message))
            return Empty(ctx, CopilotIntent.Unknown, "Escribe una pregunta operativa.", sw);

        if (CopilotGuardrails.IsPromptInjection(message) || CopilotGuardrails.LooksLikeRoleSpoof(message))
        {
            sw.Stop();
            await _audit.LogAsync(new CopilotAuditEvent
            {
                CompanyId = ctx.CompanyId,
                BranchId = ctx.BranchId,
                UserId = ctx.UserId,
                Question = message,
                AnswerDigest = "BLOCKED_INJECTION",
                Provider = _ai.Name,
                Intent = "Blocked",
                DurationMs = (int)sw.ElapsedMilliseconds,
                Success = false,
                TokensEst = message.Length / 4
            }, ct);
            return new CopilotAskResponse(
                Guid.Empty, CopilotIntent.Unknown,
                "Solicitud bloqueada por guardrails de seguridad. Reformula tu pregunta operativa.",
                Array.Empty<CopilotRecommendationCard>(),
                Array.Empty<CopilotActionCard>(),
                Array.Empty<string>(),
                (int)sw.ElapsedMilliseconds,
                _ai.Name);
        }

        var intent = _classifier.Classify(message);
        var conversation = await _memory.GetOrCreateConversationAsync(ctx, request.ConversationId, ct);
        ctx = ctx with { ConversationId = conversation.Id };

        await _memory.AppendMessageAsync(conversation.Id, CopilotMessageRole.User, intent, message, null, 0, ct);

        var toolResults = await _tools.InvokeForIntentAsync(intent, ctx, ct);
        var answer = await _ai.CompleteAsync(
            "Eres el Director Operativo Inteligente de RestBar. Explica causas, impacto y recomendaciones.",
            message,
            toolResults,
            ct);

        var (actionCards, writeResult) = await _actions.ExecuteIntentActionsAsync(intent, ctx, ct);
        if (!string.IsNullOrWhiteSpace(writeResult))
            answer += "\n\n" + writeResult;

        var recommendations = Array.Empty<CopilotRecommendationCard>() as IReadOnlyList<CopilotRecommendationCard>;
        var snap = toolResults.Select(t => t.Data).OfType<ExecutiveCommandCenterDto>().FirstOrDefault();
        if (snap != null)
            recommendations = _decisions.RankFromSnapshot(snap);

        if (intent == CopilotIntent.WhatShouldIDo && recommendations.Count > 0)
        {
            answer += "\n\n#### Decisiones priorizadas (impacto económico)\n";
            foreach (var r in recommendations)
                answer += $"- **[{r.Severity}]** {r.Title} — impacto ~{r.EstimatedImpact:C0} → {r.Action}\n";
        }

        sw.Stop();
        var duration = (int)sw.ElapsedMilliseconds;
        var toolsJson = JsonSerializer.Serialize(toolResults.Select(t => new { t.ToolName, t.Allowed }));
        var toolsUsed = toolResults.Where(t => t.Allowed).Select(t => t.ToolName).Distinct().ToList();

        await _memory.AppendMessageAsync(conversation.Id, CopilotMessageRole.Assistant, intent, answer, toolsJson, duration, ct);
        await _memory.UpsertPreferenceAsync(ctx.CompanyId, ctx.UserId, $"intent_count:{intent}", "1", ct);

        var digest = answer.Length > 280 ? answer[..280] : answer;
        await _audit.LogAsync(new CopilotAuditEvent
        {
            CompanyId = ctx.CompanyId,
            BranchId = ctx.BranchId,
            UserId = ctx.UserId,
            ConversationId = conversation.Id,
            Question = message,
            AnswerDigest = digest,
            ToolsJson = toolsJson,
            Provider = _ai.Name,
            Intent = intent.ToString(),
            DurationMs = duration,
            Success = true,
            TokensEst = (message.Length + answer.Length) / 4
        }, ct);

        return new CopilotAskResponse(conversation.Id, intent, answer, recommendations, actionCards, toolsUsed, duration, _ai.Name);
    }

    private CopilotAskResponse Empty(CopilotRuntimeContext ctx, CopilotIntent intent, string msg, Stopwatch sw)
    {
        sw.Stop();
        return new CopilotAskResponse(
            ctx.ConversationId ?? Guid.Empty, intent, msg,
            Array.Empty<CopilotRecommendationCard>(),
            Array.Empty<CopilotActionCard>(),
            Array.Empty<string>(),
            (int)sw.ElapsedMilliseconds,
            _ai.Name);
    }
}
