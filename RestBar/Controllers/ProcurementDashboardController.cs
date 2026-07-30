using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RestBar.Infrastructure.Foundation;
using RestBar.Interfaces;

namespace RestBar.Controllers;

[Authorize(Policy = "PurchasingAccess")]
public class ProcurementDashboardController : Controller
{
    private readonly IProcurementDashboardService _dashboard;
    private readonly IProcurementReportService _reports;
    private readonly IProcurementCostEngine _cost;
    private readonly FeatureFlags _flags;

    public ProcurementDashboardController(
        IProcurementDashboardService dashboard,
        IProcurementReportService reports,
        IProcurementCostEngine cost,
        IOptions<FeatureFlags> flags)
    {
        _dashboard = dashboard;
        _reports = reports;
        _cost = cost;
        _flags = flags.Value;
    }

    public async Task<IActionResult> Index()
    {
        if (!_flags.EnablePurchasingModule)
            return View("~/Views/Supplier/ModuleDisabled.cshtml");

        var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);
        var branchId = Guid.Parse(User.FindFirst("BranchId")!.Value);
        ViewBag.Snapshot = await _dashboard.GetCommandCenterAsync(companyId, branchId);
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> SupplierAnalysis()
    {
        if (!_flags.EnablePurchasingModule)
            return Json(new { success = false, message = "disabled" });
        var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);
        return Json(await _reports.GetSupplierAnalysisAsync(companyId));
    }

    [HttpGet]
    [Authorize(Policy = "CostingAccess")]
    public async Task<IActionResult> TheoreticalCost(Guid productId)
    {
        if (!_flags.EnablePurchasingModule)
            return Json(new { success = false, message = "disabled" });
        var cost = await _cost.GetTheoreticalFoodCostAsync(productId);
        return Json(new { productId, theoreticalCost = cost });
    }
}
