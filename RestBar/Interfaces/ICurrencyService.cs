using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface ICurrencyService
    {
        Task<IEnumerable<Currency>> GetAllAsync(Guid? companyId = null);
        Task<Currency?> GetByIdAsync(Guid id);
        Task<Currency> CreateAsync(Currency currency);
        Task<Currency> UpdateAsync(Currency currency);
        Task<bool> DeleteAsync(Guid id);
        Task<Currency?> GetDefaultCurrencyAsync(Guid? companyId = null);
        Task<bool> SetDefaultCurrencyAsync(Guid id, Guid? companyId = null);
        Task<Currency?> GetByCodeAsync(string code, Guid? companyId = null);
        Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency, Guid? companyId = null);
    }
} 