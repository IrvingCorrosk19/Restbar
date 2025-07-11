using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface IInvoiceService
    {
        Task<IEnumerable<Invoice>> GetAllAsync();
        Task<Invoice?> GetByIdAsync(Guid id);
        Task<Invoice> CreateAsync(Invoice invoice);
        Task UpdateAsync(Invoice invoice);
        Task DeleteAsync(Guid id);

        // Métodos adicionales específicos para Invoice
        Task<IEnumerable<Invoice>> GetByCustomerIdAsync(Guid customerId);
        Task<IEnumerable<Invoice>> GetByOrderIdAsync(Guid orderId);
        Task<IEnumerable<Invoice>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Invoice?> GetInvoiceWithDetailsAsync(Guid id);
        Task<decimal> CalculateInvoiceTotalAsync(Guid id);
        Task<IEnumerable<Invoice>> GetInvoicesByTotalRangeAsync(decimal minTotal, decimal maxTotal);
    }
} 