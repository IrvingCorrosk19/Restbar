using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RestBar.Infrastructure.Foundation;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Controllers;

[Authorize(Policy = "CashAccess")]
public class CashSessionController : Controller
{
    private readonly ICashSessionService _sessions;
    private readonly ICashRegisterService _registers;
    private readonly ICashReconciliationService _reconciliation;
    private readonly ICashReportService _reports;
    private readonly RestBarContext _context;
    private readonly FeatureFlags _flags;

    public CashSessionController(
        ICashSessionService sessions,
        ICashRegisterService registers,
        ICashReconciliationService reconciliation,
        ICashReportService reports,
        RestBarContext context,
        IOptions<FeatureFlags> flags)
    {
        _sessions = sessions;
        _registers = registers;
        _reconciliation = reconciliation;
        _reports = reports;
        _context = context;
        _flags = flags.Value;
    }

    private IActionResult? Disabled() =>
        !_flags.EnableCashModule ? View("ModuleDisabled") : null;

    private bool CanAccessCashSession(CashSession session)
    {
        var role = User.FindFirst("UserRole")?.Value;
        if (string.Equals(role, "superadmin", StringComparison.OrdinalIgnoreCase))
            return true;

        var companyId = Guid.TryParse(User.FindFirst("CompanyId")?.Value, out var cid) ? cid : (Guid?)null;
        var branchId = Guid.TryParse(User.FindFirst("BranchId")?.Value, out var bid) ? bid : (Guid?)null;

        if (companyId.HasValue && session.CompanyId != companyId.Value)
            return false;
        if (branchId.HasValue && session.BranchId != branchId.Value)
            return false;
        // Fail closed for non-superadmin without claims
        if (!companyId.HasValue && !branchId.HasValue)
            return false;
        return true;
    }

    public async Task<IActionResult> Dashboard()
    {
        if (Disabled() is { } d) return d;
        var branchId = Guid.Parse(User.FindFirst("BranchId")!.Value);
        var snapshot = await _reports.GetDashboardSnapshotAsync(branchId);
        ViewBag.Snapshot = snapshot;

        var registerIds = await _context.CashRegisters.AsNoTracking()
            .Where(r => r.BranchId == branchId)
            .Select(r => r.Id)
            .ToListAsync();

        var activeSessions = await _context.CashSessions.AsNoTracking()
            .Where(s => (s.BranchId == branchId || registerIds.Contains(s.CashRegisterId)) &&
                        s.Status != CashSessionStatus.Closed &&
                        s.Status != CashSessionStatus.Historical &&
                        s.Status != CashSessionStatus.Audited)
            .OrderByDescending(s => s.OpenedAt)
            .Select(s => new CashSessionListItem
            {
                Id = s.Id,
                Status = s.Status.ToString(),
                ExpectedCash = s.ExpectedCash,
                OpenedAt = s.OpenedAt,
                SessionNumber = s.SessionNumber
            })
            .ToListAsync();
        ViewBag.ActiveSessions = activeSessions;
        return View();
    }

    public class CashSessionListItem
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = "";
        public decimal ExpectedCash { get; set; }
        public DateTime OpenedAt { get; set; }
        public int SessionNumber { get; set; }
    }

    [HttpGet]
    public async Task<IActionResult> OpenWizard()
    {
        if (Disabled() is { } d) return d;

        var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);
        var branchId = Guid.Parse(User.FindFirst("BranchId")!.Value);
        var registers = await _registers.GetBranchRegistersAsync(branchId);
        if (registers.Count == 0)
        {
            await _registers.CreateRegisterAsync(new CashRegister
            {
                CompanyId = companyId,
                BranchId = branchId,
                Code = "CAJA-1",
                Name = "Caja Principal",
                DefaultOpeningFloat = 100m
            });
            registers = await _registers.GetBranchRegistersAsync(branchId);
        }

        ViewBag.Registers = registers;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Open(Guid registerId, decimal openingFloat)
    {
        if (Disabled() is { } d) return d;
        var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
        try
        {
            var shift = await _context.Shifts.AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

            var session = await _sessions.OpenSessionAsync(registerId, userId, openingFloat, shift?.Id);
            return RedirectToAction(nameof(Detail), new { id = session.Id });
        }
        catch (InvalidOperationException ex)
        {
            // If already open, jump to that session detail instead of dead-end wizard
            var existing = await _context.CashSessions.AsNoTracking()
                .Where(s => s.CashRegisterId == registerId &&
                            s.Status != CashSessionStatus.Closed &&
                            s.Status != CashSessionStatus.Historical &&
                            s.Status != CashSessionStatus.Audited)
                .OrderByDescending(s => s.OpenedAt)
                .FirstOrDefaultAsync();
            if (existing != null)
                return RedirectToAction(nameof(Detail), new { id = existing.Id });

            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(OpenWizard));
        }
    }

    public async Task<IActionResult> Detail(Guid id)
    {
        if (Disabled() is { } d) return d;
        var session = await _sessions.GetByIdAsync(id);
        if (session == null || !CanAccessCashSession(session))
            return NotFound();

        ViewBag.ExpectedCash = await _reconciliation.GetExpectedCashAsync(id);
        ViewBag.XReport = await _reports.GenerateXReportAsync(id);
        return View(session);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartClose(Guid id)
    {
        if (Disabled() is { } d) return d;
        var existing = await _sessions.GetByIdAsync(id);
        if (existing == null || !CanAccessCashSession(existing))
            return NotFound();
        try
        {
            var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
            await _sessions.StartCloseAsync(id, userId);
            return RedirectToAction(nameof(Arqueo), new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Detail), new { id });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Arqueo(Guid id)
    {
        if (Disabled() is { } d) return d;
        var session = await _sessions.GetByIdAsync(id);
        if (session == null || !CanAccessCashSession(session))
            return NotFound();

        ViewBag.ExpectedCash = await _reconciliation.GetExpectedCashAsync(id);
        return View(session);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitCount(Guid id, decimal totalCounted, bool isBlind)
    {
        if (Disabled() is { } d) return d;
        var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
        await _reconciliation.SubmitClosingCountAsync(id, userId, totalCounted, null, isBlind);
        return RedirectToAction(nameof(Reconciliation), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Reconciliation(Guid id)
    {
        if (Disabled() is { } d) return d;
        var session = await _sessions.GetByIdAsync(id);
        if (session == null || !CanAccessCashSession(session))
            return NotFound();
        return View(session);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveClose(Guid id, string? notes)
    {
        if (Disabled() is { } d) return d;
        var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
        await _reconciliation.ApproveCloseAsync(id, userId, notes);
        await _reports.GenerateZReportAsync(id, userId);
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AbortClose(Guid id)
    {
        if (Disabled() is { } d) return d;
        try
        {
            var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
            await _reconciliation.AbortCloseAsync(id, userId);
            return RedirectToAction(nameof(Detail), new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}
