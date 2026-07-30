using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RestBar.Infrastructure.Foundation;
using RestBar.Interfaces;

namespace RestBar.Controllers;

[Authorize(Policy = "ReportAccess")]
public class ExecutiveCommandCenterController : Controller
{
    private readonly IExecutiveCommandCenterService _cc;
    private readonly FeatureFlags _flags;

    public ExecutiveCommandCenterController(IExecutiveCommandCenterService cc, IOptions<FeatureFlags> flags)
    {
        _cc = cc;
        _flags = flags.Value;
    }

    public async Task<IActionResult> Index()
    {
        if (!_flags.EnableCommandCenter)
            return View("ModuleDisabled");

        var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);
        var branchId = Guid.Parse(User.FindFirst("BranchId")!.Value);
        var userId = Guid.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : (Guid?)null;
        var snap = await _cc.GetSnapshotAsync(companyId, branchId, userId);
        return View(snap);
    }

    [HttpGet]
    public async Task<IActionResult> Snapshot()
    {
        if (!_flags.EnableCommandCenter)
            return NotFound(new { message = "Command Center disabled" });

        var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);
        var branchId = Guid.Parse(User.FindFirst("BranchId")!.Value);
        var userId = Guid.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : (Guid?)null;
        return Json(await _cc.GetSnapshotAsync(companyId, branchId, userId));
    }
}
