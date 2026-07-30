using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models;

public enum CashRegisterType
{
    Physical, Virtual, Mobile, Station, Shared, Central, Temporary, SelfService, Delivery, Franchise
}

public enum CashSessionStatus
{
    Prepared, Open, Operating, Suspended, Counting, Reconciling, Closed, Blocked, Audited, Historical
}

public enum CashMovementType
{
    OpeningFloat, SaleCash, SaleCard, SaleYappy, SaleACH, SaleOther,
    TipCash, TipNonCash, ChangeGiven, PaidIn, PaidOut, PettyPurchase,
    RefundCash, VoidReversal, AdjustmentIn, AdjustmentOut, DropToSafe,
    DepositBank, TransferOut, TransferIn, ConciliationDiff, SessionClose, ReopenMarker
}

public enum CashMovementDirection { In, Out }

public enum CashMovementSource { Manual, Payment, Refund, Void, System, Adjustment }

public enum CashCountType { Opening, MidShift, Closing, SpotCheck }

public enum CashApprovalType
{
    Variance, Reopen, LargePaidOut, RefundOverride, SessionClose
}

public enum CashApprovalStatus { Pending, Approved, Rejected }

public enum CashIncidentType
{
    Shortage, Overage, SuspiciousVoid, ForcedClose, SystemError
}

public enum CashIncidentSeverity { Low, Medium, High, Critical }

public enum CashIncidentStatus { Open, Investigating, Resolved, Escalated }

public class CashRegister : ITrackableEntity
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    public CashRegisterType RegisterType { get; set; } = CashRegisterType.Physical;
    [Column(TypeName = "decimal(18,2)")]
    public decimal DefaultOpeningFloat { get; set; }
    public bool RequiresBlindClose { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal VarianceThresholdAmount { get; set; } = 5m;
    [Column(TypeName = "decimal(8,4)")]
    public decimal VarianceThresholdPercent { get; set; } = 0.001m;
    [Column(TypeName = "decimal(18,2)")]
    public decimal MaxPaidOutWithoutApproval { get; set; } = 20m;
    public Guid? StationId { get; set; }
    public bool IsActive { get; set; } = true;
    public int BusinessDayCutoffHour { get; set; } = 4;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public virtual Company? Company { get; set; }
    public virtual Branch? Branch { get; set; }
    public virtual Station? Station { get; set; }
    public virtual ICollection<CashSession> Sessions { get; set; } = new List<CashSession>();
}

public class CashSession
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    public Guid CashRegisterId { get; set; }
    public Guid? ShiftId { get; set; }
    public int SessionNumber { get; set; }
    public CashSessionStatus Status { get; set; } = CashSessionStatus.Prepared;
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
    public Guid OpenedByUserId { get; set; }
    public Guid? ClosedByUserId { get; set; }
    public Guid? SupervisorUserId { get; set; }
    public Guid? ManagerUserId { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal OpeningFloatDeclared { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ExpectedCash { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal CountedCash { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Variance { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ExpectedCard { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ExpectedDigital { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalSales { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalRefunds { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalTips { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPaidIn { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPaidOut { get; set; }
    public bool BlindCloseEnabled { get; set; }
    [StringLength(1000)]
    public string? CloseNotes { get; set; }
    public Guid? ReopenedFromSessionId { get; set; }
    public byte[] RowVersion { get; set; } = Guid.NewGuid().ToByteArray();
    public virtual CashRegister? CashRegister { get; set; }
    public virtual Shift? Shift { get; set; }
    public virtual User? OpenedByUser { get; set; }
    public virtual ICollection<CashMovement> Movements { get; set; } = new List<CashMovement>();
    public virtual ICollection<CashCount> Counts { get; set; } = new List<CashCount>();
    public virtual ICollection<CashApproval> Approvals { get; set; } = new List<CashApproval>();
}

public class CashMovement
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    public Guid CashSessionId { get; set; }
    public CashMovementType MovementType { get; set; }
    public CashMovementDirection Direction { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    [StringLength(3)]
    public string CurrencyCode { get; set; } = "USD";
    public Guid? PaymentId { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? PaymentRefundId { get; set; }
    public Guid? RelatedMovementId { get; set; }
    [StringLength(50)]
    public string? ReasonCode { get; set; }
    [StringLength(500)]
    public string? Comments { get; set; }
    public Guid PerformedByUserId { get; set; }
    public Guid? AuthorizedByUserId { get; set; }
    public int SequenceNumber { get; set; }
    [StringLength(64)]
    public string? PreviousHash { get; set; }
    [StringLength(64)]
    public string? RecordHash { get; set; }
    [StringLength(100)]
    public string? IdempotencyKey { get; set; }
    public CashMovementSource Source { get; set; } = CashMovementSource.Manual;
    [StringLength(100)]
    public string? DeviceId { get; set; }
    [StringLength(45)]
    public string? IpAddress { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool AffectsCashDrawer { get; set; } = true;
    public virtual CashSession? CashSession { get; set; }
    public virtual Payment? Payment { get; set; }
}

public class CashCount
{
    public Guid Id { get; set; }
    public Guid CashSessionId { get; set; }
    public CashCountType CountType { get; set; }
    public DateTime CountedAtUtc { get; set; } = DateTime.UtcNow;
    public Guid CountedByUserId { get; set; }
    public Guid? WitnessUserId { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCounted { get; set; }
    public bool IsBlind { get; set; }
    public virtual CashSession? CashSession { get; set; }
    public virtual ICollection<CashCountLine> Lines { get; set; } = new List<CashCountLine>();
}

public class CashCountLine
{
    public Guid Id { get; set; }
    public Guid CashCountId { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal DenominationValue { get; set; }
    public int Quantity { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }
    public virtual CashCount? CashCount { get; set; }
}

public class CashApproval
{
    public Guid Id { get; set; }
    public Guid CashSessionId { get; set; }
    public Guid? CashMovementId { get; set; }
    public CashApprovalType ApprovalType { get; set; }
    public CashApprovalStatus Status { get; set; } = CashApprovalStatus.Pending;
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
    public virtual CashSession? CashSession { get; set; }
}

public class CashIncident
{
    public Guid Id { get; set; }
    public Guid CashSessionId { get; set; }
    public CashIncidentType IncidentType { get; set; }
    public CashIncidentSeverity Severity { get; set; }
    public CashIncidentStatus Status { get; set; } = CashIncidentStatus.Open;
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    public Guid? ResolvedByUserId { get; set; }
    public DateTime? ResolvedAt { get; set; }
    [StringLength(1000)]
    public string? ResolutionNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual CashSession? CashSession { get; set; }
}

public class CashAuditEvent
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    public Guid? CashSessionId { get; set; }
    public Guid? CashMovementId { get; set; }
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

public class CashZReport
{
    public Guid Id { get; set; }
    public Guid CashSessionId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    public string ReportJson { get; set; } = "{}";
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
    public Guid GeneratedByUserId { get; set; }
    [StringLength(64)]
    public string IntegrityHash { get; set; } = string.Empty;
    public virtual CashSession? CashSession { get; set; }
}
