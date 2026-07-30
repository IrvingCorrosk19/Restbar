using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RestBar.Helpers;

/// <summary>
/// Resolves safe in-app return URLs for POS/KDS exit navigation (no open redirects).
/// </summary>
public static class NavigationHelper
{
    public static string ResolveSafeReturnUrl(
        HttpRequest request,
        IUrlHelper url,
        string? returnUrl,
        string fallbackController = "Home",
        string fallbackAction = "Index")
    {
        var fallback = url.Action(fallbackAction, fallbackController) ?? "/";

        if (string.IsNullOrWhiteSpace(returnUrl))
            return fallback;

        // Absolute URLs to other hosts are rejected
        if (Uri.TryCreate(returnUrl, UriKind.Absolute, out var absolute))
        {
            if (!string.Equals(absolute.Host, request.Host.Host, StringComparison.OrdinalIgnoreCase))
                return fallback;
            return absolute.PathAndQuery;
        }

        // Local relative path only
        if (!returnUrl.StartsWith('/') || returnUrl.StartsWith("//", StringComparison.Ordinal))
            return fallback;

        // Avoid looping back into the same operational surface as "home"
        var path = returnUrl.Split('?', 2)[0];
        if (path.Equals("/Order", StringComparison.OrdinalIgnoreCase)
            || path.Equals("/Order/Index", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/Order/StationOrders", StringComparison.OrdinalIgnoreCase))
            return fallback;

        return returnUrl;
    }
}
