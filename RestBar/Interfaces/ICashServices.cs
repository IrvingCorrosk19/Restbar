using RestBar.Models;

namespace RestBar.Interfaces;

public interface ICashSessionService
{
    Task<CashSession> OpenSessionAsync(Guid registerId, Guid userId, decimal openingFloat, Guid? shiftId, CancellationToken ct = default);
    Task<CashSession> TransitionToOperatingAsync(Guid sessionId, Guid userId, CancellationToken ct = default);
    Task<CashSession> SuspendSessionAsync(Guid sessionId, Guid userId, CancellationToken ct = default);
    Task<CashSession> ResumeSessionAsync(Guid sessionId, Guid userId, CancellationToken ct = default);
    Task<CashSession> StartCloseAsync(Guid sessionId, Guid userId, CancellationToken ct = default);
    Task<CashSession?> GetActiveSessionForUserAsync(Guid userId, Guid branchId, CancellationToken ct = default);
    Task<CashSession?> GetByIdAsync(Guid sessionId, CancellationToken ct = default);
    Task<CashSession> ReopenSessionAsync(Guid closedSessionId, Guid managerUserId, string reason, CancellationToken ct = default);
}

public interface ICashMovementService
{
    Task<CashMovement> RecordMovementAsync(CashMovementRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<CashMovement>> GetSessionMovementsAsync(Guid sessionId, CancellationToken ct = default);
}

public interface ICashReconciliationService
{
    Task<decimal> GetExpectedCashAsync(Guid sessionId, CancellationToken ct = default);
    Task<CashSession> SubmitClosingCountAsync(Guid sessionId, Guid userId, decimal totalCounted, IEnumerable<CashCountLineInput>? lines, bool isBlind, CancellationToken ct = default);
    Task<CashSession> ApproveCloseAsync(Guid sessionId, Guid approverUserId, string? notes, CancellationToken ct = default);
    Task<CashSession> AbortCloseAsync(Guid sessionId, Guid userId, CancellationToken ct = default);
}

public interface ICashApprovalService
{
    Task<CashApproval> RequestApprovalAsync(CashApprovalRequest request, CancellationToken ct = default);
    Task<CashApproval> ResolveApprovalAsync(Guid approvalId, Guid approverUserId, bool approved, string? notes, CancellationToken ct = default);
    Task<bool> RequiresDualApprovalAsync(Guid sessionId, CashApprovalType type, decimal amount, CancellationToken ct = default);
}

public interface ICashReportService
{
    Task<CashZReport> GenerateZReportAsync(Guid sessionId, Guid userId, CancellationToken ct = default);
    Task<object> GenerateXReportAsync(Guid sessionId, CancellationToken ct = default);
    Task<object> GetDashboardSnapshotAsync(Guid branchId, CancellationToken ct = default);
}

public interface ICashIntegrityService
{
    Task AppendAuditEventAsync(CashAuditEventInput input, CancellationToken ct = default);
    Task<bool> VerifyMovementChainAsync(Guid sessionId, CancellationToken ct = default);
}

public interface ICashPaymentHook
{
    Task OnPaymentCompletedAsync(Payment payment, Order order, CancellationToken ct = default);
    Task OnRefundCompletedAsync(PaymentRefund refund, Payment payment, CancellationToken ct = default);
}

public interface ICashRegisterService
{
    Task<CashRegister> CreateRegisterAsync(CashRegister register, CancellationToken ct = default);
    Task<IReadOnlyList<CashRegister>> GetBranchRegistersAsync(Guid branchId, CancellationToken ct = default);
    Task<CashRegister?> GetByIdAsync(Guid registerId, CancellationToken ct = default);
}

public record CashMovementRequest(
    Guid CashSessionId,
    CashMovementType MovementType,
    CashMovementDirection Direction,
    decimal Amount,
    Guid PerformedByUserId,
    Guid? PaymentId = null,
    Guid? OrderId = null,
    Guid? PaymentRefundId = null,
    string? ReasonCode = null,
    string? Comments = null,
    string? IdempotencyKey = null,
    CashMovementSource Source = CashMovementSource.Manual,
    bool? AffectsCashDrawer = null,
    Guid? AuthorizedByUserId = null);

public record CashApprovalRequest(
    Guid CashSessionId,
    CashApprovalType ApprovalType,
    Guid RequestedByUserId,
    decimal? ActualAmount = null,
    string? Reason = null,
    Guid? CashMovementId = null);

public record CashAuditEventInput(
    Guid CompanyId,
    Guid BranchId,
    Guid ActorUserId,
    string EventType,
    Guid? CashSessionId = null,
    Guid? CashMovementId = null,
    string? ActorRole = null,
    string? BeforeJson = null,
    string? AfterJson = null,
    string? IpAddress = null,
    string? DeviceId = null);

public record CashCountLineInput(decimal DenominationValue, int Quantity);
