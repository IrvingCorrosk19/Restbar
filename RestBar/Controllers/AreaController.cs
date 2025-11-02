using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestBar.Interfaces;
using RestBar.Models;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace RestBar.Controllers
{
    [Authorize(Roles = "admin,manager")]
    public class AreaController : Controller
    {
        private readonly IAreaService _areaService;
        private readonly IBranchService _branchService;

        public AreaController(IAreaService areaService, IBranchService branchService)
        {
            _areaService = areaService;
            _branchService = branchService;
        }

        public async Task<IActionResult> Index()
        {
            var areas = await _areaService.GetAllAsync();
            return View(areas);
        }

        [HttpGet]
        public async Task<IActionResult> GetAreas()
        {
            var areas = await _areaService.GetAllAsync();
            var data = areas.Select(a => new {
                id = a.Id,
                name = a.Name,
                description = a.Description,
                branchId = a.BranchId,
                branchName = a.Branch != null ? a.Branch.Name : null
            }).ToList();
            return Json(new { success = true, data });
        }

        [HttpGet]
        public async Task<IActionResult> Get(Guid id)
        {
            var area = await _areaService.GetByIdAsync(id);
            if (area == null)
                return Json(new { success = false, message = "Área no encontrada" });
            return Json(new { success = true, data = new {
                id = area.Id,
                name = area.Name,
                description = area.Description,
                branchId = area.BranchId
            }});
        }

        [HttpGet]
        public async Task<IActionResult> GetBranches()
        {
            try
            {
                Console.WriteLine("🔍 [AreaController] GetBranches() - Iniciando carga de sucursales...");
                
                // Obtener el usuario actual con sus asignaciones
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    Console.WriteLine("❌ [AreaController] GetBranches() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var userId = Guid.Parse(userIdClaim.Value);
                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(userId);
                
                if (currentUser == null || currentUser.Branch == null)
                {
                    Console.WriteLine("❌ [AreaController] GetBranches() - Usuario o sucursal no encontrado");
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }
                
                Console.WriteLine($"✅ [AreaController] GetBranches() - Usuario actual: {currentUser.Email}");
                Console.WriteLine($"🏢 [AreaController] GetBranches() - Compañía: {currentUser.Branch?.Company?.Name}");
                Console.WriteLine($"🏪 [AreaController] GetBranches() - Sucursal: {currentUser.Branch?.Name}");

                // Obtener sucursales de la compañía del usuario actual
                var branches = await _branchService.GetByCompanyIdAsync(currentUser.Branch.CompanyId.Value);
                
                Console.WriteLine($"📊 [AreaController] GetBranches() - Sucursales encontradas: {branches.Count()}");
                
                var data = branches.Select(b => new { id = b.Id, name = b.Name }).ToList();
                
                Console.WriteLine($"📤 [AreaController] GetBranches() - Enviando {data.Count} sucursales");
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AreaController] GetBranches() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [AreaController] GetBranches() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error al cargar sucursales: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Area model)
        {
            try
            {
                Console.WriteLine("🔍 [AreaController] Create() - Iniciando creación de área...");
                
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    Console.WriteLine("⚠️ [AreaController] Create() - Nombre requerido");
                    return Json(new { success = false, message = "El nombre es requerido" });
                }
                
                // Obtener el usuario actual con sus asignaciones
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    Console.WriteLine("❌ [AreaController] Create() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }
                
                var userId = Guid.Parse(userIdClaim.Value);
                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(userId);
                
                if (currentUser == null || currentUser.Branch == null)
                {
                    Console.WriteLine("❌ [AreaController] Create() - Usuario o sucursal no encontrado");
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }
                
                // Asignar automáticamente CompanyId y BranchId del usuario actual
                model.CompanyId = currentUser.Branch.CompanyId;
                model.BranchId = currentUser.BranchId;
                
                Console.WriteLine($"✅ [AreaController] Create() - Usuario: {currentUser.Email}");
                Console.WriteLine($"🏢 [AreaController] Create() - Compañía asignada: {currentUser.Branch.CompanyId}");
                Console.WriteLine($"🏪 [AreaController] Create() - Sucursal asignada: {currentUser.BranchId}");
                Console.WriteLine($"📝 [AreaController] Create() - Área a crear: {model.Name}");
                
                var created = await _areaService.CreateAsync(model);
                
                Console.WriteLine($"✅ [AreaController] Create() - Área creada exitosamente: {created.Id}");
                return Json(new { success = true, data = created });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AreaController] Create() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [AreaController] Create() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error al crear área: {ex.Message}" });
            }
        }

        // ✅ NUEVO: Método CreateAjax para crear áreas desde modales
        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromForm] Area model)
        {
            try
            {
                Console.WriteLine("🔍 [AreaController] CreateAjax() - Iniciando creación de área...");
                
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    Console.WriteLine("⚠️ [AreaController] CreateAjax() - Nombre requerido");
                    return Json(new { success = false, message = "El nombre es requerido" });
                }
                
                // Obtener el usuario actual con sus asignaciones
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    Console.WriteLine("❌ [AreaController] CreateAjax() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }
                
                var userId = Guid.Parse(userIdClaim.Value);
                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(userId);
                
                if (currentUser == null || currentUser.Branch == null)
                {
                    Console.WriteLine("❌ [AreaController] CreateAjax() - Usuario o sucursal no encontrado");
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }
                
                // Asignar automáticamente CompanyId y BranchId del usuario actual
                model.CompanyId = currentUser.Branch.CompanyId;
                model.BranchId = currentUser.BranchId;
                
                Console.WriteLine($"✅ [AreaController] CreateAjax() - Usuario: {currentUser.Email}");
                Console.WriteLine($"🏢 [AreaController] CreateAjax() - Compañía asignada: {currentUser.Branch.CompanyId}");
                Console.WriteLine($"🏪 [AreaController] CreateAjax() - Sucursal asignada: {currentUser.BranchId}");
                Console.WriteLine($"📝 [AreaController] CreateAjax() - Área a crear: {model.Name}");
                
                var created = await _areaService.CreateAsync(model);
                
                Console.WriteLine($"✅ [AreaController] CreateAjax() - Área creada exitosamente: {created.Id}");
                return Json(new { success = true, data = new { id = created.Id, name = created.Name } });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AreaController] CreateAjax() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [AreaController] CreateAjax() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error al crear área: {ex.Message}" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Edit(Guid id, [FromBody] Area model)
        {
            try
            {
                Console.WriteLine("🔍 [AreaController] Edit() - Iniciando edición de área...");
                
                if (id != model.Id)
                {
                    Console.WriteLine("⚠️ [AreaController] Edit() - ID no coincide");
                    return Json(new { success = false, message = "ID no coincide" });
                }
                
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    Console.WriteLine("⚠️ [AreaController] Edit() - Nombre requerido");
                    return Json(new { success = false, message = "El nombre es requerido" });
                }
                
                // Obtener el usuario actual con sus asignaciones
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    Console.WriteLine("❌ [AreaController] Edit() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }
                
                var userId = Guid.Parse(userIdClaim.Value);
                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(userId);
                
                if (currentUser == null || currentUser.Branch == null)
                {
                    Console.WriteLine("❌ [AreaController] Edit() - Usuario o sucursal no encontrado");
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }
                
                // Asignar automáticamente CompanyId y BranchId del usuario actual
                model.CompanyId = currentUser.Branch.CompanyId;
                model.BranchId = currentUser.BranchId;
                
                // ✅ NUEVO: Obtener usuario actual para tracking
                var userNameClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Name) ?? 
                                   User.FindFirst(System.Security.Claims.ClaimTypes.Email);
                model.UpdatedBy = userNameClaim?.Value ?? currentUser.Email;
                
                Console.WriteLine($"✅ [AreaController] Edit() - Usuario: {currentUser.Email}");
                Console.WriteLine($"🏢 [AreaController] Edit() - Compañía asignada: {currentUser.Branch.CompanyId}");
                Console.WriteLine($"🏪 [AreaController] Edit() - Sucursal asignada: {currentUser.BranchId}");
                Console.WriteLine($"📝 [AreaController] Edit() - Área a editar: {model.Name}");
                Console.WriteLine($"👤 [AreaController] Edit() - Actualizado por: {model.UpdatedBy}");
                
                await _areaService.UpdateAsync(model);
                
                Console.WriteLine($"✅ [AreaController] Edit() - Área editada exitosamente: {model.Id}");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AreaController] Edit() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [AreaController] Edit() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error al editar área: {ex.Message}" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _areaService.DeleteAsync(id);
            return Json(new { success = true });
        }
    }
} 