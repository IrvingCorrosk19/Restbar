using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(Guid id);
        Task<Customer> CreateAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task DeleteAsync(Guid id);

        // Métodos adicionales específicos para Customer
        Task<Customer?> GetByEmailAsync(string email);
        Task<Customer?> GetByPhoneAsync(string phone);
        Task<IEnumerable<Customer>> GetByLoyaltyPointsRangeAsync(int minPoints, int maxPoints);
        Task<Customer?> GetCustomerWithOrdersAsync(Guid id);
        Task<Customer?> GetCustomerWithInvoicesAsync(Guid id);
        Task UpdateLoyaltyPointsAsync(Guid id, int points);
        Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm);
    }
} 