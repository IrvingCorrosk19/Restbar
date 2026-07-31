using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RestBar.Domain.Analytics;
using RestBar.Domain.DecisionIntelligence;
using RestBar.Infrastructure.Foundation;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Controllers;

[Authorize(Policy = "AnalyticsView")]
public class DecisionIntelligenceController : Controller
{
    private readonly IDecisionIntelligenceService _di;
    private readonly IAnalyticsScopeService _scope;
    private readonly FeatureFlags _flags;

    public DecisionIntelligenceController(
        IDecisionIntelligenceService di,
        IAnalyticsScopeService scope,
        IOptions<FeatureFlags> flags)
    {
        _di = di;
        _scope = scope;
        _flags = flags.Value;
    }

    private bool ModuleEnabled => _flags.EnableDecisionIntelligence;

    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(Cockpit));

    [HttpGet]
    public async Task<IActionResult> Cockpit([FromQuery] AnalyticsFilterRequest? req, CancellationToken ct)
    {
        if (!ModuleEnabled) return View("ModuleDisabled");
        try
        {
            var filter = _scope.Resolve(User, req ?? new(), User.IsInRole("admin"));
            var userId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;
            var model = await _di.GetCockpitAsync(filter, userId, ct);
            ViewBag.Filter = filter;
            return View(model);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpGet]
    public async Task<IActionResult> Forecast([FromQuery] AnalyticsFilterRequest? req, [FromQuery] int horizon = 7, CancellationToken ct = default)
    {
        if (!ModuleEnabled) return View("ModuleDisabled");
        try
        {
            var filter = _scope.Resolve(User, req ?? new(), User.IsInRole("admin"));
            var userId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;
            var model = await _di.GetSalesForecastAsync(filter, horizon, userId, persistRun: true, ct);
            ViewBag.Filter = filter;
            return View(model);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpGet]
    public async Task<IActionResult> Recommendations([FromQuery] AnalyticsFilterRequest? req, CancellationToken ct)
    {
        if (!ModuleEnabled) return View("ModuleDisabled");
        try
        {
            var filter = _scope.Resolve(User, req ?? new(), User.IsInRole("admin"));
            var model = await _di.GetRecommendationsAsync(filter, ct);
            ViewBag.Filter = filter;
            return View(model);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpGet]
    public async Task<IActionResult> Decisions([FromQuery] AnalyticsFilterRequest? req, CancellationToken ct)
    {
        if (!ModuleEnabled) return View("ModuleDisabled");
        try
        {
            var filter = _scope.Resolve(User, req ?? new(), User.IsInRole("admin"));
            var model = await _di.ListDecisionsAsync(filter.CompanyId, filter.BranchId, ct);
            return View(model);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpGet]
    public IActionResult DataQuality()
    {
        if (!ModuleEnabled) return View("ModuleDisabled");
        return View(_di.GetDataQualityBanner());
    }

    [HttpGet]
    public IActionResult Simulations()
    {
        if (!ModuleEnabled) return View("ModuleDisabled");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RunSalesSimulation([FromQuery] AnalyticsFilterRequest? req, [FromForm] decimal pctChange, CancellationToken ct)
    {
        if (!ModuleEnabled) return View("ModuleDisabled");
        try
        {
            var filter = _scope.Resolve(User, req ?? new(), User.IsInRole("admin"));
            var result = await _di.SimulateSalesDeltaAsync(filter, pctChange, ct);
            return View("Simulations", result);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost]
    [Authorize(Policy = "AnalyticsExport")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptRecommendation([FromForm] string code, [FromForm] string category,
        [FromForm] string observation, [FromForm] string evidence, [FromForm] string action,
        [FromForm] string expectedImpact, [FromForm] string confidence, [FromForm] string severity,
        [FromForm] string? comment, [FromQuery] AnalyticsFilterRequest? req, CancellationToken ct)
    {
        if (!ModuleEnabled) return Forbid();
        var filter = _scope.Resolve(User, req ?? new(), User.IsInRole("admin"));
        var userId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;
        var dto = RecommendationComposer.Build(code, category, observation, evidence, action, expectedImpact, confidence, "manager", severity);
        await _di.AcceptRecommendationAsync(filter.CompanyId, filter.BranchId, userId, dto, comment, ct);
        return RedirectToAction(nameof(Decisions));
    }
}
