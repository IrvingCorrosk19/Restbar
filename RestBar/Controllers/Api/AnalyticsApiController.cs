using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestBar.Domain.Analytics;
using RestBar.Interfaces;

namespace RestBar.Controllers.Api;

[ApiController]
[Route("api/analytics")]
[Authorize(Policy = "AnalyticsView")]
public class AnalyticsApiController : ControllerBase
{
    private readonly IAnalyticsScopeService _scope;
    private readonly IAnalyticsQueryService _query;
    private readonly IAnalyticsExportService _export;

    public AnalyticsApiController(IAnalyticsScopeService scope, IAnalyticsQueryService query, IAnalyticsExportService export)
    {
        _scope = scope;
        _query = query;
        _export = export;
    }

    [HttpGet("executive-summary")]
    public Task<IActionResult> ExecutiveSummary([FromQuery] AnalyticsFilterRequest req, CancellationToken ct)
        => Data("executive-summary", req, ct);

    [HttpGet("sales/trend")]
    public Task<IActionResult> SalesTrend([FromQuery] AnalyticsFilterRequest req, CancellationToken ct)
        => Data("sales-trend", req, ct);

    [HttpGet("sales/products")]
    public Task<IActionResult> SalesProducts([FromQuery] AnalyticsFilterRequest req, CancellationToken ct)
        => Data("sales-product", req, ct);

    [HttpGet("profitability/products")]
    public Task<IActionResult> ProfitProducts([FromQuery] AnalyticsFilterRequest req, CancellationToken ct)
        => Data("profitability-product", req, ct);

    [HttpGet("inventory/health")]
    public Task<IActionResult> InventoryHealth([FromQuery] AnalyticsFilterRequest req, CancellationToken ct)
        => Data("inventory-health", req, ct);

    [HttpGet("purchases/suppliers")]
    public Task<IActionResult> PurchasesSuppliers([FromQuery] AnalyticsFilterRequest req, CancellationToken ct)
        => Data("purchases-supplier", req, ct);

    [HttpGet("cash/summary")]
    public Task<IActionResult> CashSummary([FromQuery] AnalyticsFilterRequest req, CancellationToken ct)
        => Data("cash-summary", req, ct);

    [HttpGet("operations/kitchen")]
    public Task<IActionResult> Kitchen([FromQuery] AnalyticsFilterRequest req, CancellationToken ct)
        => Data("kitchen", req, ct);

    [HttpGet("live")]
    public async Task<IActionResult> Live([FromQuery] AnalyticsFilterRequest req, CancellationToken ct)
    {
        try
        {
            var filter = _scope.Resolve(User, req ?? new(), User.IsInRole("admin"));
            return Ok(await _query.GetLiveAsync(filter, ct));
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("decisions")]
    public async Task<IActionResult> Decisions([FromQuery] AnalyticsFilterRequest req, CancellationToken ct)
    {
        try
        {
            req ??= new();
            req.Compare = true;
            var filter = _scope.Resolve(User, req, User.IsInRole("admin"));
            return Ok(await _query.GetDecisionsAsync(filter, ct));
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("kpis")]
    public IActionResult Kpis() => Ok(KpiCatalog.All);

    [HttpGet("reports")]
    public IActionResult Reports() => Ok(AnalyticsReportCatalog.All);

    [HttpPost("export")]
    [Authorize(Policy = "AnalyticsExport")]
    public async Task<IActionResult> Export([FromBody] AnalyticsExportRequest body, CancellationToken ct)
    {
        if (body is null || string.IsNullOrWhiteSpace(body.ReportKey))
            return BadRequest(new { message = "reportKey required" });
        try
        {
            var filter = _scope.Resolve(User, body.Filter ?? new(), User.IsInRole("admin"));
            var userName = User.Identity?.Name ?? "user";
            var (bytes, contentType, fileName) = await _export.ExportAsync(body.ReportKey, filter, body.Format ?? "csv", userName, ct);
            return File(bytes, contentType, fileName);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    private async Task<IActionResult> Data(string key, AnalyticsFilterRequest? req, CancellationToken ct)
    {
        try
        {
            var filter = _scope.Resolve(User, req ?? new(), User.IsInRole("admin"));
            var data = await _query.GetReportDataAsync(key, filter, ct);
            return Ok(new { key, filter, data, generatedAtUtc = DateTime.UtcNow });
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }
}

public sealed class AnalyticsExportRequest
{
    public string ReportKey { get; set; } = "";
    public string? Format { get; set; } = "csv";
    public AnalyticsFilterRequest? Filter { get; set; }
}
