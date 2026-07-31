using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RestBar.Domain.Analytics;
using RestBar.Domain.DecisionIntelligence;
using RestBar.Infrastructure.Foundation;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Controllers.Api;

[ApiController]
[Route("api/decision-intelligence")]
[Authorize(Policy = "AnalyticsView")]
public class DecisionIntelligenceApiController : ControllerBase
{
    private readonly IDecisionIntelligenceService _di;
    private readonly IAnalyticsScopeService _scope;
    private readonly FeatureFlags _flags;

    public DecisionIntelligenceApiController(IDecisionIntelligenceService di, IAnalyticsScopeService scope, IOptions<FeatureFlags> flags)
    {
        _di = di;
        _scope = scope;
        _flags = flags.Value;
    }

    IActionResult Disabled() => StatusCode(503, new { message = "Decision Intelligence module disabled (FeatureFlags:EnableDecisionIntelligence)." });

    [HttpGet("executive")]
    public async Task<IActionResult> Executive([FromQuery] AnalyticsFilterRequest req, CancellationToken ct)
    {
        if (!_flags.EnableDecisionIntelligence) return Disabled();
        try
        {
            var filter = _scope.Resolve(User, req ?? new(), User.IsInRole("admin"));
            var userId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;
            return Ok(await _di.GetCockpitAsync(filter, userId, ct));
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (Exception ex) { return StatusCode(503, new { message = "Decision Intelligence temporarily unavailable.", detail = ex.Message }); }
    }

    [HttpGet("forecast")]
    public async Task<IActionResult> Forecast([FromQuery] AnalyticsFilterRequest req, [FromQuery] int horizon = 7, CancellationToken ct = default)
    {
        if (!_flags.EnableDecisionIntelligence) return Disabled();
        try
        {
            var filter = _scope.Resolve(User, req ?? new(), User.IsInRole("admin"));
            var userId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;
            return Ok(await _di.GetSalesForecastAsync(filter, horizon, userId, persistRun: true, ct));
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception ex) { return StatusCode(503, new { message = "Forecast unavailable.", detail = ex.Message }); }
    }

    [HttpGet("recommendations")]
    public async Task<IActionResult> Recommendations([FromQuery] AnalyticsFilterRequest req, CancellationToken ct)
    {
        if (!_flags.EnableDecisionIntelligence) return Disabled();
        try
        {
            var filter = _scope.Resolve(User, req ?? new(), User.IsInRole("admin"));
            return Ok(await _di.GetRecommendationsAsync(filter, ct));
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception ex) { return StatusCode(503, new { message = "Recommendations unavailable.", detail = ex.Message }); }
    }

    [HttpGet("data-quality")]
    public IActionResult DataQuality()
    {
        if (!_flags.EnableDecisionIntelligence) return Disabled();
        return Ok(_di.GetDataQualityBanner());
    }

    [HttpPost("simulations/sales")]
    public async Task<IActionResult> SimulateSales([FromQuery] AnalyticsFilterRequest req, [FromBody] SalesSimRequest body, CancellationToken ct)
    {
        if (!_flags.EnableDecisionIntelligence) return Disabled();
        try
        {
            var filter = _scope.Resolve(User, req ?? new(), User.IsInRole("admin"));
            return Ok(await _di.SimulateSalesDeltaAsync(filter, body?.PctChange ?? 0, ct));
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost("recommendations/accept")]
    [Authorize(Policy = "AnalyticsExport")]
    public async Task<IActionResult> Accept([FromQuery] AnalyticsFilterRequest req, [FromBody] DiRecommendationDto body, CancellationToken ct)
    {
        if (!_flags.EnableDecisionIntelligence) return Disabled();
        if (body == null) return BadRequest(new { message = "Body required" });
        try
        {
            var filter = _scope.Resolve(User, req ?? new(), User.IsInRole("admin"));
            var userId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;
            var saved = await _di.AcceptRecommendationAsync(filter.CompanyId, filter.BranchId, userId, body, null, ct);
            return Ok(saved);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpGet("decisions")]
    public async Task<IActionResult> Decisions([FromQuery] AnalyticsFilterRequest req, CancellationToken ct)
    {
        if (!_flags.EnableDecisionIntelligence) return Disabled();
        try
        {
            var filter = _scope.Resolve(User, req ?? new(), User.IsInRole("admin"));
            return Ok(await _di.ListDecisionsAsync(filter.CompanyId, filter.BranchId, ct));
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    public sealed class SalesSimRequest
    {
        public decimal PctChange { get; set; }
    }
}
