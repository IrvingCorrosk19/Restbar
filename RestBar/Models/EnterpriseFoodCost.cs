using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models;

public enum WasteReasonCode
{
    Spoilage, PrepError, Theft, Quality, OverProduction, Other
}

public enum MenuQuadrant
{
    Star, PlowHorse, Puzzle, Dog
}

public enum VarianceAlertType
{
    OverUsage, WasteSpike, CostSpike, NegativeMargin
}

public enum VarianceSeverity { Low, Medium, High, Critical }

public class FoodCostSnapshot
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal SalesTotal { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TheoreticalCogs { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ActualCogs { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal VarianceAmount { get; set; }
    [Column(TypeName = "decimal(8,4)")]
    public decimal VariancePercent { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal WasteCost { get; set; }
    [Column(TypeName = "decimal(8,4)")]
    public decimal FoodCostPercentTheo { get; set; }
    [Column(TypeName = "decimal(8,4)")]
    public decimal FoodCostPercentActual { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public Guid? GeneratedByUserId { get; set; }
}

public class RecipeCostHistory
{
    public Guid Id { get; set; }
    public Guid RecipeId { get; set; }
    public Guid ProductId { get; set; }
    public Guid CompanyId { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal TheoreticalCost { get; set; }
    [Column(TypeName = "decimal(8,4)")]
    public decimal FoodCostPercent { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal MarginAmount { get; set; }
    [StringLength(40)]
    public string Source { get; set; } = "Recalc";
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}

public class WasteEvent
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? StationId { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal UnitCost { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCost { get; set; }
    public WasteReasonCode ReasonCode { get; set; } = WasteReasonCode.Other;
    [StringLength(500)]
    public string? ReasonNotes { get; set; }
    public Guid ResponsibleUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public Guid? InventoryMovementId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual Product? Product { get; set; }
}

public class VarianceAlert
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    public VarianceAlertType AlertType { get; set; }
    public VarianceSeverity Severity { get; set; }
    [StringLength(500)]
    public string Message { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    [Column(TypeName = "decimal(8,4)")]
    public decimal VariancePercent { get; set; }
    public bool IsResolved { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class FoodCostAuditEvent
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    [StringLength(40)]
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    [StringLength(80)]
    public string EventType { get; set; } = string.Empty;
    public Guid ActorUserId { get; set; }
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    [StringLength(64)]
    public string? PreviousEventHash { get; set; }
    [StringLength(64)]
    public string? EventHash { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
