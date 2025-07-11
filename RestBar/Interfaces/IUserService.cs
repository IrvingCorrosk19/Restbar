using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(Guid id);
        Task<User> CreateAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(Guid id);

        // Métodos adicionales específicos para User
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetByBranchIdAsync(Guid branchId);
        Task<IEnumerable<User>> GetByRoleAsync(UserRole role);
        Task<IEnumerable<User>> GetActiveUsersAsync();
        Task<User?> GetUserWithOrdersAsync(Guid id);
        Task<User?> GetUserWithAuditLogsAsync(Guid id);
        Task<bool> ValidateUserAsync(string email, string password);
    }
} 