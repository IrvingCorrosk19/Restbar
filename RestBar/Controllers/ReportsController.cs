using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestBar.Interfaces;
using RestBar.ViewModels;
using System.Security.Claims;

namespace RestBar.Controllers
{
    [Authorize(Policy = "ReportAccess")]
    public class ReportsController : Controller
    {
        private readonly ISalesReportService _salesReportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(ISalesReportService salesReportService, ILogger<ReportsController> logger)
        {
            _salesReportService = salesReportService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

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
                    BranchId = branchId,
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
                    BranchId = branchId
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
                    BranchId = branchId
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
                    BranchId = branchId
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
                    BranchId = branchId
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
                    BranchId = branchId
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
                    BranchId = branchId
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

        // ✅ Exportar reporte a PDF
        [HttpGet]
        public async Task<IActionResult> ExportPdf(DateTime? startDate, DateTime? endDate, Guid? branchId)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-30),
                    EndDate = endDate ?? DateTime.Today,
                    BranchId = branchId
                };

                var report = await _salesReportService.GetCompleteSalesReportAsync(filters);

                // TODO: Implementar generación de PDF
                return Json(new { success = true, message = "Exportación a PDF en desarrollo" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReportsController] Error exportando PDF");
                return Json(new { success = false, message = "Error exportando PDF" });
            }
        }

        // ✅ Exportar reporte a Excel
        [HttpGet]
        public async Task<IActionResult> ExportExcel(DateTime? startDate, DateTime? endDate, Guid? branchId)
        {
            try
            {
                var filters = new ReportFilters
                {
                    StartDate = startDate ?? DateTime.Today.AddDays(-30),
                    EndDate = endDate ?? DateTime.Today,
                    BranchId = branchId
                };

                var report = await _salesReportService.GetCompleteSalesReportAsync(filters);

                // TODO: Implementar generación de Excel
                return Json(new { success = true, message = "Exportación a Excel en desarrollo" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReportsController] Error exportando Excel");
                return Json(new { success = false, message = "Error exportando Excel" });
            }
        }
    }
} 