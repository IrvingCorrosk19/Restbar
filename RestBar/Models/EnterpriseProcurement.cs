using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models;

public enum SupplierStatus { Active, Inactive, OnHold, Blacklisted, Preferred }

public enum PurchaseRequestStatus
{
    Draft, Pending, Approved, Rejected, Cancelled, Converted, Completed, Audited
}

public enum PurchaseOrderStatus
{
    Draft, PendingApproval, Approved, Sent, PartiallyReceived,
    FullyReceived, Closed, Cancelled, Returned, Audited
}

public enum GoodsReceiptStatus { Draft, InProgress, Completed, Cancelled, Disputed }

public enum ReceiptLineDisposition { Accepted, Partial, Rejected, Damaged, Short, Over }

public enum PurchaseApprovalType { Request, Order, Variance, Emergency, Return }

public enum PurchaseApprovalStatus { Pending, Approved, Rejected }

public enum PriceHistorySource { Receipt, Manual, Import }

public class Supplier : ITrackableEntity
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    [StringLength(30)]
    public string Code { get; set; } = string.Empty;
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    [StringLength(50)]
    public string? TaxId { get; set; }
    [StringLength(200)]
    public string? Email { get; set; }
    [StringLength(50)]
    public string? Phone { get; set; }
    public int PaymentTermsDays { get; set; } = 30;
    public int LeadTimeDays { get; set; } = 2;
    public SupplierStatus Status { get; set; } = SupplierStatus.Active;
    public bool IsPreferred { get; set; }
    [Column(TypeName = "decimal(5,2)")]
    public decimal ScoreOverall { get; set; } = 70m;
    [StringLength(1000)]
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public virtual Company? Company { get; set; }
    public virtual ICollection<SupplierContact> Contacts { get; set; } = new List<SupplierContact>();
    public virtual ICollection<SupplierProduct> Products { get; set; } = new List<SupplierProduct>();
}

public class SupplierContact
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;
    [StringLength(80)]
    public string? Role { get; set; }
    [StringLength(200)]
    public string? Email { get; set; }
    [StringLength(50)]
    public string? Phone { get; set; }
    public bool IsPrimary { get; set; }
    public virtual Supplier? Supplier { get; set; }
}

public class SupplierProduct
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public Guid ProductId { get; set; }
    public Guid CompanyId { get; set; }
    [StringLength(60)]
    public string? SupplierSku { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal PackSize { get; set; } = 1m;
    [StringLength(20)]
    public string UnitOfMeasure { get; set; } = "UND";
    [Column(TypeName = "decimal(18,4)")]
    public decimal AgreedUnitPrice { get; set; }
    [StringLength(3)]
    public string CurrencyCode { get; set; } = "USD";
    [Column(TypeName = "decimal(18,4)")]
    public decimal MinOrderQty { get; set; } = 1m;
    public bool IsActive { get; set; } = true;
    public int? LeadTimeOverrideDays { get; set; }
    public virtual Supplier? Supplier { get; set; }
    public virtual Product? Product { get; set; }
}

public class PurchaseRequest
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    [StringLength(40)]
    public string RequestNumber { get; set; } = string.Empty;
    public PurchaseRequestStatus Status { get; set; } = PurchaseRequestStatus.Draft;
    public Guid RequestedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    [StringLength(1000)]
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public virtual ICollection<PurchaseRequestLine> Lines { get; set; } = new List<PurchaseRequestLine>();
}

public class PurchaseRequestLine
{
    public Guid Id { get; set; }
    public Guid PurchaseRequestId { get; set; }
    public Guid ProductId { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }
    [StringLength(20)]
    public string UnitOfMeasure { get; set; } = "UND";
    public Guid? PreferredSupplierId { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal? EstimatedUnitCost { get; set; }
    public Guid? StationId { get; set; }
    [StringLength(500)]
    public string? Notes { get; set; }
    public virtual PurchaseRequest? PurchaseRequest { get; set; }
    public virtual Product? Product { get; set; }
}

public class PurchaseOrder
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    public Guid SupplierId { get; set; }
    public Guid? PurchaseRequestId { get; set; }
    [StringLength(40)]
    public string PoNumber { get; set; } = string.Empty;
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpectedDelivery { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Tax { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }
    [StringLength(3)]
    public string CurrencyCode { get; set; } = "USD";
    public Guid RequestedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    [StringLength(1000)]
    public string? Notes { get; set; }
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public virtual Supplier? Supplier { get; set; }
    public virtual ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
    public virtual ICollection<GoodsReceipt> Receipts { get; set; } = new List<GoodsReceipt>();
}

public class PurchaseOrderLine
{
    public Guid Id { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? SupplierProductId { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal QuantityOrdered { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal QuantityReceived { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal UnitPrice { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal LineTotal { get; set; }
    [StringLength(20)]
    public string UnitOfMeasure { get; set; } = "UND";
    public Guid? StationId { get; set; }
    [StringLength(500)]
    public string? Notes { get; set; }
    public virtual PurchaseOrder? PurchaseOrder { get; set; }
    public virtual Product? Product { get; set; }
}

public class GoodsReceipt
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    public Guid PurchaseOrderId { get; set; }
    [StringLength(40)]
    public string ReceiptNumber { get; set; } = string.Empty;
    public GoodsReceiptStatus Status { get; set; } = GoodsReceiptStatus.Draft;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public Guid ReceivedByUserId { get; set; }
    public Guid? SupervisedByUserId { get; set; }
    public bool? TemperatureOk { get; set; }
    [StringLength(1000)]
    public string? Notes { get; set; }
    public virtual PurchaseOrder? PurchaseOrder { get; set; }
    public virtual ICollection<GoodsReceiptLine> Lines { get; set; } = new List<GoodsReceiptLine>();
}

public class GoodsReceiptLine
{
    public Guid Id { get; set; }
    public Guid GoodsReceiptId { get; set; }
    public Guid PurchaseOrderLineId { get; set; }
    public Guid ProductId { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal QtyOrdered { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal QtyReceived { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal QtyAccepted { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal QtyRejected { get; set; }
    public ReceiptLineDisposition Disposition { get; set; } = ReceiptLineDisposition.Accepted;
    [Column(TypeName = "decimal(18,4)")]
    public decimal UnitCost { get; set; }
    [StringLength(60)]
    public string? LotNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    [StringLength(500)]
    public string? Notes { get; set; }
    public virtual GoodsReceipt? GoodsReceipt { get; set; }
    public virtual PurchaseOrderLine? PurchaseOrderLine { get; set; }
    public virtual Product? Product { get; set; }
}

public class PurchaseApproval
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    [StringLength(30)]
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public PurchaseApprovalType ApprovalType { get; set; }
    public PurchaseApprovalStatus Status { get; set; } = PurchaseApprovalStatus.Pending;
    public Guid RequestedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal? ThresholdAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal? ActualAmount { get; set; }
    [StringLength(500)]
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}

public class SupplierScore
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public Guid CompanyId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    [Column(TypeName = "decimal(5,2)")]
    public decimal PriceScore { get; set; }
    [Column(TypeName = "decimal(5,2)")]
    public decimal OtifScore { get; set; }
    [Column(TypeName = "decimal(5,2)")]
    public decimal QualityScore { get; set; }
    [Column(TypeName = "decimal(5,2)")]
    public decimal ReliabilityScore { get; set; }
    [Column(TypeName = "decimal(5,2)")]
    public decimal OverallScore { get; set; }
    public DateTime ComputedAt { get; set; } = DateTime.UtcNow;
    public virtual Supplier? Supplier { get; set; }
}

public class PriceHistory
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? SupplierId { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal UnitCost { get; set; }
    public PriceHistorySource Source { get; set; } = PriceHistorySource.Receipt;
    public Guid? GoodsReceiptId { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public virtual Product? Product { get; set; }
}

public class ProcurementAuditEvent
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
    [StringLength(50)]
    public string? ActorRole { get; set; }
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    [StringLength(45)]
    public string? IpAddress { get; set; }
    [StringLength(100)]
    public string? DeviceId { get; set; }
    [StringLength(64)]
    public string? PreviousEventHash { get; set; }
    [StringLength(64)]
    public string? EventHash { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
