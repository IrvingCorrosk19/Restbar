namespace RestBar.Domain.DecisionIntelligence;

/// <summary>Explicable statistical forecast — no LLM. Always compare to naive baseline.</summary>
public static class ForecastEngine
{
    public const string Naive = "naive";
    public const string MovingAverage = "ma";
    public const string WeightedMovingAverage = "wma";
    public const string Ses = "ses";
    public const string Linear = "linear";
    public const string DayOfWeek = "dow";
    public const string TrendDow = "trend_dow";

    public static IReadOnlyList<string> ModelIds { get; } =
        [Naive, MovingAverage, WeightedMovingAverage, Ses, Linear, DayOfWeek, TrendDow];

    /// <summary>Point forecast for the next <paramref name="horizon"/> periods after the series.</summary>
    public static IReadOnlyList<decimal> Forecast(string modelId, IReadOnlyList<decimal> series, int horizon, int window = 7, double alpha = 0.3)
    {
        if (series.Count == 0 || horizon <= 0) return Array.Empty<decimal>();
        return modelId switch
        {
            Naive => Enumerable.Repeat(series[^1], horizon).ToList(),
            MovingAverage => MaForecast(series, horizon, window),
            WeightedMovingAverage => WmaForecast(series, horizon, window),
            Ses => SesForecast(series, horizon, alpha),
            Linear => LinearForecast(series, horizon),
            DayOfWeek => DowForecast(series, horizon),
            TrendDow => TrendDowForecast(series, horizon),
            _ => throw new ArgumentException($"Unknown model '{modelId}'", nameof(modelId)),
        };
    }

    public static ForecastAccuracyMetrics Evaluate(IReadOnlyList<decimal> actual, IReadOnlyList<decimal> predicted)
    {
        var n = Math.Min(actual.Count, predicted.Count);
        if (n == 0) return ForecastAccuracyMetrics.Empty;
        double mae = 0, mse = 0, mape = 0, bias = 0;
        var mapeCount = 0;
        for (var i = 0; i < n; i++)
        {
            var e = (double)(predicted[i] - actual[i]);
            mae += Math.Abs(e);
            mse += e * e;
            bias += e;
            if (actual[i] != 0)
            {
                mape += Math.Abs(e) / (double)Math.Abs(actual[i]);
                mapeCount++;
            }
        }
        return new ForecastAccuracyMetrics(
            Mae: (decimal)(mae / n),
            Rmse: (decimal)Math.Sqrt(mse / n),
            Mape: mapeCount == 0 ? null : (decimal)(mape / mapeCount * 100.0),
            Bias: (decimal)(bias / n),
            N: n);
    }

    /// <summary>
    /// Temporal holdout: train on prefix, forecast holdout length, score vs actual suffix.
    /// Never uses future points in training.
    /// </summary>
    public static ForecastBacktestResult Backtest(string modelId, IReadOnlyList<decimal> series, int holdout, int window = 7)
    {
        if (series.Count < holdout + 3)
            return new ForecastBacktestResult(modelId, false, "Insufficient history", ForecastAccuracyMetrics.Empty, ForecastAccuracyMetrics.Empty);

        var train = series.Take(series.Count - holdout).ToList();
        var actual = series.Skip(series.Count - holdout).ToList();
        var pred = Forecast(modelId, train, holdout, window);
        var metrics = Evaluate(actual, pred);
        var naivePred = Forecast(Naive, train, holdout, window);
        var baseline = Evaluate(actual, naivePred);
        return new ForecastBacktestResult(modelId, true, null, metrics, baseline);
    }

    public static string SelectBestModel(IReadOnlyList<decimal> series, int holdout = 7)
    {
        ForecastBacktestResult? best = null;
        foreach (var id in ModelIds)
        {
            var r = Backtest(id, series, holdout);
            if (!r.Ok || r.Metrics.Mae is null) continue;
            if (best == null || r.Metrics.Mae < best.Metrics.Mae)
                best = r;
        }
        return best?.ModelId ?? Naive;
    }

    public static (decimal Lower, decimal Upper) Interval(decimal point, decimal mae, decimal z = 1.28m)
    {
        var half = Math.Max(mae * z, point * 0.05m);
        return (Math.Max(0, point - half), point + half);
    }

    public static string ConfidenceLabel(int historyPoints, decimal? mape, bool beatNaive)
    {
        if (historyPoints < 14) return "Baja";
        if (mape is null) return "Baja";
        if (historyPoints >= 28 && mape <= 20m && beatNaive) return "Alta";
        if (mape <= 35m) return "Media";
        return "Baja";
    }

    static List<decimal> MaForecast(IReadOnlyList<decimal> s, int h, int w)
    {
        var win = Math.Min(w, s.Count);
        var avg = s.Skip(s.Count - win).Average();
        return Enumerable.Repeat(avg, h).ToList();
    }

    static List<decimal> WmaForecast(IReadOnlyList<decimal> s, int h, int w)
    {
        var win = Math.Min(w, s.Count);
        var slice = s.Skip(s.Count - win).ToList();
        decimal num = 0, den = 0;
        for (var i = 0; i < slice.Count; i++)
        {
            var weight = i + 1;
            num += slice[i] * weight;
            den += weight;
        }
        var v = den == 0 ? 0 : num / den;
        return Enumerable.Repeat(v, h).ToList();
    }

    static List<decimal> SesForecast(IReadOnlyList<decimal> s, int h, double alpha)
    {
        double level = (double)s[0];
        for (var i = 1; i < s.Count; i++)
            level = alpha * (double)s[i] + (1 - alpha) * level;
        return Enumerable.Repeat((decimal)level, h).ToList();
    }

    static List<decimal> LinearForecast(IReadOnlyList<decimal> s, int h)
    {
        var n = s.Count;
        double sumX = 0, sumY = 0, sumXX = 0, sumXY = 0;
        for (var i = 0; i < n; i++)
        {
            sumX += i;
            sumY += (double)s[i];
            sumXX += i * (double)i;
            sumXY += i * (double)s[i];
        }
        var den = n * sumXX - sumX * sumX;
        var slope = den == 0 ? 0 : (n * sumXY - sumX * sumY) / den;
        var intercept = (sumY - slope * sumX) / n;
        var list = new List<decimal>(h);
        for (var i = 0; i < h; i++)
            list.Add((decimal)Math.Max(0, intercept + slope * (n + i)));
        return list;
    }

    /// <summary>Assumes daily series aligned to calendar; index 0 is oldest. Uses mean of same weekday in history.</summary>
    static List<decimal> DowForecast(IReadOnlyList<decimal> s, int h)
    {
        // Without explicit dates, treat last point as "today" weekday offset 0 from end.
        var buckets = new Dictionary<int, List<decimal>>();
        for (var i = 0; i < s.Count; i++)
        {
            var dow = i % 7;
            if (!buckets.TryGetValue(dow, out var list))
            {
                list = [];
                buckets[dow] = list;
            }
            list.Add(s[i]);
        }
        var startDow = s.Count % 7;
        var result = new List<decimal>(h);
        for (var i = 0; i < h; i++)
        {
            var dow = (startDow + i) % 7;
            result.Add(buckets.TryGetValue(dow, out var list) && list.Count > 0 ? list.Average() : s[^1]);
        }
        return result;
    }

    static List<decimal> TrendDowForecast(IReadOnlyList<decimal> s, int h)
    {
        var trend = LinearForecast(s, h);
        var dow = DowForecast(s, h);
        var overall = s.Average();
        if (overall == 0) return trend;
        var list = new List<decimal>(h);
        for (var i = 0; i < h; i++)
        {
            var seasonalFactor = overall == 0 ? 1m : dow[i] / overall;
            list.Add(Math.Max(0, trend[i] * seasonalFactor));
        }
        return list;
    }
}

public sealed record ForecastAccuracyMetrics(decimal? Mae, decimal? Rmse, decimal? Mape, decimal? Bias, int N)
{
    public static ForecastAccuracyMetrics Empty { get; } = new(null, null, null, null, 0);
}

public sealed record ForecastBacktestResult(
    string ModelId,
    bool Ok,
    string? Reason,
    ForecastAccuracyMetrics Metrics,
    ForecastAccuracyMetrics NaiveBaseline)
{
    public bool BeatsNaive =>
        Ok && Metrics.Mae is not null && NaiveBaseline.Mae is not null && Metrics.Mae <= NaiveBaseline.Mae;
}
