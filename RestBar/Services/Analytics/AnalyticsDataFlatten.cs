using System.Text.Json;

namespace RestBar.Services.Analytics;

public static class AnalyticsDataFlatten
{
    public static List<Dictionary<string, object?>> Flatten(object? data)
    {
        if (data is List<Dictionary<string, object?>> list) return list;
        if (data is Dictionary<string, object?> one) return [one];
        if (data is null) return [];

        var json = JsonSerializer.Serialize(data);
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind == JsonValueKind.Array)
            return JsonSerializer.Deserialize<List<Dictionary<string, object?>>>(json) ?? [];

        if (doc.RootElement.TryGetProperty("rows", out var rows) && rows.ValueKind == JsonValueKind.Array)
            return JsonSerializer.Deserialize<List<Dictionary<string, object?>>>(rows.GetRawText()) ?? [];

        if (doc.RootElement.TryGetProperty("executive", out var exec))
        {
            var d = new Dictionary<string, object?>();
            foreach (var p in exec.EnumerateObject())
                d[p.Name] = p.Value.ToString();
            return [d];
        }

        if (doc.RootElement.TryGetProperty("comparison", out var comparison) && comparison.ValueKind == JsonValueKind.Array)
            return JsonSerializer.Deserialize<List<Dictionary<string, object?>>>(comparison.GetRawText()) ?? [];

        var flat = new Dictionary<string, object?>();
        foreach (var p in doc.RootElement.EnumerateObject())
            flat[p.Name] = p.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array
                ? p.Value.GetRawText()
                : p.Value.ToString();
        return [flat];
    }
}
