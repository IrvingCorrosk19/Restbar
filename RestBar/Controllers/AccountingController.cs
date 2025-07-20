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

        public AccountingController(
            ILogger<AccountingController> logger,
            IAccountService accountService,
            IJournalEntryService journalEntryService)
        {
            _logger = logger;
            _accountService = accountService;
            _journalEntryService = journalEntryService;
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
    }
} 