using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RestBar.Infrastructure.Foundation;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Controllers;

[Authorize(Policy = "PurchasingAccess")]
public class SupplierController : Controller
{
    private readonly ISupplierService _suppliers;
    private readonly FeatureFlags _flags;

    public SupplierController(ISupplierService suppliers, IOptions<FeatureFlags> flags)
    {
        _suppliers = suppliers;
        _flags = flags.Value;
    }

    public async Task<IActionResult> Index()
    {
        if (!_flags.EnablePurchasingModule)
            return View("ModuleDisabled");
        var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);
        return View(await _suppliers.ListByCompanyAsync(companyId));
    }

    [HttpGet]
    public async Task<IActionResult> GetSuppliers()
    {
        if (!_flags.EnablePurchasingModule)
            return Json(new { success = false, message = "Purchasing module disabled" });
        var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);
        var list = await _suppliers.ListByCompanyAsync(companyId);
        return Json(new { success = true, data = list });
    }

    [HttpPost]
    public async Task<IActionResult> CreateSupplier([FromBody] Supplier dto)
    {
        if (!_flags.EnablePurchasingModule)
            return Json(new { success = false, message = "Purchasing module disabled" });
        dto.CompanyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);
        var created = await _suppliers.CreateAsync(dto);
        return Json(new { success = true, data = created });
    }

    [HttpPost]
    public async Task<IActionResult> Edit([FromBody] Supplier dto)
    {
        if (!_flags.EnablePurchasingModule)
            return Json(new { success = false, message = "Purchasing module disabled" });
        var updated = await _suppliers.UpdateAsync(dto);
        return Json(new { success = true, data = updated });
    }

    [HttpPost]
    public async Task<IActionResult> Delete([FromBody] Guid id)
    {
        if (!_flags.EnablePurchasingModule)
            return Json(new { success = false, message = "Purchasing module disabled" });
        var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
        await _suppliers.BlacklistAsync(id, userId, "Deleted via UI");
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> GetSupplierProducts(Guid supplierId)
    {
        if (!_flags.EnablePurchasingModule)
            return Json(new { success = false, message = "Purchasing module disabled" });
        var list = await _suppliers.GetSupplierProductsAsync(supplierId);
        return Json(new { success = true, data = list });
    }
}
