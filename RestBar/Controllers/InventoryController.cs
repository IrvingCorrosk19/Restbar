using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RestBar.Controllers
{
    [Authorize(Policy = "InventoryAccess")]
    public class InventoryController : Controller
    {
        private static readonly TimeZoneInfo PanamaTz = ResolvePanamaTz();

        private readonly RestBarContext _context;
        private readonly IAreaService _areaService;

        public InventoryController(RestBarContext context, IAreaService areaService)
        {
            _context = context;
            _areaService = areaService;
        }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetLowStockProducts()
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });

                var branchId = currentUser.BranchId;
                var companyId = currentUser.Branch!.CompanyId;

                var globalLow = await _context.Products.AsNoTracking()
                    .Where(p => p.IsActive
                                && p.TrackInventory
                                && p.CompanyId == companyId
                                && (p.BranchId == branchId || p.BranchId == null)
                                && p.Stock != null
                                && p.MinStock != null
                                && p.Stock <= p.MinStock)
                    .Select(p => new
                    {
                        productId = p.Id,
                        productName = p.Name,
                        stock = p.Stock!.Value,
                        minStock = p.MinStock!.Value,
                        availableStock = p.Stock!.Value,
                        categoryName = p.Category != null ? p.Category.Name : "Sin categoría",
                        stationName = (string?)null,
                        type = "global",
                        stationId = (Guid?)null
                    })
                    .ToListAsync();

                var stationLow = await _context.ProductStockAssignments.AsNoTracking()
                    .Where(sa => sa.IsActive
                                 && sa.BranchId == branchId
                                 && sa.MinStock != null
                                 && sa.Stock <= sa.MinStock.Value
                                 && sa.Product != null
                                 && sa.Product.IsActive
                                 && sa.Product.TrackInventory
                                 && sa.Product.CompanyId == companyId)
                    .Select(sa => new
                    {
                        productId = sa.ProductId,
                        productName = sa.Product!.Name,
                        stock = sa.Stock,
                        minStock = sa.MinStock!.Value,
                        availableStock = sa.Stock,
                        categoryName = sa.Product.Category != null ? sa.Product.Category.Name : "Sin categoría",
                        stationName = sa.Station != null ? sa.Station.Name : "Sin estación",
                        type = "station",
                        stationId = (Guid?)sa.StationId
                    })
                    .ToListAsync();

                var data = globalLow.Cast<object>().Concat(stationLow.Cast<object>()).ToList();
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetInventoryData()
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });

                var branchId = currentUser.BranchId;
                var companyId = currentUser.Branch!.CompanyId;
                var branchName = currentUser.Branch.Name;

                var inventory = await _context.Products.AsNoTracking()
                    .Where(p => p.IsActive
                                && p.TrackInventory
                                && p.CompanyId == companyId
                                && (p.BranchId == branchId || p.BranchId == null))
                    .Select(p => new
                    {
                        productId = p.Id,
                        productName = p.Name,
                        productDescription = p.Description,
                        categoryId = p.CategoryId,
                        categoryName = p.Category != null ? p.Category.Name : "Sin categoría",
                        branchId = p.BranchId ?? branchId,
                        branchName = p.Branch != null ? p.Branch.Name : branchName,
                        quantity = p.Stock ?? 0m,
                        minStock = p.MinStock ?? 0m,
                        lastUpdated = p.UpdatedAt
                    })
                    .ToListAsync();

                return Json(new { success = true, inventory });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });

                var branchId = currentUser.BranchId;
                var companyId = currentUser.Branch!.CompanyId;

                var products = await _context.Products.AsNoTracking()
                    .Where(p => p.IsActive
                                && p.CompanyId == companyId
                                && (p.BranchId == branchId || p.BranchId == null))
                    .OrderBy(p => p.Name)
                    .Select(p => new
                    {
                        id = p.Id,
                        name = p.Name,
                        description = p.Description,
                        categoryId = p.CategoryId,
                        categoryName = p.Category != null ? p.Category.Name : "Sin categoría"
                    })
                    .ToListAsync();

                return Json(new { success = true, products });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });

                var branchId = currentUser.BranchId;

                var categories = await _context.Categories.AsNoTracking()
                    .Where(c => c.IsActive && (c.BranchId == branchId || c.BranchId == null))
                    .OrderBy(c => c.Name)
                    .Select(c => new
                    {
                        id = c.Id,
                        name = c.Name,
                        description = c.Description
                    })
                    .ToListAsync();

                return Json(new { success = true, categories });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBranches()
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });

                var companyId = currentUser.Branch!.CompanyId;

                var branches = await _context.Branches.AsNoTracking()
                    .Where(b => b.IsActive && b.CompanyId == companyId)
                    .OrderBy(b => b.Name)
                    .Select(b => new
                    {
                        id = b.Id,
                        name = b.Name,
                        address = b.Address,
                        phone = b.Phone
                    })
                    .ToListAsync();

                return Json(new { success = true, branches });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ConsumptionReport(DateTime? startDate, DateTime? endDate, Guid? productId, Guid? stationId)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });

                var branchId = currentUser.BranchId;
                var startLocal = (startDate ?? TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PanamaTz).Date.AddDays(-30)).Date;
                var endLocal = (endDate ?? TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PanamaTz).Date).Date;
                var start = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(startLocal, DateTimeKind.Unspecified), PanamaTz);
                var endExclusive = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(endLocal.AddDays(1), DateTimeKind.Unspecified), PanamaTz);

                var query = _context.OrderItems.AsNoTracking()
                    .Where(oi => oi.Order!.BranchId == branchId
                                 && oi.Order.CreatedAt >= start
                                 && oi.Order.CreatedAt < endExclusive
                                 && oi.ProductId != null
                                 && oi.Status != OrderItemStatus.Cancelled);

                if (productId.HasValue)
                    query = query.Where(oi => oi.ProductId == productId.Value);

                if (stationId.HasValue)
                    query = query.Where(oi => oi.PreparedByStationId == stationId.Value);

                var consumptionData = await query
                    .GroupBy(oi => new
                    {
                        oi.ProductId,
                        ProductName = oi.Product!.Name,
                        oi.PreparedByStationId,
                        StationName = oi.PreparedByStation != null ? oi.PreparedByStation.Name : null
                    })
                    .Select(g => new
                    {
                        productId = g.Key.ProductId,
                        productName = g.Key.ProductName,
                        stationId = g.Key.PreparedByStationId,
                        stationName = g.Key.StationName ?? "Sin estación",
                        totalQuantity = g.Sum(x => x.Quantity),
                        totalOrders = g.Select(x => x.OrderId).Distinct().Count(),
                        averageQuantity = g.Average(x => x.Quantity),
                        minQuantity = g.Min(x => x.Quantity),
                        maxQuantity = g.Max(x => x.Quantity)
                    })
                    .OrderByDescending(x => x.totalQuantity)
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    data = consumptionData,
                    filters = new { startDate = startLocal, endDate = endLocal, productId, stationId }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return null;
            var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(Guid.Parse(userIdClaim.Value));
            return currentUser?.Branch == null ? null : currentUser;
        }

        private static TimeZoneInfo ResolvePanamaTz()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("America/Panama"); }
            catch
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"); }
                catch { return TimeZoneInfo.Utc; }
            }
        }
    }
}
