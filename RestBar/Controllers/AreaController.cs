using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestBar.Interfaces;
using RestBar.Models;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;

namespace RestBar.Controllers
{
    [Authorize(Roles = "admin,manager")]
    public class AreaController : Controller
    {
        private readonly IAreaService _areaService;
        private readonly IBranchService _branchService;
        private readonly ICompanyService _companyService;

        public AreaController(IAreaService areaService, IBranchService branchService, ICompanyService companyService)
        {
            _areaService = areaService;
            _branchService = branchService;
            _companyService = companyService;
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
                branchName = a.Branch != null ? a.Branch.Name : null,
                companyId = a.CompanyId,
                companyName = a.Company != null ? a.Company.Name : null
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
                branchId = area.BranchId,
                companyId = area.CompanyId
            }});
        }

        [HttpGet]
        public async Task<IActionResult> GetBranches()
        {
            try
            {
                Console.WriteLine("🔍 [AreaController] GetBranches() - Iniciando carga de sucursal del usuario actual...");
                
                // Obtener el usuario actual
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    Console.WriteLine("⚠️ [AreaController] GetBranches() - Usuario no autenticado o ID inválido");
                    return Json(new { success = false, message = "Usuario no autenticado", data = new List<object>() });
                }

                // Obtener el usuario con sus datos
                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(userId);
                if (currentUser == null)
                {
                    Console.WriteLine("⚠️ [AreaController] GetBranches() - Usuario no encontrado");
                    return Json(new { success = false, message = "Usuario no encontrado", data = new List<object>() });
                }

                Console.WriteLine($"👤 [AreaController] GetBranches() - Usuario: {currentUser.FullName ?? currentUser.Email}");
                Console.WriteLine($"🏪 [AreaController] GetBranches() - BranchId del usuario: {currentUser.BranchId}");

                if (currentUser.BranchId == null)
                {
                    Console.WriteLine("⚠️ [AreaController] GetBranches() - Usuario no tiene sucursal asignada");
                    return Json(new { success = false, message = "Usuario no tiene sucursal asignada", data = new List<object>() });
                }

                // Obtener la sucursal específica del usuario
                var userBranch = await _branchService.GetByIdAsync(currentUser.BranchId.Value);
                if (userBranch == null)
                {
                    Console.WriteLine($"⚠️ [AreaController] GetBranches() - Sucursal no encontrada para ID: {currentUser.BranchId}");
                    return Json(new { success = false, message = "Sucursal del usuario no encontrada", data = new List<object>() });
                }

                Console.WriteLine($"✅ [AreaController] GetBranches() - Sucursal encontrada: {userBranch.Name}");

                // Crear lista con solo la sucursal del usuario
                var data = new List<object> { new { id = userBranch.Id, name = userBranch.Name } };
                Console.WriteLine($"📊 [AreaController] GetBranches() - Datos procesados: {data.Count}");

                var response = new { success = true, data };
                Console.WriteLine($"📤 [AreaController] GetBranches() - Enviando respuesta: {System.Text.Json.JsonSerializer.Serialize(response)}");

                return Json(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AreaController] GetBranches() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [AreaController] GetBranches() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error al cargar sucursal: {ex.Message}", data = new List<object>() });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            try
            {
                Console.WriteLine("🔍 [AreaController] GetCompanies() - Iniciando carga de compañía del usuario actual...");
                
                // Obtener el usuario actual
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    Console.WriteLine("⚠️ [AreaController] GetCompanies() - Usuario no autenticado o ID inválido");
                    return Json(new { success = false, message = "Usuario no autenticado", data = new List<object>() });
                }

                // Obtener el usuario con sus datos
                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(userId);
                if (currentUser == null)
                {
                    Console.WriteLine("⚠️ [AreaController] GetCompanies() - Usuario no encontrado");
                    return Json(new { success = false, message = "Usuario no encontrado", data = new List<object>() });
                }

                Console.WriteLine($"👤 [AreaController] GetCompanies() - Usuario: {currentUser.FullName ?? currentUser.Email}");
                Console.WriteLine($"🏢 [AreaController] GetCompanies() - CompanyId del usuario: {currentUser.CompanyId}");

                if (currentUser.CompanyId == null)
                {
                    Console.WriteLine("⚠️ [AreaController] GetCompanies() - Usuario no tiene compañía asignada");
                    return Json(new { success = false, message = "Usuario no tiene compañía asignada", data = new List<object>() });
                }

                // Obtener la compañía específica del usuario
                var userCompany = await _companyService.GetByIdAsync(currentUser.CompanyId.Value);
                if (userCompany == null)
                {
                    Console.WriteLine($"⚠️ [AreaController] GetCompanies() - Compañía no encontrada para ID: {currentUser.CompanyId}");
                    return Json(new { success = false, message = "Compañía del usuario no encontrada", data = new List<object>() });
                }

                Console.WriteLine($"✅ [AreaController] GetCompanies() - Compañía encontrada: {userCompany.Name}");

                // Crear lista con solo la compañía del usuario
                var data = new List<object> { new { id = userCompany.Id, name = userCompany.Name } };
                Console.WriteLine($"📊 [AreaController] GetCompanies() - Datos procesados: {data.Count}");

                var response = new { success = true, data };
                Console.WriteLine($"📤 [AreaController] GetCompanies() - Enviando respuesta: {System.Text.Json.JsonSerializer.Serialize(response)}");

                return Json(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AreaController] GetCompanies() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [AreaController] GetCompanies() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error al cargar compañía: {ex.Message}", data = new List<object>() });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrentUserData()
        {
            try
            {
                Console.WriteLine("🔍 [AreaController] GetCurrentUserData() - Obteniendo datos del usuario actual...");
                
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var userRoleClaim = User.FindFirst(ClaimTypes.Role);
                
                Console.WriteLine($"👤 [AreaController] GetCurrentUserData() - UserId: {userIdClaim?.Value ?? "NULL"}");
                Console.WriteLine($"👤 [AreaController] GetCurrentUserData() - UserRole: {userRoleClaim?.Value ?? "NULL"}");
                
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    Console.WriteLine("⚠️ [AreaController] GetCurrentUserData() - Usuario no autenticado o ID inválido");
                    return Json(new { success = false, message = "Usuario no autenticado", data = new { companyId = (Guid?)null, branchId = (Guid?)null } });
                }

                // Obtener el usuario actual con sus asignaciones
                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(userId);
                
                if (currentUser == null)
                {
                    Console.WriteLine("⚠️ [AreaController] GetCurrentUserData() - Usuario no encontrado en la base de datos");
                    return Json(new { success = false, message = "Usuario no encontrado", data = new { companyId = (Guid?)null, branchId = (Guid?)null } });
                }

                Console.WriteLine($"✅ [AreaController] GetCurrentUserData() - Usuario encontrado: {currentUser.FullName ?? currentUser.Email}");
                Console.WriteLine($"🏢 [AreaController] GetCurrentUserData() - CompanyId: {currentUser.CompanyId}");
                Console.WriteLine($"🏪 [AreaController] GetCurrentUserData() - BranchId: {currentUser.BranchId}");

                return Json(new { 
                    success = true, 
                    data = new { 
                        companyId = currentUser.CompanyId, // ✅ Ahora sí está disponible
                        branchId = currentUser.BranchId,
                        userName = currentUser.FullName ?? currentUser.Email,
                        userRole = userRoleClaim?.Value
                    } 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AreaController] GetCurrentUserData() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [AreaController] GetCurrentUserData() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error al obtener datos del usuario: {ex.Message}", data = new { companyId = (Guid?)null, branchId = (Guid?)null } });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Area model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return Json(new { success = false, message = "El nombre es requerido" });
            if (model.BranchId == null)
                return Json(new { success = false, message = "La sucursal es requerida" });
            if (model.CompanyId == null)
                return Json(new { success = false, message = "La compañía es requerida" });
            var created = await _areaService.CreateAsync(model);
            return Json(new { success = true, data = created });
        }

        [HttpPut]
        public async Task<IActionResult> Edit(Guid id, [FromBody] Area model)
        {
            if (id != model.Id)
                return Json(new { success = false, message = "ID no coincide" });
            if (string.IsNullOrWhiteSpace(model.Name))
                return Json(new { success = false, message = "El nombre es requerido" });
            if (model.BranchId == null)
                return Json(new { success = false, message = "La sucursal es requerida" });
            if (model.CompanyId == null)
                return Json(new { success = false, message = "La compañía es requerida" });
            await _areaService.UpdateAsync(model);
            return Json(new { success = true });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _areaService.DeleteAsync(id);
            return Json(new { success = true });
        }
    }
} 