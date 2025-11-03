using System.ComponentModel.DataAnnotations;

namespace RestBar.ViewModels
{
    // ===== ANÁLISIS DE RENTABILIDAD =====
    public class ProfitabilityAnalysis
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalCosts { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        public decimal GrossMargin { get; set; }
        public List<ProductProfitability> ProductProfitability { get; set; } = new();
        public List<CategoryProfitability> CategoryProfitability { get; set; } = new();
        public List<DailyProfitability> DailyProfitability { get; set; } = new();
    }

    public class ProductProfitability
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitMargin { get; set; }
        public int UnitsSold { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal AverageCost { get; set; }
    }

    public class CategoryProfitability
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitMargin { get; set; }
        public int ProductsCount { get; set; }
        public int UnitsSold { get; set; }
    }

    public class DailyProfitability
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitMargin { get; set; }
        public int OrdersCount { get; set; }
        public int ItemsSold { get; set; }
    }

    // ===== ANÁLISIS DE VENTAS =====
    public class SalesAnalysisReport
    {
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public int TotalItems { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal AverageItemPrice { get; set; }
        public List<TopSellingProduct> TopSellingProducts { get; set; } = new();
        public List<TopSellingCategory> TopSellingCategories { get; set; } = new();
        public List<SalesByHour> SalesByHour { get; set; } = new();
        public List<SalesByDay> SalesByDay { get; set; } = new();
        public List<SalesByMonth> SalesByMonth { get; set; } = new();
    }

    public class TopSellingProduct
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitMargin { get; set; }
    }

    public class TopSellingCategory
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitMargin { get; set; }
        public int ProductsCount { get; set; }
    }

    public class SalesByHour
    {
        public int Hour { get; set; }
        public decimal Revenue { get; set; }
        public int OrdersCount { get; set; }
        public int ItemsSold { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public class SalesByDay
    {
        public DateTime Date { get; set; }
        public string DayName { get; set; } = "";
        public decimal Revenue { get; set; }
        public int OrdersCount { get; set; }
        public int ItemsSold { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public class SalesByMonth
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = "";
        public decimal Revenue { get; set; }
        public int OrdersCount { get; set; }
        public int ItemsSold { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    // ===== ANÁLISIS DE CLIENTES =====
    public class CustomerAnalysisReport
    {
        public int TotalCustomers { get; set; }
        public int NewCustomers { get; set; }
        public int ReturningCustomers { get; set; }
        public decimal AverageCustomerValue { get; set; }
        public decimal CustomerLifetimeValue { get; set; }
        public List<TopCustomer> TopCustomers { get; set; } = new();
        public List<CustomerSegment> CustomerSegments { get; set; } = new();
        public List<CustomerRetention> CustomerRetention { get; set; } = new();
    }

    public class TopCustomer
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public int OrdersCount { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal AverageOrderValue { get; set; }
        public DateTime FirstOrderDate { get; set; }
        public DateTime LastOrderDate { get; set; }
        public int DaysSinceLastOrder { get; set; }
    }

    public class CustomerSegment
    {
        public string SegmentName { get; set; } = "";
        public string Description { get; set; } = "";
        public int CustomersCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal CustomerLifetimeValue { get; set; }
        public decimal RetentionRate { get; set; }
    }

    public class CustomerRetention
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int NewCustomers { get; set; }
        public int RetainedCustomers { get; set; }
        public decimal RetentionRate { get; set; }
        public decimal ChurnRate { get; set; }
    }

    // ===== ANÁLISIS DE OPERACIONES =====
    public class OperationalAnalysisReport
    {
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal AverageOrderTime { get; set; }
        public decimal AveragePreparationTime { get; set; }
        public List<StationPerformance> StationPerformance { get; set; } = new();
        public List<TableUtilization> TableUtilization { get; set; } = new();
        public List<PeakHours> PeakHours { get; set; } = new();
    }

    public class StationPerformance
    {
        public Guid StationId { get; set; }
        public string StationName { get; set; } = "";
        public int OrdersProcessed { get; set; }
        public int ItemsProcessed { get; set; }
        public decimal AveragePreparationTime { get; set; }
        public decimal Efficiency { get; set; }
        public int PeakHour { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TableUtilization
    {
        public Guid TableId { get; set; }
        public string TableName { get; set; } = "";
        public int Capacity { get; set; }
        public int OrdersCount { get; set; }
        public decimal UtilizationRate { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal TotalRevenue { get; set; }
        public int AverageGuests { get; set; }
    }

    public class PeakHours
    {
        public int Hour { get; set; }
        public int OrdersCount { get; set; }
        public decimal Revenue { get; set; }
        public int AverageGuests { get; set; }
        public decimal AverageOrderValue { get; set; }
        public string PeakType { get; set; } = ""; // "Breakfast", "Lunch", "Dinner", "Late"
    }

    // ===== ANÁLISIS DE INVENTARIO =====
    public class InventoryAnalysisReport
    {
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public List<LowStockAlert> LowStockAlerts { get; set; } = new();
        public List<InventoryTurnover> TurnoverData { get; set; } = new();
        public List<InventoryValueReport> ValueReport { get; set; } = new();
    }

    public class LowStockAlert
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public string BranchName { get; set; } = "";
        public decimal CurrentStock { get; set; }
        public decimal MinStock { get; set; }
        public decimal ReorderPoint { get; set; }
        public string AlertLevel { get; set; } = ""; // "Critical", "Warning", "Normal"
        public DateTime LastUpdated { get; set; }
    }

    public class InventoryTurnover
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public decimal AverageStock { get; set; }
        public decimal TotalSold { get; set; }
        public decimal TurnoverRate { get; set; }
        public int DaysToSell { get; set; }
        public string Efficiency { get; set; } = ""; // "High", "Medium", "Low"
    }

    public class InventoryValueReport
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public decimal CurrentStock { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalValue { get; set; }
        public decimal LastMonthValue { get; set; }
        public decimal ValueChange { get; set; }
        public string BranchName { get; set; } = "";
    }

    // ===== ANÁLISIS DE PROVEEDORES =====
    public class SupplierAnalysisReport
    {
        public int TotalSuppliers { get; set; }
        public int ActiveSuppliers { get; set; }
        public decimal TotalPurchases { get; set; }
        public decimal AverageDeliveryTime { get; set; }
        public List<SupplierPerformance> SupplierPerformance { get; set; } = new();
        public List<PurchaseOrderAnalysis> PurchaseOrders { get; set; } = new();
        public List<DeliveryTimeAnalysis> DeliveryTimes { get; set; } = new();
    }

    public class SupplierPerformance
    {
        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; } = "";
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal AverageDeliveryTime { get; set; }
        public decimal QualityRating { get; set; }
        public string PerformanceStatus { get; set; } = ""; // "Excellent", "Good", "Fair", "Poor"
    }

    public class PurchaseOrderAnalysis
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = "";
        public string SupplierName { get; set; } = "";
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "";
        public int DaysToDelivery { get; set; }
    }

    public class DeliveryTimeAnalysis
    {
        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; } = "";
        public decimal AverageDays { get; set; }
        public decimal MinDays { get; set; }
        public decimal MaxDays { get; set; }
        public int OnTimeDeliveries { get; set; }
        public int LateDeliveries { get; set; }
        public decimal OnTimeRate { get; set; }
    }

    // ===== ANÁLISIS DE TENDENCIAS =====
    public class TrendAnalysisReport
    {
        public List<SalesTrend> SalesTrends { get; set; } = new();
        public List<ProductTrend> ProductTrends { get; set; } = new();
        public List<CategoryTrend> CategoryTrends { get; set; } = new();
        public List<SeasonalPattern> SeasonalPatterns { get; set; } = new();
        public List<GrowthForecast> GrowthForecasts { get; set; } = new();
    }

    public class SalesTrend
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrdersCount { get; set; }
        public decimal GrowthRate { get; set; }
        public string TrendDirection { get; set; } = ""; // "Up", "Down", "Stable"
    }

    public class ProductTrend
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
        public decimal GrowthRate { get; set; }
        public string TrendDirection { get; set; } = "";
        public string CategoryName { get; set; } = "";
    }

    public class CategoryTrend
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
        public decimal GrowthRate { get; set; }
        public string TrendDirection { get; set; } = "";
    }

    public class SeasonalPattern
    {
        public string Season { get; set; } = "";
        public string Month { get; set; } = "";
        public decimal AverageRevenue { get; set; }
        public int AverageOrders { get; set; }
        public decimal PeakMultiplier { get; set; }
    }

    public class GrowthForecast
    {
        public DateTime ForecastDate { get; set; }
        public decimal PredictedRevenue { get; set; }
        public decimal ConfidenceLevel { get; set; }
        public decimal GrowthRate { get; set; }
        public string ForecastType { get; set; } = ""; // "Optimistic", "Realistic", "Pessimistic"
    }

    // ===== REPORTE DE AUDITORÍA =====
    public class AuditReportViewModel
    {
        public int TotalLogs { get; set; }
        public int ErrorLogs { get; set; }
        public int WarningLogs { get; set; }
        public int InfoLogs { get; set; }
        public List<AuditLogEntry> RecentLogs { get; set; } = new();
        public List<UserActivity> UserActivities { get; set; } = new();
        public List<SystemEvent> SystemEvents { get; set; } = new();
        public List<SecurityEvent> SecurityEvents { get; set; } = new();
    }

    public class AuditLogEntry
    {
        public Guid Id { get; set; }
        public string Action { get; set; } = "";
        public string Module { get; set; } = "";
        public string UserEmail { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string LogLevel { get; set; } = "";
        public string Description { get; set; } = "";
        public string IpAddress { get; set; } = "";
    }

    public class UserActivity
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public int LoginCount { get; set; }
        public DateTime LastLogin { get; set; }
        public int ActionsCount { get; set; }
        public string MostActiveModule { get; set; } = "";
    }

    public class SystemEvent
    {
        public Guid Id { get; set; }
        public string EventType { get; set; } = "";
        public string Module { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string Description { get; set; } = "";
        public string Status { get; set; } = "";
    }

    public class SecurityEvent
    {
        public Guid Id { get; set; }
        public string EventType { get; set; } = "";
        public string UserEmail { get; set; } = "";
        public string IpAddress { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string Description { get; set; } = "";
        public string Severity { get; set; } = ""; // "Low", "Medium", "High", "Critical"
    }

    // ===== SALUD DEL SISTEMA =====
    public class SystemHealthReport
    {
        public string OverallStatus { get; set; } = ""; // "Healthy", "Warning", "Critical"
        public decimal SystemUptime { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal DatabaseSize { get; set; }
        public decimal AverageResponseTime { get; set; }
        public List<SystemMetric> Metrics { get; set; } = new();
        public List<SystemAlert> Alerts { get; set; } = new();
    }

    public class SystemMetric
    {
        public string MetricName { get; set; } = "";
        public decimal Value { get; set; }
        public decimal Threshold { get; set; }
        public string Status { get; set; } = ""; // "Normal", "Warning", "Critical"
        public DateTime LastUpdated { get; set; }
    }

    public class SystemAlert
    {
        public Guid Id { get; set; }
        public string AlertType { get; set; } = "";
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string Severity { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
    }

    // ===== FILTROS PARA REPORTES =====
    public class ReportFilters
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? StationId { get; set; }
        public Guid? UserId { get; set; }
        public string? OrderStatus { get; set; }
        public string? PaymentMethod { get; set; }
        public int? TopN { get; set; } = 10;
    }
} 