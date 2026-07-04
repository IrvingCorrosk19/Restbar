using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.ViewModel;
using System.Security.Claims;

namespace RestBar.Controllers;

[ApiController]
[Route("api/kitchen")]
[Authorize(Policy = "KitchenAccess")]
public class KitchenApiController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly RestBar.Models.RestBarContext _context;
    private readonly ILogger<KitchenApiController> _logger;

    public KitchenApiController(IOrderService orderService, RestBar.Models.RestBarContext context, ILogger<KitchenApiController> logger)
    {
        _orderService = orderService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Snapshot de órdenes para recuperación tras desconexión.
    /// GET /api/kitchen/current?stationType=kitchen&stationId=&areaId=
    /// </summary>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentOrders(
        [FromQuery] string? stationType = null,
        [FromQuery] Guid? stationId = null,
        [FromQuery] Guid? areaId = null)
    {
        try
        {
            var userBranchClaim = User.FindFirst("BranchId")?.Value;
            var userCompanyClaim = User.FindFirst("CompanyId")?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var isGlobal = userRole is "superadmin" or "admin" && string.IsNullOrEmpty(userBranchClaim);

            Guid? branchId = Guid.TryParse(userBranchClaim, out var b) ? b : null;
            Guid? companyId = Guid.TryParse(userCompanyClaim, out var c) ? c : null;

            var allOrders = await _orderService.GetKitchenOrdersAsync(
                isGlobal ? null : branchId,
                isGlobal ? null : companyId);

            List<Guid> matchingStationIds;
            if (stationId.HasValue)
            {
                matchingStationIds = new List<Guid> { stationId.Value };
            }
            else if (!string.IsNullOrWhiteSpace(stationType))
            {
                var q = _context.Stations.AsNoTracking()
                    .Where(s => s.IsActive && s.Type.ToLower() == stationType.Trim().ToLower());
                if (!isGlobal && branchId.HasValue) q = q.Where(s => s.BranchId == branchId);
                if (!isGlobal && companyId.HasValue) q = q.Where(s => s.CompanyId == companyId);
                if (areaId.HasValue) q = q.Where(s => s.AreaId == areaId);
                matchingStationIds = await q.Select(s => s.Id).ToListAsync();
            }
            else
            {
                matchingStationIds = new List<Guid>();
            }

            IEnumerable<KitchenOrderViewModel> filtered = allOrders;
            if (stationId.HasValue || !string.IsNullOrWhiteSpace(stationType))
            {
                filtered = allOrders
                    .Select(o => new KitchenOrderViewModel
                    {
                        OrderId = o.OrderId,
                        TableNumber = o.TableNumber,
                        TableAreaId = o.TableAreaId,
                        TableAreaName = o.TableAreaName,
                        BranchId = o.BranchId,
                        OpenedAt = o.OpenedAt,
                        OrderStatus = o.OrderStatus,
                        Items = o.Items.Where(i => i.StationId.HasValue && matchingStationIds.Contains(i.StationId.Value)).ToList(),
                        PendingItems = o.PendingItems,
                        ReadyItems = o.ReadyItems,
                        PreparingItems = o.PreparingItems,
                        Notes = o.Notes
                    })
                    .Where(o => o.Items.Any());
            }

            var result = filtered.Select(o => new
            {
                orderId = o.OrderId,
                tableNumber = o.TableNumber,
                tableAreaName = o.TableAreaName,
                branchId = o.BranchId,
                openedAt = o.OpenedAt,
                orderStatus = o.OrderStatus,
                items = o.Items.Select(i => new
                {
                    itemId = i.ItemId,
                    productName = i.ProductName,
                    quantity = i.Quantity,
                    notes = i.Notes,
                    status = i.Status,
                    kitchenStatus = i.KitchenStatus,
                    stationName = i.StationName,
                    stationDisplayName = i.StationDisplayName,
                    stationId = i.StationId,
                    isReady = i.IsReady
                }).ToList()
            }).ToList();

            return Ok(new
            {
                success = true,
                timestamp = DateTime.UtcNow,
                stationType = stationType ?? "all",
                stationId,
                areaId,
                orderCount = result.Count,
                orders = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[KitchenApi] Error en GetCurrentOrders");
            return StatusCode(500, new { success = false, message = "Error al obtener órdenes de cocina" });
        }
    }
}
