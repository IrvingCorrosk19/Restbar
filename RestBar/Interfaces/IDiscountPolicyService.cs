using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface IDiscountPolicyService
    {
        Task<IEnumerable<DiscountPolicy>> GetAllAsync(Guid? companyId = null);
        Task<DiscountPolicy?> GetByIdAsync(Guid id);
        Task<DiscountPolicy> CreateAsync(DiscountPolicy discountPolicy);
        Task<DiscountPolicy> UpdateAsync(DiscountPolicy discountPolicy);
        Task<bool> DeleteAsync(Guid id);
        Task<decimal> CalculateDiscountAsync(decimal amount, Guid discountPolicyId);
        Task<IEnumerable<DiscountPolicy>> GetAvailableDiscountsAsync(decimal orderAmount, Guid? companyId = null);
        Task<bool> ValidateDiscountAsync(Guid discountPolicyId, decimal orderAmount);
    }
} 