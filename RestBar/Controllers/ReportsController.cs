using System.Net;
using System.Text;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestBar.Helpers;
using RestBar.Interfaces;
using RestBar.Models;
using RestBar.ViewModels;
using System.Security.Claims;

namespace RestBar.Controllers
{
    [Authorize(Policy = "ReportAccess")]
    public class ReportsController : Controller
    {
        private readonly ISalesReportService _salesReportService;
        private readonly RestBarContext _db;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(ISalesReportService salesReportService, RestBarContext db, ILogger<ReportsController> logger)
        {
            _salesReportService = salesReportService;
            _db = db;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        private async Task<Guid?> ResolveBranchIdAsync(Guid? requested) =>
            await TenantScope.ResolveBranchIdAsync(_db, User, requested);

        // ✅ Reporte completo de ventas
        [HttpGet]
        public async Task<IActionResult> SalesReport(DateTime? startDate, DateTime? endDate, Guid? branchId, Guid? userId, Guid? categoryId)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-30),
                    EndDate = endDate ?? DateTime.Today,
                    BranchId = await ResolveBranchIdAsync(branchId),
                    UserId = userId,
                    CategoryId = categoryId
                };

                var report = await _salesReportService.GetCompleteSalesReportAsync(filters);

                return View(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReportsController] Error generando reporte de ventas");
                return View(new SalesReportViewModel());
            }
        }

        // ✅ API para métricas de ventas
        [HttpGet]
        public async Task<IActionResult> GetSalesMetrics(DateTime? startDate, DateTime? endDate, Guid? branchId)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    BranchId = await ResolveBranchIdAsync(branchId)
                };

                var metrics = await _salesReportService.GetSalesMetricsAsync(filters);
                return Json(new { success = true, data = metrics });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReportsController] Error obteniendo métricas");
                return Json(new { success = false, message = "Error obteniendo métricas" });
            }
        }

        // ✅ API para ventas diarias
        [HttpGet]
        public async Task<IActionResult> GetDailySales(DateTime? startDate, DateTime? endDate, Guid? branchId)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    BranchId = await ResolveBranchIdAsync(branchId)
                };

                var dailySales = await _salesReportService.GetDailySalesAsync(filters);
                return Json(new { success = true, data = dailySales });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReportsController] Error obteniendo ventas diarias");
                return Json(new { success = false, message = "Error obteniendo ventas diarias" });
            }
        }

        // ✅ API para top productos
        [HttpGet]
        public async Task<IActionResult> GetTopProducts(DateTime? startDate, DateTime? endDate, Guid? branchId)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    BranchId = await ResolveBranchIdAsync(branchId)
                };

                var topProducts = await _salesReportService.GetTopProductsAsync(filters);
                return Json(new { success = true, data = topProducts });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReportsController] Error obteniendo top productos");
                return Json(new { success = false, message = "Error obteniendo top productos" });
            }
        }

        // ✅ API para ventas por categoría
        [HttpGet]
        public async Task<IActionResult> GetCategorySales(DateTime? startDate, DateTime? endDate, Guid? branchId)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    BranchId = await ResolveBranchIdAsync(branchId)
                };

                var categorySales = await _salesReportService.GetCategorySalesAsync(filters);
                return Json(new { success = true, data = categorySales });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReportsController] Error obteniendo ventas por categoría");
                return Json(new { success = false, message = "Error obteniendo ventas por categoría" });
            }
        }

        // ✅ API para ventas por empleado
        [HttpGet]
        public async Task<IActionResult> GetEmployeeSales(DateTime? startDate, DateTime? endDate, Guid? branchId)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    BranchId = await ResolveBranchIdAsync(branchId)
                };

                var employeeSales = await _salesReportService.GetEmployeeSalesAsync(filters);
                return Json(new { success = true, data = employeeSales });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReportsController] Error obteniendo ventas por empleado");
                return Json(new { success = false, message = "Error obteniendo ventas por empleado" });
            }
        }

        // ✅ API para ventas por sucursal
        [HttpGet]
        public async Task<IActionResult> GetBranchSales(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate,
                    EndDate = endDate
                };

                var branchSales = await _salesReportService.GetBranchSalesAsync(filters);
                return Json(new { success = true, data = branchSales });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReportsController] Error obteniendo ventas por sucursal");
                return Json(new { success = false, message = "Error obteniendo ventas por sucursal" });
            }
        }

        // ✅ API para reporte de descuentos
        [HttpGet]
        public async Task<IActionResult> GetDiscounts(DateTime? startDate, DateTime? endDate, Guid? branchId)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    BranchId = await ResolveBranchIdAsync(branchId)
                };

                var discounts = await _salesReportService.GetDiscountsAsync(filters);
                return Json(new { success = true, data = discounts });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReportsController] Error obteniendo descuentos");
                return Json(new { success = false, message = "Error obteniendo descuentos" });
            }
        }

        // HTML imprimible (guardar como PDF) — mismo enfoque que AdvancedReports / Analytics
        [HttpGet]
        public async Task<IActionResult> ExportPdf(DateTime? startDate, DateTime? endDate, Guid? branchId)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-30),
                    EndDate = endDate ?? DateTime.Today,
                    BranchId = await ResolveBranchIdAsync(branchId)
                };

                var report = await _salesReportService.GetCompleteSalesReportAsync(filters);
                var html = BuildSalesReportHtml(report, filters);
                var bytes = Encoding.UTF8.GetBytes(html);
                return File(bytes, "text/html; charset=utf-8", $"ventas_{filters.StartDate:yyyyMMdd}_{filters.EndDate:yyyyMMdd}_print.html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReportsController] Error exportando PDF");
                return Json(new { success = false, message = "Error exportando PDF" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(DateTime? startDate, DateTime? endDate, Guid? branchId)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-30),
                    EndDate = endDate ?? DateTime.Today,
                    BranchId = await ResolveBranchIdAsync(branchId)
                };

                var report = await _salesReportService.GetCompleteSalesReportAsync(filters);
                var bytes = BuildSalesReportExcel(report, filters);
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"ventas_{filters.StartDate:yyyyMMdd}_{filters.EndDate:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReportsController] Error exportando Excel");
                return Json(new { success = false, message = "Error exportando Excel" });
            }
        }

        private static string BuildSalesReportHtml(SalesReportViewModel report, ReportFilters filters)
        {
            var sb = new StringBuilder();
            sb.Append("<!doctype html><html><head><meta charset='utf-8'><title>Reporte de ventas</title>");
            sb.Append("<style>body{font-family:Segoe UI,Arial,sans-serif;font-size:12px;margin:24px}table{border-collapse:collapse;width:100%;margin-bottom:16px}th,td{border:1px solid #ccc;padding:4px}th{background:#eee}@media print{.no-print{display:none}}</style></head><body>");
            sb.Append("<h1>Reporte de ventas</h1><p>")
              .Append(WebUtility.HtmlEncode($"{filters.StartDate:d} — {filters.EndDate:d}"))
              .Append(" · Generado ").Append(DateTime.UtcNow.ToString("u")).Append("</p>");
            sb.Append("<p class='no-print'><button onclick='window.print()'>Imprimir / Guardar como PDF</button></p>");
            sb.Append("<h2>Métricas</h2><table><tr><th>Ingresos</th><th>Órdenes</th><th>Ticket promedio</th><th>Descuentos</th><th>Neto</th></tr><tr>");
            sb.Append($"<td>{report.Metrics.TotalRevenue:N2}</td><td>{report.Metrics.TotalOrders}</td><td>{report.Metrics.AverageTicket:N2}</td><td>{report.Metrics.TotalDiscounts:N2}</td><td>{report.Metrics.NetRevenue:N2}</td></tr></table>");
            sb.Append("<h2>Ventas diarias</h2><table><tr><th>Fecha</th><th>Ingresos</th><th>Órdenes</th><th>Items</th></tr>");
            foreach (var d in report.DailySales)
                sb.Append($"<tr><td>{d.Date:d}</td><td>{d.Revenue:N2}</td><td>{d.Orders}</td><td>{d.Items}</td></tr>");
            sb.Append("</table><h2>Top productos</h2><table><tr><th>#</th><th>Producto</th><th>Cantidad</th><th>Ingresos</th></tr>");
            foreach (var p in report.TopProducts)
                sb.Append($"<tr><td>{p.Rank}</td><td>{WebUtility.HtmlEncode(p.ProductName)}</td><td>{p.QuantitySold}</td><td>{p.Revenue:N2}</td></tr>");
            sb.Append("</table></body></html>");
            return sb.ToString();
        }

        private static byte[] BuildSalesReportExcel(SalesReportViewModel report, ReportFilters filters)
        {
            using var wb = new XLWorkbook();
            var summary = wb.Worksheets.Add("Resumen");
            summary.Cell(1, 1).Value = "Reporte de ventas";
            summary.Cell(2, 1).Value = "Periodo";
            summary.Cell(2, 2).Value = $"{filters.StartDate:d} — {filters.EndDate:d}";
            summary.Cell(3, 1).Value = "Ingresos";
            summary.Cell(3, 2).Value = report.Metrics.TotalRevenue;
            summary.Cell(4, 1).Value = "Órdenes";
            summary.Cell(4, 2).Value = report.Metrics.TotalOrders;
            summary.Cell(5, 1).Value = "Ticket promedio";
            summary.Cell(5, 2).Value = report.Metrics.AverageTicket;
            summary.Cell(6, 1).Value = "Descuentos";
            summary.Cell(6, 2).Value = report.Metrics.TotalDiscounts;
            summary.Cell(7, 1).Value = "Neto";
            summary.Cell(7, 2).Value = report.Metrics.NetRevenue;
            summary.Columns().AdjustToContents();

            var daily = wb.Worksheets.Add("Diario");
            daily.Cell(1, 1).Value = "Fecha";
            daily.Cell(1, 2).Value = "Ingresos";
            daily.Cell(1, 3).Value = "Órdenes";
            daily.Cell(1, 4).Value = "Items";
            for (var i = 0; i < report.DailySales.Count; i++)
            {
                var d = report.DailySales[i];
                daily.Cell(i + 2, 1).Value = d.Date;
                daily.Cell(i + 2, 2).Value = d.Revenue;
                daily.Cell(i + 2, 3).Value = d.Orders;
                daily.Cell(i + 2, 4).Value = d.Items;
            }
            daily.Columns().AdjustToContents();

            var products = wb.Worksheets.Add("TopProductos");
            products.Cell(1, 1).Value = "Rank";
            products.Cell(1, 2).Value = "Producto";
            products.Cell(1, 3).Value = "Categoría";
            products.Cell(1, 4).Value = "Cantidad";
            products.Cell(1, 5).Value = "Ingresos";
            for (var i = 0; i < report.TopProducts.Count; i++)
            {
                var p = report.TopProducts[i];
                products.Cell(i + 2, 1).Value = p.Rank;
                products.Cell(i + 2, 2).Value = p.ProductName;
                products.Cell(i + 2, 3).Value = p.CategoryName;
                products.Cell(i + 2, 4).Value = p.QuantitySold;
                products.Cell(i + 2, 5).Value = p.Revenue;
            }
            products.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
} 