using RestBar.Domain.Analytics;
using RestBar.Services.Analytics;

namespace RestBar.Tests.Analytics;

public class AnalyticsCatalogTests
{
    [Fact]
    public void Kpi_catalog_marks_unavailable_metrics_explicitly()
    {
        var tax = KpiCatalog.Get("EXE.TAX");
        Assert.NotNull(tax);
        Assert.Equal(KpiAvailability.NotAvailable, tax!.Availability);

        var guests = KpiCatalog.Get("EXE.GUESTS");
        Assert.Equal(KpiAvailability.RequiresModelChange, guests!.Availability);

        var reserved = KpiCatalog.Get("INV.RESERVED");
        Assert.Equal(KpiAvailability.NotAvailable, reserved!.Availability);
    }

    [Fact]
    public void Report_catalog_has_core_decision_reports()
    {
        Assert.NotNull(AnalyticsReportCatalog.Get("executive-summary"));
        Assert.NotNull(AnalyticsReportCatalog.Get("cash-variance"));
        Assert.NotNull(AnalyticsReportCatalog.Get("menu-engineering"));
        Assert.Contains(AnalyticsReportCatalog.All, r => r.Key == "sales-hour");
    }

    [Fact]
    public void Period_helper_last_30_and_previous_length()
    {
        var now = new DateTime(2026, 7, 30, 12, 0, 0, DateTimeKind.Utc);
        var (start, end) = AnalyticsPeriodHelper.Resolve("last_30", null, null, now);
        Assert.Equal(now.Date.AddDays(-30), start);
        Assert.Equal(now.Date.AddDays(1), end);

        var (ps, pe) = AnalyticsPeriodHelper.PreviousEqualLength(start, end);
        Assert.Equal(end - start, pe - ps);
        Assert.Equal(start, pe);
    }

    [Fact]
    public void Export_flatten_handles_executive_wrapper()
    {
        var data = new { available = true, executive = new { revenue = 10m, ordersCompleted = 2 } };
        var rows = AnalyticsDataFlatten.Flatten(data);
        Assert.Single(rows);
        Assert.True(rows[0].ContainsKey("revenue"));
    }
}
