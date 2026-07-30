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

    public async Task<IActionResult> Dashboard()
    {
        if (Disabled() is { } d) return d;
        var branchId = Guid.Parse(User.FindFirst("BranchId")!.Value);
        ViewBag.Snapshot = await _reports.GetDashboardSnapshotAsync(branchId);
        return View();
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
        var shift = await _context.Shifts.AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);

        var session = await _sessions.OpenSessionAsync(registerId, userId, openingFloat, shift?.Id);
        return RedirectToAction(nameof(Detail), new { id = session.Id });
    }

    public async Task<IActionResult> Detail(Guid id)
    {
        if (Disabled() is { } d) return d;
        var session = await _sessions.GetByIdAsync(id);
        if (session == null)
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
        var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
        await _sessions.StartCloseAsync(id, userId);
        return RedirectToAction(nameof(Arqueo), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Arqueo(Guid id)
    {
        if (Disabled() is { } d) return d;
        var session = await _sessions.GetByIdAsync(id);
        if (session == null)
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
        if (session == null)
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
        var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
        await _reconciliation.AbortCloseAsync(id, userId);
        return RedirectToAction(nameof(Detail), new { id });
    }
}
