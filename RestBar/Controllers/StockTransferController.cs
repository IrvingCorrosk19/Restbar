using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestBar.Helpers;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Controllers;

[Authorize(Policy = "InventoryAccess")]
public class StockTransferController : Controller
{
    private readonly RestBarContext _context;
    private readonly IInventoryOperationsService _inventoryOps;

    public StockTransferController(RestBarContext context, IInventoryOperationsService inventoryOps)
    {
        _context = context;
        _inventoryOps = inventoryOps;
    }

    public class TransferRequestDto
    {
        public Guid ProductId { get; set; }
        public Guid FromStationId { get; set; }
        public Guid ToStationId { get; set; }
        public decimal Quantity { get; set; }
        public Guid? BranchId { get; set; }
        public string? Notes { get; set; }
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var companyId = TenantScope.CompanyId(User);
        var query = _context.StockTransfers.AsNoTracking()
            .Include(t => t.Product)
            .Include(t => t.FromStation)
            .Include(t => t.ToStation)
            .AsQueryable();
        if (companyId.HasValue)
            query = query.Where(t => t.CompanyId == companyId.Value);
        var transfers = await query
            .OrderByDescending(t => t.RequestedAt)
            .Take(100)
            .ToListAsync();
        return Json(new { success = true, data = transfers });
    }

    [HttpPost]
    public async Task<IActionResult> Request([FromBody] TransferRequestDto dto)
    {
        var userId = Guid.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : (Guid?)null;
        var companyId = TenantScope.CompanyId(User);
        var branchId = await TenantScope.ResolveBranchIdAsync(_context, User, dto.BranchId);

        var transfer = new StockTransfer
        {
            Id = Guid.NewGuid(),
            ProductId = dto.ProductId,
            FromStationId = dto.FromStationId,
            ToStationId = dto.ToStationId,
            Quantity = dto.Quantity,
            BranchId = branchId,
            CompanyId = companyId,
            Status = StockTransferStatus.Pending,
            RequestedByUserId = userId,
            Notes = dto.Notes
        };
        _context.StockTransfers.Add(transfer);
        await _context.SaveChangesAsync();
        return Json(new { success = true, transferId = transfer.Id });
    }

    [HttpPost]
    public async Task<IActionResult> Approve(Guid id)
    {
        var companyId = TenantScope.CompanyId(User);
        var transfer = await _context.StockTransfers.FindAsync(id);
        if (transfer == null || (companyId.HasValue && transfer.CompanyId != companyId))
            return NotFound(new { success = false, message = "Transferencia no encontrada" });

        if (transfer.Status != StockTransferStatus.Pending)
            return BadRequest(new { success = false, message = "La transferencia ya fue procesada" });

        var approverId = Guid.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : (Guid?)null;

        await _inventoryOps.TransferStockBetweenStationsAsync(
            transfer.ProductId, transfer.FromStationId, transfer.ToStationId,
            transfer.Quantity, transfer.BranchId, transfer.CompanyId, approverId, transfer.Notes);

        transfer.Status = StockTransferStatus.Completed;
        transfer.ApprovedByUserId = approverId;
        transfer.CompletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectDto? dto)
    {
        var companyId = TenantScope.CompanyId(User);
        var transfer = await _context.StockTransfers.FindAsync(id);
        if (transfer == null || (companyId.HasValue && transfer.CompanyId != companyId))
            return NotFound(new { success = false, message = "Transferencia no encontrada" });

        if (transfer.Status != StockTransferStatus.Pending)
            return BadRequest(new { success = false, message = "Solo se pueden rechazar transferencias pendientes" });

        transfer.Status = StockTransferStatus.Rejected;
        if (!string.IsNullOrWhiteSpace(dto?.Reason))
            transfer.Notes = string.IsNullOrWhiteSpace(transfer.Notes)
                ? $"Rechazado: {dto.Reason}"
                : $"{transfer.Notes} | Rechazado: {dto.Reason}";
        await _context.SaveChangesAsync();
        return Json(new { success = true });
    }

    public class RejectDto
    {
        public string? Reason { get; set; }
    }
}
