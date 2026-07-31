using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models;

public enum BrRuleStatus { Draft = 0, InReview = 1, Published = 2, Disabled = 3, Archived = 4 }
public enum BrLogicGate { And = 0, Or = 1 }
public enum BrConditionOp
{
    Eq, Neq, Gt, Gte, Lt, Lte, Contains, NotContains, Between, In, NotIn
}
public enum BrActionType
{
    CreateAlert,
    CreateNotification,
    CreateRecommendation,
    CreateTask,
    WriteAudit,
    // Destructive / mutating ops intentionally omitted in v1
}
public enum BrExecutionMode { Live = 0, Simulation = 1 }
public enum BrExecutionResult { Matched = 0, NotMatched = 1, Skipped = 2, Error = 3, Duplicate = 4 }

public class BrRule
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;
    [StringLength(500)]
    public string? Description { get; set; }
    [StringLength(40)]
    public string Category { get; set; } = "General";
    public int Priority { get; set; } = 100;
    public BrRuleStatus Status { get; set; } = BrRuleStatus.Draft;
    public int CurrentVersionNumber { get; set; }
    public DateTime? EffectiveFromUtc { get; set; }
    public DateTime? EffectiveToUtc { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool RequireApprovalToPublish { get; set; }
    [StringLength(80)]
    public string? TemplateCode { get; set; }
    public virtual ICollection<BrRuleVersion> Versions { get; set; } = new List<BrRuleVersion>();
}

public class BrRuleVersion
{
    public Guid Id { get; set; }
    public Guid RuleId { get; set; }
    public int VersionNumber { get; set; }
    public BrLogicGate RootLogic { get; set; } = BrLogicGate.And;
    /// <summary>Visual flow builder JSON (nodes/edges). Source of truth for editor; compiled into conditions/actions on publish.</summary>
    public string FlowJson { get; set; } = "{}";
    public string? Notes { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsPublished { get; set; }
    public virtual BrRule? Rule { get; set; }
    public virtual ICollection<BrRuleCondition> Conditions { get; set; } = new List<BrRuleCondition>();
    public virtual ICollection<BrRuleAction> Actions { get; set; } = new List<BrRuleAction>();
}

public class BrRuleCondition
{
    public Guid Id { get; set; }
    public Guid RuleVersionId { get; set; }
    public int SortOrder { get; set; }
    public bool Negate { get; set; }
    [StringLength(80)]
    public string FieldKey { get; set; } = string.Empty; // e.g. sales.dropPercent, inventory.lowStockCount
    public BrConditionOp Operator { get; set; }
    [StringLength(500)]
    public string ValueJson { get; set; } = "null"; // scalar, [min,max], or list
    public virtual BrRuleVersion? RuleVersion { get; set; }
}

public class BrRuleAction
{
    public Guid Id { get; set; }
    public Guid RuleVersionId { get; set; }
    public int SortOrder { get; set; }
    public BrActionType ActionType { get; set; }
    public string ParametersJson { get; set; } = "{}";
    public virtual BrRuleVersion? RuleVersion { get; set; }
}

public class BrRuleSchedule
{
    public Guid Id { get; set; }
    public Guid RuleId { get; set; }
    [StringLength(40)]
    public string CronOrInterval { get; set; } = "manual"; // manual | on_demand | interval:5m
    public bool IsEnabled { get; set; } = true;
    public DateTime? LastRunUtc { get; set; }
    public virtual BrRule? Rule { get; set; }
}

public class BrRuleExecution
{
    public Guid Id { get; set; }
    public Guid RuleId { get; set; }
    public Guid RuleVersionId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public BrExecutionMode Mode { get; set; }
    public BrExecutionResult Result { get; set; }
    [StringLength(120)]
    public string DedupeKey { get; set; } = string.Empty;
    public string FactsJson { get; set; } = "{}";
    public string? ErrorMessage { get; set; }
    public int DurationMs { get; set; }
    public Guid? TriggeredByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public virtual ICollection<BrRuleExecutionLog> Logs { get; set; } = new List<BrRuleExecutionLog>();
}

public class BrRuleExecutionLog
{
    public Guid Id { get; set; }
    public Guid ExecutionId { get; set; }
    [StringLength(40)]
    public string StepType { get; set; } = "condition"; // condition | action | info
    [StringLength(200)]
    public string Message { get; set; } = string.Empty;
    public string? DetailJson { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class BrRuleTemplate
{
    public Guid Id { get; set; }
    [StringLength(40)]
    public string Code { get; set; } = string.Empty;
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;
    [StringLength(500)]
    public string? Description { get; set; }
    [StringLength(40)]
    public string Category { get; set; } = "General";
    public string FlowJson { get; set; } = "{}";
    public bool IsSystem { get; set; } = true;
}
