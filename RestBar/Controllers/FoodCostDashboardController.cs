using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RestBar.Infrastructure.Foundation;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Controllers;

[Authorize(Policy = "CostingAccess")]
public class FoodCostDashboardController : Controller
{
    private readonly IFoodCostDashboardService _dashboard;
    private readonly IFoodCostEngine _engine;
    private readonly IMenuEngineeringService _menu;
    private readonly ICostSimulationService _sim;
    private readonly IWasteService _waste;
    private readonly FeatureFlags _flags;

    public FoodCostDashboardController(
        IFoodCostDashboardService dashboard,
        IFoodCostEngine engine,
        IMenuEngineeringService menu,
        ICostSimulationService sim,
        IWasteService waste,
        IOptions<FeatureFlags> flags)
    {
        _dashboard = dashboard;
        _engine = engine;
        _menu = menu;
        _sim = sim;
        _waste = waste;
        _flags = flags.Value;
    }

    public async Task<IActionResult> Index()
    {
        if (!_flags.EnableFoodCostModule)
            return View("ModuleDisabled");
        var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);
        var branchId = Guid.Parse(User.FindFirst("BranchId")!.Value);
        ViewBag.Snapshot = await _dashboard.GetCommandCenterAsync(companyId, branchId);
        return View();
    }

    public async Task<IActionResult> MenuEngineering()
    {
        if (!_flags.EnableFoodCostModule)
            return View("ModuleDisabled");
        var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);
        var branchId = Guid.Parse(User.FindFirst("BranchId")!.Value);
        var to = DateTime.UtcNow;
        var items = await _menu.AnalyzeAsync(companyId, branchId, to.AddDays(-30), to);
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> PlateCost(Guid productId)
    {
        if (!_flags.EnableFoodCostModule)
            return NotFound(new { message = "Food Cost module disabled" });
        var cost = await _engine.GetPlateCostAsync(productId);
        return Json(cost);
    }

    [HttpPost]
    public async Task<IActionResult> Snapshot(DateTime? from, DateTime? to)
    {
        if (!_flags.EnableFoodCostModule)
            return NotFound(new { message = "Food Cost module disabled" });
        var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);
        var branchId = Guid.Parse(User.FindFirst("BranchId")!.Value);
        var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
        var end = to ?? DateTime.UtcNow;
        var start = from ?? end.AddDays(-7);
        var snap = await _engine.GenerateSnapshotAsync(companyId, branchId, start, end, userId);
        return Json(snap);
    }

    [HttpPost]
    public IActionResult Simulate([FromBody] CostSimulationRequest request)
    {
        if (!_flags.EnableFoodCostModule)
            return NotFound(new { message = "Food Cost module disabled" });
        return Json(_sim.Simulate(request));
    }

    [HttpPost]
    public async Task<IActionResult> RecordWaste([FromBody] WasteDto dto)
    {
        if (!_flags.EnableFoodCostModule)
            return NotFound(new { message = "Food Cost module disabled" });
        var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);
        var branchId = Guid.Parse(User.FindFirst("BranchId")!.Value);
        var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
        var evt = await _waste.RecordWasteAsync(new WasteRequest(
            companyId, branchId, dto.ProductId, dto.Quantity, userId, dto.ReasonCode, dto.StationId, dto.Notes));
        return Json(new { success = true, id = evt.Id, totalCost = evt.TotalCost });
    }

    public record WasteDto(Guid ProductId, decimal Quantity, WasteReasonCode ReasonCode, Guid? StationId, string? Notes);
}
