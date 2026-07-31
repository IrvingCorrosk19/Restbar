using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestBar.Interfaces;
using RestBar.ViewModels;
using System.Security.Claims;

namespace RestBar.Controllers
{
    [Authorize(Policy = "ReportAccess")]
    public class AdvancedReportsController : Controller
    {
        private readonly IAdvancedReportsService _advancedReportsService;
        private readonly ILogger<AdvancedReportsController> _logger;

        public AdvancedReportsController(IAdvancedReportsService advancedReportsService, ILogger<AdvancedReportsController> logger)
        {
            _advancedReportsService = advancedReportsService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        // ===== ANÁLISIS DE RENTABILIDAD =====
        public async Task<IActionResult> ProfitabilityAnalysis(DateTime? startDate, DateTime? endDate, Guid? branchId)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-30),
                    EndDate = endDate ?? DateTime.Today,
                    BranchId = branchId
                };

                var report = await _advancedReportsService.GetProfitabilityAnalysisAsync(filters);
                return View(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsController] Error en análisis de rentabilidad");
                return View(new ProfitabilityAnalysis());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductProfitability(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-30),
                    EndDate = endDate ?? DateTime.Today
                };

                var profitability = await _advancedReportsService.GetProductProfitabilityAsync(filters);
                return Json(new { success = true, data = profitability });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsController] Error obteniendo rentabilidad de productos");
                return Json(new { success = false, message = "Error obteniendo rentabilidad" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryProfitability(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-30),
                    EndDate = endDate ?? DateTime.Today
                };

                var profitability = await _advancedReportsService.GetCategoryProfitabilityAsync(filters);
                return Json(new { success = true, data = profitability });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsController] Error obteniendo rentabilidad por categoría");
                return Json(new { success = false, message = "Error obteniendo rentabilidad" });
            }
        }

        // ===== ANÁLISIS DE VENTAS =====
        public async Task<IActionResult> SalesAnalysis(DateTime? startDate, DateTime? endDate, Guid? branchId)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-30),
                    EndDate = endDate ?? DateTime.Today,
                    BranchId = branchId
                };

                var report = await _advancedReportsService.GetSalesAnalysisAsync(filters);
                return View(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsController] Error en análisis de ventas");
                return View(new SalesAnalysisReport());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTopSellingProducts(DateTime? startDate, DateTime? endDate, int? topCount)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-30),
                    EndDate = endDate ?? DateTime.Today,
                    TopN = topCount ?? 10
                };

                var topProducts = await _advancedReportsService.GetTopSellingProductsAsync(filters);
                return Json(new { success = true, data = topProducts });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsController] Error obteniendo productos más vendidos");
                return Json(new { success = false, message = "Error obteniendo productos" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTopSellingCategories(DateTime? startDate, DateTime? endDate, int? topCount)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-30),
                    EndDate = endDate ?? DateTime.Today,
                    TopN = topCount ?? 10
                };

                var topCategories = await _advancedReportsService.GetTopSellingCategoriesAsync(filters);
                return Json(new { success = true, data = topCategories });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsController] Error obteniendo categorías más vendidas");
                return Json(new { success = false, message = "Error obteniendo categorías" });
            }
        }

        // ===== ANÁLISIS DE CLIENTES =====
        public async Task<IActionResult> CustomerAnalysis(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-90),
                    EndDate = endDate ?? DateTime.Today
                };

                var report = await _advancedReportsService.GetCustomerAnalysisAsync(filters);
                return View(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsController] Error en análisis de clientes");
                return View(new CustomerAnalysisReport());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTopCustomers(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-90),
                    EndDate = endDate ?? DateTime.Today
                };

                var topCustomers = await _advancedReportsService.GetTopCustomersAsync(filters);
                return Json(new { success = true, data = topCustomers });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsController] Error obteniendo clientes principales");
                return Json(new { success = false, message = "Error obteniendo clientes" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerSegments(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-90),
                    EndDate = endDate ?? DateTime.Today
                };

                var segments = await _advancedReportsService.GetCustomerSegmentsAsync(filters);
                return Json(new { success = true, data = segments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsController] Error obteniendo segmentos de clientes");
                return Json(new { success = false, message = "Error obteniendo segmentos" });
            }
        }

        // ===== ANÁLISIS DE OPERACIONES =====
        public async Task<IActionResult> OperationalAnalysis(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-30),
                    EndDate = endDate ?? DateTime.Today
                };

                var report = await _advancedReportsService.GetOperationalAnalysisAsync(filters);
                return View(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsController] Error en análisis operacional");
                return View(new OperationalAnalysisReport());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStationPerformance(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-30),
                    EndDate = endDate ?? DateTime.Today
                };

                var performance = await _advancedReportsService.GetStationPerformanceAsync(filters);
                return Json(new { success = true, data = performance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsController] Error obteniendo rendimiento de estaciones");
                return Json(new { success = false, message = "Error obteniendo rendimiento" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTableUtilization(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-30),
                    EndDate = endDate ?? DateTime.Today
                };

                var utilization = await _advancedReportsService.GetTableUtilizationAsync(filters);
                return Json(new { success = true, data = utilization });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsController] Error obteniendo utilización de mesas");
                return Json(new { success = false, message = "Error obteniendo utilización" });
            }
        }

        // ===== ANÁLISIS DE INVENTARIO =====
        public async Task<IActionResult> InventoryAnalysis(DateTime? startDate, DateTime? endDate, Guid? branchId)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-30),
                    EndDate = endDate ?? DateTime.Today,
                    BranchId = branchId
                };

                var report = await _advancedReportsService.GetInventoryAnalysisAsync(filters);
                return View(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsController] Error en análisis de inventario");
                return View(new InventoryAnalysisReport());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetInventoryAnalysis(DateTime? startDate, DateTime? endDate, Guid? branchId)
        {
            try
            {
                Console.WriteLine("🔍 [AdvancedReportsController] GetInventoryAnalysis() - Iniciando...");
                Console.WriteLine($"📋 [AdvancedReportsController] GetInventoryAnalysis() - Parámetros: startDate={startDate}, endDate={endDate}, branchId={branchId}");
                
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-30),
                    EndDate = endDate ?? DateTime.Today,
                    BranchId = branchId
                };

                Console.WriteLine($"📋 [AdvancedReportsController] GetInventoryAnalysis() - Filtros aplicados: StartDate={filters.StartDate}, EndDate={filters.EndDate}");

                var report = await _advancedReportsService.GetInventoryAnalysisAsync(filters);
                
                Console.WriteLine($"✅ [AdvancedReportsController] GetInventoryAnalysis() - Reporte generado: TotalProducts={report.TotalProducts}, LowStockProducts={report.LowStockProducts}, OutOfStockProducts={report.OutOfStockProducts}");
                
                return Json(new { success = true, data = report });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AdvancedReportsController] GetInventoryAnalysis() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [AdvancedReportsController] GetInventoryAnalysis() - StackTrace: {ex.StackTrace}");
                _logger.LogError(ex, "[AdvancedReportsController] Error obteniendo análisis de inventario");
                return Json(new { success = false, message = "Error obteniendo análisis" });
            }
        }

        // ===== ANÁLISIS DE PROVEEDORES =====
        public async Task<IActionResult> SupplierAnalysis(DateTime? startDate, DateTime? endDate, Guid? branchId)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-30),
                    EndDate = endDate ?? DateTime.Today,
                    BranchId = branchId
                };

                var report = await _advancedReportsService.GetSupplierAnalysisAsync(filters);
                return View(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsController] Error en análisis de proveedores");
                return View(new SupplierAnalysisReport());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSupplierAnalysis(DateTime? startDate, DateTime? endDate, Guid? branchId)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-30),
                    EndDate = endDate ?? DateTime.Today,
                    BranchId = branchId
                };

                var report = await _advancedReportsService.GetSupplierAnalysisAsync(filters);
                return Json(new { success = true, data = report });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsController] Error obteniendo análisis de proveedores");
                return Json(new { success = false, message = "Error obteniendo análisis" });
            }
        }

        // ===== ANÁLISIS DE TENDENCIAS =====
        public async Task<IActionResult> TrendAnalysis(DateTime? startDate, DateTime? endDate, Guid? branchId)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-90),
                    EndDate = endDate ?? DateTime.Today,
                    BranchId = branchId
                };

                var report = await _advancedReportsService.GetTrendAnalysisAsync(filters);
                return View(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsController] Error en análisis de tendencias");
                return View(new TrendAnalysisReport());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTrendAnalysis(DateTime? startDate, DateTime? endDate, Guid? branchId)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-90),
                    EndDate = endDate ?? DateTime.Today,
                    BranchId = branchId
                };

                var report = await _advancedReportsService.GetTrendAnalysisAsync(filters);
                return Json(new { success = true, data = report });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsController] Error obteniendo análisis de tendencias");
                return Json(new { success = false, message = "Error obteniendo análisis" });
            }
        }

        // ===== REPORTE DE AUDITORÍA =====
        public async Task<IActionResult> AuditReport(DateTime? startDate, DateTime? endDate, Guid? branchId)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-30),
                    EndDate = endDate ?? DateTime.Today,
                    BranchId = branchId
                };

                var report = await _advancedReportsService.GetAuditReportAsync(filters);
                return View(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsController] Error en reporte de auditoría");
                return View(new AuditReportViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditReport(DateTime? startDate, DateTime? endDate, Guid? branchId)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-30),
                    EndDate = endDate ?? DateTime.Today,
                    BranchId = branchId
                };

                var report = await _advancedReportsService.GetAuditReportAsync(filters);
                return Json(new { success = true, data = report });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsController] Error obteniendo reporte de auditoría");
                return Json(new { success = false, message = "Error obteniendo reporte" });
            }
        }

        // ===== SALUD DEL SISTEMA =====
        public async Task<IActionResult> SystemHealth()
        {
            try
            {
                var report = await _advancedReportsService.GetSystemHealthAsync();
                return View(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsController] Error en salud del sistema");
                return View(new SystemHealthReport());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSystemHealth()
        {
            try
            {
                var report = await _advancedReportsService.GetSystemHealthAsync();
                return Json(new { success = true, data = report });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsController] Error obteniendo salud del sistema");
                return Json(new { success = false, message = "Error obteniendo salud" });
            }
        }

        // ===== EXPORTACIÓN =====
        [HttpGet]
        public async Task<IActionResult> ExportToPdf(string reportType, DateTime? startDate, DateTime? endDate, Guid? branchId)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-30),
                    EndDate = endDate ?? DateTime.Today,
                    BranchId = branchId
                };

                // HTML imprimible (guardar como PDF desde el navegador) — mismo enfoque que Executive Analytics.
                var htmlBytes = await _advancedReportsService.ExportAdvancedReportToPdfAsync(reportType, filters);
                return File(htmlBytes, "text/html; charset=utf-8", $"reporte_{reportType}_{DateTime.Now:yyyyMMdd}_print.html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsController] Error exportando a PDF");
                return Json(new { success = false, message = "Error exportando reporte" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(string reportType, DateTime? startDate, DateTime? endDate, Guid? branchId)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-30),
                    EndDate = endDate ?? DateTime.Today,
                    BranchId = branchId
                };

                var excelBytes = await _advancedReportsService.ExportAdvancedReportToExcelAsync(reportType, filters);
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"reporte_{reportType}_{DateTime.Now:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AdvancedReportsController] Error exportando a Excel");
                return Json(new { success = false, message = "Error exportando reporte" });
            }
        }
    }
} 