using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RestBar.Infrastructure.Foundation;
using RestBar.Services.BusinessRules;

namespace RestBar.Controllers;

[Authorize(Policy = "AnalyticsView")]
public class BusinessRulesController : Controller
{
    private readonly IBusinessRulesEngine _engine;
    private readonly FeatureFlags _flags;

    public BusinessRulesController(IBusinessRulesEngine engine, IOptions<FeatureFlags> flags)
    {
        _engine = engine;
        _flags = flags.Value;
    }

    Guid? CompanyId => Guid.TryParse(User.FindFirstValue("CompanyId"), out var id) ? id : null;
    Guid UserId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;
    Guid? BranchId => Guid.TryParse(User.FindFirstValue("BranchId"), out var id) ? id : null;

    bool Enabled => _flags.EnableBusinessRules;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (!Enabled) return View("ModuleDisabled");
        if (CompanyId is null) return Forbid();
        await _engine.EnsureTemplatesAsync(ct);
        var rules = await _engine.ListAsync(CompanyId.Value, BranchId, ct);
        ViewBag.Templates = _engine.GetTemplates();
        return View(rules);
    }

    [HttpGet]
    public IActionResult Templates()
    {
        if (!Enabled) return View("ModuleDisabled");
        return View(_engine.GetTemplates());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AnalyticsExport")]
    public async Task<IActionResult> CreateFromTemplate(string templateCode, CancellationToken ct)
    {
        if (!Enabled || CompanyId is null) return Forbid();
        var rule = await _engine.CreateFromTemplateAsync(CompanyId.Value, BranchId, UserId, templateCode, ct);
        return RedirectToAction(nameof(Edit), new { id = rule.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        if (!Enabled || CompanyId is null) return Forbid();
        var rule = await _engine.GetAsync(CompanyId.Value, id, ct);
        if (rule == null) return NotFound();
        return View(rule);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AnalyticsExport")]
    public async Task<IActionResult> SaveDraft(Guid id, string flowJson, string? notes, CancellationToken ct)
    {
        if (!Enabled || CompanyId is null) return Forbid();
        try
        {
            await _engine.SaveDraftAsync(CompanyId.Value, id, UserId, flowJson, notes, ct);
            TempData["Ok"] = "Borrador guardado (nueva versión).";
        }
        catch (Exception ex)
        {
            TempData["Err"] = ex.Message;
        }
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AnalyticsExport")]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        if (!Enabled || CompanyId is null) return Forbid();
        try
        {
            await _engine.PublishAsync(CompanyId.Value, id, UserId, ct);
            TempData["Ok"] = "Regla publicada.";
        }
        catch (Exception ex) { TempData["Err"] = ex.Message; }
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AnalyticsExport")]
    public async Task<IActionResult> Disable(Guid id, CancellationToken ct)
    {
        if (!Enabled || CompanyId is null) return Forbid();
        await _engine.DisableAsync(CompanyId.Value, id, UserId, ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Simulate(Guid id, CancellationToken ct)
    {
        if (!Enabled || CompanyId is null) return Forbid();
        var rule = await _engine.GetAsync(CompanyId.Value, id, ct);
        if (rule == null) return NotFound();
        var facts = _engine.BuildOperationalFacts(CompanyId.Value, BranchId);
        var result = await _engine.SimulateAsync(CompanyId.Value, id, facts, UserId, ct);
        ViewBag.Rule = rule;
        ViewBag.Facts = facts;
        return View(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AnalyticsExport")]
    public async Task<IActionResult> RunLive(Guid id, CancellationToken ct)
    {
        if (!Enabled || CompanyId is null) return Forbid();
        var facts = _engine.BuildOperationalFacts(CompanyId.Value, BranchId);
        var results = await _engine.EvaluatePublishedAsync(CompanyId.Value, BranchId, facts, UserId, live: true, ct);
        TempData["Ok"] = $"Ejecución live: {results.Count} regla(s) evaluadas.";
        return RedirectToAction(nameof(Executions));
    }

    [HttpGet]
    public async Task<IActionResult> Executions(Guid? ruleId, CancellationToken ct)
    {
        if (!Enabled || CompanyId is null) return Forbid();
        var list = await _engine.ListExecutionsAsync(CompanyId.Value, ruleId, 100, ct);
        return View(list);
    }
}
