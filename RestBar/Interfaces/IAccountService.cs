using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface IAccountService
    {
        // Operaciones básicas de cuentas
        Task<IEnumerable<Account>> GetAllAccountsAsync();
        Task<Account?> GetAccountByIdAsync(Guid id);
        Task<Account?> GetAccountByCodeAsync(string code);
        Task<Account> CreateAccountAsync(Account account);
        Task<Account> UpdateAccountAsync(Account account);
        Task<bool> DeleteAccountAsync(Guid id);
        
        // Operaciones de jerarquía
        Task<IEnumerable<Account>> GetRootAccountsAsync();
        Task<IEnumerable<Account>> GetSubAccountsAsync(Guid parentId);
        Task<IEnumerable<Account>> GetAccountTreeAsync();
        Task<Account?> GetParentAccountAsync(Guid accountId);
        
        // Operaciones por tipo y categoría
        Task<IEnumerable<Account>> GetAccountsByTypeAsync(AccountType type);
        Task<IEnumerable<Account>> GetAccountsByCategoryAsync(AccountCategory category);
        Task<IEnumerable<Account>> GetActiveAccountsAsync();
        
        // Operaciones de validación
        Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);
        Task<bool> CanDeleteAccountAsync(Guid id);
        
        // Operaciones de balance
        Task<decimal> GetAccountBalanceAsync(Guid accountId, DateTime? asOfDate = null);
        Task<Dictionary<Guid, decimal>> GetAccountsBalanceAsync(IEnumerable<Guid> accountIds, DateTime? asOfDate = null);
        
        // Operaciones de reportes
        Task<IEnumerable<Account>> GetAccountsForBalanceSheetAsync();
        Task<IEnumerable<Account>> GetAccountsForIncomeStatementAsync();
        
        // Operaciones de inicialización
        Task InitializeChartOfAccountsAsync();
        Task<bool> IsChartOfAccountsInitializedAsync();
    }
} 