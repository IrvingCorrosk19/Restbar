using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RestBar.Infrastructure.Foundation;
using RestBar.Services.BusinessRules;

namespace RestBar.Controllers.Api;

[ApiController]
[Route("api/business-rules")]
[Authorize(Policy = "AnalyticsView")]
public class BusinessRulesApiController : ControllerBase
{
    private readonly IBusinessRulesEngine _engine;
    private readonly FeatureFlags _flags;

    public BusinessRulesApiController(IBusinessRulesEngine engine, IOptions<FeatureFlags> flags)
    {
        _engine = engine;
        _flags = flags.Value;
    }

    Guid? CompanyId => Guid.TryParse(User.FindFirstValue("CompanyId"), out var id) ? id : null;
    Guid UserId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;
    Guid? BranchId => Guid.TryParse(User.FindFirstValue("BranchId"), out var id) ? id : null;

    IActionResult Disabled() => StatusCode(503, new { message = "Business Rules module disabled" });

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        if (!_flags.EnableBusinessRules) return Disabled();
        if (CompanyId is null) return Forbid();
        return Ok(await _engine.ListAsync(CompanyId.Value, BranchId, ct));
    }

    [HttpGet("templates")]
    public IActionResult Templates()
    {
        if (!_flags.EnableBusinessRules) return Disabled();
        return Ok(_engine.GetTemplates());
    }

    [HttpPost("from-template/{code}")]
    [Authorize(Policy = "AnalyticsExport")]
    public async Task<IActionResult> FromTemplate(string code, CancellationToken ct)
    {
        if (!_flags.EnableBusinessRules) return Disabled();
        if (CompanyId is null) return Forbid();
        var rule = await _engine.CreateFromTemplateAsync(CompanyId.Value, BranchId, UserId, code, ct);
        return Ok(rule);
    }

    [HttpPost("{id:guid}/draft")]
    [Authorize(Policy = "AnalyticsExport")]
    public async Task<IActionResult> Draft(Guid id, [FromBody] DraftRequest body, CancellationToken ct)
    {
        if (!_flags.EnableBusinessRules) return Disabled();
        if (CompanyId is null) return Forbid();
        try
        {
            var v = await _engine.SaveDraftAsync(CompanyId.Value, id, UserId, body.FlowJson ?? "{}", body.Notes, ct);
            return Ok(v);
        }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{id:guid}/publish")]
    [Authorize(Policy = "AnalyticsExport")]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        if (!_flags.EnableBusinessRules) return Disabled();
        if (CompanyId is null) return Forbid();
        try { return Ok(await _engine.PublishAsync(CompanyId.Value, id, UserId, ct)); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{id:guid}/simulate")]
    public async Task<IActionResult> Simulate(Guid id, [FromBody] Dictionary<string, object?>? facts, CancellationToken ct)
    {
        if (!_flags.EnableBusinessRules) return Disabled();
        if (CompanyId is null) return Forbid();
        facts ??= _engine.BuildOperationalFacts(CompanyId.Value, BranchId);
        return Ok(await _engine.SimulateAsync(CompanyId.Value, id, facts, UserId, ct));
    }

    [HttpPost("evaluate")]
    [Authorize(Policy = "AnalyticsExport")]
    public async Task<IActionResult> Evaluate([FromQuery] bool live = false, CancellationToken ct = default)
    {
        if (!_flags.EnableBusinessRules) return Disabled();
        if (CompanyId is null) return Forbid();
        var facts = _engine.BuildOperationalFacts(CompanyId.Value, BranchId);
        return Ok(await _engine.EvaluatePublishedAsync(CompanyId.Value, BranchId, facts, UserId, live, ct));
    }

    [HttpGet("executions")]
    public async Task<IActionResult> Executions([FromQuery] Guid? ruleId, CancellationToken ct)
    {
        if (!_flags.EnableBusinessRules) return Disabled();
        if (CompanyId is null) return Forbid();
        return Ok(await _engine.ListExecutionsAsync(CompanyId.Value, ruleId, 100, ct));
    }

    public sealed class DraftRequest
    {
        public string? FlowJson { get; set; }
        public string? Notes { get; set; }
    }
}
