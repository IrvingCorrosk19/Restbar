using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services.Cash;

public class CashApprovalService : ICashApprovalService
{
    private readonly RestBarContext _context;
    private readonly ICashIntegrityService _integrity;

    public CashApprovalService(RestBarContext context, ICashIntegrityService integrity)
    {
        _context = context;
        _integrity = integrity;
    }

    public async Task<CashApproval> RequestApprovalAsync(CashApprovalRequest request, CancellationToken ct = default)
    {
        var session = await _context.CashSessions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.CashSessionId, ct)
            ?? throw new InvalidOperationException("Session not found.");

        var register = await _context.CashRegisters.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == session.CashRegisterId, ct);

        var approval = new CashApproval
        {
            Id = Guid.NewGuid(),
            CashSessionId = request.CashSessionId,
            CashMovementId = request.CashMovementId,
            ApprovalType = request.ApprovalType,
            Status = CashApprovalStatus.Pending,
            RequestedByUserId = request.RequestedByUserId,
            ActualAmount = request.ActualAmount,
            ThresholdAmount = register?.MaxPaidOutWithoutApproval,
            Reason = request.Reason
        };

        _context.CashApprovals.Add(approval);
        await _context.SaveChangesAsync(ct);

        await _integrity.AppendAuditEventAsync(new CashAuditEventInput(
            session.CompanyId, session.BranchId, request.RequestedByUserId, "ApprovalRequested",
            CashSessionId: request.CashSessionId,
            AfterJson: $"{{\"type\":\"{request.ApprovalType}\",\"amount\":{request.ActualAmount}}}"), ct);

        return approval;
    }

    public async Task<CashApproval> ResolveApprovalAsync(Guid approvalId, Guid approverUserId, bool approved, string? notes, CancellationToken ct = default)
    {
        var approval = await _context.CashApprovals.FirstOrDefaultAsync(a => a.Id == approvalId, ct)
            ?? throw new InvalidOperationException("Approval not found.");

        if (approval.Status != CashApprovalStatus.Pending)
            throw new InvalidOperationException("Approval already resolved.");

        approval.Status = approved ? CashApprovalStatus.Approved : CashApprovalStatus.Rejected;
        approval.ApprovedByUserId = approverUserId;
        approval.ResolvedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(notes))
            approval.Reason = $"{approval.Reason}; {notes}";

        await _context.SaveChangesAsync(ct);
        return approval;
    }

    public async Task<bool> RequiresDualApprovalAsync(Guid sessionId, CashApprovalType type, decimal amount, CancellationToken ct = default)
    {
        var session = await _context.CashSessions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct);
        if (session == null) return true;

        var register = await _context.CashRegisters.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == session.CashRegisterId, ct);

        return type switch
        {
            CashApprovalType.LargePaidOut => amount > (register?.MaxPaidOutWithoutApproval ?? 20m),
            CashApprovalType.Variance => Math.Abs(amount) > (register?.VarianceThresholdAmount ?? 5m),
            CashApprovalType.Reopen => true,
            CashApprovalType.RefundOverride => true,
            CashApprovalType.SessionClose => Math.Abs(session.Variance) > (register?.VarianceThresholdAmount ?? 5m),
            _ => false
        };
    }
}
