using RestBar.Domain.Analytics;
using RestBar.Domain.DecisionIntelligence;
using RestBar.Models;

namespace RestBar.Interfaces;

public interface IDecisionIntelligenceService
{
    Task<DiCockpitDto> GetCockpitAsync(AnalyticsFilter filter, Guid userId, CancellationToken ct = default);
    Task<DiForecastDto> GetSalesForecastAsync(AnalyticsFilter filter, int horizonDays, Guid? userId, bool persistRun, CancellationToken ct = default);
    Task<IReadOnlyList<DiRecommendationDto>> GetRecommendationsAsync(AnalyticsFilter filter, CancellationToken ct = default);
    Task<DiDecisionRecord> AcceptRecommendationAsync(Guid companyId, Guid? branchId, Guid userId, DiRecommendationDto rec, string? comment, CancellationToken ct = default);
    Task<DiDecisionRecord?> UpdateDecisionStatusAsync(Guid companyId, Guid decisionId, Guid userId, DiDecisionStatus status, string? comment, decimal? actualImpact, CancellationToken ct = default);
    Task<IReadOnlyList<DiDecisionRecord>> ListDecisionsAsync(Guid companyId, Guid? branchId, CancellationToken ct = default);
    Task<DiSimulationResult> SimulateSalesDeltaAsync(AnalyticsFilter filter, decimal pctChange, CancellationToken ct = default);
    DiDataQualityBanner GetDataQualityBanner();
}

public sealed record DiCockpitDto(
    DiDataQualityBanner Quality,
    object? ExecutiveSummary,
    IReadOnlyList<object> Live,
    DiForecastDto Forecast,
    IReadOnlyList<DiRecommendationDto> Recommendations,
    IReadOnlyList<object> Alerts,
    string GeneratedAtUtc);

public sealed record DiForecastDto(
    string MetricCode,
    string SelectedModel,
    int HorizonDays,
    int HistoryPoints,
    IReadOnlyList<decimal> History,
    IReadOnlyList<DiForecastPoint> Points,
    ForecastAccuracyMetrics Backtest,
    ForecastAccuracyMetrics NaiveBaseline,
    bool BeatsNaive,
    string Confidence,
    string Limitations,
    DateTime GeneratedAtUtc);

public sealed record DiForecastPoint(int DayOffset, decimal Point, decimal Lower, decimal Upper);

public sealed record DiSimulationResult(
    string Scenario,
    decimal BaselineRevenue,
    decimal SimulatedRevenue,
    decimal DeltaRevenue,
    string Note);

public sealed record DiDataQualityBanner(int GlobalScore, string Level, string Message);
