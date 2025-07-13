using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Claims;

namespace RestBar.Services
{
    public class DiscountPolicyService : IDiscountPolicyService
    {
        private readonly RestBarContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DiscountPolicyService(RestBarContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<DiscountPolicy>> GetAllAsync(Guid? companyId = null)
        {
            var targetCompanyId = companyId ?? await GetCurrentUserCompanyIdAsync();
            
            return await _context.DiscountPolicies
                .Where(d => d.CompanyId == targetCompanyId && d.IsActive)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<DiscountPolicy?> GetByIdAsync(Guid id)
        {
            return await _context.DiscountPolicies
                .FirstOrDefaultAsync(d => d.Id == id && d.IsActive);
        }

        public async Task<DiscountPolicy> CreateAsync(DiscountPolicy discountPolicy)
        {
            discountPolicy.Id = Guid.NewGuid();
            // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService
            discountPolicy.IsActive = true;

            _context.DiscountPolicies.Add(discountPolicy);
            await _context.SaveChangesAsync();

            return discountPolicy;
        }

        public async Task<DiscountPolicy> UpdateAsync(DiscountPolicy discountPolicy)
        {
            // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService
            _context.DiscountPolicies.Update(discountPolicy);
            await _context.SaveChangesAsync();

            return discountPolicy;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var discountPolicy = await _context.DiscountPolicies.FindAsync(id);
                if (discountPolicy != null)
                {
                    discountPolicy.IsActive = false;
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

        public async Task<decimal> CalculateDiscountAsync(decimal amount, Guid discountPolicyId)
        {
            var discountPolicy = await GetByIdAsync(discountPolicyId);
            if (discountPolicy != null)
            {
                var discountAmount = amount * (discountPolicy.DiscountPercentage / 100m);
                
                // Aplicar límite máximo si está configurado
                if (discountPolicy.MaximumDiscountAmount.HasValue && discountAmount > discountPolicy.MaximumDiscountAmount.Value)
                {
                    discountAmount = discountPolicy.MaximumDiscountAmount.Value;
                }
                
                return discountAmount;
            }
            return 0m;
        }

        public async Task<IEnumerable<DiscountPolicy>> GetAvailableDiscountsAsync(decimal orderAmount, Guid? companyId = null)
        {
            var targetCompanyId = companyId ?? await GetCurrentUserCompanyIdAsync();
            
            return await _context.DiscountPolicies
                .Where(d => d.CompanyId == targetCompanyId && d.IsActive && 
                           (!d.MinimumOrderAmount.HasValue || orderAmount >= d.MinimumOrderAmount.Value))
                .OrderBy(d => d.DiscountPercentage)
                .ToListAsync();
        }

        public async Task<bool> ValidateDiscountAsync(Guid discountPolicyId, decimal orderAmount)
        {
            var discountPolicy = await GetByIdAsync(discountPolicyId);
            if (discountPolicy != null)
            {
                // Verificar monto mínimo si está configurado
                if (discountPolicy.MinimumOrderAmount.HasValue && orderAmount < discountPolicy.MinimumOrderAmount.Value)
                {
                    return false;
                }
                
                return true;
            }
            return false;
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