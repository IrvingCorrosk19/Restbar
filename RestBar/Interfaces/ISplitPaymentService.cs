using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface ISplitPaymentService
    {
        Task<IEnumerable<SplitPayment>> GetAllAsync();
        Task<SplitPayment?> GetByIdAsync(Guid id);
        Task<SplitPayment> CreateAsync(SplitPayment splitPayment);
        Task UpdateAsync(SplitPayment splitPayment);
        Task DeleteAsync(Guid id);

        // Métodos adicionales específicos para SplitPayment
        Task<IEnumerable<SplitPayment>> GetByPaymentIdAsync(Guid paymentId);
        Task<SplitPayment?> GetSplitPaymentWithDetailsAsync(Guid id);
        Task<decimal> GetTotalSplitAmountAsync(Guid paymentId);
        Task<IEnumerable<SplitPayment>> GetByAmountRangeAsync(decimal minAmount, decimal maxAmount);
        Task<IEnumerable<SplitPayment>> GetByPersonNameAsync(string personName);
    }
} 