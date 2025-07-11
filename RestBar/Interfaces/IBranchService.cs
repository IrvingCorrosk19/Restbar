using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface IBranchService
    {
        Task<IEnumerable<Branch>> GetAllAsync();
        Task<Branch?> GetByIdAsync(Guid id);
        Task<Branch> CreateAsync(Branch branch);
        Task UpdateAsync(Branch branch);
        Task DeleteAsync(Guid id);

        // Métodos adicionales específicos para Branch
        Task<IEnumerable<Branch>> GetByCompanyIdAsync(Guid companyId);
        Task<IEnumerable<Branch>> GetActiveBranchesAsync();
        Task<IEnumerable<Branch>> GetBranchesWithAreasAsync();
    }
} 