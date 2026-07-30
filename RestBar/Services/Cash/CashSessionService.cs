using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestBar.Domain.Cash;
using RestBar.Infrastructure.Cash;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services.Cash;

public class CashSessionService : ICashSessionService
{
    private readonly RestBarContext _context;
    private readonly ICashMovementService _movementService;
    private readonly ICashIntegrityService _integrity;
    private readonly ILogger<CashSessionService> _logger;

    public CashSessionService(
        RestBarContext context,
        ICashMovementService movementService,
        ICashIntegrityService integrity,
        ILogger<CashSessionService> logger)
    {
        _context = context;
        _movementService = movementService;
        _integrity = integrity;
        _logger = logger;
    }

    public async Task<CashSession> OpenSessionAsync(Guid registerId, Guid userId, decimal openingFloat, Guid? shiftId, CancellationToken ct = default)
    {
        var register = await _context.CashRegisters.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == registerId && r.IsActive, ct)
            ?? throw new InvalidOperationException("Cash register not found or inactive.");

        var hasOpen = await _context.CashSessions.AnyAsync(s =>
            s.CashRegisterId == registerId &&
            s.Status != CashSessionStatus.Closed &&
            s.Status != CashSessionStatus.Historical &&
            s.Status != CashSessionStatus.Audited, ct);

        if (hasOpen)
            throw new InvalidOperationException("Register already has an active session.");

        var sessionNumber = await _context.CashSessions
            .Where(s => s.CashRegisterId == registerId)
            .Select(s => (int?)s.SessionNumber)
            .MaxAsync(ct) ?? 0;

        var session = new CashSession
        {
            Id = Guid.NewGuid(),
            CompanyId = register.CompanyId,
            BranchId = register.BranchId,
            CashRegisterId = registerId,
            ShiftId = shiftId,
            SessionNumber = sessionNumber + 1,
            Status = CashSessionStatus.Open,
            OpenedByUserId = userId,
            OpeningFloatDeclared = openingFloat,
            ExpectedCash = 0,
            BlindCloseEnabled = register.RequiresBlindClose
        };

        _context.CashSessions.Add(session);
        await _context.SaveChangesAsync(ct);

        await _movementService.RecordMovementAsync(new CashMovementRequest(
            session.Id,
            CashMovementType.OpeningFloat,
            CashMovementDirection.In,
            openingFloat,
            userId,
            Source: CashMovementSource.System,
            AffectsCashDrawer: true), ct);

        await _integrity.AppendAuditEventAsync(new CashAuditEventInput(
            session.CompanyId, session.BranchId, userId, "SessionOpened",
            CashSessionId: session.Id,
            AfterJson: $"{{\"registerId\":\"{registerId}\",\"float\":{openingFloat}}}"), ct);

        _logger.LogInformation("[Cash] Session opened {SessionId} on register {RegisterId}", session.Id, registerId);
        return session;
    }

    public async Task<CashSession> TransitionToOperatingAsync(Guid sessionId, Guid userId, CancellationToken ct = default)
    {
        var session = await GetTrackedSessionAsync(sessionId, ct);
        CashSessionStateMachine.EnsureTransition(session.Status, CashSessionStatus.Operating);
        session.Status = CashSessionStatus.Operating;
        await _context.SaveChangesAsync(ct);
        return session;
    }

    public async Task<CashSession> SuspendSessionAsync(Guid sessionId, Guid userId, CancellationToken ct = default)
    {
        var session = await GetTrackedSessionAsync(sessionId, ct);
        CashSessionStateMachine.EnsureTransition(session.Status, CashSessionStatus.Suspended);
        session.Status = CashSessionStatus.Suspended;
        session.SupervisorUserId = userId;
        await _context.SaveChangesAsync(ct);
        return session;
    }

    public async Task<CashSession> ResumeSessionAsync(Guid sessionId, Guid userId, CancellationToken ct = default)
    {
        var session = await GetTrackedSessionAsync(sessionId, ct);
        CashSessionStateMachine.EnsureTransition(session.Status, CashSessionStatus.Operating);
        session.Status = CashSessionStatus.Operating;
        await _context.SaveChangesAsync(ct);
        return session;
    }

    public async Task<CashSession> StartCloseAsync(Guid sessionId, Guid userId, CancellationToken ct = default)
    {
        var session = await GetTrackedSessionAsync(sessionId, ct);
        CashSessionStateMachine.EnsureTransition(session.Status, CashSessionStatus.Counting);
        session.Status = CashSessionStatus.Counting;
        await _context.SaveChangesAsync(ct);
        return session;
    }

    public async Task<CashSession?> GetActiveSessionForUserAsync(Guid userId, Guid branchId, CancellationToken ct = default)
    {
        return await _context.CashSessions.AsNoTracking()
            .Where(s => s.BranchId == branchId &&
                        s.OpenedByUserId == userId &&
                        (s.Status == CashSessionStatus.Open ||
                         s.Status == CashSessionStatus.Operating ||
                         s.Status == CashSessionStatus.Suspended ||
                         s.Status == CashSessionStatus.Counting ||
                         s.Status == CashSessionStatus.Reconciling))
            .OrderByDescending(s => s.OpenedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<CashSession?> GetByIdAsync(Guid sessionId, CancellationToken ct = default)
    {
        return await _context.CashSessions.AsNoTracking()
            .Include(s => s.CashRegister)
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct);
    }

    public async Task<CashSession> ReopenSessionAsync(Guid closedSessionId, Guid managerUserId, string reason, CancellationToken ct = default)
    {
        var closed = await GetTrackedSessionAsync(closedSessionId, ct);
        if (closed.Status != CashSessionStatus.Closed)
            throw new InvalidOperationException("Only closed sessions can be reopened.");

        var approval = new CashApproval
        {
            Id = Guid.NewGuid(),
            CashSessionId = closedSessionId,
            ApprovalType = CashApprovalType.Reopen,
            Status = CashApprovalStatus.Approved,
            RequestedByUserId = managerUserId,
            ApprovedByUserId = managerUserId,
            Reason = reason,
            ResolvedAt = DateTime.UtcNow
        };
        _context.CashApprovals.Add(approval);

        var newSession = await OpenSessionAsync(closed.CashRegisterId, managerUserId, closed.OpeningFloatDeclared, closed.ShiftId, ct);
        newSession.ReopenedFromSessionId = closedSessionId;
        await _context.SaveChangesAsync(ct);
        return newSession;
    }

    private async Task<CashSession> GetTrackedSessionAsync(Guid sessionId, CancellationToken ct)
    {
        return await _context.CashSessions.FirstOrDefaultAsync(s => s.Id == sessionId, ct)
            ?? throw new InvalidOperationException("Cash session not found.");
    }
}
