using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Claims;

namespace RestBar.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly RestBarContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrencyService(RestBarContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<Currency>> GetAllAsync(Guid? companyId = null)
        {
            var targetCompanyId = companyId ?? await GetCurrentUserCompanyIdAsync();
            
            return await _context.Currencies
                .Where(c => c.CompanyId == targetCompanyId && c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Currency?> GetByIdAsync(Guid id)
        {
            return await _context.Currencies
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        }

        public async Task<Currency> CreateAsync(Currency currency)
        {
            currency.Id = Guid.NewGuid();
            // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService
            currency.IsActive = true;

            _context.Currencies.Add(currency);
            await _context.SaveChangesAsync();

            return currency;
        }

        public async Task<Currency> UpdateAsync(Currency currency)
        {
            // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService
            _context.Currencies.Update(currency);
            await _context.SaveChangesAsync();

            return currency;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var currency = await _context.Currencies.FindAsync(id);
                if (currency != null)
                {
                    currency.IsActive = false;
                    // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Currency?> GetDefaultCurrencyAsync(Guid? companyId = null)
        {
            var targetCompanyId = companyId ?? await GetCurrentUserCompanyIdAsync();
            
            return await _context.Currencies
                .FirstOrDefaultAsync(c => c.IsDefault && c.CompanyId == targetCompanyId && c.IsActive);
        }

        public async Task<bool> SetDefaultCurrencyAsync(Guid id, Guid? companyId = null)
        {
            try
            {
                var targetCompanyId = companyId ?? await GetCurrentUserCompanyIdAsync();
                var currency = await _context.Currencies.FindAsync(id);
                
                if (currency != null && currency.CompanyId == targetCompanyId)
                {
                    // Desactivar otras monedas como default
                    var otherCurrencies = await _context.Currencies
                        .Where(c => c.CompanyId == targetCompanyId && c.IsActive)
                        .ToListAsync();

                    foreach (var otherCurrency in otherCurrencies)
                    {
                        otherCurrency.IsDefault = false;
                        otherCurrency.UpdatedAt = DateTime.UtcNow;
                    }

                    // Establecer la nueva moneda default
                    currency.IsDefault = true;
                    // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService

                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Currency?> GetByCodeAsync(string code, Guid? companyId = null)
        {
            var targetCompanyId = companyId ?? await GetCurrentUserCompanyIdAsync();
            
            return await _context.Currencies
                .FirstOrDefaultAsync(c => c.Code == code && c.CompanyId == targetCompanyId && c.IsActive);
        }

        public async Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency, Guid? companyId = null)
        {
            var targetCompanyId = companyId ?? await GetCurrentUserCompanyIdAsync();
            
            var fromCurr = await GetByCodeAsync(fromCurrency, targetCompanyId);
            var toCurr = await GetByCodeAsync(toCurrency, targetCompanyId);

            if (fromCurr != null && toCurr != null)
            {
                // Conversión simple usando las tasas de cambio
                return amount * (toCurr.ExchangeRate / fromCurr.ExchangeRate);
            }

            return amount; // Si no se pueden convertir, devolver el monto original
        }

        private async Task<Guid?> GetCurrentUserCompanyIdAsync()
        {
            var userEmail = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail)) return null;

            var user = await _context.Users
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            return user?.Branch?.CompanyId;
        }
    }
} 