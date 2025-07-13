using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Claims;

namespace RestBar.Services
{
    public class TaxRateService : ITaxRateService
    {
        private readonly RestBarContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TaxRateService(RestBarContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<TaxRate>> GetAllAsync(Guid? companyId = null)
        {
            var targetCompanyId = companyId ?? await GetCurrentUserCompanyIdAsync();
            
            return await _context.TaxRates
                .Where(t => t.CompanyId == targetCompanyId && t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<TaxRate?> GetByIdAsync(Guid id)
        {
            return await _context.TaxRates
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
        }

        public async Task<TaxRate> CreateAsync(TaxRate taxRate)
        {
            taxRate.Id = Guid.NewGuid();
            // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService
            taxRate.IsActive = true;

            _context.TaxRates.Add(taxRate);
            await _context.SaveChangesAsync();

            return taxRate;
        }

        public async Task<TaxRate> UpdateAsync(TaxRate taxRate)
        {
            // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService
            _context.TaxRates.Update(taxRate);
            await _context.SaveChangesAsync();

            return taxRate;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var taxRate = await _context.TaxRates.FindAsync(id);
                if (taxRate != null)
                {
                    taxRate.IsActive = false;
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

        public async Task<TaxRate?> GetByCodeAsync(string taxCode, Guid? companyId = null)
        {
            var targetCompanyId = companyId ?? await GetCurrentUserCompanyIdAsync();
            
            return await _context.TaxRates
                .FirstOrDefaultAsync(t => t.TaxCode == taxCode && t.CompanyId == targetCompanyId && t.IsActive);
        }

        public async Task<decimal> CalculateTaxAsync(decimal amount, Guid taxRateId)
        {
            var taxRate = await GetByIdAsync(taxRateId);
            if (taxRate != null)
            {
                return amount * (taxRate.Rate / 100m);
            }
            return 0m;
        }

        public async Task<decimal> CalculateTaxAsync(decimal amount, string taxCode, Guid? companyId = null)
        {
            var taxRate = await GetByCodeAsync(taxCode, companyId);
            if (taxRate != null)
            {
                return amount * (taxRate.Rate / 100m);
            }
            return 0m;
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