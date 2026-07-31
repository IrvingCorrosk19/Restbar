using System.Text;
using ClosedXML.Excel;
using RestBar.Domain.Analytics;
using RestBar.Interfaces;

namespace RestBar.Services.Analytics;

public sealed class AnalyticsExportService : IAnalyticsExportService
{
    private readonly IAnalyticsQueryService _query;

    public AnalyticsExportService(IAnalyticsQueryService query) => _query = query;

    public async Task<(byte[] bytes, string contentType, string fileName)> ExportAsync(
        string reportKey, AnalyticsFilter filter, string format, string userName, CancellationToken ct = default)
    {
        var def = AnalyticsReportCatalog.Get(reportKey) ?? throw new KeyNotFoundException(reportKey);
        var data = await _query.GetReportDataAsync(reportKey, filter, ct);
        var rows = AnalyticsDataFlatten.Flatten(data);
        var stamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        format = format.ToLowerInvariant();

        return format switch
        {
            "csv" => (Encoding.UTF8.GetBytes(ToCsv(rows)), "text/csv", $"{reportKey}_{stamp}.csv"),
            "xlsx" or "excel" => (ToExcel(def, filter, userName, rows),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{reportKey}_{stamp}.xlsx"),
            "pdf" => (Encoding.UTF8.GetBytes(HtmlPrintRenderer.Render(def, filter, userName, rows)),
                "text/html; charset=utf-8", $"{reportKey}_{stamp}_print.html"),
            _ => throw new ArgumentException("Unsupported format. Use csv|xlsx|pdf")
        };
    }

    private static string ToCsv(List<Dictionary<string, object?>> rows)
    {
        if (rows.Count == 0) return "message\nNo data\n";
        var cols = rows.SelectMany(r => r.Keys).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", cols.Select(Escape)));
        foreach (var r in rows)
            sb.AppendLine(string.Join(",", cols.Select(c => Escape(Convert.ToString(r.GetValueOrDefault(c))))));
        return sb.ToString();

        static string Escape(string? v)
        {
            v ??= "";
            if (v.Contains('"') || v.Contains(',') || v.Contains('\n')) return $"\"{v.Replace("\"", "\"\"")}\"";
            return v;
        }
    }

    private static byte[] ToExcel(AnalyticsReportDefinition def, AnalyticsFilter f, string user, List<Dictionary<string, object?>> rows)
    {
        using var wb = new XLWorkbook();
        var summary = wb.Worksheets.Add("Resumen");
        summary.Cell(1, 1).Value = def.Title;
        summary.Cell(2, 1).Value = "Generado";
        summary.Cell(2, 2).Value = DateTime.UtcNow;
        summary.Cell(3, 1).Value = "Usuario";
        summary.Cell(3, 2).Value = user;
        summary.Cell(4, 1).Value = "CompanyId";
        summary.Cell(4, 2).Value = f.CompanyId.ToString();
        summary.Cell(5, 1).Value = "BranchId";
        summary.Cell(5, 2).Value = f.BranchId.ToString();
        summary.Cell(6, 1).Value = "Periodo";
        summary.Cell(6, 2).Value = $"{f.StartUtc:u} — {f.EndUtc:u}";
        summary.Cell(7, 1).Value = "Moneda";
        summary.Cell(7, 2).Value = f.Currency;
        summary.Cell(8, 1).Value = "Fuente";
        summary.Cell(8, 2).Value = def.ProcedureOrSource;
        summary.Columns().AdjustToContents();

        var data = wb.Worksheets.Add("Datos");
        if (rows.Count == 0)
        {
            data.Cell(1, 1).Value = "Sin datos";
        }
        else
        {
            var cols = rows.SelectMany(r => r.Keys).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            for (var c = 0; c < cols.Count; c++) data.Cell(1, c + 1).Value = cols[c];
            for (var r = 0; r < rows.Count; r++)
            for (var c = 0; c < cols.Count; c++)
            {
                var val = rows[r].GetValueOrDefault(cols[c]);
                if (val is DateTime dt) data.Cell(r + 2, c + 1).Value = dt;
                else if (val is decimal dec) data.Cell(r + 2, c + 1).Value = dec;
                else if (val is double dbl) data.Cell(r + 2, c + 1).Value = dbl;
                else if (val is int or long or short) data.Cell(r + 2, c + 1).Value = Convert.ToInt64(val);
                else data.Cell(r + 2, c + 1).Value = Convert.ToString(val);
            }
            data.RangeUsed()?.SetAutoFilter();
            data.SheetView.FreezeRows(1);
            data.Columns().AdjustToContents();
        }

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}

internal static class HtmlPrintRenderer
{
    public static string Render(AnalyticsReportDefinition def, AnalyticsFilter f, string user, List<Dictionary<string, object?>> rows)
    {
        var cols = rows.Count == 0 ? new List<string> { "message" }
            : rows.SelectMany(r => r.Keys).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var sb = new StringBuilder();
        sb.Append("<!doctype html><html><head><meta charset='utf-8'><title>")
          .Append(System.Net.WebUtility.HtmlEncode(def.Title))
          .Append("</title><style>body{font-family:Segoe UI,Arial,sans-serif;font-size:12px;margin:24px} table{border-collapse:collapse;width:100%} th,td{border:1px solid #ccc;padding:4px} th{background:#eee}@media print{.no-print{display:none}}</style></head><body>");
        sb.Append("<h1>").Append(System.Net.WebUtility.HtmlEncode(def.Title)).Append("</h1>");
        sb.Append("<p>Company ").Append(f.CompanyId).Append(" · Branch ").Append(f.BranchId).Append("<br>")
          .Append(f.StartUtc.ToString("u")).Append(" — ").Append(f.EndUtc.ToString("u"))
          .Append(" · ").Append(f.Currency).Append(" · ").Append(f.TimeZone)
          .Append("<br>Generated ").Append(DateTime.UtcNow.ToString("u")).Append(" by ")
          .Append(System.Net.WebUtility.HtmlEncode(user))
          .Append("<br>Source ").Append(System.Net.WebUtility.HtmlEncode(def.ProcedureOrSource)).Append("</p>");
        sb.Append("<p class='no-print'><button onclick='window.print()'>Imprimir / Guardar como PDF</button></p>");
        sb.Append("<table><thead><tr>");
        foreach (var c in cols) sb.Append("<th>").Append(System.Net.WebUtility.HtmlEncode(c)).Append("</th>");
        sb.Append("</tr></thead><tbody>");
        if (rows.Count == 0) sb.Append("<tr><td>Sin datos</td></tr>");
        else foreach (var r in rows.Take(500))
        {
            sb.Append("<tr>");
            foreach (var c in cols)
                sb.Append("<td>").Append(System.Net.WebUtility.HtmlEncode(Convert.ToString(r.GetValueOrDefault(c)))).Append("</td>");
            sb.Append("</tr>");
        }
        sb.Append("</tbody></table></body></html>");
        return sb.ToString();
    }
}
