using Microsoft.EntityFrameworkCore;
using RestBar.Models;

namespace RestBar.Interfaces;

public interface IPriceScheduleService
{
    Task<decimal> GetEffectiveUnitPriceAsync(Guid productId, Guid? companyId, DateTime? atUtc = null);
    bool IsPolicyActiveNow(DiscountPolicy policy, TimeSpan timeOfDay);
}
