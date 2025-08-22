using System.ComponentModel.DataAnnotations;

namespace RestBar.ViewModels
{
    // ===== ANÁLISIS DE INVENTARIO =====
    public class InventoryAnalysisReport
    {
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public decimal AverageStockLevel { get; set; }
        public decimal StockTurnoverRate { get; set; }
        public List<LowStockAlert> LowStockAlerts { get; set; } = new();
        public List<InventoryTurnoverData> TurnoverData { get; set; } = new();
        public List<InventoryValueReport> ValueReport { get; set; } = new();
    }

    public class LowStockAlert
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public decimal CurrentStock { get; set; }
        public decimal MinStock { get; set; }
        public decimal ReorderPoint { get; set; }
        public string BranchName { get; set; } = "";
        public DateTime LastUpdated { get; set; }
        public string AlertLevel { get; set; } = ""; // "Critical", "Warning", "Info"
    }

    public class InventoryTurnoverData
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


    // ===== ANÁLISIS DE RENTABILIDAD =====
    public class ProfitabilityAnalysis
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        public decimal CostOfGoodsSold { get; set; }
        public decimal OperatingExpenses { get; set; }
        public List<ProductProfitabilityData> ProductProfitability { get; set; } = new();
        public List<CategoryProfitabilityData> CategoryProfitability { get; set; } = new();
    }

    public class ProductProfitabilityData
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
        public string ProfitabilityLevel { get; set; } = ""; // "High", "Medium", "Low"
    }

    public class CategoryProfitabilityData
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitMargin { get; set; }
        public int ProductsCount { get; set; }
        public int TotalUnitsSold { get; set; }
        public string PerformanceRating { get; set; } = ""; // "Excellent", "Good", "Average", "Poor"
    }

    // ===== ANÁLISIS DE TENDENCIAS =====
    public class TrendAnalysisReport
    {
        public decimal RevenueGrowth { get; set; }
        public decimal OrderGrowth { get; set; }
        public decimal CustomerGrowth { get; set; }
        public List<SeasonalTrendData> SeasonalTrends { get; set; } = new();
        public List<GrowthAnalysisData> GrowthAnalysis { get; set; } = new();
    }

    public class SeasonalTrendData
    {
        public string Period { get; set; } = ""; // "Q1", "Q2", "Q3", "Q4" o "Jan", "Feb", etc.
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
        public decimal AverageTicket { get; set; }
        public decimal GrowthRate { get; set; }
        public string Trend { get; set; } = ""; // "Up", "Down", "Stable"
    }

    public class GrowthAnalysisData
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
        public decimal GrowthRate { get; set; }
        public decimal CumulativeGrowth { get; set; }
        public string GrowthTrend { get; set; } = ""; // "Accelerating", "Decelerating", "Stable"
    }

    // ===== REPORTES DE AUDITORÍA =====
    public class AuditReport
    {
        public int TotalAuditLogs { get; set; }
        public int UserActions { get; set; }
        public int SystemActions { get; set; }
        public int SecurityEvents { get; set; }
        public List<UserActivityData> UserActivity { get; set; } = new();
        public List<SystemLogData> SystemLogs { get; set; } = new();
    }

    public class UserActivityData
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = "";
        public string UserRole { get; set; } = "";
        public int LoginCount { get; set; }
        public int ActionsPerformed { get; set; }
        public DateTime LastActivity { get; set; }
        public string MostUsedFeature { get; set; } = "";
        public string ActivityLevel { get; set; } = ""; // "High", "Medium", "Low"
    }

    public class SystemLogData
    {
        public DateTime Timestamp { get; set; }
        public string LogLevel { get; set; } = ""; // "Info", "Warning", "Error"
        public string Action { get; set; } = "";
        public string TableName { get; set; } = "";
        public string RecordId { get; set; } = "";
        public string UserId { get; set; } = "";
        public string Details { get; set; } = "";
    }



    // ===== REPORTES DE CONFIGURACIÓN AVANZADA =====
    public class SystemHealthReport
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal SystemUptime { get; set; }
        public int ErrorCount { get; set; }
        public string SystemStatus { get; set; } = ""; // "Healthy", "Warning", "Critical"
        public List<PerformanceMetricsData> PerformanceMetrics { get; set; } = new();
    }

    public class PerformanceMetricsData
    {
        public DateTime Date { get; set; }
        public int OrdersProcessed { get; set; }
        public decimal AverageResponseTime { get; set; }
        public int ErrorCount { get; set; }
        public int ActiveUsers { get; set; }
        public decimal SystemLoad { get; set; }
        public string PerformanceRating { get; set; } = ""; // "Excellent", "Good", "Average", "Poor"
    }
} 