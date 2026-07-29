using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace RestBar.Infrastructure.Foundation;

/// <summary>
/// Tenant context extracted from claims. Foundation for Cash/Purchasing/IDOR-safe APIs.
/// Does not change existing service behavior until callers opt in.
/// </summary>
public sealed class TenantScope
{
    public Guid? UserId { get; init; }
    public Guid? CompanyId { get; init; }
    public Guid? BranchId { get; init; }
    public string? Role { get; init; }
    public bool IsAuthenticated { get; init; }
    public bool IsSuperAdmin =>
        string.Equals(Role, "superadmin", StringComparison.OrdinalIgnoreCase);

    public static TenantScope FromPrincipal(ClaimsPrincipal? principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return new TenantScope { IsAuthenticated = false };
        }

        return new TenantScope
        {
            IsAuthenticated = true,
            UserId = ParseGuid(principal, "UserId") ?? ParseGuid(principal, ClaimTypes.NameIdentifier),
            CompanyId = ParseGuid(principal, "CompanyId"),
            BranchId = ParseGuid(principal, "BranchId"),
            Role = principal.FindFirst("UserRole")?.Value
                   ?? principal.FindFirst(ClaimTypes.Role)?.Value
        };
    }

    public static TenantScope FromHttpContext(IHttpContextAccessor accessor)
        => FromPrincipal(accessor.HttpContext?.User);

    /// <summary>
    /// Returns false if the resource belongs to another company (unless SuperAdmin).
    /// Null resourceCompanyId is treated as inaccessible for non-superadmin (fail closed for new modules).
    /// </summary>
    public bool CanAccessCompany(Guid? resourceCompanyId)
    {
        if (!IsAuthenticated) return false;
        if (IsSuperAdmin) return true;
        if (CompanyId == null || resourceCompanyId == null) return false;
        return CompanyId == resourceCompanyId;
    }

    public bool CanAccessBranch(Guid? resourceBranchId)
    {
        if (!IsAuthenticated) return false;
        if (IsSuperAdmin) return true;
        if (BranchId == null || resourceBranchId == null) return false;
        return BranchId == resourceBranchId;
    }

    public void EnsureCompanyAccess(Guid? resourceCompanyId)
    {
        if (!CanAccessCompany(resourceCompanyId))
            throw new UnauthorizedAccessException("Tenant isolation: company access denied.");
    }

    public void EnsureBranchAccess(Guid? resourceBranchId)
    {
        if (!CanAccessBranch(resourceBranchId))
            throw new UnauthorizedAccessException("Tenant isolation: branch access denied.");
    }

    private static Guid? ParseGuid(ClaimsPrincipal principal, string claimType)
    {
        var value = principal.FindFirst(claimType)?.Value;
        return Guid.TryParse(value, out var id) ? id : null;
    }
}

public interface ITenantScopeAccessor
{
    TenantScope Current { get; }
}

public sealed class TenantScopeAccessor : ITenantScopeAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantScopeAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public TenantScope Current => TenantScope.FromHttpContext(_httpContextAccessor);
}
