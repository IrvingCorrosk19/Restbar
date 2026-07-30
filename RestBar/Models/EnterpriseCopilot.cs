using System.ComponentModel.DataAnnotations;

namespace RestBar.Models;

public enum CopilotMessageRole { User, Assistant, System, Tool }

public enum CopilotIntent
{
    Unknown,
    Help,
    ExecutiveBriefing,
    SalesToday,
    FoodCostWhy,
    PurchasingWhat,
    CashStatus,
    AlertsNow,
    WhatShouldIDo,
    RecommendMenu,
    WasteStatus,
    SupplierAdvice,
    DraftPurchaseRequest
}

public class CopilotConversation
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    public Guid UserId { get; set; }
    [StringLength(120)]
    public string Title { get; set; } = "Sesión operativa";
    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastMessageAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsClosed { get; set; }
    public virtual ICollection<CopilotMessage> Messages { get; set; } = new List<CopilotMessage>();
}

public class CopilotMessage
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public CopilotMessageRole Role { get; set; }
    public CopilotIntent Intent { get; set; } = CopilotIntent.Unknown;
    public string Content { get; set; } = string.Empty;
    public string? ToolsJson { get; set; }
    public int DurationMs { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public virtual CopilotConversation? Conversation { get; set; }
}

public class CopilotMemoryItem
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid UserId { get; set; }
    [StringLength(80)]
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class CopilotAuditEvent
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ConversationId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string AnswerDigest { get; set; } = string.Empty;
    public string? ToolsJson { get; set; }
    [StringLength(40)]
    public string Provider { get; set; } = "Deterministic";
    [StringLength(40)]
    public string Intent { get; set; } = "Unknown";
    public int DurationMs { get; set; }
    public int TokensEst { get; set; }
    public bool Success { get; set; } = true;
    [StringLength(64)]
    public string? ContentHash { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class CopilotActionLog
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    public Guid UserId { get; set; }
    [StringLength(80)]
    public string ActionCode { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = "{}";
    public bool Succeeded { get; set; }
    [StringLength(500)]
    public string? Error { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
