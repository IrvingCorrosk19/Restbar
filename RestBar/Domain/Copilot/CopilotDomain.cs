using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using RestBar.Models;

namespace RestBar.Domain.Copilot;

public static class CopilotGuardrails
{
    private static readonly Regex InjectionPattern = new(
        @"ignore\s+(all\s+)?(previous|prior)\s+instructions|system\s*prompt|act\s+as\s+(?:dan|root)|jailbreak|bypass\s+(?:rbac|security)|reveal\s+(?:api\s*key|secrets?)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public const int MaxMessageLength = 2000;

    public static string Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var trimmed = input.Trim();
        if (trimmed.Length > MaxMessageLength)
            trimmed = trimmed[..MaxMessageLength];
        return trimmed;
    }

    public static bool IsPromptInjection(string message) =>
        InjectionPattern.IsMatch(message);

    public static bool LooksLikeRoleSpoof(string message) =>
        Regex.IsMatch(message, @"^\s*(system|assistant)\s*:", RegexOptions.IgnoreCase);
}

public static class CopilotDecisionMath
{
    public static int SeverityRank(BiSeverity s) => s switch
    {
        BiSeverity.Critical => 100,
        BiSeverity.High => 80,
        BiSeverity.Medium => 50,
        BiSeverity.Low => 20,
        _ => 10
    };

    public static decimal EstimateImpact(BiSeverity severity, decimal revenueToday) =>
        severity switch
        {
            BiSeverity.Critical => Math.Max(500m, revenueToday * 0.08m),
            BiSeverity.High => Math.Max(250m, revenueToday * 0.04m),
            BiSeverity.Medium => Math.Max(100m, revenueToday * 0.02m),
            BiSeverity.Low => Math.Max(25m, revenueToday * 0.005m),
            _ => 10m
        };

    public static IReadOnlyList<CopilotDecisionItem> Rank(
        IEnumerable<(BiSeverity Severity, string Title, string Action, string Source)> items,
        decimal revenueToday)
    {
        return items
            .Select(i => new CopilotDecisionItem(
                i.Title,
                i.Action,
                i.Source,
                i.Severity,
                EstimateImpact(i.Severity, revenueToday),
                SeverityRank(i.Severity)))
            .OrderByDescending(d => d.RankScore)
            .ThenByDescending(d => d.EstimatedImpact)
            .Take(5)
            .ToList();
    }
}

public record CopilotDecisionItem(
    string Title,
    string Action,
    string Source,
    BiSeverity Severity,
    decimal EstimatedImpact,
    int RankScore);

public static class CopilotHash
{
    public static string Sha256(string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

public static class CopilotPolicyMap
{
    private static readonly Dictionary<string, HashSet<string>> RolePolicies = new(StringComparer.OrdinalIgnoreCase)
    {
        ["admin"] = new(StringComparer.OrdinalIgnoreCase) { "ReportAccess", "CashAccess", "PurchasingAccess", "CostingAccess", "InventoryAccess" },
        ["manager"] = new(StringComparer.OrdinalIgnoreCase) { "ReportAccess", "CashAccess", "PurchasingAccess", "CostingAccess", "InventoryAccess" },
        ["supervisor"] = new(StringComparer.OrdinalIgnoreCase) { "ReportAccess", "CashAccess", "PurchasingAccess" },
        ["accountant"] = new(StringComparer.OrdinalIgnoreCase) { "ReportAccess", "CashAccess", "PurchasingAccess", "CostingAccess" },
        ["cashier"] = new(StringComparer.OrdinalIgnoreCase) { "CashAccess" },
        ["inventarista"] = new(StringComparer.OrdinalIgnoreCase) { "PurchasingAccess", "InventoryAccess" },
        ["chef"] = new(StringComparer.OrdinalIgnoreCase) { "CostingAccess" },
        ["superadmin"] = new(StringComparer.OrdinalIgnoreCase) { "ReportAccess", "CashAccess", "PurchasingAccess", "CostingAccess", "InventoryAccess", "FranchiseAccess" }
    };

    public static bool HasPolicy(string? role, string policy)
    {
        if (string.IsNullOrWhiteSpace(role)) return false;
        return RolePolicies.TryGetValue(role, out var set) && set.Contains(policy);
    }
}
