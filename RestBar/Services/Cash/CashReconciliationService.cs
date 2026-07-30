using Microsoft.EntityFrameworkCore;
using RestBar.Domain.Cash;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services.Cash;

public class CashReconciliationService : ICashReconciliationService
{
    private readonly RestBarContext _context;
    private readonly ICashIntegrityService _integrity;
    private readonly ICashMovementService _movementService;

    public CashReconciliationService(
        RestBarContext context,
        ICashIntegrityService integrity,
        ICashMovementService movementService)
    {
        _context = context;
        _integrity = integrity;
        _movementService = movementService;
    }

    public async Task<decimal> GetExpectedCashAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await _context.CashSessions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct)
            ?? throw new InvalidOperationException("Session not found.");

        if (session.ExpectedCash != 0)
            return session.ExpectedCash;

        var net = await _context.CashMovements.AsNoTracking()
            .Where(m => m.CashSessionId == sessionId && m.AffectsCashDrawer)
            .SumAsync(m => m.Direction == CashMovementDirection.In ? m.Amount : -m.Amount, ct);

        return net;
    }

    public async Task<CashSession> SubmitClosingCountAsync(
        Guid sessionId, Guid userId, decimal totalCounted,
        IEnumerable<CashCountLineInput>? lines, bool isBlind, CancellationToken ct = default)
    {
        var session = await _context.CashSessions.FirstOrDefaultAsync(s => s.Id == sessionId, ct)
            ?? throw new InvalidOperationException("Session not found.");

        CashSessionStateMachine.EnsureTransition(session.Status, CashSessionStatus.Reconciling);

        var count = new CashCount
        {
            Id = Guid.NewGuid(),
            CashSessionId = sessionId,
            CountType = CashCountType.Closing,
            CountedByUserId = userId,
            TotalCounted = totalCounted,
            IsBlind = isBlind
        };

        if (lines != null)
        {
            foreach (var line in lines)
            {
                count.Lines.Add(new CashCountLine
                {
                    Id = Guid.NewGuid(),
                    DenominationValue = line.DenominationValue,
                    Quantity = line.Quantity,
                    Subtotal = line.DenominationValue * line.Quantity
                });
            }
        }

        _context.CashCounts.Add(count);

        session.CountedCash = totalCounted;
        session.ExpectedCash = await GetExpectedCashAsync(sessionId, ct);
        session.Variance = totalCounted - session.ExpectedCash;
        session.Status = CashSessionStatus.Reconciling;

        await _context.SaveChangesAsync(ct);

        await _integrity.AppendAuditEventAsync(new CashAuditEventInput(
            session.CompanyId, session.BranchId, userId, "ClosingCountSubmitted",
            CashSessionId: sessionId,
            AfterJson: $"{{\"counted\":{totalCounted},\"variance\":{session.Variance}}}"), ct);

        return session;
    }

    public async Task<CashSession> ApproveCloseAsync(Guid sessionId, Guid approverUserId, string? notes, CancellationToken ct = default)
    {
        var session = await _context.CashSessions.FirstOrDefaultAsync(s => s.Id == sessionId, ct)
            ?? throw new InvalidOperationException("Session not found.");

        CashSessionStateMachine.EnsureTransition(session.Status, CashSessionStatus.Closed);

        var hasClosingCount = await _context.CashCounts.AnyAsync(c =>
            c.CashSessionId == sessionId && c.CountType == CashCountType.Closing, ct);
        if (!hasClosingCount)
            throw new InvalidOperationException("Closing count required before close.");

        session.Status = CashSessionStatus.Closed;
        session.ClosedAt = DateTime.UtcNow;
        session.ClosedByUserId = approverUserId;
        session.CloseNotes = notes;

        await _movementService.RecordMovementAsync(new CashMovementRequest(
            session.Id,
            CashMovementType.SessionClose,
            CashMovementDirection.Out,
            0,
            approverUserId,
            Source: CashMovementSource.System,
            AffectsCashDrawer: false), ct);
        await _context.SaveChangesAsync(ct);

        await _integrity.AppendAuditEventAsync(new CashAuditEventInput(
            session.CompanyId, session.BranchId, approverUserId, "SessionClosed",
            CashSessionId: sessionId), ct);

        return session;
    }

    public async Task<CashSession> AbortCloseAsync(Guid sessionId, Guid userId, CancellationToken ct = default)
    {
        var session = await _context.CashSessions.FirstOrDefaultAsync(s => s.Id == sessionId, ct)
            ?? throw new InvalidOperationException("Session not found.");

        CashSessionStateMachine.EnsureTransition(session.Status, CashSessionStatus.Operating);
        session.Status = CashSessionStatus.Operating;
        await _context.SaveChangesAsync(ct);
        return session;
    }
}
