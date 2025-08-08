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
        public async Task<InventoryAnalysisReport> GetInventoryAnalysisAsync(ReportFilters filters)
        {
            try
            {
                _logger.LogInformation("[AdvancedReportsService] Generando análisis de inventario");

                var inventoryData = await _context.Inventories
                    .Include(i => i.Product)
                    .Include(i => i.Product.Category)
                    .Include(i => i.Branch)
                    .Where(i => i.IsActive)
                    .ToListAsync();

                var report = new InventoryAnalysisReport
                {
                    TotalProducts = inventoryData.Count,
                    LowStockProducts = inventoryData.Count(i => i.Stock < i.MinStock),
                    OutOfStockProducts = inventoryData.Count(i => i.Stock <= 0),
                    TotalInventoryValue = inventoryData.Sum(i => (i.Stock ?? 0) * (i.UnitCost ?? 0)),
                    AverageStockLevel = (decimal)inventoryData.Average(i => i.Stock ?? 0),
                    StockTurnoverRate = 2.5m, // Valor simulado
                    LowStockAlerts = await GetLowStockAlertsAsync(),
                    TurnoverData = await GetInventoryTurnoverAsync(filters),
                    ValueReport = await GetInventoryValueReportAsync()
                };

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error generando análisis de inventario");
                return new InventoryAnalysisReport();
            }
        }

        public async Task<List<LowStockAlert>> GetLowStockAlertsAsync()
        {
            try
            {
                var alerts = await _context.Inventories
                    .Include(i => i.Product)
                    .Include(i => i.Product.Category)
                    .Include(i => i.Branch)
                    .Where(i => i.Stock <= i.MinStock && i.IsActive)
                    .Select(i => new LowStockAlert
                    {
                        ProductId = i.ProductId.Value,
                        ProductName = i.Product.Name,
                        CategoryName = i.Product.Category.Name,
                        CurrentStock = i.Stock ?? 0,
                        MinStock = i.MinStock ?? 0,
                        ReorderPoint = i.ReorderPoint ?? 0,
                        BranchName = i.Branch.Name,
                        LastUpdated = i.LastUpdated ?? DateTime.UtcNow,
                        AlertLevel = i.Stock <= 0 ? "Critical" : "Warning"
                    })
                    .ToListAsync();

                return alerts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error obteniendo alertas de bajo stock");
                return new List<LowStockAlert>();
            }
        }

        public async Task<List<InventoryTurnoverData>> GetInventoryTurnoverAsync(ReportFilters filters)
        {
            try
            {
                var turnoverData = await _context.Inventories
                    .Include(i => i.Product)
                    .Include(i => i.Product.Category)
                    .Where(i => i.IsActive)
                    .Select(i => new InventoryTurnoverData
                    {
                        ProductId = i.ProductId.Value,
                        ProductName = i.Product.Name,
                        CategoryName = i.Product.Category.Name,
                        AverageStock = i.Stock ?? 0,
                        TotalSold = 0, // Se calcularía con datos históricos
                        TurnoverRate = 1.5m, // Valor simulado
                        DaysToSell = 20, // Valor simulado
                        Efficiency = "Medium"
                    })
                    .ToListAsync();

                return turnoverData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error obteniendo datos de rotación de inventario");
                return new List<InventoryTurnoverData>();
            }
        }

        public async Task<List<InventoryValueReport>> GetInventoryValueReportAsync()
        {
            try
            {
                var valueReport = await _context.Inventories
                    .Include(i => i.Product)
                    .Include(i => i.Product.Category)
                    .Include(i => i.Branch)
                    .Where(i => i.IsActive)
                    .Select(i => new InventoryValueReport
                    {
                        ProductId = i.ProductId.Value,
                        ProductName = i.Product.Name,
                        CategoryName = i.Product.Category.Name,
                        CurrentStock = i.Stock ?? 0,
                        UnitCost = i.UnitCost ?? 0,
                        TotalValue = (i.Stock ?? 0) * (i.UnitCost ?? 0),
                        LastMonthValue = 0,
                        ValueChange = 0,
                        BranchName = i.Branch.Name
                    })
                    .ToListAsync();

                return valueReport.OrderByDescending(x => x.TotalValue).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error obteniendo reporte de valor de inventario");
                return new List<InventoryValueReport>();
            }
        }

        // ===== ANÁLISIS DE PROVEEDORES =====
        public async Task<SupplierAnalysisReport> GetSupplierAnalysisAsync(ReportFilters filters)
        {
            try
            {
                var suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
                var purchaseOrders = await _context.PurchaseOrders
                    .Include(po => po.Supplier)
                    .Where(po => po.OrderDate >= filters.StartDate && po.OrderDate <= filters.EndDate)
                    .ToListAsync();

                var report = new SupplierAnalysisReport
                {
                    TotalSuppliers = suppliers.Count,
                    ActiveSuppliers = suppliers.Count(s => s.IsActive),
                    TotalPurchases = purchaseOrders.Sum(po => po.TotalAmount),
                    AverageOrderValue = purchaseOrders.Any() ? purchaseOrders.Average(po => po.TotalAmount) : 0,
                    TotalPurchaseOrders = purchaseOrders.Count,
                    PerformanceData = await GetSupplierPerformanceAsync(filters),
                    PurchaseOrderAnalysis = await GetPurchaseOrderAnalysisAsync(filters)
                };

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error generando análisis de proveedores");
                return new SupplierAnalysisReport();
            }
        }

        public async Task<List<SupplierPerformanceData>> GetSupplierPerformanceAsync(ReportFilters filters)
        {
            try
            {
                var performanceData = await _context.Suppliers
                    .Where(s => s.IsActive)
                    .Select(s => new SupplierPerformanceData
                    {
                        SupplierId = s.Id,
                        SupplierName = s.Name,
                        ContactEmail = s.Email,
                        Phone = s.Phone,
                        TotalOrders = _context.PurchaseOrders.Count(po => po.SupplierId == s.Id),
                        TotalSpent = _context.PurchaseOrders.Where(po => po.SupplierId == s.Id).Sum(po => po.TotalAmount),
                        AverageOrderValue = _context.PurchaseOrders.Where(po => po.SupplierId == s.Id).Average(po => po.TotalAmount),
                        OnTimeDeliveries = 8, // Valor simulado
                        LateDeliveries = 2, // Valor simulado
                        OnTimePercentage = 80m, // Valor simulado
                        PerformanceRating = "Good"
                    })
                    .ToListAsync();

                return performanceData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error obteniendo rendimiento de proveedores");
                return new List<SupplierPerformanceData>();
            }
        }

        public async Task<List<PurchaseOrderAnalysis>> GetPurchaseOrderAnalysisAsync(ReportFilters filters)
        {
            try
            {
                var analysis = await _context.PurchaseOrders
                    .Include(po => po.Supplier)
                    .Where(po => po.OrderDate >= filters.StartDate && po.OrderDate <= filters.EndDate)
                    .Select(po => new PurchaseOrderAnalysis
                    {
                        PurchaseOrderId = po.Id,
                        PurchaseOrderNumber = po.OrderNumber,
                        SupplierName = po.Supplier.Name,
                        OrderDate = po.OrderDate,
                        ExpectedDeliveryDate = po.ExpectedDeliveryDate,
                        ActualDeliveryDate = po.ActualDeliveryDate,
                        TotalAmount = po.TotalAmount,
                        Status = po.Status.ToString(),
                        DaysToDeliver = po.ActualDeliveryDate.HasValue ? (po.ActualDeliveryDate.Value - po.OrderDate).Days : 0,
                        IsOnTime = po.ActualDeliveryDate.HasValue && po.ExpectedDeliveryDate.HasValue && 
                                  po.ActualDeliveryDate.Value <= po.ExpectedDeliveryDate.Value
                    })
                    .ToListAsync();

                return analysis.OrderByDescending(x => x.OrderDate).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error obteniendo análisis de órdenes de compra");
                return new List<PurchaseOrderAnalysis>();
            }
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

        // ===== REPORTES DE TRANSFERENCIAS =====
        public async Task<TransferAnalysisReport> GetTransferAnalysisAsync(ReportFilters filters)
        {
            try
            {
                var transfers = await _context.Transfers
                    .Include(t => t.SourceBranch)
                    .Include(t => t.DestinationBranch)
                    .Where(t => t.TransferDate >= filters.StartDate && t.TransferDate <= filters.EndDate)
                    .ToListAsync();

                var report = new TransferAnalysisReport
                {
                    TotalTransfers = transfers.Count,
                    CompletedTransfers = transfers.Count(t => t.Status == TransferStatus.Delivered),
                    PendingTransfers = transfers.Count(t => t.Status == TransferStatus.Pending),
                    TotalTransferValue = transfers.Sum(t => t.TotalAmount),
                    AverageTransferValue = transfers.Any() ? transfers.Average(t => t.TotalAmount) : 0,
                    AverageTransferTime = 3, // Valor simulado
                    EfficiencyData = await GetTransferEfficiencyAsync(filters)
                };

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error generando análisis de transferencias");
                return new TransferAnalysisReport();
            }
        }

        public async Task<List<TransferEfficiencyData>> GetTransferEfficiencyAsync(ReportFilters filters)
        {
            try
            {
                var efficiencyData = await _context.Transfers
                    .Include(t => t.SourceBranch)
                    .Include(t => t.DestinationBranch)
                    .Where(t => t.TransferDate >= filters.StartDate && t.TransferDate <= filters.EndDate)
                    .Select(t => new TransferEfficiencyData
                    {
                        TransferId = t.Id,
                        TransferNumber = t.TransferNumber,
                        SourceBranch = t.SourceBranch.Name,
                        DestinationBranch = t.DestinationBranch.Name,
                        TransferDate = t.TransferDate,
                        ExpectedDeliveryDate = t.ExpectedDeliveryDate,
                        ActualDeliveryDate = t.ActualDeliveryDate,
                        TotalValue = t.TotalAmount,
                        Status = t.Status.ToString(),
                        DaysToDeliver = t.ActualDeliveryDate.HasValue ? (t.ActualDeliveryDate.Value - t.TransferDate).Days : 0,
                        IsOnTime = t.ActualDeliveryDate.HasValue && t.ExpectedDeliveryDate.HasValue && 
                                  t.ActualDeliveryDate.Value <= t.ExpectedDeliveryDate.Value,
                        Efficiency = "Medium"
                    })
                    .ToListAsync();

                return efficiencyData.OrderByDescending(x => x.TransferDate).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsService] Error obteniendo eficiencia de transferencias");
                return new List<TransferEfficiencyData>();
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
                        ErrorCount = _context.AuditLogs.Count(al => al.Timestamp.HasValue && al.Timestamp.Value.Date == g.Key && al.Action.Contains("Error")),
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