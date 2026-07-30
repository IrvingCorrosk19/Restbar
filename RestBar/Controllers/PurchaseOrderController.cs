using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RestBar.Infrastructure.Foundation;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Controllers;

[Authorize(Policy = "PurchasingAccess")]
public class PurchaseOrderController : Controller
{
    private readonly IPurchaseOrderService _orders;
    private readonly IGoodsReceiptService _receipts;
    private readonly ISupplierService _suppliers;
    private readonly FeatureFlags _flags;

    public PurchaseOrderController(
        IPurchaseOrderService orders,
        IGoodsReceiptService receipts,
        ISupplierService suppliers,
        IOptions<FeatureFlags> flags)
    {
        _orders = orders;
        _receipts = receipts;
        _suppliers = suppliers;
        _flags = flags.Value;
    }

    private IActionResult? DisabledView() =>
        !_flags.EnablePurchasingModule ? View("~/Views/Supplier/ModuleDisabled.cshtml") : null;

    private IActionResult? DisabledJson() =>
        !_flags.EnablePurchasingModule
            ? Json(new { success = false, message = "Purchasing module disabled" })
            : null;

    public async Task<IActionResult> Index()
    {
        if (DisabledView() is { } d) return d;
        var branchId = Guid.Parse(User.FindFirst("BranchId")!.Value);
        return View(await _orders.ListOpenByBranchAsync(branchId));
    }

    public async Task<IActionResult> Detail(Guid id)
    {
        if (DisabledView() is { } d) return d;
        var po = await _orders.GetByIdAsync(id);
        if (po == null) return NotFound();
        return View(po);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (DisabledView() is { } d) return d;
        var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);
        ViewBag.Suppliers = await _suppliers.ListByCompanyAsync(companyId);
        return View();
    }

    public record CreatePoDto(Guid SupplierId, List<CreatePoLineDto> Lines, DateTime? ExpectedDelivery, string? Notes);
    public record CreatePoLineDto(Guid ProductId, decimal Quantity, decimal UnitPrice, Guid? StationId);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePoDto dto)
    {
        if (DisabledJson() is { } d) return d;
        if (dto.SupplierId == Guid.Empty || dto.Lines == null || dto.Lines.Count == 0
            || dto.Lines.Any(l => l.ProductId == Guid.Empty || l.Quantity <= 0))
            return Json(new { success = false, message = "Proveedor, producto y cantidad son obligatorios." });

        var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
        var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);
        var branchId = Guid.Parse(User.FindFirst("BranchId")!.Value);

        var order = new PurchaseOrder
        {
            CompanyId = companyId,
            BranchId = branchId,
            SupplierId = dto.SupplierId,
            RequestedByUserId = userId,
            ExpectedDelivery = dto.ExpectedDelivery ?? DateTime.UtcNow.AddDays(2),
            Notes = dto.Notes,
            Lines = dto.Lines.Select(l => new PurchaseOrderLine
            {
                ProductId = l.ProductId,
                QuantityOrdered = l.Quantity,
                UnitPrice = l.UnitPrice,
                StationId = l.StationId
            }).ToList()
        };

        var created = await _orders.CreateDraftAsync(order);
        return Json(new { success = true, id = created.Id, poNumber = created.PoNumber });
    }

    [HttpPost]
    public async Task<IActionResult> Submit(Guid id)
    {
        if (DisabledView() is { } d) return d;
        var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
        await _orders.SubmitForApprovalAsync(id, userId);
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Approve(Guid id)
    {
        if (DisabledView() is { } d) return d;
        var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
        await _orders.ApproveAsync(id, userId);
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Send(Guid id)
    {
        if (DisabledView() is { } d) return d;
        var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
        await _orders.SendAsync(id, userId);
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Receive(Guid id)
    {
        if (DisabledView() is { } d) return d;
        var po = await _orders.GetByIdAsync(id);
        if (po == null) return NotFound();
        return View(po);
    }

    public record CompleteReceiptDto(Guid ReceiptId, List<GoodsReceiptLineInput> Lines);

    [HttpPost]
    public async Task<IActionResult> StartReceive(Guid id)
    {
        if (DisabledJson() is { } d) return d;
        var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
        var receipt = await _receipts.CreateDraftAsync(id, userId);
        return Json(new { success = true, receiptId = receipt.Id });
    }

    [HttpPost]
    public async Task<IActionResult> CompleteReceive([FromBody] CompleteReceiptDto dto)
    {
        if (DisabledJson() is { } d) return d;
        var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
        var receipt = await _receipts.CompleteAsync(dto.ReceiptId, dto.Lines, userId);
        return Json(new { success = true, receiptId = receipt.Id, status = receipt.Status.ToString() });
    }
}
