using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Controllers
{
    [Authorize(Policy = "AccountingAccess")]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var accounts = await _accountService.GetAccountTreeAsync();
                return View(accounts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el catálogo de cuentas");
                TempData["Error"] = "Error al cargar el catálogo de cuentas";
                return View(new List<Account>());
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                var parentAccounts = await _accountService.GetActiveAccountsAsync();
                ViewBag.ParentAccounts = parentAccounts;
                ViewBag.AccountTypes = Enum.GetValues<AccountType>();
                ViewBag.AccountCategories = Enum.GetValues<AccountCategory>();
                ViewBag.AccountNatures = Enum.GetValues<AccountNature>();

                return View(new Account());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar formulario de creación de cuenta");
                TempData["Error"] = "Error al cargar el formulario";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Account account)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    account.CreatedBy = User.Identity?.Name;
                    await _accountService.CreateAccountAsync(account);
                    TempData["Success"] = $"Cuenta '{account.Name}' creada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cuenta");
                ModelState.AddModelError("", "Error al crear la cuenta");
            }

            // Recargar datos para el formulario
            var parentAccounts = await _accountService.GetActiveAccountsAsync();
            ViewBag.ParentAccounts = parentAccounts;
            ViewBag.AccountTypes = Enum.GetValues<AccountType>();
            ViewBag.AccountCategories = Enum.GetValues<AccountCategory>();
            ViewBag.AccountNatures = Enum.GetValues<AccountNature>();

            return View(account);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var account = await _accountService.GetAccountByIdAsync(id);
                if (account == null)
                {
                    TempData["Error"] = "Cuenta no encontrada";
                    return RedirectToAction(nameof(Index));
                }

                var parentAccounts = await _accountService.GetActiveAccountsAsync();
                ViewBag.ParentAccounts = parentAccounts;
                ViewBag.AccountTypes = Enum.GetValues<AccountType>();
                ViewBag.AccountCategories = Enum.GetValues<AccountCategory>();
                ViewBag.AccountNatures = Enum.GetValues<AccountNature>();

                return View(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar cuenta para edición");
                TempData["Error"] = "Error al cargar la cuenta";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Account account)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    account.Id = id;
                    account.UpdatedBy = User.Identity?.Name;
                    await _accountService.UpdateAccountAsync(account);
                    TempData["Success"] = $"Cuenta '{account.Name}' actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cuenta");
                ModelState.AddModelError("", "Error al actualizar la cuenta");
            }

            // Recargar datos para el formulario
            var parentAccounts = await _accountService.GetActiveAccountsAsync();
            ViewBag.ParentAccounts = parentAccounts;
            ViewBag.AccountTypes = Enum.GetValues<AccountType>();
            ViewBag.AccountCategories = Enum.GetValues<AccountCategory>();
            ViewBag.AccountNatures = Enum.GetValues<AccountNature>();

            return View(account);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var account = await _accountService.GetAccountByIdAsync(id);
                if (account == null)
                {
                    TempData["Error"] = "Cuenta no encontrada";
                    return RedirectToAction(nameof(Index));
                }

                // Obtener balance de la cuenta
                var balance = await _accountService.GetAccountBalanceAsync(id);
                ViewBag.AccountBalance = balance;

                return View(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar detalles de cuenta");
                TempData["Error"] = "Error al cargar los detalles";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _accountService.DeleteAccountAsync(id);
                TempData["Success"] = "Cuenta eliminada exitosamente";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cuenta");
                TempData["Error"] = "Error al eliminar la cuenta";
            }

            return RedirectToAction(nameof(Index));
        }

        // API endpoints para AJAX
        [HttpGet]
        public async Task<IActionResult> GetAccountsByType(AccountType type)
        {
            try
            {
                var accounts = await _accountService.GetAccountsByTypeAsync(type);
                return Json(accounts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuentas por tipo");
                return Json(new { error = "Error al obtener las cuentas" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAccountBalance(Guid accountId, DateTime? asOfDate = null)
        {
            try
            {
                var balance = await _accountService.GetAccountBalanceAsync(accountId, asOfDate);
                return Json(new { balance = balance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener balance de cuenta");
                return Json(new { error = "Error al obtener el balance" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ValidateCode(string code, Guid? excludeId = null)
        {
            try
            {
                var isUnique = await _accountService.IsCodeUniqueAsync(code, excludeId);
                return Json(new { isUnique = isUnique });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar código de cuenta");
                return Json(new { error = "Error al validar el código" });
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