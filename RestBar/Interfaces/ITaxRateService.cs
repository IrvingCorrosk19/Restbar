using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface ITaxRateService
    {
        Task<IEnumerable<TaxRate>> GetAllAsync(Guid? companyId = null);
        Task<TaxRate?> GetByIdAsync(Guid id);
        Task<TaxRate> CreateAsync(TaxRate taxRate);
        Task<TaxRate> UpdateAsync(TaxRate taxRate);
        Task<bool> DeleteAsync(Guid id);
        Task<TaxRate?> GetByCodeAsync(string taxCode, Guid? companyId = null);
        Task<decimal> CalculateTaxAsync(decimal amount, Guid taxRateId);
        Task<decimal> CalculateTaxAsync(decimal amount, string taxCode, Guid? companyId = null);
    }
} 