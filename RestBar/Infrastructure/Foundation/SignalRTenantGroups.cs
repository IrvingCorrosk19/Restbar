namespace RestBar.Infrastructure.Foundation;

/// <summary>
/// SignalR group names scoped by company to prevent cross-tenant fan-out.
/// </summary>
public static class SignalRTenantGroups
{
    public static string Kitchen(Guid companyId) => $"c_{companyId:N}_kitchen";
    public static string Orders(Guid companyId) => $"c_{companyId:N}_orders";
    public static string TableAll(Guid companyId) => $"c_{companyId:N}_table_all";
    public static string Stock(Guid companyId) => $"c_{companyId:N}_stock_updates";
    public static string CashDashboard(Guid companyId) => $"c_{companyId:N}_cash_dashboard";
    public static string Station(Guid companyId, string stationType) =>
        $"c_{companyId:N}_station_{stationType.Trim().ToLowerInvariant()}";
}
