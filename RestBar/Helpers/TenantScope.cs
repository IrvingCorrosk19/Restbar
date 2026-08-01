using Microsoft.EntityFrameworkCore;
using RestBar.Models;
using System.Security.Claims;

namespace RestBar.Helpers;

/// <summary>Resolves branch/company scope from claims; never trusts client IDs across tenants.</summary>
public static class TenantScope
{
    public static Guid? CompanyId(ClaimsPrincipal user) =>
        Guid.TryParse(user.FindFirst("CompanyId")?.Value, out var id) ? id : null;

    public static Guid? BranchId(ClaimsPrincipal user) =>
        Guid.TryParse(user.FindFirst("BranchId")?.Value, out var id) ? id : null;

    public static bool HasGlobalTenantAccess(ClaimsPrincipal user)
    {
        var role = user.FindFirst(ClaimTypes.Role)?.Value ?? user.FindFirst("UserRole")?.Value;
        if (string.Equals(role, "superadmin", StringComparison.OrdinalIgnoreCase)) return true;
        return string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase) && BranchId(user) == null;
    }

    public static async Task<Guid?> ResolveBranchIdAsync(RestBarContext db, ClaimsPrincipal user, Guid? requestedBranchId)
    {
        var companyId = CompanyId(user);
        var claimBranch = BranchId(user);

        if (!HasGlobalTenantAccess(user))
            return claimBranch;

        if (!requestedBranchId.HasValue || requestedBranchId == Guid.Empty)
            return claimBranch;

        if (companyId == null)
            return claimBranch;

        var ok = await db.Branches.AsNoTracking()
            .AnyAsync(b => b.Id == requestedBranchId.Value && b.CompanyId == companyId.Value);
        return ok ? requestedBranchId : claimBranch;
    }

    public static Guid? ResolveCompanyId(ClaimsPrincipal user, Guid? requestedCompanyId)
    {
        var claim = CompanyId(user);
        var role = user.FindFirst(ClaimTypes.Role)?.Value ?? user.FindFirst("UserRole")?.Value;
        if (string.Equals(role, "superadmin", StringComparison.OrdinalIgnoreCase))
            return requestedCompanyId ?? claim;
        return claim;
    }
}
