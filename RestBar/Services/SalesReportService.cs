using Microsoft.EntityFrameworkCore;
using RestBar.Models;
using RestBar.ViewModels;
using RestBar.Interfaces;

namespace RestBar.Services
{
    public class SalesReportService : ISalesReportService
    {
        private readonly RestBarContext _context;
        private readonly ILogger<SalesReportService> _logger;

        public SalesReportService(RestBarContext context, ILogger<SalesReportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ✅ Métricas generales de ventas
        public async Task<SalesMetrics> GetSalesMetricsAsync(ReportFilters filters)
        {
            try
            {
                _logger.LogInformation("[SalesReportService] Generando métricas de ventas");

                var query = _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.Payments)
                    .Include(o => o.User)
                    .Include(o => o.Table)
                    .ThenInclude(t => t.Area)
                    .AsQueryable();

                // Aplicar filtros
                query = ApplyFilters(query, filters);

                var metrics = await query
                    .Where(o => o.Status == OrderStatus.Completed)
                    .GroupBy(o => 1)
                    .Select(g => new SalesMetrics
                    {
                        TotalRevenue = g.Sum(o => o.TotalAmount ?? 0),
                        TotalOrders = g.Count(),
                        AverageTicket = g.Average(o => o.TotalAmount ?? 0),
                        TotalItems = g.SelectMany(o => o.OrderItems).Sum(oi => (int)oi.Quantity),
                        TotalDiscounts = g.SelectMany(o => o.OrderItems).Sum(oi => oi.Discount),
                        NetRevenue = g.Sum(o => o.TotalAmount ?? 0) - g.SelectMany(o => o.OrderItems).Sum(oi => oi.Discount),
                        AverageItemsPerOrder = g.SelectMany(o => o.OrderItems).Average(oi => oi.Quantity),
                        ProfitMargin = 0 // Se calculará por separado
                    })
                    .FirstOrDefaultAsync();

                var result = metrics ?? new SalesMetrics();

                // Calcular margen de ganancia
                if (result.TotalRevenue > 0)
                {
                    var totalCost = await CalculateTotalCostAsync(filters);
                    result.ProfitMargin = totalCost > 0 ? ((result.TotalRevenue - totalCost) / result.TotalRevenue) * 100 : 0;
                }

                _logger.LogInformation($"[SalesReportService] Métricas generadas - Revenue: {result.TotalRevenue:C}, Orders: {result.TotalOrders}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SalesReportService] Error generando métricas de ventas");
                return new SalesMetrics();
            }
        }

        // ✅ Ventas por día/semana/mes/año
        public async Task<List<DailySalesData>> GetDailySalesAsync(ReportFilters filters)
        {
            try
            {
                _logger.LogInformation("[SalesReportService] Generando ventas diarias");

                var query = _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.User)
                    .Include(o => o.Table)
                    .ThenInclude(t => t.Area)
                    .AsQueryable();

                query = ApplyFilters(query, filters);

                var dailySales = await query
                    .Where(o => o.Status == OrderStatus.Completed)
                    .GroupBy(o => o.ClosedAt.Value.Date)
                    .Select(g => new DailySalesData
                    {
                        Date = g.Key,
                        Revenue = g.Sum(o => o.TotalAmount ?? 0),
                        Orders = g.Count(),
                        Items = g.SelectMany(o => o.OrderItems).Sum(oi => (int)oi.Quantity),
                        AverageTicket = g.Average(o => o.TotalAmount ?? 0),
                        Discounts = g.SelectMany(o => o.OrderItems).Sum(oi => oi.Discount)
                    })
                    .OrderBy(d => d.Date)
                    .ToListAsync();

                _logger.LogInformation($"[SalesReportService] Ventas diarias generadas - {dailySales.Count} días");
                return dailySales;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SalesReportService] Error generando ventas diarias");
                return new List<DailySalesData>();
            }
        }

        // ✅ Top productos más vendidos
        public async Task<List<TopProductData>> GetTopProductsAsync(ReportFilters filters)
        {
            try
            {
                _logger.LogInformation("[SalesReportService] Generando top productos");

                var query = _context.OrderItems
                    .Include(oi => oi.Order)
                    .Include(oi => oi.Product)
                    .ThenInclude(p => p.Category)
                    .AsQueryable();

                // Aplicar filtros de fecha
                if (filters.StartDate.HasValue)
                    query = query.Where(oi => oi.Order.ClosedAt >= filters.StartDate.Value);

                if (filters.EndDate.HasValue)
                    query = query.Where(oi => oi.Order.ClosedAt <= filters.EndDate.Value);

                var topProducts = await query
                    .Where(oi => oi.Order.Status == OrderStatus.Completed)
                    .GroupBy(oi => new { 
                        oi.ProductId, 
                        oi.Product.Name, 
                        oi.Product.Price, 
                        oi.Product.Cost, 
                        CategoryName = oi.Product.Category.Name 
                    })
                    .Select(g => new TopProductData
                    {
                        ProductId = g.Key.ProductId.Value,
                        ProductName = g.Key.Name,
                        CategoryName = g.Key.CategoryName ?? "",
                        QuantitySold = g.Sum(oi => (int)oi.Quantity),
                        Revenue = g.Sum(oi => oi.UnitPrice * oi.Quantity),
                        UnitPrice = g.Key.Price,
                        TotalCost = g.Sum(oi => (oi.Product.Cost ?? 0) * oi.Quantity),
                        Profit = g.Sum(oi => (oi.UnitPrice - (oi.Product.Cost ?? 0)) * oi.Quantity),
                        ProfitMargin = g.Average(oi => oi.Product.Cost > 0 ? ((oi.UnitPrice - oi.Product.Cost.Value) / oi.UnitPrice) * 100 : 0)
                    })
                    .OrderByDescending(p => p.QuantitySold)
                    .Take(10)
                    .ToListAsync();

                // Asignar ranking
                for (int i = 0; i < topProducts.Count; i++)
                {
                    topProducts[i].Rank = i + 1;
                }

                _logger.LogInformation($"[SalesReportService] Top productos generados - {topProducts.Count} productos");
                return topProducts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SalesReportService] Error generando top productos");
                return new List<TopProductData>();
            }
        }

        // ✅ Ventas por categoría
        public async Task<List<CategorySalesData>> GetCategorySalesAsync(ReportFilters filters)
        {
            try
            {
                _logger.LogInformation("[SalesReportService] Generando ventas por categoría");

                var query = _context.OrderItems
                    .Include(oi => oi.Order)
                    .Include(oi => oi.Product)
                    .ThenInclude(p => p.Category)
                    .AsQueryable();

                // Aplicar filtros
                if (filters.StartDate.HasValue)
                    query = query.Where(oi => oi.Order.ClosedAt >= filters.StartDate.Value);

                if (filters.EndDate.HasValue)
                    query = query.Where(oi => oi.Order.ClosedAt <= filters.EndDate.Value);

                var categorySales = await query
                    .Where(oi => oi.Order.Status == OrderStatus.Completed)
                    .GroupBy(oi => new { oi.Product.CategoryId, oi.Product.Category.Name })
                    .Select(g => new CategorySalesData
                    {
                        CategoryId = g.Key.CategoryId.Value,
                        CategoryName = g.Key.Name ?? "",
                        ItemsSold = g.Sum(oi => (int)oi.Quantity),
                        Revenue = g.Sum(oi => oi.UnitPrice * oi.Quantity),
                        TotalCost = g.Sum(oi => (oi.Product.Cost ?? 0) * oi.Quantity),
                        Profit = g.Sum(oi => (oi.UnitPrice - (oi.Product.Cost ?? 0)) * oi.Quantity),
                        ProfitMargin = g.Average(oi => oi.Product.Cost > 0 ? ((oi.UnitPrice - oi.Product.Cost.Value) / oi.UnitPrice) * 100 : 0),
                        ProductCount = g.Select(oi => oi.ProductId).Distinct().Count()
                    })
                    .OrderByDescending(c => c.Revenue)
                    .ToListAsync();

                // Calcular porcentajes
                var totalRevenue = categorySales.Sum(c => c.Revenue);
                if (totalRevenue > 0)
                {
                    foreach (var category in categorySales)
                    {
                        category.PercentageOfTotal = (category.Revenue / totalRevenue) * 100;
                    }
                }

                _logger.LogInformation($"[SalesReportService] Ventas por categoría generadas - {categorySales.Count} categorías");
                return categorySales;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SalesReportService] Error generando ventas por categoría");
                return new List<CategorySalesData>();
            }
        }

        // ✅ Ventas por empleado
        public async Task<List<EmployeeSalesData>> GetEmployeeSalesAsync(ReportFilters filters)
        {
            try
            {
                _logger.LogInformation("[SalesReportService] Generando ventas por empleado");

                var query = _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.User)
                    .Include(o => o.Table)
                    .ThenInclude(t => t.Area)
                    .AsQueryable();

                query = ApplyFilters(query, filters);

                var employeeSales = await query
                    .Where(o => o.Status == OrderStatus.Completed && o.UserId.HasValue)
                    .GroupBy(o => new { o.UserId, o.User.FullName, o.User.Role })
                    .Select(g => new EmployeeSalesData
                    {
                        UserId = g.Key.UserId.Value,
                        EmployeeName = g.Key.FullName ?? "Sin nombre",
                        Role = g.Key.Role.ToString(),
                        BranchName = "Sucursal Principal", // Por ahora hardcodeado
                        OrdersHandled = g.Count(),
                        Revenue = g.Sum(o => o.TotalAmount ?? 0),
                        AverageTicket = g.Average(o => o.TotalAmount ?? 0),
                        ItemsSold = g.SelectMany(o => o.OrderItems).Sum(oi => (int)oi.Quantity),
                        Commission = g.Sum(o => o.TotalAmount ?? 0) * 0.05m, // 5% comisión ejemplo
                        Performance = 100 // Se calculará vs meta
                    })
                    .OrderByDescending(e => e.Revenue)
                    .ToListAsync();

                _logger.LogInformation($"[SalesReportService] Ventas por empleado generadas - {employeeSales.Count} empleados");
                return employeeSales;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SalesReportService] Error generando ventas por empleado");
                return new List<EmployeeSalesData>();
            }
        }

        // ✅ Ventas por sucursal
        public async Task<List<BranchSalesData>> GetBranchSalesAsync(ReportFilters filters)
        {
            try
            {
                _logger.LogInformation("[SalesReportService] Generando ventas por sucursal");

                var query = _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.User)
                    .Include(o => o.Table)
                    .ThenInclude(t => t.Area)
                    .AsQueryable();

                query = ApplyFilters(query, filters);

                var branchSales = await query
                    .Where(o => o.Status == OrderStatus.Completed)
                    .GroupBy(o => 1) // Por ahora agrupamos todo como una sucursal
                    .Select(g => new BranchSalesData
                    {
                        BranchId = Guid.Empty,
                        BranchName = "Sucursal Principal",
                        Orders = g.Count(),
                        Revenue = g.Sum(o => o.TotalAmount ?? 0),
                        AverageTicket = g.Average(o => o.TotalAmount ?? 0),
                        ItemsSold = g.SelectMany(o => o.OrderItems).Sum(oi => (int)oi.Quantity),
                        TotalCost = g.SelectMany(o => o.OrderItems).Sum(oi => (oi.Product.Cost ?? 0) * oi.Quantity),
                        Profit = g.Sum(o => o.TotalAmount ?? 0) - g.SelectMany(o => o.OrderItems).Sum(oi => (oi.Product.Cost ?? 0) * oi.Quantity),
                        ProfitMargin = 0, // Se calculará
                        EmployeeCount = g.Select(o => o.UserId).Distinct().Count()
                    })
                    .OrderByDescending(b => b.Revenue)
                    .ToListAsync();

                // Calcular márgenes y porcentajes
                var totalRevenue = branchSales.Sum(b => b.Revenue);
                foreach (var branch in branchSales)
                {
                    if (branch.Revenue > 0)
                    {
                        branch.ProfitMargin = ((branch.Revenue - branch.TotalCost) / branch.Revenue) * 100;
                        branch.PercentageOfTotal = (branch.Revenue / totalRevenue) * 100;
                    }
                }

                _logger.LogInformation($"[SalesReportService] Ventas por sucursal generadas - {branchSales.Count} sucursales");
                return branchSales;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SalesReportService] Error generando ventas por sucursal");
                return new List<BranchSalesData>();
            }
        }

        // ✅ Reporte de descuentos
        public async Task<List<DiscountData>> GetDiscountsAsync(ReportFilters filters)
        {
            try
            {
                _logger.LogInformation("[SalesReportService] Generando reporte de descuentos");

                var query = _context.OrderItems
                    .Include(oi => oi.Order)
                    .Include(oi => oi.Product)
                    .ThenInclude(p => p.Category)
                    .Include(oi => oi.Order.User)
                    .Include(oi => oi.Order.Table)
                    .ThenInclude(t => t.Area)
                    .AsQueryable();

                // Aplicar filtros
                if (filters.StartDate.HasValue)
                    query = query.Where(oi => oi.Order.ClosedAt >= filters.StartDate.Value);

                if (filters.EndDate.HasValue)
                    query = query.Where(oi => oi.Order.ClosedAt <= filters.EndDate.Value);

                var discounts = await query
                    .Where(oi => oi.Order.Status == OrderStatus.Completed && oi.Discount > 0)
                    .Select(oi => new DiscountData
                    {
                        OrderId = oi.OrderId.Value,
                        ProductName = oi.Product.Name,
                        CategoryName = oi.Product.Category.Name ?? "",
                        OriginalPrice = oi.UnitPrice,
                        DiscountedPrice = oi.UnitPrice - oi.Discount,
                        DiscountAmount = oi.Discount,
                        DiscountPercentage = oi.UnitPrice > 0 ? (oi.Discount / oi.UnitPrice) * 100 : 0,
                        OrderDate = oi.Order.ClosedAt ?? DateTime.Now,
                        EmployeeName = oi.Order.User.FullName ?? "Sin nombre",
                        BranchName = "Sucursal Principal", // Por ahora hardcodeado
                        Quantity = (int)oi.Quantity,
                        TotalDiscount = oi.Discount * oi.Quantity
                    })
                    .OrderByDescending(d => d.TotalDiscount)
                    .ToListAsync();

                _logger.LogInformation($"[SalesReportService] Reporte de descuentos generado - {discounts.Count()} descuentos");
                return discounts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SalesReportService] Error generando reporte de descuentos");
                return new List<DiscountData>();
            }
        }

        // ✅ Método auxiliar para aplicar filtros
        private IQueryable<Order> ApplyFilters(IQueryable<Order> query, ReportFilters filters)
        {
            if (filters.StartDate.HasValue)
                query = query.Where(o => o.ClosedAt >= filters.StartDate.Value);

            if (filters.EndDate.HasValue)
                query = query.Where(o => o.ClosedAt <= filters.EndDate.Value);

            if (!string.IsNullOrEmpty(filters.OrderStatus))
                query = query.Where(o => o.Status.ToString() == filters.OrderStatus);

            return query;
        }

        // ✅ Método auxiliar para calcular costo total
        private async Task<decimal> CalculateTotalCostAsync(ReportFilters filters)
        {
            try
            {
                var query = _context.OrderItems
                    .Include(oi => oi.Order)
                    .Include(oi => oi.Product)
                    .AsQueryable();

                // Aplicar filtros de fecha
                if (filters.StartDate.HasValue)
                    query = query.Where(oi => oi.Order.ClosedAt >= filters.StartDate.Value);

                if (filters.EndDate.HasValue)
                    query = query.Where(oi => oi.Order.ClosedAt <= filters.EndDate.Value);

                return await query
                    .Where(oi => oi.Order.Status == OrderStatus.Completed)
                    .SumAsync(oi => (oi.Product.Cost ?? 0) * oi.Quantity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SalesReportService] Error calculando costo total");
                return 0;
            }
        }

        // ✅ Reporte completo de ventas
        public async Task<SalesReportViewModel> GetCompleteSalesReportAsync(ReportFilters filters)
        {
            try
            {
                _logger.LogInformation("[SalesReportService] Generando reporte completo de ventas");

                var report = new SalesReportViewModel
                {
                    StartDate = filters.StartDate ?? DateTime.Today.AddDays(-30),
                    EndDate = filters.EndDate ?? DateTime.Today,

                    BranchId = filters.BranchId,
                    UserId = filters.UserId,
                    CategoryId = filters.CategoryId
                };

                // Generar todos los reportes en paralelo para mejor rendimiento
                var metricsTask = GetSalesMetricsAsync(filters);
                var dailySalesTask = GetDailySalesAsync(filters);
                var topProductsTask = GetTopProductsAsync(filters);
                var categorySalesTask = GetCategorySalesAsync(filters);
                var employeeSalesTask = GetEmployeeSalesAsync(filters);
                var branchSalesTask = GetBranchSalesAsync(filters);
                var discountsTask = GetDiscountsAsync(filters);

                await Task.WhenAll(metricsTask, dailySalesTask, topProductsTask, categorySalesTask, employeeSalesTask, branchSalesTask, discountsTask);

                report.Metrics = await metricsTask;
                report.DailySales = await dailySalesTask;
                report.TopProducts = await topProductsTask;
                report.CategorySales = await categorySalesTask;
                report.EmployeeSales = await employeeSalesTask;
                report.BranchSales = await branchSalesTask;
                report.Discounts = await discountsTask;

                _logger.LogInformation($"[SalesReportService] Reporte completo generado - Revenue: {report.Metrics.TotalRevenue:C}");
                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SalesReportService] Error generando reporte completo");
                return new SalesReportViewModel();
            }
        }
    }
} 