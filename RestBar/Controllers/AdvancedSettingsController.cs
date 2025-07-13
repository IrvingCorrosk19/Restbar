using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Claims;

namespace RestBar.Controllers
{
    [Authorize]
    public class AdvancedSettingsController : Controller
    {
        private readonly ISystemSettingsService _systemSettingsService;

        private readonly ICurrencyService _currencyService;
        private readonly ITaxRateService _taxRateService;
        private readonly IDiscountPolicyService _discountPolicyService;
        private readonly IOperatingHoursService _operatingHoursService;
        private readonly INotificationSettingsService _notificationSettingsService;
        private readonly IBackupSettingsService _backupSettingsService;
        private readonly RestBarContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdvancedSettingsController(
            ISystemSettingsService systemSettingsService,

            ICurrencyService currencyService,
            ITaxRateService taxRateService,
            IDiscountPolicyService discountPolicyService,
            IOperatingHoursService operatingHoursService,
            INotificationSettingsService notificationSettingsService,
            IBackupSettingsService backupSettingsService,
            RestBarContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _systemSettingsService = systemSettingsService;

            _currencyService = currencyService;
            _taxRateService = taxRateService;
            _discountPolicyService = discountPolicyService;
            _operatingHoursService = operatingHoursService;
            _notificationSettingsService = notificationSettingsService;
            _backupSettingsService = backupSettingsService;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user?.Branch?.CompanyId == null)
                {
                    TempData["Error"] = "Usuario sin empresa asignada";
                    return RedirectToAction("Login", "Auth");
                }

                var companyId = user.Branch.CompanyId.Value;

                // Obtener estadísticas de configuración
                var currencies = await _currencyService.GetAllAsync(companyId);
                var taxRates = await _taxRateService.GetAllAsync(companyId);
                var discountPolicies = await _discountPolicyService.GetAllAsync(companyId);
                var operatingHours = await _operatingHoursService.GetAllAsync(companyId);
                var notificationSettings = await _notificationSettingsService.GetAllAsync(companyId);
                var backupSettings = await _backupSettingsService.GetAllAsync(companyId);

                ViewBag.CurrencyCount = currencies.Count();
                ViewBag.TaxRateCount = taxRates.Count();
                ViewBag.DiscountPolicyCount = discountPolicies.Count();
                ViewBag.OperatingHoursCount = operatingHours.Count();
                ViewBag.NotificationSettingsCount = notificationSettings.Count();
                ViewBag.BackupSettingsCount = backupSettings.Count();

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar los ajustes: {ex.Message}";
                return View();
            }
        }

        // ===== CONFIGURACIÓN DEL SISTEMA =====
        public async Task<IActionResult> SystemSettings()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var settings = await _systemSettingsService.GetAllSettingsAsync(user?.Branch?.CompanyId);
                return View(settings);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar la configuración: {ex.Message}";
                return View(new List<SystemSettings>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSystemSetting(string key, string value, string? description = null, string? category = null)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var companyId = user?.Branch?.CompanyId;

                var result = await _systemSettingsService.SetSettingAsync(key, value, description, category, companyId);
                
                if (result)
                {
                    TempData["Success"] = "Configuración guardada exitosamente";
                }
                else
                {
                    TempData["Error"] = "Error al guardar la configuración";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al guardar la configuración: {ex.Message}";
            }

            return RedirectToAction(nameof(SystemSettings));
        }



        // ===== CONFIGURACIÓN DE MONEDAS =====
        public async Task<IActionResult> Currencies()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var currencies = await _currencyService.GetAllAsync(user?.Branch?.CompanyId);
                return View(currencies);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar las monedas: {ex.Message}";
                return View(new List<Currency>());
            }
        }

        public async Task<IActionResult> CreateCurrency()
        {
            return View(new Currency());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCurrency(Currency currency)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(currency);
                }

                var user = await GetCurrentUserAsync();
                currency.CompanyId = user?.Branch?.CompanyId;

                var createdCurrency = await _currencyService.CreateAsync(currency);
                TempData["Success"] = $"Moneda {createdCurrency.Name} creada exitosamente";
                return RedirectToAction(nameof(Currencies));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear la moneda: {ex.Message}";
                return View(currency);
            }
        }

        // ===== CONFIGURACIÓN DE IMPUESTOS =====
        public async Task<IActionResult> TaxRates()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var taxRates = await _taxRateService.GetAllAsync(user?.Branch?.CompanyId);
                return View(taxRates);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar los impuestos: {ex.Message}";
                return View(new List<TaxRate>());
            }
        }

        public async Task<IActionResult> CreateTaxRate()
        {
            return View(new TaxRate());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTaxRate(TaxRate taxRate)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(taxRate);
                }

                var user = await GetCurrentUserAsync();
                taxRate.CompanyId = user?.Branch?.CompanyId;

                var createdTaxRate = await _taxRateService.CreateAsync(taxRate);
                TempData["Success"] = $"Impuesto {createdTaxRate.Name} creado exitosamente";
                return RedirectToAction(nameof(TaxRates));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear el impuesto: {ex.Message}";
                return View(taxRate);
            }
        }

        // ===== CONFIGURACIÓN DE DESCUENTOS =====
        public async Task<IActionResult> DiscountPolicies()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var discountPolicies = await _discountPolicyService.GetAllAsync(user?.Branch?.CompanyId);
                return View(discountPolicies);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar las políticas de descuento: {ex.Message}";
                return View(new List<DiscountPolicy>());
            }
        }

        public async Task<IActionResult> CreateDiscountPolicy()
        {
            return View(new DiscountPolicy());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDiscountPolicy(DiscountPolicy discountPolicy)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(discountPolicy);
                }

                var user = await GetCurrentUserAsync();
                discountPolicy.CompanyId = user?.Branch?.CompanyId;

                var createdDiscountPolicy = await _discountPolicyService.CreateAsync(discountPolicy);
                TempData["Success"] = $"Política de descuento {createdDiscountPolicy.Name} creada exitosamente";
                return RedirectToAction(nameof(DiscountPolicies));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear la política de descuento: {ex.Message}";
                return View(discountPolicy);
            }
        }

        // ===== CONFIGURACIÓN DE HORARIOS =====
        public async Task<IActionResult> OperatingHours()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var operatingHours = await _operatingHoursService.GetAllAsync(user?.Branch?.CompanyId);
                return View(operatingHours);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar los horarios: {ex.Message}";
                return View(new List<OperatingHours>());
            }
        }

        public async Task<IActionResult> CreateOperatingHours()
        {
            return View(new OperatingHours());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOperatingHours(OperatingHours operatingHours)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(operatingHours);
                }

                var user = await GetCurrentUserAsync();
                operatingHours.CompanyId = user?.Branch?.CompanyId;

                var createdOperatingHours = await _operatingHoursService.CreateAsync(operatingHours);
                TempData["Success"] = $"Horario {createdOperatingHours.DayOfWeek} creado exitosamente";
                return RedirectToAction(nameof(OperatingHours));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear el horario: {ex.Message}";
                return View(operatingHours);
            }
        }

        // ===== CONFIGURACIÓN DE NOTIFICACIONES =====
        public async Task<IActionResult> NotificationSettings()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var notificationSettings = await _notificationSettingsService.GetAllAsync(user?.Branch?.CompanyId);
                return View(notificationSettings);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar las notificaciones: {ex.Message}";
                return View(new List<NotificationSettings>());
            }
        }

        public async Task<IActionResult> CreateNotificationSettings()
        {
            return View(new NotificationSettings());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNotificationSettings(NotificationSettings notificationSettings)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(notificationSettings);
                }

                var user = await GetCurrentUserAsync();
                notificationSettings.CompanyId = user?.Branch?.CompanyId;

                var createdNotificationSettings = await _notificationSettingsService.CreateAsync(notificationSettings);
                TempData["Success"] = $"Configuración de notificación {createdNotificationSettings.NotificationType} creada exitosamente";
                return RedirectToAction(nameof(NotificationSettings));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear la configuración de notificación: {ex.Message}";
                return View(notificationSettings);
            }
        }

        // ===== CONFIGURACIÓN DE RESPALDOS =====
        public async Task<IActionResult> BackupSettings()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var backupSettings = await _backupSettingsService.GetAllAsync(user?.Branch?.CompanyId);
                return View(backupSettings);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar la configuración de respaldos: {ex.Message}";
                return View(new List<BackupSettings>());
            }
        }

        public async Task<IActionResult> CreateBackupSettings()
        {
            return View(new BackupSettings());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBackupSettings(BackupSettings backupSettings)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(backupSettings);
                }

                var user = await GetCurrentUserAsync();
                backupSettings.CompanyId = user?.Branch?.CompanyId;

                var createdBackupSettings = await _backupSettingsService.CreateAsync(backupSettings);
                TempData["Success"] = $"Configuración de respaldo {createdBackupSettings.BackupType} creada exitosamente";
                return RedirectToAction(nameof(BackupSettings));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear la configuración de respaldo: {ex.Message}";
                return View(backupSettings);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExecuteBackup(string backupType)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var result = await _backupSettingsService.ExecuteBackupAsync(backupType, user?.Branch?.CompanyId);
                
                if (result)
                {
                    TempData["Success"] = $"Respaldo {backupType} ejecutado exitosamente";
                }
                else
                {
                    TempData["Error"] = $"Error al ejecutar el respaldo {backupType}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al ejecutar el respaldo: {ex.Message}";
            }

            return RedirectToAction(nameof(BackupSettings));
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail)) return null;

            return await _context.Users
                .Include(u => u.Branch)
                .ThenInclude(b => b.Company)
                .FirstOrDefaultAsync(u => u.Email == userEmail);
        }
    }
} 