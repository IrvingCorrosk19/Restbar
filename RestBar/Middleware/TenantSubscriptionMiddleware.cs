using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RestBar.Models;

namespace RestBar.Middleware;

/// <summary>
/// Bloquea operación POS cuando empresa o sucursal están inactivas (suspensión SaaS).
/// SuperAdmin y rutas de exportación/lectura histórica permanecen accesibles.
/// </summary>
public class TenantSubscriptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantSubscriptionMiddleware> _logger;

    private static readonly string[] ReadOnlyPrefixes =
    {
        "/Audit", "/Reports", "/AdvancedReports", "/PaymentView/ExportPayments",
        "/PaymentView/GenerateReport", "/PaymentView/Index", "/Home/Index"
    };

    private static readonly string[] PublicPrefixes =
    {
        "/Auth", "/Seed", "/Home/Error", "/css", "/js", "/images", "/lib", "/favicon.ico"
    };

    public TenantSubscriptionMiddleware(RequestDelegate next, ILogger<TenantSubscriptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RestBarContext db)
    {
        var path = context.Request.Path.Value ?? "";

        foreach (var prefix in PublicPrefixes)
        {
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) || path == "/")
            {
                await _next(context);
                return;
            }
        }

        if (!context.User.Identity?.IsAuthenticated == true)
        {
            await _next(context);
            return;
        }

        var role = context.User.FindFirst(ClaimTypes.Role)?.Value?.ToLowerInvariant();
        if (role == "superadmin")
        {
            await _next(context);
            return;
        }

        if (!Guid.TryParse(context.User.FindFirst("CompanyId")?.Value, out var companyId) ||
            !Guid.TryParse(context.User.FindFirst("BranchId")?.Value, out var branchId))
        {
            await _next(context);
            return;
        }

        var tenant = await db.Companies.AsNoTracking()
            .Where(c => c.Id == companyId)
            .Select(c => new { c.IsActive, BranchActive = c.Branches.Where(b => b.Id == branchId).Select(b => b.IsActive).FirstOrDefault() })
            .FirstOrDefaultAsync();

        if (tenant == null)
        {
            await _next(context);
            return;
        }

        if (tenant.IsActive && tenant.BranchActive)
        {
            await _next(context);
            return;
        }

        var isReadOnly = context.Request.Method == "GET" &&
                         ReadOnlyPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));

        if (isReadOnly)
        {
            await _next(context);
            return;
        }

        _logger.LogWarning("[TenantSubscription] Operación bloqueada — tenant suspendido. Path={Path} Company={CompanyId}",
            path, companyId);

        context.Response.StatusCode = 403;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            success = false,
            suspended = true,
            code = "TENANT_SUSPENDED",
            message = "Su cuenta está suspendida por falta de pago o cancelación. Puede consultar reportes históricos o contactar soporte para reactivar."
        });
    }
}
