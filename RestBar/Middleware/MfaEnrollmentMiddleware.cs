using Microsoft.EntityFrameworkCore;
using RestBar.Models;

namespace RestBar.Middleware;

/// <summary>Forces privileged users without MFA enrollment to complete /Auth/MfaSetup when FeatureFlags:RequireMfa is true.</summary>
public class MfaEnrollmentMiddleware
{
    private static readonly HashSet<string> Privileged = new(StringComparer.OrdinalIgnoreCase)
    {
        "superadmin", "admin", "manager", "supervisor"
    };

    private readonly RequestDelegate _next;

    public MfaEnrollmentMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, RestBarContext db, IConfiguration config)
    {
        if (!config.GetValue("FeatureFlags:RequireMfa", false))
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? "";
        if (context.User.Identity?.IsAuthenticated != true
            || path.StartsWith("/Auth/MfaSetup", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/Auth/Logout", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/Auth/Login", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/Auth/MfaChallenge", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/health", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/css", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/js", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/lib", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/images", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var role = context.User.FindFirst("UserRole")?.Value
                   ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(role) || !Privileged.Contains(role))
        {
            await _next(context);
            return;
        }

        var mfaClaim = context.User.FindFirst("MfaEnabled")?.Value;
        if (string.Equals(mfaClaim, "true", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Claim missing/false: confirm against DB (covers cookies issued before MFA claim existed).
        if (Guid.TryParse(context.User.FindFirst("UserId")?.Value, out var userId))
        {
            var enabled = await db.Users.AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.MfaEnabled)
                .FirstOrDefaultAsync();
            if (enabled)
            {
                await _next(context);
                return;
            }
        }

        context.Response.Redirect("/Auth/MfaSetup");
    }
}
