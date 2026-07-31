using System.Security.Claims;
using RestBar.Domain.Analytics;
using RestBar.Interfaces;

namespace RestBar.Services.Analytics;

public sealed class AnalyticsScopeService : IAnalyticsScopeService
{
    public AnalyticsFilter Resolve(ClaimsPrincipal user, AnalyticsFilterRequest req, bool allowCrossBranch)
    {
        var companyId = Guid.Parse(user.FindFirst("CompanyId")?.Value
            ?? throw new UnauthorizedAccessException("CompanyId claim missing"));
        var claimBranch = Guid.Parse(user.FindFirst("BranchId")?.Value
            ?? throw new UnauthorizedAccessException("BranchId claim missing"));

        var branchId = claimBranch;
        var cross = false;
        if (req.BranchId.HasValue && req.BranchId.Value != Guid.Empty && req.BranchId.Value != claimBranch)
        {
            if (!allowCrossBranch)
                throw new UnauthorizedAccessException("Cross-branch analytics not permitted");
            branchId = req.BranchId.Value;
            cross = true;
        }

        var (start, end) = AnalyticsPeriodHelper.Resolve(
            string.IsNullOrWhiteSpace(req.Period) ? "last_30" : req.Period,
            req.Start,
            req.End,
            DateTime.UtcNow);
        if (end <= start)
        {
            // Never hard-fail executive/DI calls on bad client ranges — clamp to a valid day window.
            start = end.AddDays(-1);
            if (end <= start)
            {
                var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
                start = today.AddDays(-30);
                end = today.AddDays(1);
            }
        }

        DateTime? cStart = null, cEnd = null;
        if (req.Compare)
        {
            var prev = AnalyticsPeriodHelper.PreviousEqualLength(start, end);
            cStart = prev.start;
            cEnd = prev.end;
        }

        return new AnalyticsFilter
        {
            CompanyId = companyId,
            BranchId = branchId,
            CrossBranch = cross || allowCrossBranch,
            StartUtc = start,
            EndUtc = end,
            CompareStartUtc = cStart,
            CompareEndUtc = cEnd,
            TimeZone = string.IsNullOrWhiteSpace(req.TimeZone) ? "UTC" : req.TimeZone!,
            Currency = string.IsNullOrWhiteSpace(req.Currency) ? "USD" : req.Currency!,
            AreaId = req.AreaId,
            StationId = req.StationId,
            WaiterUserId = req.WaiterUserId,
            CategoryId = req.CategoryId,
            ProductId = req.ProductId,
            SupplierId = req.SupplierId,
            PaymentMethod = req.PaymentMethod,
            QuickPeriod = req.Period,
            Page = Math.Max(1, req.Page),
            PageSize = Math.Clamp(req.PageSize <= 0 ? 50 : req.PageSize, 1, 500),
            Sort = req.Sort,
            Search = req.Search
        };
    }
}
