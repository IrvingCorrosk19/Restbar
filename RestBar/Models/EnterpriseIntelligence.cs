using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models;

public enum BiSeverity { Info, Low, Medium, High, Critical }

public enum BiInsightType
{
    SalesDrop, FoodCostHigh, WasteSpike, CashRisk, SupplierCritical,
    LowStock, NegativeMargin, Opportunity, OverduePurchases
}

public enum BiSubjectType { Branch, Supplier, Product, Company }

public class ExecutiveSnapshot
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    [StringLength(20)]
    public string PeriodType { get; set; } = "Today";
    public string SnapshotJson { get; set; } = "{}";
    [Column(TypeName = "decimal(5,2)")]
    public decimal EnterpriseScore { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class BiInsight
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    public BiInsightType InsightType { get; set; }
    public BiSeverity Severity { get; set; }
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    [StringLength(1000)]
    public string Explanation { get; set; } = string.Empty;
    [StringLength(500)]
    public string RecommendedAction { get; set; } = string.Empty;
    [StringLength(40)]
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public bool IsDismissed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class BiAlert
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    [StringLength(40)]
    public string AlertCode { get; set; } = string.Empty;
    public BiSeverity Severity { get; set; }
    [StringLength(500)]
    public string Message { get; set; } = string.Empty;
    [StringLength(40)]
    public string SourceModule { get; set; } = string.Empty;
    public bool IsResolved { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class BiScore
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public BiSubjectType SubjectType { get; set; }
    public Guid SubjectId { get; set; }
    [Column(TypeName = "decimal(5,2)")]
    public decimal Score { get; set; }
    public string DimensionsJson { get; set; } = "{}";
    public DateTime ComputedAt { get; set; } = DateTime.UtcNow;
}

public class BiAuditEvent
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    public Guid ActorUserId { get; set; }
    [StringLength(80)]
    public string QueryName { get; set; } = string.Empty;
    public string? FiltersJson { get; set; }
    public int DurationMs { get; set; }
    [StringLength(45)]
    public string? IpAddress { get; set; }
    [StringLength(64)]
    public string? EventHash { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class ForecastSeed
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    [StringLength(40)]
    public string MetricCode { get; set; } = string.Empty;
    public DateTime AsOfDate { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal Value { get; set; }
    [StringLength(40)]
    public string Source { get; set; } = "Historical";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
