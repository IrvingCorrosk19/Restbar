using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface IAccountingService
    {
        // Dashboard y estadísticas
        Task<FinancialSummaryDto> GetFinancialSummaryAsync(string period);
        Task<IncomeDetailsDto> GetIncomeDetailsAsync(string period);
        Task<ExpenseDetailsDto> GetExpenseDetailsAsync(string period);
        Task<TaxSummaryDto> GetTaxSummaryAsync(string period);
        
        // Cálculos financieros
        Task<decimal> GetTotalIncomeAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalExpensesAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetNetProfitAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalTaxesAsync(DateTime startDate, DateTime endDate);
        
        // Análisis por período
        Task<IEnumerable<MonthlyFinancialData>> GetMonthlyFinancialDataAsync(int year);
        Task<IEnumerable<DailyFinancialData>> GetDailyFinancialDataAsync(DateTime startDate, DateTime endDate);
        
        // Reportes
        Task<byte[]> GenerateFinancialReportAsync(string reportType, string period, DateTime? startDate = null, DateTime? endDate = null);
        Task<byte[]> ExportAccountingDataAsync(string format = "xlsx");
        
        // Integración con ventas
        Task<bool> CreateAccountingEntryFromOrderAsync(Guid orderId);
        Task<bool> CreateAccountingEntryFromPaymentAsync(Guid paymentId);
        Task<bool> RecordPaymentAsync(Guid orderId, decimal amount, string method, bool isShared, string? payerName, List<SplitPaymentDto>? splitPayments = null);
        
        // Cálculos de impuestos
        Task<decimal> CalculateIVACollectedAsync(DateTime startDate, DateTime endDate);
        Task<decimal> CalculateIVAPaidAsync(DateTime startDate, DateTime endDate);
        Task<decimal> CalculateIVAToPayAsync(DateTime startDate, DateTime endDate);
        Task<decimal> CalculateISRAsync(DateTime startDate, DateTime endDate);
    }

    public class FinancialSummaryDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public decimal TotalTaxes { get; set; }
        public decimal ProfitMargin { get; set; }
        public int TotalOrders { get; set; }
        public int TotalPayments { get; set; }
    }

    public class IncomeDetailsDto
    {
        public List<IncomeItem> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public int TotalCount { get; set; }
    }

    public class IncomeItem
    {
        public string Concept { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
    }

    public class ExpenseDetailsDto
    {
        public List<ExpenseItem> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public int TotalCount { get; set; }
    }

    public class ExpenseItem
    {
        public string Concept { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Supplier { get; set; } = string.Empty;
    }

    public class TaxSummaryDto
    {
        public decimal IVACollected { get; set; }
        public decimal IVAPaid { get; set; }
        public decimal IVAToPay { get; set; }
        public decimal ISR { get; set; }
        public decimal TotalTaxes { get; set; }
    }

    public class MonthlyFinancialData
    {
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public decimal Profit { get; set; }
        public int OrderCount { get; set; }
    }

    public class DailyFinancialData
    {
        public DateTime Date { get; set; }
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public decimal Profit { get; set; }
        public int OrderCount { get; set; }
    }

    public class SplitPaymentDto
    {
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string? PayerName { get; set; }
    }
} 