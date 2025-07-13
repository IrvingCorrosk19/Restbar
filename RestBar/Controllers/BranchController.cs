using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestBar.Interfaces;
using RestBar.Models;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace RestBar.Controllers
{
    [Authorize(Roles = "admin")]
    public class BranchController : Controller
    {
        private readonly IBranchService _branchService;
        private readonly IUserService _userService;

        public BranchController(IBranchService branchService, IUserService userService)
        {
            _branchService = branchService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                Console.WriteLine("🔍 [BranchController] Index() - Iniciando...");
                
                // Obtener usuario actual y sus asignaciones
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    Console.WriteLine("❌ [BranchController] Index() - Usuario no autenticado");
                    return View(new List<Branch>());
                }

                var userId = Guid.Parse(userIdClaim.Value);
                var currentUser = await _userService.GetCurrentUserWithAssignmentsAsync(userId);
                
                if (currentUser == null || currentUser.Branch == null)
                {
                    Console.WriteLine("❌ [BranchController] Index() - Usuario o sucursal no encontrado");
                    return View(new List<Branch>());
                }

                Console.WriteLine($"✅ [BranchController] Index() - Usuario: {currentUser.Email}, CompanyId: {currentUser.Branch.CompanyId}");

                // Filtrar sucursales por compañía del usuario actual
                var branches = await _branchService.GetByCompanyIdAsync(currentUser.Branch.CompanyId.Value);
                
                Console.WriteLine($"📊 [BranchController] Index() - Sucursales encontradas: {branches.Count()}");
                return View(branches);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [BranchController] Index() - Error: {ex.Message}");
                return View(new List<Branch>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBranches()
        {
            try
            {
                Console.WriteLine("🔍 [BranchController] GetBranches() - Iniciando...");
                
                // Obtener usuario actual y sus asignaciones
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    Console.WriteLine("❌ [BranchController] GetBranches() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var userId = Guid.Parse(userIdClaim.Value);
                var currentUser = await _userService.GetCurrentUserWithAssignmentsAsync(userId);
                
                if (currentUser == null || currentUser.Branch == null)
                {
                    Console.WriteLine("❌ [BranchController] GetBranches() - Usuario o sucursal no encontrado");
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }

                Console.WriteLine($"✅ [BranchController] GetBranches() - Usuario: {currentUser.Email}, CompanyId: {currentUser.Branch.CompanyId}");

                // Filtrar sucursales por compañía del usuario actual
                var branches = await _branchService.GetByCompanyIdAsync(currentUser.Branch.CompanyId.Value);
                
                var data = branches.Select(b => new {
                    id = b.Id,
                    name = b.Name,
                    address = b.Address,
                    phone = b.Phone,
                    isActive = b.IsActive,
                    createdAt = b.CreatedAt
                }).ToList();
                
                Console.WriteLine($"📤 [BranchController] GetBranches() - Enviando {data.Count} sucursales");
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [BranchController] GetBranches() - Error: {ex.Message}");
                return Json(new { success = false, message = $"Error al cargar sucursales: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("Branch/Get/{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var branch = await _branchService.GetByIdAsync(id);
            if (branch == null)
                return Json(new { success = false, message = "Sucursal no encontrada" });
            return Json(new { success = true, data = new {
                id = branch.Id,
                name = branch.Name,
                address = branch.Address,
                phone = branch.Phone,
                isActive = branch.IsActive,
                createdAt = branch.CreatedAt,
                companyId = branch.CompanyId
            }});
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Branch model)
        {
            try
            {
                Console.WriteLine("🔍 [BranchController] Create() - Iniciando...");
                
                if (string.IsNullOrWhiteSpace(model.Name))
                    return Json(new { success = false, message = "El nombre es requerido" });

                // Obtener usuario actual y sus asignaciones
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    Console.WriteLine("❌ [BranchController] Create() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var userId = Guid.Parse(userIdClaim.Value);
                var currentUser = await _userService.GetCurrentUserWithAssignmentsAsync(userId);
                
                if (currentUser == null || currentUser.Branch == null)
                {
                    Console.WriteLine("❌ [BranchController] Create() - Usuario o sucursal no encontrado");
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }

                Console.WriteLine($"✅ [BranchController] Create() - Usuario: {currentUser.Email}, CompanyId: {currentUser.Branch.CompanyId}");

                // Asignar automáticamente CompanyId del usuario actual
                model.CompanyId = currentUser.Branch.CompanyId.Value;
                // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService

                Console.WriteLine($"✅ [BranchController] Create() - Asignando CompanyId: {model.CompanyId}");

                var created = await _branchService.CreateAsync(model);
                return Json(new { success = true, data = created });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [BranchController] Create() - Error: {ex.Message}");
                return Json(new { success = false, message = $"Error al crear sucursal: {ex.Message}" });
            }
        }

        [HttpPut]
        [Route("Branch/Edit/{id}")]
        public async Task<IActionResult> Edit(Guid id, [FromBody] Branch model)
        {
            if (id != model.Id)
                return Json(new { success = false, message = "ID no coincide" });
            if (string.IsNullOrWhiteSpace(model.Name))
                return Json(new { success = false, message = "El nombre es requerido" });
            if (model.CompanyId == null)
                return Json(new { success = false, message = "La compañía es requerida" });
            await _branchService.UpdateAsync(model);
            return Json(new { success = true });
        }

        [HttpDelete]
        [Route("Branch/Delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _branchService.DeleteAsync(id);
                return Json(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error interno del servidor. Por favor intenta nuevamente." });
            }
        }
    }
} 