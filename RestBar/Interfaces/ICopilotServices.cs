using RestBar.Models;

namespace RestBar.Interfaces;

public record CopilotAskRequest(Guid? ConversationId, string Message);

public record CopilotActionCard(string Code, string Label, string Url, string? Hint = null);

public record CopilotRecommendationCard(
    string Title,
    string Explanation,
    string Action,
    BiSeverity Severity,
    decimal EstimatedImpact,
    string Source);

public record CopilotAskResponse(
    Guid ConversationId,
    CopilotIntent Intent,
    string AnswerMarkdown,
    IReadOnlyList<CopilotRecommendationCard> Recommendations,
    IReadOnlyList<CopilotActionCard> Actions,
    IReadOnlyList<string> ToolsUsed,
    int DurationMs,
    string Provider);

public record CopilotRuntimeContext(
    Guid CompanyId,
    Guid BranchId,
    Guid UserId,
    string Role,
    string Language,
    Guid? ConversationId);

public record CopilotToolResult(string ToolName, bool Allowed, string PayloadMarkdown, object? Data = null);

public interface ICopilotOrchestrator
{
    Task<CopilotAskResponse> AskAsync(CopilotRuntimeContext ctx, CopilotAskRequest request, CancellationToken ct = default);
}

public interface ICopilotIntentClassifier
{
    CopilotIntent Classify(string message);
}

public interface IAiProvider
{
    string Name { get; }
    Task<string> CompleteAsync(string systemPrompt, string userMessage, IReadOnlyList<CopilotToolResult> tools, CancellationToken ct = default);
}

public interface ICopilotTool
{
    string Name { get; }
    string RequiredPolicy { get; }
    IReadOnlyList<CopilotIntent> Intents { get; }
    Task<CopilotToolResult> InvokeAsync(CopilotRuntimeContext ctx, CancellationToken ct = default);
}

public interface ICopilotToolRegistry
{
    Task<IReadOnlyList<CopilotToolResult>> InvokeForIntentAsync(CopilotIntent intent, CopilotRuntimeContext ctx, CancellationToken ct = default);
}

public interface ICopilotMemoryService
{
    Task<CopilotConversation> GetOrCreateConversationAsync(CopilotRuntimeContext ctx, Guid? conversationId, CancellationToken ct = default);
    Task AppendMessageAsync(Guid conversationId, CopilotMessageRole role, CopilotIntent intent, string content, string? toolsJson, int durationMs, CancellationToken ct = default);
    Task UpsertPreferenceAsync(Guid companyId, Guid userId, string key, string value, CancellationToken ct = default);
}

public interface ICopilotAuditService
{
    Task LogAsync(CopilotAuditEvent evt, CancellationToken ct = default);
}

public interface ICopilotDecisionService
{
    IReadOnlyList<CopilotRecommendationCard> RankFromSnapshot(ExecutiveCommandCenterDto snapshot);
}

public interface ICopilotActionService
{
    Task<(IReadOnlyList<CopilotActionCard> Cards, string? WriteResult)> ExecuteIntentActionsAsync(
        CopilotIntent intent, CopilotRuntimeContext ctx, CancellationToken ct = default);
}
