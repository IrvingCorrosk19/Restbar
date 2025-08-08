using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Controllers
{
    [Authorize(Policy = "AccountingAccess")]
    public class AccountingController : Controller
    {
        private readonly ILogger<AccountingController> _logger;
        private readonly IAccountService _accountService;
        private readonly IJournalEntryService _journalEntryService;
        private readonly IAccountingService _accountingService;

        public AccountingController(
            ILogger<AccountingController> logger,
            IAccountService accountService,
            IJournalEntryService journalEntryService,
            IAccountingService accountingService)
        {
            _logger = logger;
            _accountService = accountService;
            _journalEntryService = journalEntryService;
            _accountingService = accountingService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Obtener estadísticas básicas
                var accounts = await _accountService.GetActiveAccountsAsync();
                var recentEntries = await _journalEntryService.GetAllJournalEntriesAsync();
                var postedEntries = recentEntries.Where(e => e.Status == JournalEntryStatus.Posted).Take(5);
                var draftEntries = recentEntries.Where(e => e.Status == JournalEntryStatus.Draft).Take(5);

                ViewBag.TotalAccounts = accounts.Count();
                ViewBag.TotalPostedEntries = recentEntries.Count(e => e.Status == JournalEntryStatus.Posted);
                ViewBag.TotalDraftEntries = recentEntries.Count(e => e.Status == JournalEntryStatus.Draft);
                ViewBag.RecentPostedEntries = postedEntries;
                ViewBag.RecentDraftEntries = draftEntries;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar dashboard de contabilidad");
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var accounts = await _accountService.GetActiveAccountsAsync();
                var recentEntries = await _journalEntryService.GetAllJournalEntriesAsync();

                var stats = new
                {
                    totalAccounts = accounts.Count(),
                    totalPostedEntries = recentEntries.Count(e => e.Status == JournalEntryStatus.Posted),
                    totalDraftEntries = recentEntries.Count(e => e.Status == JournalEntryStatus.Draft),
                    totalVoidedEntries = recentEntries.Count(e => e.Status == JournalEntryStatus.Voided),
                    monthlyEntries = recentEntries.Count(e => e.EntryDate.Month == DateTime.Now.Month)
                };

                return Json(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas del dashboard");
                return Json(new { error = "Error al obtener estadísticas" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> FinancialSummary(string period = "month")
        {
            try
            {
                var summary = await _accountingService.GetFinancialSummaryAsync(period);
                return Json(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener resumen financiero");
                return Json(new { error = "Error al obtener resumen financiero" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> IncomeDetails(string period = "month")
        {
            try
            {
                var incomeDetails = await _accountingService.GetIncomeDetailsAsync(period);
                return Json(incomeDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles de ingresos");
                return Json(new { error = "Error al obtener detalles de ingresos" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExpenseDetails(string period = "month")
        {
            try
            {
                var expenseDetails = await _accountingService.GetExpenseDetailsAsync(period);
                return Json(expenseDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles de gastos");
                return Json(new { error = "Error al obtener detalles de gastos" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TaxSummary(string period = "month")
        {
            try
            {
                var taxSummary = await _accountingService.GetTaxSummaryAsync(period);
                return Json(taxSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener resumen de impuestos");
                return Json(new { error = "Error al obtener resumen de impuestos" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> MonthlyData(int year)
        {
            try
            {
                var monthlyData = await _accountingService.GetMonthlyFinancialDataAsync(year);
                return Json(monthlyData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos mensuales");
                return Json(new { error = "Error al obtener datos mensuales" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DailyData(DateTime startDate, DateTime endDate)
        {
            try
            {
                var dailyData = await _accountingService.GetDailyFinancialDataAsync(startDate, endDate);
                return Json(dailyData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos diarios");
                return Json(new { error = "Error al obtener datos diarios" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerateReport([FromBody] ReportRequest request)
        {
            try
            {
                var reportData = await _accountingService.GenerateFinancialReportAsync(
                    request.ReportType, 
                    request.Period, 
                    request.StartDate, 
                    request.EndDate);

                return File(reportData, "application/pdf", $"reporte_{request.ReportType}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte");
                return Json(new { error = "Error al generar reporte" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportData(string format = "xlsx")
        {
            try
            {
                var exportData = await _accountingService.ExportAccountingDataAsync(format);
                var contentType = format.ToLower() == "xlsx" ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" : "text/csv";
                var fileName = $"datos_contables_{DateTime.Now:yyyyMMdd}.{format}";

                return File(exportData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar datos");
                return Json(new { error = "Error al exportar datos" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> InitializeChartOfAccounts()
        {
            try
            {
                await _accountService.InitializeChartOfAccountsAsync();
                return Json(new { success = true, message = "Catálogo de cuentas inicializado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al inicializar catálogo de cuentas");
                return Json(new { success = false, error = "Error al inicializar el catálogo de cuentas" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateEntryFromOrder(Guid orderId)
        {
            try
            {
                var success = await _accountingService.CreateAccountingEntryFromOrderAsync(orderId);
                return Json(new { success, message = success ? "Asiento contable creado correctamente" : "No se pudo crear el asiento contable" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear asiento contable desde orden {OrderId}", orderId);
                return Json(new { success = false, error = "Error al crear asiento contable" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateEntryFromPayment(Guid paymentId)
        {
            try
            {
                var success = await _accountingService.CreateAccountingEntryFromPaymentAsync(paymentId);
                return Json(new { success, message = success ? "Asiento contable creado correctamente" : "No se pudo crear el asiento contable" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear asiento contable desde pago {PaymentId}", paymentId);
                return Json(new { success = false, error = "Error al crear asiento contable" });
            }
        }
    }

    public class ReportRequest
    {
        public string ReportType { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
} 