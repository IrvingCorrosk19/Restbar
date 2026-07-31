namespace RestBar.Domain.Analytics;

/// <summary>Canonical analytics filter. Tenant IDs must come from auth, never unbound client trust.</summary>
public sealed class AnalyticsFilter
{
    public Guid CompanyId { get; init; }
    public Guid BranchId { get; init; }
    public bool CrossBranch { get; init; }
    public DateTime StartUtc { get; init; }
    public DateTime EndUtc { get; init; }
    public DateTime? CompareStartUtc { get; init; }
    public DateTime? CompareEndUtc { get; init; }
    public string TimeZone { get; init; } = "UTC";
    public string Currency { get; init; } = "USD";
    public Guid? AreaId { get; init; }
    public Guid? StationId { get; init; }
    public Guid? WaiterUserId { get; init; }
    public Guid? CashierUserId { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? ProductId { get; init; }
    public Guid? SupplierId { get; init; }
    public string? PaymentMethod { get; init; }
    public string? QuickPeriod { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
    public string? Sort { get; init; }
    public string? Search { get; init; }
}

public enum KpiAvailability
{
    Available,
    AvailableWithLimitations,
    NotAvailable,
    RequiresDataCorrection,
    RequiresModelChange
}

public sealed record KpiDefinition(
    string Code,
    string Name,
    string Description,
    string Formula,
    string DataSource,
    string Unit,
    string Permission,
    KpiAvailability Availability,
    string? Limitation = null,
    string? ExpectedRange = null,
    string? Interpretation = null);

public sealed record AnalyticsReportDefinition(
    string Key,
    string Title,
    string Description,
    string Category,
    string Permission,
    string ProcedureOrSource,
    bool ChartEnabled,
    KpiAvailability Availability,
    string? Limitation = null);

public sealed record AnalyticsDecision(
    string Code,
    string Problem,
    string MetricCode,
    decimal? CurrentValue,
    decimal? ReferenceValue,
    string? ImpactEstimate,
    string Priority,
    Guid? BranchId,
    string PeriodLabel,
    string SuggestedAction,
    string RelatedReportKey,
    bool IsInference);

public sealed record AnalyticsLiveSnapshot(
    decimal SalesToday,
    int OpenOrders,
    int DelayedOrders,
    int OccupiedTables,
    int FreeTables,
    int OpenCashSessions,
    int CashIncidents,
    int CriticalStock,
    int ZeroStock,
    int OverduePurchaseOrders,
    int WasteEventsToday,
    DateTime GeneratedAtUtc);

public static class AnalyticsPeriodHelper
{
    public static (DateTime start, DateTime end) Resolve(string? quick, DateTime? start, DateTime? end, DateTime utcNow)
    {
        var today = DateTime.SpecifyKind(utcNow.Date, DateTimeKind.Utc);
        return (quick?.ToLowerInvariant()) switch
        {
            "today" => (today, today.AddDays(1)),
            "yesterday" => (today.AddDays(-1), today),
            "this_week" => (today.AddDays(-(int)today.DayOfWeek), today.AddDays(1)),
            "last_week" => (today.AddDays(-(int)today.DayOfWeek - 7), today.AddDays(-(int)today.DayOfWeek)),
            "this_month" => (new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc), today.AddDays(1)),
            "last_month" => (new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1),
                             new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc)),
            "last_7" => (today.AddDays(-7), today.AddDays(1)),
            "last_30" => (today.AddDays(-30), today.AddDays(1)),
            "last_90" => (today.AddDays(-90), today.AddDays(1)),
            "this_year" => (new DateTime(today.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc), today.AddDays(1)),
            "last_year" => (new DateTime(today.Year - 1, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                            new DateTime(today.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            _ => (
                (start ?? today.AddDays(-30)).ToUniversalTime(),
                (end ?? today.AddDays(1)).ToUniversalTime())
        };
    }

    public static (DateTime start, DateTime end) PreviousEqualLength(DateTime start, DateTime end)
    {
        var len = end - start;
        return (start - len, start);
    }
}
