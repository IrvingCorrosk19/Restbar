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

        // ===== ANÁLISIS DE INVENTARIO =====









        

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
                var operatingExpenses = 1000m; // Valor simulado
                var netProfit = grossProfit - operatingExpenses;

                var report = new ProfitabilityAnalysis
                {
                    TotalRevenue = totalRevenue,
                    TotalCost = totalCost,
                    GrossProfit = grossProfit,
                    NetProfit = netProfit,
                    ProfitMargin = totalRevenue > 0 ? (netProfit / totalRevenue) * 100 : 0,
                    CostOfGoodsSold = totalCost,
                    OperatingExpenses = operatingExpenses,
                    ProductProfitability = await GetProductProfitabilityAsync(filters),
                    CategoryProfitability = await GetCategoryProfitabilityAsync(filters)
                };

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error generando análisis de rentabilidad");
                return new ProfitabilityAnalysis();
            }
        }

        public async Task<List<ProductProfitabilityData>> GetProductProfitabilityAsync(ReportFilters filters)
        {
            try
            {
                var profitabilityData = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsActive == true)
                    .Select(p => new ProductProfitabilityData
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
                        UnitsSold = _context.OrderItems
                            .Where(oi => oi.ProductId == p.Id && 
                                       oi.Order.Status == OrderStatus.Completed &&
                                       oi.Order.ClosedAt >= filters.StartDate && 
                                       oi.Order.ClosedAt <= filters.EndDate)
                            .Sum(oi => (int)oi.Quantity),
                        AveragePrice = 0,
                        AverageCost = p.Cost ?? 0,
                        Profit = 0,
                        ProfitMargin = 0,
                        ProfitabilityLevel = "Medium"
                    })
                    .ToListAsync();

                // Calcular métricas adicionales
                foreach (var product in profitabilityData)
                {
                    product.Profit = product.Revenue - product.Cost;
                    product.AveragePrice = product.UnitsSold > 0 ? product.Revenue / product.UnitsSold : 0;
                    product.ProfitMargin = product.Revenue > 0 ? (product.Profit / product.Revenue) * 100 : 0;
                    product.ProfitabilityLevel = product.ProfitMargin > 30 ? "High" : 
                                               product.ProfitMargin > 15 ? "Medium" : "Low";
                }

                return profitabilityData.OrderByDescending(x => x.Profit).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error obteniendo rentabilidad de productos");
                return new List<ProductProfitabilityData>();
            }
        }

        public async Task<List<CategoryProfitabilityData>> GetCategoryProfitabilityAsync(ReportFilters filters)
        {
            try
            {
                var categoryProfitability = await _context.Categories
                    .Include(c => c.Products)
                    .Select(c => new CategoryProfitabilityData
                    {
                        CategoryId = c.Id,
                        CategoryName = c.Name,
                        Revenue = c.Products.Sum(p => _context.OrderItems
                            .Where(oi => oi.ProductId == p.Id && 
                                       oi.Order.Status == OrderStatus.Completed &&
                                       oi.Order.ClosedAt >= filters.StartDate && 
                                       oi.Order.ClosedAt <= filters.EndDate)
                            .Sum(oi => oi.Quantity * oi.UnitPrice)),
                        Cost = c.Products.Sum(p => _context.OrderItems
                            .Where(oi => oi.ProductId == p.Id && 
                                       oi.Order.Status == OrderStatus.Completed &&
                                       oi.Order.ClosedAt >= filters.StartDate && 
                                       oi.Order.ClosedAt <= filters.EndDate)
                            .Sum(oi => oi.Quantity * (p.Cost ?? 0))),
                        ProductsCount = c.Products.Count(p => p.IsActive == true),
                        TotalUnitsSold = c.Products.Sum(p => _context.OrderItems
                            .Where(oi => oi.ProductId == p.Id && 
                                       oi.Order.Status == OrderStatus.Completed &&
                                       oi.Order.ClosedAt >= filters.StartDate && 
                                       oi.Order.ClosedAt <= filters.EndDate)
                            .Sum(oi => (int)oi.Quantity)),
                        Profit = 0,
                        ProfitMargin = 0,
                        PerformanceRating = "Good"
                    })
                    .ToListAsync();

                // Calcular métricas adicionales
                foreach (var category in categoryProfitability)
                {
                    category.Profit = category.Revenue - category.Cost;
                    category.ProfitMargin = category.Revenue > 0 ? (category.Profit / category.Revenue) * 100 : 0;
                    category.PerformanceRating = category.ProfitMargin > 25 ? "Excellent" : 
                                               category.ProfitMargin > 15 ? "Good" : 
                                               category.ProfitMargin > 5 ? "Average" : "Poor";
                }

                return categoryProfitability.OrderByDescending(x => x.Profit).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error obteniendo rentabilidad por categorías");
                return new List<CategoryProfitabilityData>();
            }
        }

        // ===== ANÁLISIS DE TENDENCIAS =====
        public async Task<TrendAnalysisReport> GetTrendAnalysisAsync(ReportFilters filters)
        {
            try
            {
                var report = new TrendAnalysisReport
                {
                    RevenueGrowth = 15.5m, // Valor simulado
                    OrderGrowth = 12.3m, // Valor simulado
                    CustomerGrowth = 8.7m, // Valor simulado
                    SeasonalTrends = await GetSeasonalTrendsAsync(filters),
                    GrowthAnalysis = await GetGrowthAnalysisAsync(filters)
                };

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error generando análisis de tendencias");
                return new TrendAnalysisReport();
            }
        }

        public async Task<List<SeasonalTrendData>> GetSeasonalTrendsAsync(ReportFilters filters)
        {
            try
            {
                var seasonalData = await _context.Orders
                    .Where(o => o.Status == OrderStatus.Completed && 
                               o.ClosedAt >= filters.StartDate && o.ClosedAt <= filters.EndDate)
                    .GroupBy(o => new { Month = o.ClosedAt.Value.Month, Year = o.ClosedAt.Value.Year })
                    .Select(g => new SeasonalTrendData
                    {
                        Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                        Revenue = g.Sum(o => o.TotalAmount ?? 0),
                        Orders = g.Count(),
                        AverageTicket = g.Average(o => o.TotalAmount ?? 0),
                        GrowthRate = 5.2m, // Valor simulado
                        Trend = "Up"
                    })
                    .OrderBy(x => x.Period)
                    .ToListAsync();

                return seasonalData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error obteniendo tendencias estacionales");
                return new List<SeasonalTrendData>();
            }
        }

        public async Task<List<GrowthAnalysisData>> GetGrowthAnalysisAsync(ReportFilters filters)
        {
            try
            {
                var growthData = await _context.Orders
                    .Where(o => o.Status == OrderStatus.Completed && 
                               o.ClosedAt >= filters.StartDate && o.ClosedAt <= filters.EndDate)
                    .GroupBy(o => o.ClosedAt.Value.Date)
                    .Select(g => new GrowthAnalysisData
                    {
                        Date = g.Key,
                        Revenue = g.Sum(o => o.TotalAmount ?? 0),
                        Orders = g.Count(),
                        GrowthRate = 3.5m, // Valor simulado
                        CumulativeGrowth = g.Sum(o => o.TotalAmount ?? 0),
                        GrowthTrend = "Stable"
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                return growthData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error obteniendo análisis de crecimiento");
                return new List<GrowthAnalysisData>();
            }
        }

        // ===== REPORTES DE AUDITORÍA =====
        public async Task<AuditReport> GetAuditReportAsync(ReportFilters filters)
        {
            try
            {
                var auditLogs = await _context.AuditLogs
                    .Where(al => al.Timestamp >= filters.StartDate && al.Timestamp <= filters.EndDate)
                    .ToListAsync();

                var report = new AuditReport
                {
                    TotalAuditLogs = auditLogs.Count,
                    UserActions = auditLogs.Count(al => al.Action.Contains("User")),
                    SystemActions = auditLogs.Count(al => al.Action.Contains("System")),
                    SecurityEvents = auditLogs.Count(al => al.Action.Contains("Security")),
                    UserActivity = await GetUserActivityReportAsync(filters),
                    SystemLogs = await GetSystemLogReportAsync(filters)
                };

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error generando reporte de auditoría");
                return new AuditReport();
            }
        }

        public async Task<List<UserActivityData>> GetUserActivityReportAsync(ReportFilters filters)
        {
            try
            {
                var userActivity = await _context.Users
                    .Include(u => u.Branch)
                    .Select(u => new UserActivityData
                    {
                        UserId = u.Id,
                        UserName = u.FullName,
                        UserRole = u.Role.ToString(),
                        LoginCount = _context.AuditLogs.Count(al => al.UserId == u.Id && 
                                                                   al.Action.Contains("Login") &&
                                                                   al.Timestamp >= filters.StartDate && 
                                                                   al.Timestamp <= filters.EndDate),
                        ActionsPerformed = _context.AuditLogs.Count(al => al.UserId == u.Id &&
                                                                        al.Timestamp >= filters.StartDate && 
                                                                        al.Timestamp <= filters.EndDate),
                        LastActivity = _context.AuditLogs
                            .Where(al => al.UserId == u.Id)
                            .Max(al => al.Timestamp) ?? DateTime.UtcNow,
                        MostUsedFeature = "Orders",
                        ActivityLevel = "Medium"
                    })
                    .ToListAsync();

                return userActivity.OrderByDescending(x => x.ActionsPerformed).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error obteniendo actividad de usuarios");
                return new List<UserActivityData>();
            }
        }

        public async Task<List<SystemLogData>> GetSystemLogReportAsync(ReportFilters filters)
        {
            try
            {
                var systemLogs = await _context.AuditLogs
                    .Where(al => al.Timestamp >= filters.StartDate && al.Timestamp <= filters.EndDate)
                    .OrderByDescending(al => al.Timestamp)
                    .Take(100)
                    .Select(al => new SystemLogData
                    {
                        Timestamp = al.Timestamp ?? DateTime.UtcNow,
                        LogLevel = al.Action.Contains("Error") ? "Error" : 
                                  al.Action.Contains("Warning") ? "Warning" : "Info",
                        Action = al.Action,
                        TableName = al.TableName ?? "",
                        RecordId = al.RecordId != null ? al.RecordId.ToString() : "",
                        UserId = al.UserId != null ? al.UserId.ToString() : "",
                        Details = al.Action
                    })
                    .ToListAsync();

                return systemLogs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error obteniendo logs del sistema");
                return new List<SystemLogData>();
            }
        }



        // ===== REPORTES DE CONFIGURACIÓN AVANZADA =====
        public async Task<SystemHealthReport> GetSystemHealthReportAsync()
        {
            try
            {
                var report = new SystemHealthReport
                {
                    TotalUsers = await _context.Users.CountAsync(),
                    ActiveUsers = await _context.Users.CountAsync(u => u.IsActive == true),
                    TotalProducts = await _context.Products.CountAsync(),
                    ActiveProducts = await _context.Products.CountAsync(p => p.IsActive == true),
                    TotalOrders = await _context.Orders.CountAsync(),
                    PendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending),
                    SystemUptime = 99.5m,
                    ErrorCount = await _context.AuditLogs.CountAsync(al => al.Action.Contains("Error")),
                    SystemStatus = "Healthy",
                    PerformanceMetrics = await GetPerformanceMetricsAsync(new ReportFilters())
                };

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error generando reporte de salud del sistema");
                return new SystemHealthReport();
            }
        }

        public async Task<List<PerformanceMetricsData>> GetPerformanceMetricsAsync(ReportFilters filters)
        {
            try
            {
                var metrics = await _context.Orders
                    .Where(o => o.ClosedAt.HasValue && o.ClosedAt >= filters.StartDate && o.ClosedAt <= filters.EndDate)
                    .GroupBy(o => o.ClosedAt.Value.Date)
                    .Select(g => new PerformanceMetricsData
                    {
                        Date = g.Key,
                        OrdersProcessed = g.Count(),
                        AverageResponseTime = 2.5m,
                        ErrorCount = _context.AuditLogs.Count(al => al.Timestamp != null && al.Timestamp.Value.Date == g.Key && al.Action.Contains("Error")),
                        ActiveUsers = _context.Users.Count(u => u.IsActive == true),
                        SystemLoad = 75.0m,
                        PerformanceRating = "Good"
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error obteniendo métricas de rendimiento");
                return new List<PerformanceMetricsData>();
            }
        }

        // ===== EXPORTACIÓN =====
        public async Task<byte[]> ExportAdvancedReportToPdfAsync(string reportType, ReportFilters filters)
        {
            try
            {
                _logger.LogInformation($"[AdvancedReportsService] Exportando reporte {reportType} a PDF");
                var pdfContent = System.Text.Encoding.UTF8.GetBytes($"Reporte {reportType} - {DateTime.Now}");
                return pdfContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[AdvancedReportsService] Error exportando reporte {reportType} a PDF");
                return new byte[0];
            }
        }

        public async Task<byte[]> ExportAdvancedReportToExcelAsync(string reportType, ReportFilters filters)
        {
            try
            {
                _logger.LogInformation($"[AdvancedReportsService] Exportando reporte {reportType} a Excel");
                var excelContent = System.Text.Encoding.UTF8.GetBytes($"Reporte {reportType} - {DateTime.Now}");
                return excelContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[AdvancedReportsService] Error exportando reporte {reportType} a Excel");
                return new byte[0];
            }
        }

        // ===== MÉTODOS PRIVADOS AUXILIARES =====
        private async Task<decimal> CalculateTotalCostAsync(ReportFilters filters)
        {
            try
            {
                return await _context.OrderItems
                    .Where(oi => oi.Order.Status == OrderStatus.Completed &&
                               oi.Order.ClosedAt >= filters.StartDate && 
                               oi.Order.ClosedAt <= filters.EndDate)
                    .SumAsync(oi => oi.Quantity * (oi.Product.Cost ?? 0));
            }
            catch
            {
                return 0;
            }
        }
    }
} 