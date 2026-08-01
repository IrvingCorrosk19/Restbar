using System.Collections;
using System.Reflection;
using System.Text;
using ClosedXML.Excel;
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
                var ordersQuery = _context.Orders
                    .Include(o => o.OrderItems)
                    .Where(o => o.Status == OrderStatus.Completed && 
                               o.ClosedAt >= filters.StartDate && o.ClosedAt <= filters.EndDate);
                if (filters.BranchId.HasValue)
                    ordersQuery = ordersQuery.Where(o => o.BranchId == filters.BranchId.Value);

                var orders = await ordersQuery.ToListAsync();

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
                Console.WriteLine("🔍 [AdvancedReportsService] GetCustomerAnalysisAsync() - Iniciando...");
                
                var startDate = filters.StartDate ?? DateTime.Today.AddDays(-90);
                var endDate = filters.EndDate ?? DateTime.Today;
                
                var userIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                Guid? companyId = null;
                
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    var user = await _context.Users
                        .Include(u => u.Branch)
                        .FirstOrDefaultAsync(u => u.Id == userId);
                    
                    companyId = user?.Branch?.CompanyId;
                }
                
                var ordersQuery = _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderItems)
                    .Where(o => o.Status == OrderStatus.Completed &&
                               o.ClosedAt >= startDate && o.ClosedAt <= endDate);
                
                if (companyId.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.CompanyId == companyId.Value);
                }
                
                var orders = await ordersQuery.ToListAsync();
                
                var customersWithOrders = orders
                    .Where(o => o.CustomerId.HasValue)
                    .GroupBy(o => o.CustomerId!.Value)
                    .ToList();
                
                var totalCustomers = await _context.Customers
                    .Where(c => !companyId.HasValue || c.CompanyId == companyId.Value)
                    .CountAsync();
                
                var customersInPeriod = customersWithOrders.Select(g => g.Key).Distinct().Count();
                var newCustomers = orders
                    .Where(o => o.CustomerId.HasValue && o.Customer != null && o.Customer.CreatedAt >= startDate)
                    .Select(o => o.CustomerId!.Value)
                    .Distinct()
                    .Count();
                var returningCustomers = customersInPeriod - newCustomers;
                
                var totalRevenue = orders.Where(o => o.CustomerId.HasValue).Sum(o => o.TotalAmount ?? 0);
                var averageCustomerValue = customersInPeriod > 0 ? totalRevenue / customersInPeriod : 0;
                
                var report = new CustomerAnalysisReport
                {
                    TotalCustomers = totalCustomers,
                    NewCustomers = newCustomers,
                    ReturningCustomers = returningCustomers,
                    AverageCustomerValue = averageCustomerValue,
                    CustomerLifetimeValue = averageCustomerValue,
                    TopCustomers = await GetTopCustomersAsync(filters),
                    CustomerSegments = await GetCustomerSegmentsAsync(filters),
                    CustomerRetention = new List<CustomerRetention>()
                };
                
                Console.WriteLine($"✅ [AdvancedReportsService] GetCustomerAnalysisAsync() - Reporte generado: {totalCustomers} clientes");
                return report;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AdvancedReportsService] GetCustomerAnalysisAsync() - Error: {ex.Message}");
                _logger.LogError(ex, "[AdvancedReportsService] Error generando análisis de clientes");
                return new CustomerAnalysisReport();
            }
        }

        public async Task<List<TopCustomer>> GetTopCustomersAsync(ReportFilters filters)
        {
            try
            {
                Console.WriteLine("🔍 [AdvancedReportsService] GetTopCustomersAsync() - Iniciando...");
                
                var startDate = filters.StartDate ?? DateTime.Today.AddDays(-90);
                var endDate = filters.EndDate ?? DateTime.Today;
                
                var userIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                Guid? companyId = null;
                
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    var user = await _context.Users
                        .Include(u => u.Branch)
                        .FirstOrDefaultAsync(u => u.Id == userId);
                    
                    companyId = user?.Branch?.CompanyId;
                }
                
                var ordersQuery = _context.Orders
                    .Include(o => o.Customer)
                    .Where(o => o.Status == OrderStatus.Completed &&
                               o.CustomerId.HasValue &&
                               o.ClosedAt >= startDate && o.ClosedAt <= endDate);
                
                if (companyId.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.CompanyId == companyId.Value);
                }
                
                var customerStats = await ordersQuery
                    .GroupBy(o => o.CustomerId!.Value)
                    .Select(g => new
                    {
                        CustomerId = g.Key,
                        Customer = g.First().Customer,
                        OrdersCount = g.Count(),
                        TotalSpent = g.Sum(o => o.TotalAmount ?? 0),
                        FirstOrderDate = g.Min(o => o.ClosedAt ?? o.CreatedAt),
                        LastOrderDate = g.Max(o => o.ClosedAt ?? o.CreatedAt)
                    })
                    .OrderByDescending(c => c.TotalSpent)
                    .Take(filters.TopN ?? 10)
                    .ToListAsync();
                
                var topCustomers = customerStats.Select(c => new TopCustomer
                {
                    CustomerId = c.CustomerId,
                    CustomerName = c.Customer?.FullName ?? "Sin nombre",
                    Email = c.Customer?.Email ?? "",
                    Phone = c.Customer?.Phone ?? "",
                    OrdersCount = c.OrdersCount,
                    TotalSpent = c.TotalSpent,
                    AverageOrderValue = c.OrdersCount > 0 ? c.TotalSpent / c.OrdersCount : 0,
                    FirstOrderDate = c.FirstOrderDate,
                    LastOrderDate = c.LastOrderDate,
                    DaysSinceLastOrder = (DateTime.UtcNow - c.LastOrderDate).Days
                }).ToList();
                
                Console.WriteLine($"✅ [AdvancedReportsService] GetTopCustomersAsync() - {topCustomers.Count} clientes principales");
                return topCustomers;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AdvancedReportsService] GetTopCustomersAsync() - Error: {ex.Message}");
                _logger.LogError(ex, "[AdvancedReportsService] Error obteniendo clientes principales");
                return new List<TopCustomer>();
            }
        }

        public async Task<List<CustomerSegment>> GetCustomerSegmentsAsync(ReportFilters filters)
        {
            try
            {
                Console.WriteLine("🔍 [AdvancedReportsService] GetCustomerSegmentsAsync() - Iniciando...");
                
                var startDate = filters.StartDate ?? DateTime.Today.AddDays(-90);
                var endDate = filters.EndDate ?? DateTime.Today;
                
                var userIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                Guid? companyId = null;
                
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    var user = await _context.Users
                        .Include(u => u.Branch)
                        .FirstOrDefaultAsync(u => u.Id == userId);
                    
                    companyId = user?.Branch?.CompanyId;
                }
                
                var ordersQuery = _context.Orders
                    .Include(o => o.Customer)
                    .Where(o => o.Status == OrderStatus.Completed &&
                               o.CustomerId.HasValue &&
                               o.ClosedAt >= startDate && o.ClosedAt <= endDate);
                
                if (companyId.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.CompanyId == companyId.Value);
                }
                
                var customerStats = await ordersQuery
                    .GroupBy(o => o.CustomerId!.Value)
                    .Select(g => new
                    {
                        CustomerId = g.Key,
                        TotalSpent = g.Sum(o => o.TotalAmount ?? 0),
                        OrdersCount = g.Count()
                    })
                    .ToListAsync();
                
                var totalRevenue = customerStats.Sum(c => c.TotalSpent);
                var averageOrderValue = customerStats.Count > 0 ? totalRevenue / customerStats.Count : 0;
                
                var segments = new List<CustomerSegment>();
                
                // Alto valor (> 2x promedio)
                var highValue = customerStats.Where(c => c.TotalSpent >= averageOrderValue * 2).ToList();
                if (highValue.Any())
                {
                    segments.Add(new CustomerSegment
                    {
                        SegmentName = "Alto Valor",
                        Description = "Clientes con gasto superior al doble del promedio",
                        CustomersCount = highValue.Count,
                        TotalRevenue = highValue.Sum(c => c.TotalSpent),
                        AverageOrderValue = highValue.Sum(c => c.TotalSpent) / highValue.Sum(c => c.OrdersCount),
                        CustomerLifetimeValue = highValue.Average(c => c.TotalSpent),
                        RetentionRate = 85.0m
                    });
                }
                
                // Valor medio (entre promedio y 2x promedio)
                var mediumValue = customerStats.Where(c => c.TotalSpent >= averageOrderValue && c.TotalSpent < averageOrderValue * 2).ToList();
                if (mediumValue.Any())
                {
                    segments.Add(new CustomerSegment
                    {
                        SegmentName = "Valor Medio",
                        Description = "Clientes con gasto promedio",
                        CustomersCount = mediumValue.Count,
                        TotalRevenue = mediumValue.Sum(c => c.TotalSpent),
                        AverageOrderValue = mediumValue.Sum(c => c.TotalSpent) / mediumValue.Sum(c => c.OrdersCount),
                        CustomerLifetimeValue = mediumValue.Average(c => c.TotalSpent),
                        RetentionRate = 60.0m
                    });
                }
                
                // Bajo valor (< promedio)
                var lowValue = customerStats.Where(c => c.TotalSpent < averageOrderValue).ToList();
                if (lowValue.Any())
                {
                    segments.Add(new CustomerSegment
                    {
                        SegmentName = "Bajo Valor",
                        Description = "Clientes con gasto inferior al promedio",
                        CustomersCount = lowValue.Count,
                        TotalRevenue = lowValue.Sum(c => c.TotalSpent),
                        AverageOrderValue = lowValue.Sum(c => c.TotalSpent) / lowValue.Sum(c => c.OrdersCount),
                        CustomerLifetimeValue = lowValue.Average(c => c.TotalSpent),
                        RetentionRate = 30.0m
                    });
                }
                
                Console.WriteLine($"✅ [AdvancedReportsService] GetCustomerSegmentsAsync() - {segments.Count} segmentos");
                return segments;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AdvancedReportsService] GetCustomerSegmentsAsync() - Error: {ex.Message}");
                _logger.LogError(ex, "[AdvancedReportsService] Error obteniendo segmentos de clientes");
                return new List<CustomerSegment>();
            }
        }

        // ===== ANÁLISIS DE OPERACIONES =====
        public async Task<OperationalAnalysisReport> GetOperationalAnalysisAsync(ReportFilters filters)
        {
            try
            {
                Console.WriteLine("🔍 [AdvancedReportsService] GetOperationalAnalysisAsync() - Iniciando...");
                
                var startDate = filters.StartDate ?? DateTime.Today.AddDays(-30);
                var endDate = filters.EndDate ?? DateTime.Today;
                
                var userIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                Guid? companyId = null;
                
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    var user = await _context.Users
                        .Include(u => u.Branch)
                        .FirstOrDefaultAsync(u => u.Id == userId);
                    
                    companyId = user?.Branch?.CompanyId;
                }
                
                var ordersQuery = _context.Orders
                    .Include(o => o.OrderItems)
                    .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate);
                
                if (companyId.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.CompanyId == companyId.Value);
                }
                
                var orders = await ordersQuery.ToListAsync();
                
                var totalOrders = orders.Count;
                var completedOrders = orders.Count(o => o.Status == OrderStatus.Completed);
                var cancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled);
                
                var completedOrdersWithTime = orders
                    .Where(o => o.Status == OrderStatus.Completed && o.OpenedAt.HasValue && o.ClosedAt.HasValue)
                    .ToList();
                
                var averageOrderTime = completedOrdersWithTime.Any() 
                    ? completedOrdersWithTime.Average(o => (o.ClosedAt!.Value - o.OpenedAt!.Value).TotalMinutes)
                    : 0;

                var prepSamples = orders
                    .SelectMany(o => o.OrderItems)
                    .Where(oi => oi.SentAt.HasValue && oi.PreparedAt.HasValue && oi.PreparedAt >= oi.SentAt)
                    .Select(oi => (oi.PreparedAt!.Value - oi.SentAt!.Value).TotalMinutes)
                    .ToList();

                var averagePrepTime = prepSamples.Count > 0 ? prepSamples.Average() : 0;
                
                var report = new OperationalAnalysisReport
                {
                    TotalOrders = totalOrders,
                    CompletedOrders = completedOrders,
                    CancelledOrders = cancelledOrders,
                    AverageOrderTime = (decimal)averageOrderTime,
                    AveragePreparationTime = (decimal)averagePrepTime,
                    StationPerformance = await GetStationPerformanceAsync(filters),
                    TableUtilization = await GetTableUtilizationAsync(filters),
                    PeakHours = new List<PeakHours>()
                };
                
                Console.WriteLine($"✅ [AdvancedReportsService] GetOperationalAnalysisAsync() - {totalOrders} órdenes analizadas");
                return report;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AdvancedReportsService] GetOperationalAnalysisAsync() - Error: {ex.Message}");
                _logger.LogError(ex, "[AdvancedReportsService] Error generando análisis operacional");
                return new OperationalAnalysisReport();
            }
        }

        public async Task<List<StationPerformance>> GetStationPerformanceAsync(ReportFilters filters)
        {
            try
            {
                Console.WriteLine("🔍 [AdvancedReportsService] GetStationPerformanceAsync() - Iniciando...");
                
                var startDate = filters.StartDate ?? DateTime.Today.AddDays(-30);
                var endDate = filters.EndDate ?? DateTime.Today;
                
                var userIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                Guid? companyId = null;
                
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    var user = await _context.Users
                        .Include(u => u.Branch)
                        .FirstOrDefaultAsync(u => u.Id == userId);
                    
                    companyId = user?.Branch?.CompanyId;
                }
                
                var stationsQuery = _context.Stations.AsQueryable();
                if (companyId.HasValue)
                {
                    stationsQuery = stationsQuery.Where(s => s.CompanyId == companyId.Value);
                }
                
                var stations = await stationsQuery.ToListAsync();
                
                var orderItemsQuery = _context.OrderItems
                    .Include(oi => oi.Order)
                    .Include(oi => oi.Product)
                    .Where(oi => oi.Order.ClosedAt >= startDate && oi.Order.ClosedAt <= endDate &&
                               oi.Order.Status == OrderStatus.Completed &&
                               oi.PreparedByStationId.HasValue);
                
                if (companyId.HasValue)
                {
                    orderItemsQuery = orderItemsQuery.Where(oi => oi.Order.CompanyId == companyId.Value);
                }
                
                var orderItems = await orderItemsQuery
                    .Select(oi => new
                    {
                        StationId = oi.PreparedByStationId!.Value,
                        oi.OrderId,
                        oi.Quantity,
                        oi.UnitPrice,
                        oi.SentAt,
                        oi.PreparedAt
                    })
                    .ToListAsync();

                var stationStats = orderItems
                    .GroupBy(oi => oi.StationId)
                    .Select(g => new
                    {
                        StationId = g.Key,
                        ItemsProcessed = g.Count(),
                        OrdersProcessed = g.Select(oi => oi.OrderId).Distinct().Count(),
                        Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice),
                        AvgPrep = g
                            .Where(oi => oi.SentAt.HasValue && oi.PreparedAt.HasValue && oi.PreparedAt >= oi.SentAt)
                            .Select(oi => (oi.PreparedAt!.Value - oi.SentAt!.Value).TotalMinutes)
                            .DefaultIfEmpty()
                            .Average()
                    })
                    .ToList();
                
                var performanceList = new List<StationPerformance>();
                
                foreach (var station in stations)
                {
                    var stats = stationStats.FirstOrDefault(s => s.StationId == station.Id);
                    
                    if (stats != null)
                    {
                        var hasPrep = orderItems.Any(oi =>
                            oi.StationId == station.Id &&
                            oi.SentAt.HasValue && oi.PreparedAt.HasValue && oi.PreparedAt >= oi.SentAt);
                        performanceList.Add(new StationPerformance
                        {
                            StationId = station.Id,
                            StationName = station.Name,
                            OrdersProcessed = stats.OrdersProcessed,
                            ItemsProcessed = stats.ItemsProcessed,
                            AveragePreparationTime = hasPrep ? (decimal)stats.AvgPrep : 0,
                            Efficiency = stats.OrdersProcessed > 0 ? (stats.ItemsProcessed / (decimal)stats.OrdersProcessed) : 0,
                            PeakHour = 0,
                            Revenue = stats.Revenue
                        });
                    }
                    else
                    {
                        performanceList.Add(new StationPerformance
                        {
                            StationId = station.Id,
                            StationName = station.Name,
                            OrdersProcessed = 0,
                            ItemsProcessed = 0,
                            AveragePreparationTime = 0,
                            Efficiency = 0,
                            PeakHour = 0,
                            Revenue = 0
                        });
                    }
                }
                
                Console.WriteLine($"✅ [AdvancedReportsService] GetStationPerformanceAsync() - {performanceList.Count} estaciones");
                return performanceList.OrderByDescending(p => p.OrdersProcessed).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AdvancedReportsService] GetStationPerformanceAsync() - Error: {ex.Message}");
                _logger.LogError(ex, "[AdvancedReportsService] Error obteniendo rendimiento de estaciones");
                return new List<StationPerformance>();
            }
        }

        public async Task<List<TableUtilization>> GetTableUtilizationAsync(ReportFilters filters)
        {
            try
            {
                Console.WriteLine("🔍 [AdvancedReportsService] GetTableUtilizationAsync() - Iniciando...");
                
                var startDate = filters.StartDate ?? DateTime.Today.AddDays(-30);
                var endDate = filters.EndDate ?? DateTime.Today;
                
                var userIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                Guid? companyId = null;
                Guid? branchId = null;
                
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    var user = await _context.Users
                        .Include(u => u.Branch)
                        .FirstOrDefaultAsync(u => u.Id == userId);
                    
                    companyId = user?.Branch?.CompanyId;
                    branchId = user?.BranchId;
                }
                
                var tablesQuery = _context.Tables.AsQueryable();
                
                if (companyId.HasValue)
                {
                    tablesQuery = tablesQuery.Where(t => t.CompanyId == companyId.Value);
                }
                
                if (branchId.HasValue)
                {
                    tablesQuery = tablesQuery.Where(t => t.BranchId == branchId.Value);
                }
                
                var tables = await tablesQuery.ToListAsync();
                
                var ordersQuery = _context.Orders
                    .Include(o => o.OrderItems)
                    .Where(o => o.TableId.HasValue &&
                               o.ClosedAt >= startDate && o.ClosedAt <= endDate &&
                               o.Status == OrderStatus.Completed);
                
                if (companyId.HasValue)
                {
                    ordersQuery = ordersQuery.Where(o => o.CompanyId == companyId.Value);
                }
                
                var tableStats = await ordersQuery
                    .GroupBy(o => o.TableId!.Value)
                    .Select(g => new
                    {
                        TableId = g.Key,
                        OrdersCount = g.Count(),
                        TotalRevenue = g.Sum(o => o.TotalAmount ?? 0),
                        AverageOrderValue = g.Average(o => o.TotalAmount ?? 0)
                    })
                    .ToListAsync();
                
                var utilizationList = tables.Select(table =>
                {
                    var stats = tableStats.FirstOrDefault(s => s.TableId == table.Id);
                    
                    var daysInPeriod = (endDate - startDate).Days;
                    var maxPossibleOrders = daysInPeriod * 8; // Asumiendo 8 turnos por día
                    var utilizationRate = maxPossibleOrders > 0 
                        ? (stats?.OrdersCount ?? 0) / (decimal)maxPossibleOrders * 100 
                        : 0;
                    
                    return new TableUtilization
                    {
                        TableId = table.Id,
                        TableName = table.TableNumber,
                        Capacity = table.Capacity,
                        OrdersCount = stats?.OrdersCount ?? 0,
                        UtilizationRate = utilizationRate,
                        AverageOrderValue = stats?.AverageOrderValue ?? 0,
                        TotalRevenue = stats?.TotalRevenue ?? 0,
                        AverageGuests = table.Capacity // Aproximación
                    };
                }).ToList();
                
                Console.WriteLine($"✅ [AdvancedReportsService] GetTableUtilizationAsync() - {utilizationList.Count} mesas");
                return utilizationList.OrderByDescending(t => t.UtilizationRate).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AdvancedReportsService] GetTableUtilizationAsync() - Error: {ex.Message}");
                _logger.LogError(ex, "[AdvancedReportsService] Error obteniendo utilización de mesas");
                return new List<TableUtilization>();
            }
        }

        // ===== ANÁLISIS DE INVENTARIO =====
        public async Task<InventoryAnalysisReport> GetInventoryAnalysisAsync(ReportFilters filters)
        {
            try
            {
                Console.WriteLine("🔍 [AdvancedReportsService] GetInventoryAnalysisAsync() - Iniciando...");
                
                var userIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                Guid? companyId = null;
                Guid? branchId = null;
                
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    var user = await _context.Users
                        .Include(u => u.Branch)
                        .FirstOrDefaultAsync(u => u.Id == userId);
                    
                    companyId = user?.Branch?.CompanyId;
                    branchId = user?.BranchId;
                }
                
                var productsQuery = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Branch)
                    .Include(p => p.StockAssignments)
                    .AsQueryable();
                
                if (companyId.HasValue)
                {
                    productsQuery = productsQuery.Where(p => p.CompanyId == companyId.Value);
                }
                
                if (branchId.HasValue)
                {
                    productsQuery = productsQuery.Where(p => p.BranchId == branchId.Value);
                }
                
                var products = await productsQuery.ToListAsync();
                
                var totalProducts = products.Count;
                var lowStockProducts = products.Count(p => p.TrackInventory && p.Stock < p.MinStock && p.Stock > 0);
                var outOfStockProducts = products.Count(p => p.TrackInventory && p.Stock <= 0);
                var totalInventoryValue = products
                    .Where(p => p.TrackInventory)
                    .Sum(p => (p.Stock ?? 0) * (p.Cost ?? 0));
                
                var lowStockAlerts = products
                    .Where(p => p.TrackInventory && p.Stock <= p.MinStock)
                    .Select(p => new LowStockAlert
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        CategoryName = p.Category?.Name ?? "",
                        BranchName = p.Branch?.Name ?? "",
                        CurrentStock = p.Stock ?? 0,
                        MinStock = p.MinStock ?? 0,
                        ReorderPoint = p.MinStock ?? 0,
                        AlertLevel = (p.Stock ?? 0) <= 0 ? "Critical" : (p.Stock ?? 0) < (p.MinStock ?? 0) * 0.5m ? "Warning" : "Normal",
                        LastUpdated = p.UpdatedAt
                    })
                    .ToList();
                
                // Rotación: una query agrupada (elimina N+1 SumAsync por producto)
                var startDate = filters.StartDate ?? DateTime.Today.AddDays(-30);
                var endDate = filters.EndDate ?? DateTime.Today;
                var tracked = products.Where(p => p.TrackInventory).ToList();
                var trackedIds = tracked.Select(p => p.Id).ToList();
                var soldByProduct = trackedIds.Count == 0
                    ? new Dictionary<Guid, decimal>()
                    : await _context.OrderItems
                        .AsNoTracking()
                        .Where(oi => oi.ProductId != null
                                   && trackedIds.Contains(oi.ProductId.Value)
                                   && oi.Order != null
                                   && oi.Order.Status == OrderStatus.Completed
                                   && oi.Order.ClosedAt >= startDate
                                   && oi.Order.ClosedAt <= endDate)
                        .GroupBy(oi => oi.ProductId!.Value)
                        .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
                        .ToDictionaryAsync(x => x.ProductId, x => x.Qty);

                var turnoverData = new List<InventoryTurnover>();
                foreach (var product in tracked)
                {
                    var soldQuantity = soldByProduct.TryGetValue(product.Id, out var sq) ? sq : 0m;
                    var averageStock = product.Stock ?? 0;
                    var turnoverRate = averageStock > 0 ? soldQuantity / averageStock : 0;
                    var daysToSell = turnoverRate > 0 ? (int)(30 / turnoverRate) : 999;
                    var efficiency = turnoverRate >= 2 ? "High" : turnoverRate >= 1 ? "Medium" : "Low";

                    turnoverData.Add(new InventoryTurnover
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        CategoryName = product.Category?.Name ?? "",
                        AverageStock = averageStock,
                        TotalSold = soldQuantity,
                        TurnoverRate = turnoverRate,
                        DaysToSell = daysToSell,
                        Efficiency = efficiency
                    });
                }
                
                var report = new InventoryAnalysisReport
                {
                    TotalProducts = totalProducts,
                    LowStockProducts = lowStockProducts,
                    OutOfStockProducts = outOfStockProducts,
                    TotalInventoryValue = totalInventoryValue,
                    LowStockAlerts = lowStockAlerts,
                    TurnoverData = turnoverData,
                    ValueReport = products
                        .Where(p => p.TrackInventory)
                        .Select(p => new InventoryValueReport
                        {
                            ProductId = p.Id,
                            ProductName = p.Name,
                            CategoryName = p.Category?.Name ?? "",
                            CurrentStock = p.Stock ?? 0,
                            UnitCost = p.Cost ?? 0,
                            TotalValue = (p.Stock ?? 0) * (p.Cost ?? 0),
                            LastMonthValue = 0,
                            ValueChange = 0,
                            BranchName = p.Branch?.Name ?? ""
                        })
                        .ToList()
                };
                
                Console.WriteLine($"✅ [AdvancedReportsService] GetInventoryAnalysisAsync() - Reporte generado: {totalProducts} productos");
                return report;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AdvancedReportsService] GetInventoryAnalysisAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [AdvancedReportsService] GetInventoryAnalysisAsync() - StackTrace: {ex.StackTrace}");
                _logger.LogError(ex, "[AdvancedReportsService] Error generando análisis de inventario");
                return new InventoryAnalysisReport();
            }
        }

        // ===== ANÁLISIS DE PROVEEDORES =====
        public async Task<SupplierAnalysisReport> GetSupplierAnalysisAsync(ReportFilters filters)
        {
            try
            {
                Console.WriteLine("🔍 [AdvancedReportsService] GetSupplierAnalysisAsync() - Iniciando...");
                
                // Implementación básica - el sistema no tiene proveedores aún
                var report = new SupplierAnalysisReport
                {
                    TotalSuppliers = 0,
                    ActiveSuppliers = 0,
                    TotalPurchases = 0,
                    AverageDeliveryTime = 0,
                    SupplierPerformance = new List<SupplierPerformance>(),
                    PurchaseOrders = new List<PurchaseOrderAnalysis>(),
                    DeliveryTimes = new List<DeliveryTimeAnalysis>()
                };
                
                Console.WriteLine("✅ [AdvancedReportsService] GetSupplierAnalysisAsync() - Reporte generado");
                return report;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AdvancedReportsService] GetSupplierAnalysisAsync() - Error: {ex.Message}");
                _logger.LogError(ex, "[AdvancedReportsService] Error generando análisis de proveedores");
                return new SupplierAnalysisReport();
            }
        }

        // ===== ANÁLISIS DE TENDENCIAS =====
        public async Task<TrendAnalysisReport> GetTrendAnalysisAsync(ReportFilters filters)
        {
            try
            {
                Console.WriteLine("🔍 [AdvancedReportsService] GetTrendAnalysisAsync() - Iniciando...");
                
                var startDate = filters.StartDate ?? DateTime.Today.AddDays(-30);
                var endDate = filters.EndDate ?? DateTime.Today;
                
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Category)
                    .Where(o => o.ClosedAt >= startDate && o.ClosedAt <= endDate && o.Status == OrderStatus.Completed)
                    .ToListAsync();
                
                var salesTrends = orders
                    .GroupBy(o => o.ClosedAt!.Value.Date)
                    .Select(g => new SalesTrend
                    {
                        Date = g.Key,
                        Revenue = g.Sum(o => o.TotalAmount ?? 0),
                        OrdersCount = g.Count(),
                        GrowthRate = 0,
                        TrendDirection = "Stable"
                    })
                    .OrderBy(s => s.Date)
                    .ToList();
                
                var report = new TrendAnalysisReport
                {
                    SalesTrends = salesTrends,
                    ProductTrends = new List<ProductTrend>(),
                    CategoryTrends = new List<CategoryTrend>(),
                    SeasonalPatterns = new List<SeasonalPattern>(),
                    GrowthForecasts = new List<GrowthForecast>()
                };
                
                Console.WriteLine($"✅ [AdvancedReportsService] GetTrendAnalysisAsync() - Reporte generado: {salesTrends.Count} tendencias");
                return report;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AdvancedReportsService] GetTrendAnalysisAsync() - Error: {ex.Message}");
                _logger.LogError(ex, "[AdvancedReportsService] Error generando análisis de tendencias");
                return new TrendAnalysisReport();
            }
        }

        // ===== REPORTE DE AUDITORÍA =====
        public async Task<AuditReportViewModel> GetAuditReportAsync(ReportFilters filters)
        {
            try
            {
                Console.WriteLine("🔍 [AdvancedReportsService] GetAuditReportAsync() - Iniciando...");
                
                var startDate = filters.StartDate ?? DateTime.Today.AddDays(-30);
                var endDate = filters.EndDate ?? DateTime.Today;
                
                var logsQuery = _context.AuditLogs
                    .Include(a => a.User)
                    .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
                    .AsQueryable();
                
                var userIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    var user = await _context.Users
                        .Include(u => u.Branch)
                        .FirstOrDefaultAsync(u => u.Id == userId);
                    
                    if (user?.Branch != null)
                    {
                        logsQuery = logsQuery.Where(a => a.CompanyId == user.Branch.CompanyId);
                    }
                }
                
                var logs = await logsQuery
                    .OrderByDescending(a => a.Timestamp)
                    .Take(100)
                    .ToListAsync();
                
                var report = new AuditReportViewModel
                {
                    TotalLogs = await logsQuery.CountAsync(),
                    ErrorLogs = await logsQuery.CountAsync(l => l.LogLevel == "Error"),
                    WarningLogs = await logsQuery.CountAsync(l => l.LogLevel == "Warning"),
                    InfoLogs = await logsQuery.CountAsync(l => l.LogLevel == "Info"),
                    RecentLogs = logs.Select(l => new AuditLogEntry
                    {
                        Id = l.Id,
                        Action = l.Action,
                        Module = l.Module ?? "",
                        UserEmail = l.User?.Email ?? "",
                        Timestamp = l.Timestamp ?? DateTime.UtcNow,
                        LogLevel = l.LogLevel.ToString(),
                        Description = l.Description ?? "",
                        IpAddress = l.IpAddress ?? ""
                    }).ToList(),
                    UserActivities = new List<UserActivity>(),
                    SystemEvents = new List<SystemEvent>(),
                    SecurityEvents = logs
                        .Where(l => l.LogLevel == "Error" && (l.Module == "Security" || l.Action.Contains("Login")))
                        .Select(l => new SecurityEvent
                        {
                            Id = l.Id,
                            EventType = l.Action,
                            UserEmail = l.User?.Email ?? "",
                            IpAddress = l.IpAddress ?? "",
                            Timestamp = l.Timestamp ?? DateTime.UtcNow,
                            Description = l.Description ?? "",
                            Severity = l.LogLevel == "Error" ? "High" : "Medium"
                        })
                        .ToList()
                };
                
                Console.WriteLine($"✅ [AdvancedReportsService] GetAuditReportAsync() - Reporte generado: {report.TotalLogs} logs");
                return report;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AdvancedReportsService] GetAuditReportAsync() - Error: {ex.Message}");
                _logger.LogError(ex, "[AdvancedReportsService] Error generando reporte de auditoría");
                return new AuditReportViewModel();
            }
        }

        // ===== SALUD DEL SISTEMA =====
        public async Task<SystemHealthReport> GetSystemHealthAsync()
        {
            try
            {
                Console.WriteLine("🔍 [AdvancedReportsService] GetSystemHealthAsync() - Iniciando...");
                
                var totalUsers = await _context.Users.CountAsync();
                var activeUsers = await _context.Users
                    .CountAsync(u => u.IsActive);
                var totalOrders = await _context.Orders.CountAsync();
                var pendingOrders = await _context.Orders
                    .CountAsync(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.SentToKitchen);
                
                var report = new SystemHealthReport
                {
                    OverallStatus = "Healthy",
                    SystemUptime = 99.9m,
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUsers,
                    TotalOrders = totalOrders,
                    PendingOrders = pendingOrders,
                    DatabaseSize = 0,
                    AverageResponseTime = 0,
                    Metrics = new List<SystemMetric>
                    {
                        new SystemMetric
                        {
                            MetricName = "Users",
                            Value = totalUsers,
                            Threshold = 1000,
                            Status = totalUsers < 1000 ? "Normal" : "Warning",
                            LastUpdated = DateTime.UtcNow
                        },
                        new SystemMetric
                        {
                            MetricName = "Pending Orders",
                            Value = pendingOrders,
                            Threshold = 50,
                            Status = pendingOrders < 50 ? "Normal" : "Warning",
                            LastUpdated = DateTime.UtcNow
                        }
                    },
                    Alerts = new List<SystemAlert>()
                };
                
                Console.WriteLine($"✅ [AdvancedReportsService] GetSystemHealthAsync() - Reporte generado");
                return report;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AdvancedReportsService] GetSystemHealthAsync() - Error: {ex.Message}");
                _logger.LogError(ex, "[AdvancedReportsService] Error generando salud del sistema");
                return new SystemHealthReport();
            }
        }

        // ===== EXPORTACIÓN =====
        public async Task<byte[]> ExportAdvancedReportToPdfAsync(string reportType, ReportFilters filters)
        {
            try
            {
                var payload = await LoadExportPayloadAsync(reportType, filters);
                return Encoding.UTF8.GetBytes(BuildPrintableHtml(reportType, filters, payload));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error exportando reporte a PDF/HTML");
                return Encoding.UTF8.GetBytes("<html><body><p>Error generando reporte</p></body></html>");
            }
        }

        public async Task<byte[]> ExportAdvancedReportToExcelAsync(string reportType, ReportFilters filters)
        {
            try
            {
                var payload = await LoadExportPayloadAsync(reportType, filters);
                using var wb = new XLWorkbook();
                var summary = wb.Worksheets.Add("Resumen");
                summary.Cell(1, 1).Value = $"Advanced Report: {reportType}";
                summary.Cell(2, 1).Value = "Periodo";
                summary.Cell(2, 2).Value = $"{filters.StartDate:u} — {filters.EndDate:u}";
                summary.Cell(3, 1).Value = "BranchId";
                summary.Cell(3, 2).Value = filters.BranchId?.ToString() ?? "(all)";
                summary.Cell(4, 1).Value = "Generado UTC";
                summary.Cell(4, 2).Value = DateTime.UtcNow;
                WriteObjectSheets(wb, payload);
                using var ms = new MemoryStream();
                wb.SaveAs(ms);
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error exportando reporte a Excel");
                using var wb = new XLWorkbook();
                wb.Worksheets.Add("Error").Cell(1, 1).Value = ex.Message;
                using var ms = new MemoryStream();
                wb.SaveAs(ms);
                return ms.ToArray();
            }
        }

        private async Task<object> LoadExportPayloadAsync(string reportType, ReportFilters filters)
        {
            var key = (reportType ?? "").Trim().ToLowerInvariant();
            return key switch
            {
                "profitability" or "productprofitability" => await GetProfitabilityAnalysisAsync(filters),
                "sales" or "topsellingproducts" => await GetSalesAnalysisAsync(filters),
                "customers" or "topcustomers" => await GetCustomerAnalysisAsync(filters),
                "operational" or "stationperformance" => await GetOperationalAnalysisAsync(filters),
                "inventory" or "inventoryanalysis" => await GetInventoryAnalysisAsync(filters),
                "suppliers" or "supplieranalysis" => await GetSupplierAnalysisAsync(filters),
                "trends" or "trendanalysis" => await GetTrendAnalysisAsync(filters),
                "audit" or "auditreport" => await GetAuditReportAsync(filters),
                "health" or "systemhealth" => await GetSystemHealthAsync(),
                _ => throw new ArgumentException($"Tipo de reporte no soportado: {reportType}")
            };
        }

        private static void WriteObjectSheets(XLWorkbook wb, object payload)
        {
            foreach (var prop in payload.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var value = prop.GetValue(payload);
                if (value is null) continue;
                if (value is IEnumerable enumerable and not string)
                {
                    var rows = enumerable.Cast<object?>().Where(x => x is not null).Cast<object>().ToList();
                    if (rows.Count == 0) continue;
                    var sheetName = SanitizeSheetName(prop.Name);
                    var ws = wb.Worksheets.Add(sheetName);
                    var cols = rows[0].GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    for (var c = 0; c < cols.Length; c++)
                        ws.Cell(1, c + 1).Value = cols[c].Name;
                    for (var r = 0; r < rows.Count; r++)
                    {
                        for (var c = 0; c < cols.Length; c++)
                        {
                            var cellVal = cols[c].GetValue(rows[r]);
                            SetCell(ws.Cell(r + 2, c + 1), cellVal);
                        }
                    }
                    ws.SheetView.FreezeRows(1);
                    ws.Columns().AdjustToContents();
                }
                else if (!IsComplex(value))
                {
                    var summary = wb.Worksheet(1);
                    var next = summary.LastRowUsed()?.RowNumber() + 1 ?? 6;
                    summary.Cell(next, 1).Value = prop.Name;
                    SetCell(summary.Cell(next, 2), value);
                }
            }
        }

        private static bool IsComplex(object value)
            => value is not (string or decimal or double or float or int or long or short or byte or bool or Guid or DateTime or Enum)
               && value.GetType() is { IsPrimitive: false };

        private static void SetCell(IXLCell cell, object? value)
        {
            switch (value)
            {
                case null: cell.Value = ""; break;
                case DateTime dt: cell.Value = dt; break;
                case decimal d: cell.Value = d; break;
                case double dbl: cell.Value = dbl; break;
                case float f: cell.Value = f; break;
                case int i: cell.Value = i; break;
                case long l: cell.Value = l; break;
                case bool b: cell.Value = b; break;
                case Guid g: cell.Value = g.ToString(); break;
                default: cell.Value = Convert.ToString(value) ?? ""; break;
            }
        }

        private static string SanitizeSheetName(string name)
        {
            var clean = new string(name.Where(ch => char.IsLetterOrDigit(ch) || ch is '_' or '-').ToArray());
            if (string.IsNullOrWhiteSpace(clean)) clean = "Datos";
            return clean.Length <= 31 ? clean : clean[..31];
        }

        private static string BuildPrintableHtml(string reportType, ReportFilters filters, object payload)
        {
            var sb = new StringBuilder();
            sb.Append("<!doctype html><html><head><meta charset='utf-8'><title>")
              .Append(System.Net.WebUtility.HtmlEncode(reportType))
              .Append("</title><style>body{font-family:Segoe UI,Arial,sans-serif;font-size:12px;margin:24px}table{border-collapse:collapse;width:100%;margin-bottom:16px}th,td{border:1px solid #ccc;padding:4px}th{background:#eee}@media print{.no-print{display:none}}</style></head><body>");
            sb.Append("<h1>").Append(System.Net.WebUtility.HtmlEncode(reportType)).Append("</h1>");
            sb.Append("<p>Periodo ").Append(filters.StartDate).Append(" — ").Append(filters.EndDate)
              .Append("<br>Branch ").Append(filters.BranchId).Append("<br>UTC ").Append(DateTime.UtcNow.ToString("u")).Append("</p>");
            sb.Append("<p class='no-print'><button onclick='window.print()'>Imprimir / Guardar como PDF</button></p>");

            foreach (var prop in payload.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var value = prop.GetValue(payload);
                if (value is null) continue;
                if (value is IEnumerable enumerable and not string)
                {
                    var rows = enumerable.Cast<object?>().Where(x => x is not null).Cast<object>().ToList();
                    if (rows.Count == 0) continue;
                    var cols = rows[0].GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    sb.Append("<h2>").Append(System.Net.WebUtility.HtmlEncode(prop.Name)).Append("</h2><table><thead><tr>");
                    foreach (var c in cols) sb.Append("<th>").Append(System.Net.WebUtility.HtmlEncode(c.Name)).Append("</th>");
                    sb.Append("</tr></thead><tbody>");
                    foreach (var row in rows)
                    {
                        sb.Append("<tr>");
                        foreach (var c in cols)
                            sb.Append("<td>").Append(System.Net.WebUtility.HtmlEncode(Convert.ToString(c.GetValue(row)) ?? "")).Append("</td>");
                        sb.Append("</tr>");
                    }
                    sb.Append("</tbody></table>");
                }
                else if (!IsComplex(value))
                {
                    sb.Append("<p><strong>").Append(System.Net.WebUtility.HtmlEncode(prop.Name)).Append(":</strong> ")
                      .Append(System.Net.WebUtility.HtmlEncode(Convert.ToString(value) ?? "")).Append("</p>");
                }
            }

            sb.Append("</body></html>");
            return sb.ToString();
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