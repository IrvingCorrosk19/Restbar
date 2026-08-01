using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RestBar.Infrastructure.Foundation;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Controllers;

[Authorize(Policy = "CashAccess")]
public class CashReportController : Controller
{
    private readonly ICashReportService _reports;
    private readonly ICashIntegrityService _integrity;
    private readonly RestBarContext _context;
    private readonly FeatureFlags _flags;

    public CashReportController(
        ICashReportService reports,
        ICashIntegrityService integrity,
        RestBarContext context,
        IOptions<FeatureFlags> flags)
    {
        _reports = reports;
        _integrity = integrity;
        _context = context;
        _flags = flags.Value;
    }

    public async Task<IActionResult> ZReport(Guid sessionId)
    {
        if (!_flags.EnableCashModule)
            return View("ModuleDisabled");

        if (sessionId == Guid.Empty)
        {
            ViewBag.Message = "Indique una sesión de caja válida para generar el Z Report (Cierre).";
            return View("ReportMissing");
        }

        var session = await _context.CashSessions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sessionId);
        if (session == null || !UserCanAccessCashSession(session))
        {
            ViewBag.Message = "Sesión de caja no encontrada o no autorizada para este Z Report.";
            return View("ReportMissing");
        }

        var report = await _context.CashZReports.AsNoTracking()
            .FirstOrDefaultAsync(z => z.CashSessionId == sessionId);

        if (report == null)
        {
            var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
            report = await _reports.GenerateZReportAsync(sessionId, userId);
        }

        ViewBag.ReportJson = report.ReportJson;
        return View(report);
    }

    public async Task<IActionResult> XReport(Guid sessionId)
    {
        if (!_flags.EnableCashModule)
            return View("ModuleDisabled");

        if (sessionId == Guid.Empty)
        {
            ViewBag.Message = "Indique una sesión de caja válida para el X Report.";
            return View("ReportMissing");
        }

        var session = await _context.CashSessions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sessionId);
        if (session == null || !UserCanAccessCashSession(session))
        {
            ViewBag.Message = "Sesión de caja no encontrada o no autorizada para este X Report.";
            return View("ReportMissing");
        }

        ViewBag.Report = await _reports.GenerateXReportAsync(sessionId);
        return View();
    }

    [HttpGet("api/CashReport/verify/{sessionId:guid}")]
    public async Task<IActionResult> VerifyChain(Guid sessionId)
    {
        if (!_flags.EnableCashModule)
            return NotFound(new { message = "Cash module disabled" });

        var session = await _context.CashSessions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sessionId);
        if (session == null || !UserCanAccessCashSession(session))
            return NotFound(new { message = "Session not found" });

        var ok = await _integrity.VerifyMovementChainAsync(sessionId);
        return Ok(new { sessionId, integrityOk = ok });
    }

    private bool UserCanAccessCashSession(CashSession session)
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                   ?? User.FindFirst("UserRole")?.Value;
        if (string.Equals(role, "superadmin", StringComparison.OrdinalIgnoreCase))
            return true;

        var companyOk = Guid.TryParse(User.FindFirst("CompanyId")?.Value, out var companyId)
                        && session.CompanyId == companyId;
        if (!companyOk) return false;

        if (string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase)
            && !Guid.TryParse(User.FindFirst("BranchId")?.Value, out _))
            return true;

        if (Guid.TryParse(User.FindFirst("BranchId")?.Value, out var branchId))
            return session.BranchId == branchId;

        return false;
    }
}
