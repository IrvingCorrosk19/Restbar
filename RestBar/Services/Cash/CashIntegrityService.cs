using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RestBar.Infrastructure.Cash;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services.Cash;

public class CashIntegrityService : ICashIntegrityService
{
    private readonly RestBarContext _context;

    public CashIntegrityService(RestBarContext context) => _context = context;

    public async Task AppendAuditEventAsync(CashAuditEventInput input, CancellationToken ct = default)
    {
        var previousHash = await _context.CashAuditEvents.AsNoTracking()
            .Where(e => e.CashSessionId == input.CashSessionId)
            .OrderByDescending(e => e.CreatedAtUtc)
            .Select(e => e.EventHash)
            .FirstOrDefaultAsync(ct);

        var evt = new CashAuditEvent
        {
            Id = Guid.NewGuid(),
            CompanyId = input.CompanyId,
            BranchId = input.BranchId,
            CashSessionId = input.CashSessionId,
            CashMovementId = input.CashMovementId,
            EventType = input.EventType,
            ActorUserId = input.ActorUserId,
            ActorRole = input.ActorRole,
            BeforeJson = input.BeforeJson,
            AfterJson = input.AfterJson,
            IpAddress = input.IpAddress,
            DeviceId = input.DeviceId,
            PreviousEventHash = previousHash
        };

        evt.EventHash = CashHashChainBuilder.ComputeAuditEventHash(evt, previousHash);
        _context.CashAuditEvents.Add(evt);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> VerifyMovementChainAsync(Guid sessionId, CancellationToken ct = default)
    {
        var movements = await _context.CashMovements.AsNoTracking()
            .Where(m => m.CashSessionId == sessionId)
            .OrderBy(m => m.SequenceNumber)
            .ToListAsync(ct);

        string? prev = null;
        foreach (var m in movements)
        {
            if (m.PreviousHash != prev)
                return false;

            var expected = CashHashChainBuilder.ComputeMovementHash(m, prev);
            if (!string.Equals(m.RecordHash, expected, StringComparison.OrdinalIgnoreCase))
                return false;

            prev = m.RecordHash;
        }

        return true;
    }
}

public class CashReportService : ICashReportService
{
    private readonly RestBarContext _context;
    private readonly ICashReconciliationService _reconciliation;

    public CashReportService(RestBarContext context, ICashReconciliationService reconciliation)
    {
        _context = context;
        _reconciliation = reconciliation;
    }

    public async Task<CashZReport> GenerateZReportAsync(Guid sessionId, Guid userId, CancellationToken ct = default)
    {
        var existing = await _context.CashZReports.FirstOrDefaultAsync(z => z.CashSessionId == sessionId, ct);
        if (existing != null)
            return existing;

        var session = await _context.CashSessions.AsNoTracking()
            .Include(s => s.CashRegister)
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct)
            ?? throw new InvalidOperationException("Session not found.");

        if (session.Status != CashSessionStatus.Closed)
            throw new InvalidOperationException("Z report requires closed session.");

        var movements = await _context.CashMovements.AsNoTracking()
            .Where(m => m.CashSessionId == sessionId)
            .OrderBy(m => m.SequenceNumber)
            .ToListAsync(ct);

        var reportData = new
        {
            session.Id,
            session.SessionNumber,
            Register = session.CashRegister?.Name,
            session.OpenedAt,
            session.ClosedAt,
            session.OpeningFloatDeclared,
            ExpectedCash = await _reconciliation.GetExpectedCashAsync(sessionId, ct),
            session.CountedCash,
            session.Variance,
            session.TotalSales,
            session.TotalRefunds,
            session.TotalTips,
            session.TotalPaidIn,
            session.TotalPaidOut,
            session.ExpectedCard,
            session.ExpectedDigital,
            MovementCount = movements.Count
        };

        var json = JsonSerializer.Serialize(reportData);
        var generatedAt = DateTime.UtcNow;

        var report = new CashZReport
        {
            Id = Guid.NewGuid(),
            CashSessionId = sessionId,
            CompanyId = session.CompanyId,
            BranchId = session.BranchId,
            ReportJson = json,
            GeneratedAtUtc = generatedAt,
            GeneratedByUserId = userId,
            IntegrityHash = CashHashChainBuilder.ComputeZReportHash(json, sessionId, generatedAt)
        };

        _context.CashZReports.Add(report);
        await _context.SaveChangesAsync(ct);
        return report;
    }

    public async Task<object> GenerateXReportAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await _context.CashSessions.AsNoTracking()
            .Include(s => s.CashRegister)
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct)
            ?? throw new InvalidOperationException("Session not found.");

        var expectedCash = await _reconciliation.GetExpectedCashAsync(sessionId, ct);

        return new
        {
            session.Id,
            session.Status,
            session.OpenedAt,
            ExpectedCash = expectedCash,
            session.TotalSales,
            session.TotalRefunds,
            session.TotalTips,
            RegisterName = session.CashRegister?.Name
        };
    }

    public async Task<object> GetDashboardSnapshotAsync(Guid branchId, CancellationToken ct = default)
    {
        var activeSessions = await _context.CashSessions.AsNoTracking()
            .Where(s => s.BranchId == branchId &&
                        s.Status != CashSessionStatus.Closed &&
                        s.Status != CashSessionStatus.Historical &&
                        s.Status != CashSessionStatus.Audited)
            .Select(s => new
            {
                s.Id,
                s.Status,
                s.ExpectedCash,
                s.TotalSales,
                s.OpenedAt
            })
            .ToListAsync(ct);

        var todayStart = DateTime.UtcNow.Date;
        var closedToday = await _context.CashSessions.AsNoTracking()
            .CountAsync(s => s.BranchId == branchId && s.ClosedAt >= todayStart, ct);

        return new
        {
            ActiveSessions = activeSessions,
            ClosedSessionsToday = closedToday,
            TotalExpectedCash = activeSessions.Sum(s => s.ExpectedCash),
            TotalSalesActive = activeSessions.Sum(s => s.TotalSales)
        };
    }
}
