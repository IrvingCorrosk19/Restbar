using System;
using System.Collections.Generic;

namespace RestBar.ViewModels
{
    // ✅ ViewModel principal para reportes de ventas
    public class SalesReportViewModel
    {
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime EndDate { get; set; } = DateTime.Today;
        public string Period { get; set; } = "month"; // day, week, month, year
        public Guid? BranchId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? CategoryId { get; set; }
        
        // Métricas generales
        public SalesMetrics Metrics { get; set; } = new SalesMetrics();
        
        // Reportes específicos
        public List<DailySalesData> DailySales { get; set; } = new List<DailySalesData>();
        public List<TopProductData> TopProducts { get; set; } = new List<TopProductData>();
        public List<CategorySalesData> CategorySales { get; set; } = new List<CategorySalesData>();
        public List<EmployeeSalesData> EmployeeSales { get; set; } = new List<EmployeeSalesData>();
        public List<BranchSalesData> BranchSales { get; set; } = new List<BranchSalesData>();
        public List<DiscountData> Discounts { get; set; } = new List<DiscountData>();
    }

    // ✅ Métricas generales de ventas
    public class SalesMetrics
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageTicket { get; set; }
        public decimal TotalDiscounts { get; set; }
        public decimal NetRevenue { get; set; }
        public int TotalItems { get; set; }
        public decimal AverageItemsPerOrder { get; set; }
        public decimal ProfitMargin { get; set; }
    }

    // ✅ Datos de ventas por día
    public class DailySalesData
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
        public int Items { get; set; }
        public decimal AverageTicket { get; set; }
        public decimal Discounts { get; set; }
    }

    // ✅ Top productos más vendidos
    public class TopProductData
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalCost { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitMargin { get; set; }
        public int Rank { get; set; }
    }

    // ✅ Ventas por categoría
    public class CategorySalesData
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public int ItemsSold { get; set; }
        public decimal Revenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitMargin { get; set; }
        public int ProductCount { get; set; }
        public decimal PercentageOfTotal { get; set; }
    }

    // ✅ Ventas por empleado/mesero
    public class EmployeeSalesData
    {
        public Guid UserId { get; set; }
        public string EmployeeName { get; set; } = "";
        public string Role { get; set; } = "";
        public string BranchName { get; set; } = "";
        public int OrdersHandled { get; set; }
        public decimal Revenue { get; set; }
        public decimal AverageTicket { get; set; }
        public int ItemsSold { get; set; }
        public decimal Commission { get; set; }
        public decimal Performance { get; set; } // Porcentaje vs meta
    }

    // ✅ Ventas por sucursal
    public class BranchSalesData
    {
        public Guid BranchId { get; set; }
        public string BranchName { get; set; } = "";
        public int Orders { get; set; }
        public decimal Revenue { get; set; }
        public decimal AverageTicket { get; set; }
        public int ItemsSold { get; set; }
        public decimal TotalCost { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitMargin { get; set; }
        public int EmployeeCount { get; set; }
        public decimal PercentageOfTotal { get; set; }
    }

    // ✅ Reporte de descuentos y promociones
    public class DiscountData
    {
        public Guid OrderId { get; set; }
        public string ProductName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public DateTime OrderDate { get; set; }
        public string EmployeeName { get; set; } = "";
        public string BranchName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal TotalDiscount { get; set; }
    }


} 