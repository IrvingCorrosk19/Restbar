using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestBar.Domain.Analytics;
using RestBar.Interfaces;

namespace RestBar.Controllers;

[Authorize(Policy = "AnalyticsView")]
public class ExecutiveAnalyticsController : Controller
{
    private readonly IAnalyticsScopeService _scope;
    private readonly IAnalyticsQueryService _query;
    private readonly IAnalyticsExportService _export;

    public ExecutiveAnalyticsController(
        IAnalyticsScopeService scope,
        IAnalyticsQueryService query,
        IAnalyticsExportService export)
    {
        _scope = scope;
        _query = query;
        _export = export;
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewBag.Reports = AnalyticsReportCatalog.All
            .Where(r => r.Availability is KpiAvailability.Available or KpiAvailability.AvailableWithLimitations)
            .GroupBy(r => r.Category)
            .ToList();
        ViewBag.Kpis = KpiCatalog.All;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Live([FromQuery] AnalyticsFilterRequest req, CancellationToken ct)
    {
        try
        {
            var filter = _scope.Resolve(User, req ?? new(), User.IsInRole("admin"));
            return Json(await _query.GetLiveAsync(filter, ct));
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet]
    public async Task<IActionResult> Decisions([FromQuery] AnalyticsFilterRequest req, CancellationToken ct)
    {
        try
        {
            req ??= new();
            req.Compare = true;
            var filter = _scope.Resolve(User, req, User.IsInRole("admin"));
            return Json(await _query.GetDecisionsAsync(filter, ct));
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet]
    public IActionResult Report(string key, [FromQuery] AnalyticsFilterRequest req)
    {
        var def = AnalyticsReportCatalog.Get(key);
        if (def is null) return NotFound();
        if (!CanAccess(def)) return Forbid();

        ViewBag.Report = def;
        ViewBag.Filter = req ?? new AnalyticsFilterRequest { Period = "last_30" };
        return View("Report");
    }

    [HttpGet]
    public async Task<IActionResult> ReportData(string key, [FromQuery] AnalyticsFilterRequest req, CancellationToken ct)
    {
        var def = AnalyticsReportCatalog.Get(key);
        if (def is null) return NotFound();
        if (!CanAccess(def)) return Forbid();
        try
        {
            var cross = User.IsInRole("admin") || User.HasClaim("perm", "Analytics.CrossBranch");
            var filter = _scope.Resolve(User, req ?? new(), cross);
            var data = await _query.GetReportDataAsync(key, filter, ct);
            return Json(new { report = def, filter, data, generatedAtUtc = DateTime.UtcNow, user = User.Identity?.Name });
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (Exception ex) when (ex is KeyNotFoundException) { return NotFound(new { message = ex.Message }); }
    }

    [HttpGet]
    [Authorize(Policy = "AnalyticsExport")]
    public async Task<IActionResult> Export(string key, string format, [FromQuery] AnalyticsFilterRequest req, CancellationToken ct)
    {
        var def = AnalyticsReportCatalog.Get(key);
        if (def is null) return NotFound();
        if (!CanAccess(def)) return Forbid();
        try
        {
            var cross = User.IsInRole("admin");
            var filter = _scope.Resolve(User, req ?? new(), cross);
            var userName = User.Identity?.Name ?? User.FindFirst("UserId")?.Value ?? "user";
            var (bytes, contentType, fileName) = await _export.ExportAsync(key, filter, format, userName, ct);
            return File(bytes, contentType, fileName);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet]
    public IActionResult Print(string key) => RedirectToAction(nameof(Report), new { key });

    private bool CanAccess(AnalyticsReportDefinition def)
    {
        if (User.IsInRole("admin") || User.IsInRole("accountant") || User.IsInRole("manager")) return true;
        return false;
    }
}
