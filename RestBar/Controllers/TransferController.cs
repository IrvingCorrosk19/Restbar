using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Claims;

namespace RestBar.Controllers
{
    [Authorize]
    public class TransferController : Controller
    {
        private readonly ITransferService _transferService;
        private readonly RestBarContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TransferController(ITransferService transferService, RestBarContext context, IHttpContextAccessor httpContextAccessor)
        {
            _transferService = transferService;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var transfers = await _transferService.GetAllAsync();
                var statistics = await _transferService.GetTransferStatisticsAsync();
                
                ViewBag.Statistics = statistics;
                return View(transfers);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar las transferencias: {ex.Message}";
                return View(new List<Transfer>());
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return RedirectToAction("Login", "Auth");

                // Verificar que el usuario tenga una sucursal asignada
                if (user.Branch?.CompanyId == null)
                {
                    TempData["Error"] = "Usuario sin sucursal asignada";
                    return RedirectToAction("Login", "Auth");
                }

                            // Obtener sucursales de la empresa
            var companyId = user.Branch.CompanyId.Value;
            var branches = await _context.Branches
                .Where(b => b.CompanyId == companyId && b.IsActive == true)
                .ToListAsync();

                // Obtener productos con inventario
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsActive == true)
                    .ToListAsync();

                ViewBag.SourceBranches = branches;
                ViewBag.DestinationBranches = branches;
                ViewBag.Products = products;

                return View(new Transfer { TransferDate = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el formulario: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Transfer transfer)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var user = await GetCurrentUserAsync();
                    var branches = await _context.Branches
                        .Where(b => b.CompanyId == user.Branch.CompanyId && b.IsActive == true)
                        .ToListAsync();
                    var products = await _context.Products
                        .Include(p => p.Category)
                        .Where(p => p.IsActive == true)
                        .ToListAsync();

                    ViewBag.SourceBranches = branches;
                    ViewBag.DestinationBranches = branches;
                    ViewBag.Products = products;

                    return View(transfer);
                }

                var createdTransfer = await _transferService.CreateAsync(transfer);
                TempData["Success"] = $"Transferencia {createdTransfer.TransferNumber} creada exitosamente";
                return RedirectToAction(nameof(Details), new { id = createdTransfer.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear la transferencia: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var transfer = await _transferService.GetByIdAsync(id);
                if (transfer == null)
                {
                    TempData["Error"] = "Transferencia no encontrada";
                    return RedirectToAction(nameof(Index));
                }

                return View(transfer);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar los detalles: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var transfer = await _transferService.GetByIdAsync(id);
                if (transfer == null)
                {
                    TempData["Error"] = "Transferencia no encontrada";
                    return RedirectToAction(nameof(Index));
                }

                if (transfer.Status != TransferStatus.Pending)
                {
                    TempData["Error"] = "Solo se pueden editar transferencias pendientes";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var user = await GetCurrentUserAsync();
                if (user?.Branch?.CompanyId == null)
                {
                    TempData["Error"] = "Usuario sin sucursal asignada";
                    return RedirectToAction("Login", "Auth");
                }
                var companyId = user.Branch.CompanyId.Value;
                var branches = await _context.Branches
                    .Where(b => b.CompanyId == companyId && b.IsActive == true)
                    .ToListAsync();
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsActive == true)
                    .ToListAsync();

                ViewBag.SourceBranches = branches;
                ViewBag.DestinationBranches = branches;
                ViewBag.Products = products;

                return View(transfer);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar la transferencia: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Transfer transfer)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var user = await GetCurrentUserAsync();
                    if (user?.Branch?.CompanyId == null)
                    {
                        TempData["Error"] = "Usuario sin sucursal asignada";
                        return RedirectToAction("Login", "Auth");
                    }
                    var companyId = user.Branch.CompanyId.Value;
                    var branches = await _context.Branches
                        .Where(b => b.CompanyId == companyId && b.IsActive == true)
                        .ToListAsync();
                    var products = await _context.Products
                        .Include(p => p.Category)
                        .Where(p => p.IsActive == true)
                        .ToListAsync();

                    ViewBag.SourceBranches = branches;
                    ViewBag.DestinationBranches = branches;
                    ViewBag.Products = products;

                    return View(transfer);
                }

                transfer.Id = id;
                var updatedTransfer = await _transferService.UpdateAsync(transfer);
                TempData["Success"] = $"Transferencia {updatedTransfer.TransferNumber} actualizada exitosamente";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar la transferencia: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid id)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return RedirectToAction("Login", "Auth");

                var transfer = await _transferService.ApproveAsync(id, user.Id);
                TempData["Success"] = $"Transferencia {transfer.TransferNumber} aprobada exitosamente";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al aprobar la transferencia: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Guid id, string reason)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return RedirectToAction("Login", "Auth");

                var transfer = await _transferService.RejectAsync(id, user.Id, reason);
                TempData["Success"] = $"Transferencia {transfer.TransferNumber} rechazada";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al rechazar la transferencia: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid id, string reason)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return RedirectToAction("Login", "Auth");

                var transfer = await _transferService.CancelAsync(id, user.Id, reason);
                TempData["Success"] = $"Transferencia {transfer.TransferNumber} cancelada";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cancelar la transferencia: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkInTransit(Guid id)
        {
            try
            {
                var transfer = await _transferService.MarkInTransitAsync(id);
                TempData["Success"] = $"Transferencia {transfer.TransferNumber} marcada como en tránsito";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al marcar la transferencia como en tránsito: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Receive(Guid id)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return RedirectToAction("Login", "Auth");

                var transfer = await _transferService.ReceiveAsync(id, user.Id);
                TempData["Success"] = $"Transferencia {transfer.TransferNumber} recibida exitosamente";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al recibir la transferencia: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _transferService.DeleteAsync(id);
                if (result)
                {
                    TempData["Success"] = "Transferencia eliminada exitosamente";
                }
                else
                {
                    TempData["Error"] = "No se pudo eliminar la transferencia";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar la transferencia: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // API endpoints
        [HttpGet]
        public async Task<IActionResult> GetTransfers()
        {
            try
            {
                var transfers = await _transferService.GetAllAsync();
                return Json(new { success = true, data = transfers });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetByStatus(string status)
        {
            try
            {
                if (Enum.TryParse<TransferStatus>(status, true, out var transferStatus))
                {
                    var transfers = await _transferService.GetByStatusAsync(transferStatus);
                    return Json(new { success = true, data = transfers });
                }
                return Json(new { success = false, message = "Estado de transferencia inválido" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var statistics = await _transferService.GetTransferStatisticsAsync();
                return Json(new { success = true, data = statistics });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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