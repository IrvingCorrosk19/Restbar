using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Controllers
{
    [Authorize(Policy = "AccountingAccess")]
    public class JournalEntryController : Controller
    {
        private readonly IJournalEntryService _journalEntryService;
        private readonly IAccountService _accountService;
        private readonly ILogger<JournalEntryController> _logger;

        public JournalEntryController(
            IJournalEntryService journalEntryService,
            IAccountService accountService,
            ILogger<JournalEntryController> logger)
        {
            _journalEntryService = journalEntryService;
            _accountService = accountService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var journalEntries = await _journalEntryService.GetAllJournalEntriesAsync();
                return View(journalEntries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar asientos contables");
                TempData["Error"] = "Error al cargar los asientos contables";
                return View(new List<JournalEntry>());
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                var accounts = await _accountService.GetActiveAccountsAsync();
                ViewBag.Accounts = accounts;
                ViewBag.JournalEntryTypes = Enum.GetValues<JournalEntryType>();

                var journalEntry = new JournalEntry
                {
                    EntryDate = DateTime.Today,
                    Details = new List<JournalEntryDetail>
                    {
                        new JournalEntryDetail(),
                        new JournalEntryDetail()
                    }
                };

                return View(journalEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar formulario de creación de asiento");
                TempData["Error"] = "Error al cargar el formulario";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JournalEntry journalEntry)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    journalEntry.CreatedBy = User.Identity?.Name;
                    await _journalEntryService.CreateJournalEntryAsync(journalEntry);
                    TempData["Success"] = $"Asiento '{journalEntry.EntryNumber}' creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear asiento contable");
                ModelState.AddModelError("", "Error al crear el asiento contable");
            }

            // Recargar datos para el formulario
            var accounts = await _accountService.GetActiveAccountsAsync();
            ViewBag.Accounts = accounts;
            ViewBag.JournalEntryTypes = Enum.GetValues<JournalEntryType>();

            return View(journalEntry);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var journalEntry = await _journalEntryService.GetJournalEntryByIdAsync(id);
                if (journalEntry == null)
                {
                    TempData["Error"] = "Asiento contable no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar que no esté registrado
                if (journalEntry.Status == JournalEntryStatus.Posted)
                {
                    TempData["Error"] = "No se pueden editar asientos ya registrados";
                    return RedirectToAction(nameof(Index));
                }

                var accounts = await _accountService.GetActiveAccountsAsync();
                ViewBag.Accounts = accounts;
                ViewBag.JournalEntryTypes = Enum.GetValues<JournalEntryType>();

                return View(journalEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar asiento para edición");
                TempData["Error"] = "Error al cargar el asiento";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, JournalEntry journalEntry)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    journalEntry.Id = id;
                    journalEntry.UpdatedBy = User.Identity?.Name;
                    await _journalEntryService.UpdateJournalEntryAsync(journalEntry);
                    TempData["Success"] = $"Asiento '{journalEntry.EntryNumber}' actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar asiento contable");
                ModelState.AddModelError("", "Error al actualizar el asiento contable");
            }

            // Recargar datos para el formulario
            var accounts = await _accountService.GetActiveAccountsAsync();
            ViewBag.Accounts = accounts;
            ViewBag.JournalEntryTypes = Enum.GetValues<JournalEntryType>();

            return View(journalEntry);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var journalEntry = await _journalEntryService.GetJournalEntryByIdAsync(id);
                if (journalEntry == null)
                {
                    TempData["Error"] = "Asiento contable no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                return View(journalEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar detalles de asiento");
                TempData["Error"] = "Error al cargar los detalles";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Post(Guid id)
        {
            try
            {
                var postedBy = User.Identity?.Name ?? "System";
                var success = await _journalEntryService.PostJournalEntryAsync(id, postedBy);
                
                if (success)
                {
                    TempData["Success"] = "Asiento registrado exitosamente";
                }
                else
                {
                    TempData["Error"] = "No se pudo registrar el asiento";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar asiento");
                TempData["Error"] = "Error al registrar el asiento";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Void(Guid id, string reason)
        {
            try
            {
                var voidedBy = User.Identity?.Name ?? "System";
                var success = await _journalEntryService.VoidJournalEntryAsync(id, voidedBy, reason);
                
                if (success)
                {
                    TempData["Success"] = "Asiento anulado exitosamente";
                }
                else
                {
                    TempData["Error"] = "No se pudo anular el asiento";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al anular asiento");
                TempData["Error"] = "Error al anular el asiento";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _journalEntryService.DeleteJournalEntryAsync(id);
                TempData["Success"] = "Asiento eliminado exitosamente";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar asiento");
                TempData["Error"] = "Error al eliminar el asiento";
            }

            return RedirectToAction(nameof(Index));
        }

        // API endpoints para AJAX
        [HttpGet]
        public async Task<IActionResult> GetJournalEntriesByType(JournalEntryType type)
        {
            try
            {
                var journalEntries = await _journalEntryService.GetJournalEntriesByTypeAsync(type);
                return Json(journalEntries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener asientos por tipo");
                return Json(new { error = "Error al obtener los asientos" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetJournalEntriesByStatus(JournalEntryStatus status)
        {
            try
            {
                var journalEntries = await _journalEntryService.GetJournalEntriesByStatusAsync(status);
                return Json(journalEntries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener asientos por estado");
                return Json(new { error = "Error al obtener los asientos" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetJournalEntriesByDateRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                var journalEntries = await _journalEntryService.GetJournalEntriesByDateRangeAsync(startDate, endDate);
                return Json(journalEntries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener asientos por rango de fechas");
                return Json(new { error = "Error al obtener los asientos" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ValidateEntryNumber(string entryNumber)
        {
            try
            {
                var isUnique = await _journalEntryService.IsEntryNumberUniqueAsync(entryNumber);
                return Json(new { isUnique = isUnique });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar número de asiento");
                return Json(new { error = "Error al validar el número de asiento" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GenerateEntryNumber(JournalEntryType type)
        {
            try
            {
                var entryNumber = await _journalEntryService.GenerateEntryNumberAsync(type);
                return Json(new { entryNumber = entryNumber });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar número de asiento");
                return Json(new { error = "Error al generar el número de asiento" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateFromOrder(Guid orderId)
        {
            try
            {
                var journalEntry = await _journalEntryService.CreateEntryFromOrderAsync(orderId);
                if (journalEntry != null)
                {
                    return Json(new { success = true, message = "Asiento creado automáticamente", entryNumber = journalEntry.EntryNumber });
                }
                else
                {
                    return Json(new { success = false, error = "No se pudo crear el asiento automático" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear asiento desde orden");
                return Json(new { success = false, error = "Error al crear el asiento automático" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateFromPayment(Guid paymentId)
        {
            try
            {
                var journalEntry = await _journalEntryService.CreateEntryFromPaymentAsync(paymentId);
                if (journalEntry != null)
                {
                    return Json(new { success = true, message = "Asiento creado automáticamente", entryNumber = journalEntry.EntryNumber });
                }
                else
                {
                    return Json(new { success = false, error = "No se pudo crear el asiento automático" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear asiento desde pago");
                return Json(new { success = false, error = "Error al crear el asiento automático" });
            }
        }
    }
} 