namespace RestBar.Infrastructure.Foundation;

/// <summary>Central gate for destructive/demo seed endpoints.</summary>
public static class SeedEnvironmentGate
{
    /// <summary>
    /// Seed HTTP APIs are allowed only in Development.
    /// Staging and Production must never expose AllowAnonymous seed.
    /// </summary>
    public static bool IsSeedAllowed(IWebHostEnvironment env)
        => env.IsDevelopment();
}
