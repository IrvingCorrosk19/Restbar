using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services
{
    public class AccountService : IAccountService
    {
        private readonly RestBarContext _context;
        private readonly ILogger<AccountService> _logger;

        public AccountService(RestBarContext context, ILogger<AccountService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Account>> GetAllAccountsAsync()
        {
            return await _context.Accounts
                .Include(a => a.ParentAccount)
                .Include(a => a.SubAccounts)
                .OrderBy(a => a.Code)
                .ToListAsync();
        }

        public async Task<Account?> GetAccountByIdAsync(Guid id)
        {
            return await _context.Accounts
                .Include(a => a.ParentAccount)
                .Include(a => a.SubAccounts)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Account?> GetAccountByCodeAsync(string code)
        {
            return await _context.Accounts
                .Include(a => a.ParentAccount)
                .Include(a => a.SubAccounts)
                .FirstOrDefaultAsync(a => a.Code == code);
        }

        public async Task<Account> CreateAccountAsync(Account account)
        {
            // Validar código único
            if (!await IsCodeUniqueAsync(account.Code))
            {
                throw new InvalidOperationException($"El código de cuenta '{account.Code}' ya existe.");
            }

            // Validar cuenta padre si existe
            if (account.ParentAccountId.HasValue)
            {
                var parentAccount = await GetAccountByIdAsync(account.ParentAccountId.Value);
                if (parentAccount == null)
                {
                    throw new InvalidOperationException("La cuenta padre especificada no existe.");
                }
            }

            account.Id = Guid.NewGuid();
            account.CreatedAt = DateTime.UtcNow;
            account.IsActive = true;

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Cuenta creada: {account.Code} - {account.Name}");
            return account;
        }

        public async Task<Account> UpdateAccountAsync(Account account)
        {
            var existingAccount = await GetAccountByIdAsync(account.Id);
            if (existingAccount == null)
            {
                throw new InvalidOperationException("La cuenta no existe.");
            }

            // Validar código único (excluyendo la cuenta actual)
            if (!await IsCodeUniqueAsync(account.Code, account.Id))
            {
                throw new InvalidOperationException($"El código de cuenta '{account.Code}' ya existe.");
            }

            // No permitir modificar cuentas del sistema
            if (existingAccount.IsSystem)
            {
                throw new InvalidOperationException("No se pueden modificar las cuentas del sistema.");
            }

            existingAccount.Code = account.Code;
            existingAccount.Name = account.Name;
            existingAccount.Description = account.Description;
            existingAccount.Type = account.Type;
            existingAccount.Category = account.Category;
            existingAccount.Nature = account.Nature;
            existingAccount.ParentAccountId = account.ParentAccountId;
            existingAccount.IsActive = account.IsActive;
            existingAccount.UpdatedAt = DateTime.UtcNow;
            existingAccount.UpdatedBy = account.UpdatedBy;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Cuenta actualizada: {account.Code} - {account.Name}");
            return existingAccount;
        }

        public async Task<bool> DeleteAccountAsync(Guid id)
        {
            var account = await GetAccountByIdAsync(id);
            if (account == null)
            {
                return false;
            }

            // No permitir eliminar cuentas del sistema
            if (account.IsSystem)
            {
                throw new InvalidOperationException("No se pueden eliminar las cuentas del sistema.");
            }

            // Verificar si se puede eliminar
            if (!await CanDeleteAccountAsync(id))
            {
                throw new InvalidOperationException("No se puede eliminar la cuenta porque tiene movimientos o subcuentas.");
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Cuenta eliminada: {account.Code} - {account.Name}");
            return true;
        }

        public async Task<IEnumerable<Account>> GetRootAccountsAsync()
        {
            return await _context.Accounts
                .Include(a => a.SubAccounts)
                .Where(a => a.ParentAccountId == null)
                .OrderBy(a => a.Code)
                .ToListAsync();
        }

        public async Task<IEnumerable<Account>> GetSubAccountsAsync(Guid parentId)
        {
            return await _context.Accounts
                .Include(a => a.SubAccounts)
                .Where(a => a.ParentAccountId == parentId)
                .OrderBy(a => a.Code)
                .ToListAsync();
        }

        public async Task<IEnumerable<Account>> GetAccountTreeAsync()
        {
            var rootAccounts = await GetRootAccountsAsync();
            return rootAccounts;
        }

        public async Task<Account?> GetParentAccountAsync(Guid accountId)
        {
            var account = await GetAccountByIdAsync(accountId);
            return account?.ParentAccount;
        }

        public async Task<IEnumerable<Account>> GetAccountsByTypeAsync(AccountType type)
        {
            return await _context.Accounts
                .Include(a => a.ParentAccount)
                .Where(a => a.Type == type && a.IsActive)
                .OrderBy(a => a.Code)
                .ToListAsync();
        }

        public async Task<IEnumerable<Account>> GetAccountsByCategoryAsync(AccountCategory category)
        {
            return await _context.Accounts
                .Include(a => a.ParentAccount)
                .Where(a => a.Category == category && a.IsActive)
                .OrderBy(a => a.Code)
                .ToListAsync();
        }

        public async Task<IEnumerable<Account>> GetActiveAccountsAsync()
        {
            return await _context.Accounts
                .Include(a => a.ParentAccount)
                .Where(a => a.IsActive)
                .OrderBy(a => a.Code)
                .ToListAsync();
        }

        public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null)
        {
            var query = _context.Accounts.Where(a => a.Code == code);
            
            if (excludeId.HasValue)
            {
                query = query.Where(a => a.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<bool> CanDeleteAccountAsync(Guid id)
        {
            // Verificar si tiene subcuentas
            var hasSubAccounts = await _context.Accounts.AnyAsync(a => a.ParentAccountId == id);
            if (hasSubAccounts)
            {
                return false;
            }

            // Verificar si tiene movimientos (esto se implementará cuando tengamos JournalEntryDetails)
            // var hasMovements = await _context.JournalEntryDetails.AnyAsync(d => d.AccountId == id);
            // if (hasMovements)
            // {
            //     return false;
            // }

            return true;
        }

        public async Task<decimal> GetAccountBalanceAsync(Guid accountId, DateTime? asOfDate = null)
        {
            var query = _context.JournalEntryDetails
                .Include(d => d.JournalEntry)
                .Where(d => d.AccountId == accountId && d.JournalEntry.Status == JournalEntryStatus.Posted);

            if (asOfDate.HasValue)
            {
                query = query.Where(d => d.JournalEntry.EntryDate <= asOfDate.Value);
            }

            var details = await query.ToListAsync();

            var balance = details.Sum(d => d.DebitAmount - d.CreditAmount);

            // Ajustar según la naturaleza de la cuenta
            var account = await GetAccountByIdAsync(accountId);
            if (account?.Nature == AccountNature.Credit)
            {
                balance = -balance;
            }

            return balance;
        }

        public async Task<Dictionary<Guid, decimal>> GetAccountsBalanceAsync(IEnumerable<Guid> accountIds, DateTime? asOfDate = null)
        {
            var balances = new Dictionary<Guid, decimal>();

            foreach (var accountId in accountIds)
            {
                var balance = await GetAccountBalanceAsync(accountId, asOfDate);
                balances[accountId] = balance;
            }

            return balances;
        }

        public async Task<IEnumerable<Account>> GetAccountsForBalanceSheetAsync()
        {
            return await _context.Accounts
                .Include(a => a.ParentAccount)
                .Where(a => a.IsActive && (a.Type == AccountType.Asset || a.Type == AccountType.Liability || a.Type == AccountType.Equity))
                .OrderBy(a => a.Type)
                .ThenBy(a => a.Code)
                .ToListAsync();
        }

        public async Task<IEnumerable<Account>> GetAccountsForIncomeStatementAsync()
        {
            return await _context.Accounts
                .Include(a => a.ParentAccount)
                .Where(a => a.IsActive && (a.Type == AccountType.Income || a.Type == AccountType.Expense))
                .OrderBy(a => a.Type)
                .ThenBy(a => a.Code)
                .ToListAsync();
        }

        public async Task InitializeChartOfAccountsAsync()
        {
            if (await IsChartOfAccountsInitializedAsync())
            {
                return;
            }

            var accounts = new List<Account>
            {
                // Activos
                new Account { Code = "1000", Name = "Activos", Type = AccountType.Asset, Category = AccountCategory.CurrentAssets, Nature = AccountNature.Debit, IsSystem = true },
                new Account { Code = "1100", Name = "Activos Corrientes", Type = AccountType.Asset, Category = AccountCategory.CurrentAssets, Nature = AccountNature.Debit, IsSystem = true },
                new Account { Code = "1110", Name = "Efectivo y Equivalentes", Type = AccountType.Asset, Category = AccountCategory.CurrentAssets, Nature = AccountNature.Debit, IsSystem = true },
                new Account { Code = "1120", Name = "Cuentas por Cobrar", Type = AccountType.Asset, Category = AccountCategory.CurrentAssets, Nature = AccountNature.Debit, IsSystem = true },
                new Account { Code = "1130", Name = "Inventarios", Type = AccountType.Asset, Category = AccountCategory.CurrentAssets, Nature = AccountNature.Debit, IsSystem = true },
                new Account { Code = "1200", Name = "Activos No Corrientes", Type = AccountType.Asset, Category = AccountCategory.NonCurrentAssets, Nature = AccountNature.Debit, IsSystem = true },
                new Account { Code = "1210", Name = "Propiedades y Equipos", Type = AccountType.Asset, Category = AccountCategory.NonCurrentAssets, Nature = AccountNature.Debit, IsSystem = true },

                // Pasivos
                new Account { Code = "2000", Name = "Pasivos", Type = AccountType.Liability, Category = AccountCategory.CurrentLiabilities, Nature = AccountNature.Credit, IsSystem = true },
                new Account { Code = "2100", Name = "Pasivos Corrientes", Type = AccountType.Liability, Category = AccountCategory.CurrentLiabilities, Nature = AccountNature.Credit, IsSystem = true },
                new Account { Code = "2110", Name = "Cuentas por Pagar", Type = AccountType.Liability, Category = AccountCategory.CurrentLiabilities, Nature = AccountNature.Credit, IsSystem = true },
                new Account { Code = "2120", Name = "Impuestos por Pagar", Type = AccountType.Liability, Category = AccountCategory.CurrentLiabilities, Nature = AccountNature.Credit, IsSystem = true },
                new Account { Code = "2200", Name = "Pasivos No Corrientes", Type = AccountType.Liability, Category = AccountCategory.NonCurrentLiabilities, Nature = AccountNature.Credit, IsSystem = true },

                // Capital
                new Account { Code = "3000", Name = "Capital", Type = AccountType.Equity, Category = AccountCategory.Capital, Nature = AccountNature.Credit, IsSystem = true },
                new Account { Code = "3100", Name = "Capital Social", Type = AccountType.Equity, Category = AccountCategory.Capital, Nature = AccountNature.Credit, IsSystem = true },
                new Account { Code = "3200", Name = "Utilidades Retenidas", Type = AccountType.Equity, Category = AccountCategory.RetainedEarnings, Nature = AccountNature.Credit, IsSystem = true },

                // Ingresos
                new Account { Code = "4000", Name = "Ingresos", Type = AccountType.Income, Category = AccountCategory.OperatingIncome, Nature = AccountNature.Credit, IsSystem = true },
                new Account { Code = "4100", Name = "Ingresos Operativos", Type = AccountType.Income, Category = AccountCategory.OperatingIncome, Nature = AccountNature.Credit, IsSystem = true },
                new Account { Code = "4110", Name = "Ventas de Comida", Type = AccountType.Income, Category = AccountCategory.OperatingIncome, Nature = AccountNature.Credit, IsSystem = true },
                new Account { Code = "4120", Name = "Ventas de Bebidas", Type = AccountType.Income, Category = AccountCategory.OperatingIncome, Nature = AccountNature.Credit, IsSystem = true },
                new Account { Code = "4200", Name = "Ingresos No Operativos", Type = AccountType.Income, Category = AccountCategory.NonOperatingIncome, Nature = AccountNature.Credit, IsSystem = true },

                // Gastos
                new Account { Code = "5000", Name = "Gastos", Type = AccountType.Expense, Category = AccountCategory.OperatingExpenses, Nature = AccountNature.Debit, IsSystem = true },
                new Account { Code = "5100", Name = "Gastos Operativos", Type = AccountType.Expense, Category = AccountCategory.OperatingExpenses, Nature = AccountNature.Debit, IsSystem = true },
                new Account { Code = "5110", Name = "Costo de Ventas", Type = AccountType.Expense, Category = AccountCategory.OperatingExpenses, Nature = AccountNature.Debit, IsSystem = true },
                new Account { Code = "5120", Name = "Gastos de Personal", Type = AccountType.Expense, Category = AccountCategory.OperatingExpenses, Nature = AccountNature.Debit, IsSystem = true },
                new Account { Code = "5130", Name = "Gastos Administrativos", Type = AccountType.Expense, Category = AccountCategory.OperatingExpenses, Nature = AccountNature.Debit, IsSystem = true },
                new Account { Code = "5200", Name = "Gastos No Operativos", Type = AccountType.Expense, Category = AccountCategory.NonOperatingExpenses, Nature = AccountNature.Debit, IsSystem = true }
            };

            // Establecer jerarquías
            var accountDict = accounts.ToDictionary(a => a.Code);

            // Activos Corrientes
            accountDict["1100"].ParentAccountId = accountDict["1000"].Id;
            accountDict["1110"].ParentAccountId = accountDict["1100"].Id;
            accountDict["1120"].ParentAccountId = accountDict["1100"].Id;
            accountDict["1130"].ParentAccountId = accountDict["1100"].Id;

            // Activos No Corrientes
            accountDict["1210"].ParentAccountId = accountDict["1200"].Id;

            // Pasivos Corrientes
            accountDict["2100"].ParentAccountId = accountDict["2000"].Id;
            accountDict["2110"].ParentAccountId = accountDict["2100"].Id;
            accountDict["2120"].ParentAccountId = accountDict["2100"].Id;

            // Capital
            accountDict["3100"].ParentAccountId = accountDict["3000"].Id;
            accountDict["3200"].ParentAccountId = accountDict["3000"].Id;

            // Ingresos Operativos
            accountDict["4100"].ParentAccountId = accountDict["4000"].Id;
            accountDict["4110"].ParentAccountId = accountDict["4100"].Id;
            accountDict["4120"].ParentAccountId = accountDict["4100"].Id;

            // Gastos Operativos
            accountDict["5100"].ParentAccountId = accountDict["5000"].Id;
            accountDict["5110"].ParentAccountId = accountDict["5100"].Id;
            accountDict["5120"].ParentAccountId = accountDict["5100"].Id;
            accountDict["5130"].ParentAccountId = accountDict["5100"].Id;

            foreach (var account in accounts)
            {
                account.Id = Guid.NewGuid();
                account.CreatedAt = DateTime.UtcNow;
                account.CreatedBy = "System";
            }

            _context.Accounts.AddRange(accounts);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Catálogo de cuentas inicializado correctamente");
        }

        public async Task<bool> IsChartOfAccountsInitializedAsync()
        {
            return await _context.Accounts.AnyAsync();
        }
    }
} 