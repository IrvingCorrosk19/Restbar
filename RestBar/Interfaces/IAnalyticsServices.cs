using System.Security.Claims;
using RestBar.Domain.Analytics;

namespace RestBar.Interfaces;

public interface IAnalyticsScopeService
{
    AnalyticsFilter Resolve(ClaimsPrincipal user, AnalyticsFilterRequest req, bool allowCrossBranch);
}

public interface IAnalyticsQueryService
{
    Task<object?> GetReportDataAsync(string reportKey, AnalyticsFilter filter, CancellationToken ct = default);
    Task<AnalyticsLiveSnapshot> GetLiveAsync(AnalyticsFilter filter, CancellationToken ct = default);
    Task<IReadOnlyList<AnalyticsDecision>> GetDecisionsAsync(AnalyticsFilter filter, CancellationToken ct = default);
}

public interface IAnalyticsExportService
{
    Task<(byte[] bytes, string contentType, string fileName)> ExportAsync(string reportKey, AnalyticsFilter filter, string format, string userName, CancellationToken ct = default);
}

public sealed class AnalyticsFilterRequest
{
    public string? Period { get; set; }
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
    public Guid? BranchId { get; set; }
    public bool Compare { get; set; }
    public string? TimeZone { get; set; }
    public string? Currency { get; set; }
    public Guid? AreaId { get; set; }
    public Guid? StationId { get; set; }
    public Guid? WaiterUserId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? SupplierId { get; set; }
    public string? PaymentMethod { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? Sort { get; set; }
    public string? Search { get; set; }
}
