using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models;

/// <summary>Receta BOM — producto de venta compuesto por ingredientes.</summary>
public class Recipe
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Guid? CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual Product? Product { get; set; }
    public virtual ICollection<RecipeLine> Lines { get; set; } = new List<RecipeLine>();
}

public class RecipeLine
{
    public Guid Id { get; set; }
    public Guid RecipeId { get; set; }
    /// <summary>Producto ingrediente (debe tener TrackInventory).</summary>
    public Guid IngredientProductId { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }
    public Guid? StationId { get; set; }
    public virtual Recipe? Recipe { get; set; }
    public virtual Product? IngredientProduct { get; set; }
    public virtual Station? Station { get; set; }
}

public enum InventoryMovementType
{
    Purchase, Adjustment, Waste, TransferOut, TransferIn, Sale, CancelRestore, RefundRestore
}

public class InventoryMovement
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? StationId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? CompanyId { get; set; }
    public InventoryMovementType MovementType { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal StockBefore { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal StockAfter { get; set; }
    [StringLength(500)]
    public string? Reason { get; set; }
    [StringLength(100)]
    public string? Reference { get; set; }
    public Guid? UserId { get; set; }
    public Guid? OrderId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual Product? Product { get; set; }
    public virtual Station? Station { get; set; }
    public virtual User? User { get; set; }
}

public enum StockTransferStatus
{
    Pending, Approved, Rejected, Completed
}

public class StockTransfer
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid FromStationId { get; set; }
    public Guid ToStationId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? CompanyId { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }
    public StockTransferStatus Status { get; set; } = StockTransferStatus.Pending;
    public Guid? RequestedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    [StringLength(500)]
    public string? Notes { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public virtual Product? Product { get; set; }
    public virtual Station? FromStation { get; set; }
    public virtual Station? ToStation { get; set; }
}

public class Shift
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? CompanyId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public bool IsActive { get; set; } = true;
    [StringLength(500)]
    public string? Notes { get; set; }
    public virtual User? User { get; set; }
    public virtual ICollection<ShiftTableHandoff> TableHandoffs { get; set; } = new List<ShiftTableHandoff>();
}

public class ShiftTableHandoff
{
    public Guid Id { get; set; }
    public Guid ShiftId { get; set; }
    public Guid TableId { get; set; }
    public Guid FromUserId { get; set; }
    public Guid ToUserId { get; set; }
    public DateTime HandedOffAt { get; set; } = DateTime.UtcNow;
    public virtual Shift? Shift { get; set; }
    public virtual Table? Table { get; set; }
}

public enum RefundStatus
{
    Pending, Completed, Failed
}

public class PaymentRefund
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TipAmount { get; set; }
    [StringLength(500)]
    public string? Reason { get; set; }
    public RefundStatus Status { get; set; } = RefundStatus.Completed;
    public Guid? ProcessedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual Payment? Payment { get; set; }
    public virtual Order? Order { get; set; }
}

public class CommissionRule
{
    public Guid Id { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public UserRole? Role { get; set; }
    public Guid? StationId { get; set; }
    [Column(TypeName = "decimal(8,4)")]
    public decimal Rate { get; set; } = 0.05m;
    public bool IsActive { get; set; } = true;
    [StringLength(200)]
    public string? Description { get; set; }
}

public class TipAllocation
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    [Column(TypeName = "decimal(8,4)")]
    public decimal Percentage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual Payment? Payment { get; set; }
    public virtual User? User { get; set; }
}
