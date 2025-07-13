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