using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services;

public class PriceScheduleService : IPriceScheduleService
{
    private readonly RestBarContext _context;

    public PriceScheduleService(RestBarContext context) => _context = context;

    public bool IsPolicyActiveNow(DiscountPolicy policy, TimeSpan timeOfDay)
    {
        if (!policy.ValidFromTime.HasValue || !policy.ValidUntilTime.HasValue)
            return true;

        var from = policy.ValidFromTime.Value;
        var until = policy.ValidUntilTime.Value;
        if (from <= until)
            return timeOfDay >= from && timeOfDay <= until;
        return timeOfDay >= from || timeOfDay <= until;
    }

    public async Task<decimal> GetEffectiveUnitPriceAsync(Guid productId, Guid? companyId, DateTime? atUtc = null)
    {
        var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null) return 0;

        var price = product.Price;
        var when = atUtc ?? DateTime.UtcNow;
        var time = when.TimeOfDay;

        var policies = await _context.DiscountPolicies.AsNoTracking()
            .Where(p => p.IsActive && (!companyId.HasValue || p.CompanyId == companyId))
            .Where(p => p.ValidFromTime != null && p.ValidUntilTime != null)
            .ToListAsync();

        var activePolicy = policies
            .Where(p => IsPolicyActiveNow(p, time))
            .OrderByDescending(p => p.DiscountPercentage)
            .FirstOrDefault();

        if (activePolicy != null)
        {
            price -= price * (activePolicy.DiscountPercentage / 100m);
            if (activePolicy.MaximumDiscountAmount.HasValue)
            {
                var disc = product.Price - price;
                if (disc > activePolicy.MaximumDiscountAmount.Value)
                    price = product.Price - activePolicy.MaximumDiscountAmount.Value;
            }
        }

        return Math.Max(0, Math.Round(price, 2));
    }
}
