using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface IPaymentService
    {
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<Payment?> GetByIdAsync(Guid id);
        Task<Payment> CreateAsync(Payment payment);
        Task UpdateAsync(Payment payment);
        Task DeleteAsync(Guid id);

        // Métodos adicionales específicos para Payment
        Task<IEnumerable<Payment>> GetByOrderIdAsync(Guid orderId);
        Task<IEnumerable<Payment>> GetByMethodAsync(string method);
        Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Payment?> GetPaymentWithSplitsAsync(Guid id);
        Task<decimal> GetTotalPaymentsByOrderAsync(Guid orderId);
        Task VoidPaymentAsync(Guid id);
        Task<IEnumerable<Payment>> GetVoidedPaymentsAsync();
    }
} 