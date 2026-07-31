using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models;

public enum DiDecisionStatus
{
    New = 0,
    Reviewed = 1,
    Accepted = 2,
    Rejected = 3,
    Scheduled = 4,
    InProgress = 5,
    Completed = 6,
    Expired = 7,
    Verified = 8
}

/// <summary>Tracks manager decisions against DI recommendations for outcome learning.</summary>
public class DiDecisionRecord
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    [StringLength(40)]
    public string RecommendationCode { get; set; } = string.Empty;
    [StringLength(40)]
    public string Category { get; set; } = string.Empty;
    [StringLength(2000)]
    public string Observation { get; set; } = string.Empty;
    [StringLength(2000)]
    public string Evidence { get; set; } = string.Empty;
    [StringLength(1000)]
    public string RecommendedAction { get; set; } = string.Empty;
    [StringLength(1000)]
    public string? ExpectedImpact { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal? ExpectedImpactValue { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal? ActualImpactValue { get; set; }
    public DiDecisionStatus Status { get; set; } = DiDecisionStatus.New;
    public Guid CreatedByUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    [StringLength(1000)]
    public string? Comment { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? DueAtUtc { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }
}

/// <summary>Manual calendar markers that adjust forecast confidence (not weather APIs).</summary>
public class DiManualEvent
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public DateOnly EventDate { get; set; }
    [StringLength(40)]
    public string EventType { get; set; } = "LocalEvent"; // LocalEvent, Holiday, Promo, PartialClose, Maintenance
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    [StringLength(500)]
    public string? Notes { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>Persisted forecast run metadata + accuracy snapshot (JSON series optional).</summary>
public class DiForecastRun
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    [StringLength(40)]
    public string MetricCode { get; set; } = "SALES_DAILY";
    [StringLength(40)]
    public string ModelId { get; set; } = "naive";
    public int HorizonDays { get; set; }
    public int HistoryPoints { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal? Mae { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal? Mape { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal? Rmse { get; set; }
    public bool BeatsNaive { get; set; }
    [StringLength(20)]
    public string Confidence { get; set; } = "Baja";
    public string ForecastJson { get; set; } = "[]";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
