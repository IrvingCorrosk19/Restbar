using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestBar.Interfaces;

namespace RestBar.Controllers;

/// <summary>
/// Native BI API — PostgreSQL analytical functions (RB-025).
/// Tenant scope always taken from authenticated claims (no cross-tenant branch override).
/// </summary>
[Authorize(Policy = "ReportAccess")]
public class BiNativeController : Controller
{
    private readonly IBiNativeAnalyticsService _bi;

    public BiNativeController(IBiNativeAnalyticsService bi) => _bi = bi;

    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public async Task<IActionResult> Executive(DateTime? start, DateTime? end, CancellationToken ct)
    {
        var (companyId, branchId) = Tenant();
        var (from, to) = Range(start, end);
        return Json(await _bi.GetExecutiveDashboardAsync(companyId, branchId, from, to, ct));
    }

    [HttpGet]
    public async Task<IActionResult> SalesSummary(DateTime? start, DateTime? end, CancellationToken ct)
    {
        var (companyId, branchId) = Tenant();
        var (from, to) = Range(start, end);
        return Json(await _bi.GetSalesSummaryAsync(companyId, branchId, from, to, ct));
    }

    [HttpGet]
    public async Task<IActionResult> HourlySales(DateTime? start, DateTime? end, CancellationToken ct)
    {
        var (companyId, branchId) = Tenant();
        var (from, to) = Range(start, end);
        return Json(await _bi.GetHourlySalesAsync(companyId, branchId, from, to, ct));
    }

    [HttpGet]
    public async Task<IActionResult> TopProducts(DateTime? start, DateTime? end, int limit = 20, CancellationToken ct = default)
    {
        var (companyId, branchId) = Tenant();
        var (from, to) = Range(start, end);
        return Json(await _bi.GetTopProductsAsync(companyId, branchId, from, to, limit, ct));
    }

    [HttpGet]
    public async Task<IActionResult> WaiterPerformance(DateTime? start, DateTime? end, CancellationToken ct)
    {
        var (companyId, branchId) = Tenant();
        var (from, to) = Range(start, end);
        return Json(await _bi.GetWaiterPerformanceAsync(companyId, branchId, from, to, ct));
    }

    [HttpGet]
    public async Task<IActionResult> StationPerformance(DateTime? start, DateTime? end, CancellationToken ct)
    {
        var (companyId, branchId) = Tenant();
        var (from, to) = Range(start, end);
        return Json(await _bi.GetStationPerformanceAsync(companyId, branchId, from, to, ct));
    }

    [HttpGet]
    public async Task<IActionResult> CashSummary(DateTime? start, DateTime? end, CancellationToken ct)
    {
        var (companyId, branchId) = Tenant();
        var (from, to) = Range(start, end);
        return Json(await _bi.GetCashSummaryAsync(companyId, branchId, from, to, ct));
    }

    [HttpGet]
    public async Task<IActionResult> InventoryHealth(CancellationToken ct)
    {
        var (companyId, branchId) = Tenant();
        return Json(await _bi.GetInventoryHealthAsync(companyId, branchId, ct));
    }

    [HttpGet]
    public async Task<IActionResult> FoodCostSummary(DateTime? start, DateTime? end, CancellationToken ct)
    {
        var (companyId, branchId) = Tenant();
        var (from, to) = Range(start, end);
        return Json(await _bi.GetFoodCostSummaryAsync(companyId, branchId, from, to, ct));
    }

    [HttpGet]
    public async Task<IActionResult> TopWaste(DateTime? start, DateTime? end, int limit = 20, CancellationToken ct = default)
    {
        var (companyId, branchId) = Tenant();
        var (from, to) = Range(start, end);
        return Json(await _bi.GetTopWasteAsync(companyId, branchId, from, to, limit, ct));
    }

    [HttpGet]
    public async Task<IActionResult> PurchaseAnalysis(DateTime? start, DateTime? end, CancellationToken ct)
    {
        var (companyId, branchId) = Tenant();
        var (from, to) = Range(start, end);
        return Json(await _bi.GetPurchaseAnalysisAsync(companyId, branchId, from, to, ct));
    }

    [HttpGet]
    public async Task<IActionResult> SupplierAnalysis(DateTime? start, DateTime? end, CancellationToken ct)
    {
        var (companyId, branchId) = Tenant();
        var (from, to) = Range(start, end);
        return Json(await _bi.GetSupplierAnalysisAsync(companyId, branchId, from, to, ct));
    }

    [HttpGet]
    public async Task<IActionResult> Profitability(DateTime? start, DateTime? end, CancellationToken ct)
    {
        var (companyId, branchId) = Tenant();
        var (from, to) = Range(start, end);
        return Json(await _bi.GetProfitabilityAsync(companyId, branchId, from, to, ct));
    }

    [HttpGet]
    public async Task<IActionResult> BranchComparison(DateTime? start, DateTime? end, CancellationToken ct)
    {
        var (companyId, _) = Tenant();
        var (from, to) = Range(start, end);
        return Json(await _bi.GetBranchComparisonAsync(companyId, from, to, ct));
    }

    private (Guid companyId, Guid branchId) Tenant()
    {
        var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);
        var branchId = Guid.Parse(User.FindFirst("BranchId")!.Value);
        return (companyId, branchId);
    }

    private static (DateTime from, DateTime to) Range(DateTime? start, DateTime? end)
    {
        var to = (end ?? DateTime.UtcNow.Date.AddDays(1)).ToUniversalTime();
        var from = (start ?? to.AddDays(-30)).ToUniversalTime();
        if (from > to) (from, to) = (to.AddDays(-30), to);
        return (from, to);
    }
}
