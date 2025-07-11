using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface ICompanyService
    {
        Task<IEnumerable<Company>> GetAllAsync();
        Task<Company?> GetByIdAsync(Guid id);
        Task<Company> CreateAsync(Company company);
        Task UpdateAsync(Company company);
        Task DeleteAsync(Guid id);

        // Métodos adicionales específicos para Company
        Task<Company?> GetCompanyWithBranchesAsync(Guid id);
        Task<IEnumerable<Company>> GetCompaniesWithActiveBranchesAsync();
        Task<Company?> GetByLegalIdAsync(string legalId);
    }
} 