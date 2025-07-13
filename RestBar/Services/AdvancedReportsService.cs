using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using RestBar.ViewModels;
using System.Security.Claims;

namespace RestBar.Services
{
    public class AdvancedReportsService : IAdvancedReportsService
    {
        private readonly RestBarContext _context;
        private readonly ILogger<AdvancedReportsService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdvancedReportsService(RestBarContext context, ILogger<AdvancedReportsService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        // ===== ANÁLISIS DE RENTABILIDAD =====
        public async Task<ProfitabilityAnalysis> GetProfitabilityAnalysisAsync(ReportFilters filters)
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Where(o => o.Status == OrderStatus.Completed && 
                               o.ClosedAt >= filters.StartDate && o.ClosedAt <= filters.EndDate)
                    .ToListAsync();

                var totalRevenue = orders.Sum(o => o.TotalAmount ?? 0);
                var totalCost = await CalculateTotalCostAsync(filters);
                var grossProfit = totalRevenue - totalCost;
                var netProfit = grossProfit;

                var report = new ProfitabilityAnalysis
                {
                    TotalRevenue = totalRevenue,
                    TotalCosts = totalCost,
                    GrossProfit = grossProfit,
                    NetProfit = netProfit,
                    ProfitMargin = totalRevenue > 0 ? (netProfit / totalRevenue) * 100 : 0,
                    GrossMargin = totalRevenue > 0 ? (grossProfit / totalRevenue) * 100 : 0,
                    ProductProfitability = await GetProductProfitabilityAsync(filters),
                    CategoryProfitability = await GetCategoryProfitabilityAsync(filters),
                    DailyProfitability = new List<DailyProfitability>()
                };

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error generando análisis de rentabilidad");
                return new ProfitabilityAnalysis();
            }
        }

        public async Task<List<ProductProfitability>> GetProductProfitabilityAsync(ReportFilters filters)
        {
            try
            {
                var profitabilityData = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsActive == true)
                    .Select(p => new ProductProfitability
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        CategoryName = p.Category.Name,
                        Revenue = _context.OrderItems
                            .Where(oi => oi.ProductId == p.Id && 
                                       oi.Order.Status == OrderStatus.Completed &&
                                       oi.Order.ClosedAt >= filters.StartDate && 
                                       oi.Order.ClosedAt <= filters.EndDate)
                            .Sum(oi => oi.Quantity * oi.UnitPrice),
                        Cost = _context.OrderItems
                            .Where(oi => oi.ProductId == p.Id && 
                                       oi.Order.Status == OrderStatus.Completed &&
                                       oi.Order.ClosedAt >= filters.StartDate && 
                                       oi.Order.ClosedAt <= filters.EndDate)
                            .Sum(oi => oi.Quantity * (p.Cost ?? 0)),
                        Profit = 0,
                        ProfitMargin = 0,
                        UnitsSold = _context.OrderItems
                            .Where(oi => oi.ProductId == p.Id && 
                                       oi.Order.Status == OrderStatus.Completed &&
                                       oi.Order.ClosedAt >= filters.StartDate && 
                                       oi.Order.ClosedAt <= filters.EndDate)
                            .Sum(oi => (int)oi.Quantity),
                        AveragePrice = 0,
                        AverageCost = p.Cost ?? 0
                    })
                    .ToListAsync();

                // Calcular profit y profit margin
                foreach (var item in profitabilityData)
                {
                    item.Profit = item.Revenue - item.Cost;
                    item.ProfitMargin = item.Revenue > 0 ? (item.Profit / item.Revenue) * 100 : 0;
                    item.AveragePrice = item.UnitsSold > 0 ? item.Revenue / item.UnitsSold : 0;
                }

                return profitabilityData.OrderByDescending(x => x.Profit).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error obteniendo rentabilidad de productos");
                return new List<ProductProfitability>();
            }
        }

        public async Task<List<CategoryProfitability>> GetCategoryProfitabilityAsync(ReportFilters filters)
        {
            try
            {
                var categoryProfitability = await _context.Categories
                    .Where(c => c.IsActive == true)
                    .Select(c => new CategoryProfitability
                    {
                        CategoryId = c.Id,
                        CategoryName = c.Name,
                        Revenue = _context.OrderItems
                            .Where(oi => oi.Product.CategoryId == c.Id && 
                                       oi.Order.Status == OrderStatus.Completed &&
                                       oi.Order.ClosedAt >= filters.StartDate && 
                                       oi.Order.ClosedAt <= filters.EndDate)
                            .Sum(oi => oi.Quantity * oi.UnitPrice),
                        Cost = _context.OrderItems
                            .Where(oi => oi.Product.CategoryId == c.Id && 
                                       oi.Order.Status == OrderStatus.Completed &&
                                       oi.Order.ClosedAt >= filters.StartDate && 
                                       oi.Order.ClosedAt <= filters.EndDate)
                            .Sum(oi => oi.Quantity * (oi.Product.Cost ?? 0)),
                        Profit = 0,
                        ProfitMargin = 0,
                        ProductsCount = _context.Products.Count(p => p.CategoryId == c.Id && p.IsActive == true),
                        UnitsSold = _context.OrderItems
                            .Where(oi => oi.Product.CategoryId == c.Id && 
                                       oi.Order.Status == OrderStatus.Completed &&
                                       oi.Order.ClosedAt >= filters.StartDate && 
                                       oi.Order.ClosedAt <= filters.EndDate)
                            .Sum(oi => (int)oi.Quantity)
                    })
                    .ToListAsync();

                // Calcular profit y profit margin
                foreach (var item in categoryProfitability)
                {
                    item.Profit = item.Revenue - item.Cost;
                    item.ProfitMargin = item.Revenue > 0 ? (item.Profit / item.Revenue) * 100 : 0;
                }

                return categoryProfitability.OrderByDescending(x => x.Profit).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error obteniendo rentabilidad por categoría");
                return new List<CategoryProfitability>();
            }
        }

        // ===== ANÁLISIS DE VENTAS =====
        public async Task<SalesAnalysisReport> GetSalesAnalysisAsync(ReportFilters filters)
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Where(o => o.Status == OrderStatus.Completed && 
                               o.ClosedAt >= filters.StartDate && o.ClosedAt <= filters.EndDate)
                    .ToListAsync();

                var totalSales = orders.Sum(o => o.TotalAmount ?? 0);
                var totalOrders = orders.Count;
                var totalItems = orders.Sum(o => o.OrderItems.Sum(oi => (int)oi.Quantity));

                var report = new SalesAnalysisReport
                {
                    TotalSales = totalSales,
                    TotalOrders = totalOrders,
                    TotalItems = totalItems,
                    AverageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0,
                    AverageItemPrice = totalItems > 0 ? totalSales / totalItems : 0,
                    TopSellingProducts = await GetTopSellingProductsAsync(filters),
                    TopSellingCategories = await GetTopSellingCategoriesAsync(filters),
                    SalesByHour = new List<SalesByHour>(),
                    SalesByDay = new List<SalesByDay>(),
                    SalesByMonth = new List<SalesByMonth>()
                };

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error generando análisis de ventas");
                return new SalesAnalysisReport();
            }
        }

        public async Task<List<TopSellingProduct>> GetTopSellingProductsAsync(ReportFilters filters)
        {
            try
            {
                var topProducts = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsActive == true)
                    .Select(p => new TopSellingProduct
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        CategoryName = p.Category.Name,
                        UnitsSold = _context.OrderItems
                            .Where(oi => oi.ProductId == p.Id && 
                                       oi.Order.Status == OrderStatus.Completed &&
                                       oi.Order.ClosedAt >= filters.StartDate && 
                                       oi.Order.ClosedAt <= filters.EndDate)
                            .Sum(oi => (int)oi.Quantity),
                        Revenue = _context.OrderItems
                            .Where(oi => oi.ProductId == p.Id && 
                                       oi.Order.Status == OrderStatus.Completed &&
                                       oi.Order.ClosedAt >= filters.StartDate && 
                                       oi.Order.ClosedAt <= filters.EndDate)
                            .Sum(oi => oi.Quantity * oi.UnitPrice),
                        AveragePrice = 0,
                        Profit = 0,
                        ProfitMargin = 0
                    })
                    .Where(p => p.UnitsSold > 0)
                    .OrderByDescending(p => p.UnitsSold)
                    .Take(filters.TopN ?? 10)
                    .ToListAsync();

                // Calcular valores derivados
                foreach (var product in topProducts)
                {
                    product.AveragePrice = product.UnitsSold > 0 ? product.Revenue / product.UnitsSold : 0;
                    // Profit calculation would require cost data
                    product.Profit = 0;
                    product.ProfitMargin = 0;
                }

                return topProducts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error obteniendo productos más vendidos");
                return new List<TopSellingProduct>();
            }
        }

        public async Task<List<TopSellingCategory>> GetTopSellingCategoriesAsync(ReportFilters filters)
        {
            try
            {
                var topCategories = await _context.Categories
                    .Where(c => c.IsActive == true)
                    .Select(c => new TopSellingCategory
                    {
                        CategoryId = c.Id,
                        CategoryName = c.Name,
                        UnitsSold = _context.OrderItems
                            .Where(oi => oi.Product.CategoryId == c.Id && 
                                       oi.Order.Status == OrderStatus.Completed &&
                                       oi.Order.ClosedAt >= filters.StartDate && 
                                       oi.Order.ClosedAt <= filters.EndDate)
                            .Sum(oi => (int)oi.Quantity),
                        Revenue = _context.OrderItems
                            .Where(oi => oi.Product.CategoryId == c.Id && 
                                       oi.Order.Status == OrderStatus.Completed &&
                                       oi.Order.ClosedAt >= filters.StartDate && 
                                       oi.Order.ClosedAt <= filters.EndDate)
                            .Sum(oi => oi.Quantity * oi.UnitPrice),
                        Profit = 0,
                        ProfitMargin = 0,
                        ProductsCount = _context.Products.Count(p => p.CategoryId == c.Id && p.IsActive == true)
                    })
                    .Where(c => c.UnitsSold > 0)
                    .OrderByDescending(c => c.UnitsSold)
                    .Take(filters.TopN ?? 10)
                    .ToListAsync();

                // Calcular valores derivados
                foreach (var category in topCategories)
                {
                    // Profit calculation would require cost data
                    category.Profit = 0;
                    category.ProfitMargin = 0;
                }

                return topCategories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error obteniendo categorías más vendidas");
                return new List<TopSellingCategory>();
            }
        }

        // ===== ANÁLISIS DE CLIENTES =====
        public async Task<CustomerAnalysisReport> GetCustomerAnalysisAsync(ReportFilters filters)
        {
            try
            {
                var report = new CustomerAnalysisReport
                {
                    TotalCustomers = 0,
                    NewCustomers = 0,
                    ReturningCustomers = 0,
                    AverageCustomerValue = 0,
                    CustomerLifetimeValue = 0,
                    TopCustomers = new List<TopCustomer>(),
                    CustomerSegments = new List<CustomerSegment>(),
                    CustomerRetention = new List<CustomerRetention>()
                };

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error generando análisis de clientes");
                return new CustomerAnalysisReport();
            }
        }

        public async Task<List<TopCustomer>> GetTopCustomersAsync(ReportFilters filters)
        {
            try
            {
                return new List<TopCustomer>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error obteniendo clientes principales");
                return new List<TopCustomer>();
            }
        }

        public async Task<List<CustomerSegment>> GetCustomerSegmentsAsync(ReportFilters filters)
        {
            try
            {
                return new List<CustomerSegment>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error obteniendo segmentos de clientes");
                return new List<CustomerSegment>();
            }
        }

        // ===== ANÁLISIS DE OPERACIONES =====
        public async Task<OperationalAnalysisReport> GetOperationalAnalysisAsync(ReportFilters filters)
        {
            try
            {
                var report = new OperationalAnalysisReport
                {
                    TotalOrders = 0,
                    CompletedOrders = 0,
                    CancelledOrders = 0,
                    AverageOrderTime = 0,
                    AveragePreparationTime = 0,
                    StationPerformance = new List<StationPerformance>(),
                    TableUtilization = new List<TableUtilization>(),
                    PeakHours = new List<PeakHours>()
                };

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error generando análisis operacional");
                return new OperationalAnalysisReport();
            }
        }

        public async Task<List<StationPerformance>> GetStationPerformanceAsync(ReportFilters filters)
        {
            try
            {
                return new List<StationPerformance>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error obteniendo rendimiento de estaciones");
                return new List<StationPerformance>();
            }
        }

        public async Task<List<TableUtilization>> GetTableUtilizationAsync(ReportFilters filters)
        {
            try
            {
                return new List<TableUtilization>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error obteniendo utilización de mesas");
                return new List<TableUtilization>();
            }
        }

        // ===== EXPORTACIÓN =====
        public async Task<byte[]> ExportAdvancedReportToPdfAsync(string reportType, ReportFilters filters)
        {
            try
            {
                // Implementación básica - retornar array vacío por ahora
                return new byte[0];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error exportando reporte a PDF");
                return new byte[0];
            }
        }

        public async Task<byte[]> ExportAdvancedReportToExcelAsync(string reportType, ReportFilters filters)
        {
            try
            {
                // Implementación básica - retornar array vacío por ahora
                return new byte[0];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error exportando reporte a Excel");
                return new byte[0];
            }
        }

        // ===== MÉTODOS PRIVADOS =====
        private async Task<decimal> CalculateTotalCostAsync(ReportFilters filters)
        {
            try
            {
                var totalCost = await _context.OrderItems
                    .Where(oi => oi.Order.Status == OrderStatus.Completed &&
                               oi.Order.ClosedAt >= filters.StartDate && 
                               oi.Order.ClosedAt <= filters.EndDate)
                    .SumAsync(oi => oi.Quantity * (oi.Product.Cost ?? 0));

                return totalCost;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error calculando costo total");
                return 0;
            }
        }
    }
} 