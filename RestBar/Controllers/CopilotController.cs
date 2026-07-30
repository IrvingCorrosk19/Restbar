using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RestBar.Infrastructure.Foundation;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Controllers;

[Authorize(Policy = "ReportAccess")]
public class CopilotController : Controller
{
    private readonly ICopilotOrchestrator _orchestrator;
    private readonly FeatureFlags _flags;
    private readonly RestBarContext _db;

    public CopilotController(ICopilotOrchestrator orchestrator, IOptions<FeatureFlags> flags, RestBarContext db)
    {
        _orchestrator = orchestrator;
        _flags = flags.Value;
        _db = db;
    }

    public IActionResult Index()
    {
        if (!_flags.EnableCopilot)
            return View("ModuleDisabled");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Ask([FromBody] CopilotAskRequest request, CancellationToken ct)
    {
        if (!_flags.EnableCopilot)
            return NotFound(new { message = "Copilot disabled" });

        var ctx = BuildContext();
        var response = await _orchestrator.AskAsync(ctx, request, ct);
        return Json(response);
    }

    [HttpGet]
    public async Task<IActionResult> History(Guid id, CancellationToken ct)
    {
        if (!_flags.EnableCopilot)
            return NotFound();

        var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);
        var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
        var messages = await _db.CopilotMessages.AsNoTracking()
            .Where(m => m.ConversationId == id && m.Conversation!.CompanyId == companyId && m.Conversation.UserId == userId)
            .OrderBy(m => m.CreatedAtUtc)
            .Select(m => new { m.Role, m.Intent, m.Content, m.CreatedAtUtc, m.DurationMs })
            .ToListAsync(ct);
        return Json(messages);
    }

    private CopilotRuntimeContext BuildContext()
    {
        var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);
        var branchId = Guid.Parse(User.FindFirst("BranchId")!.Value);
        var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
        var role = User.FindFirst("UserRole")?.Value
                   ?? User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                   ?? "guest";
        return new CopilotRuntimeContext(companyId, branchId, userId, role, "es", null);
    }
}
