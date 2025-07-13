using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestBar.Interfaces;
using RestBar.Models;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace RestBar.Controllers
{
    [Authorize(Policy = "SystemConfig")]
    public class CompanyController : Controller
    {
        private readonly ICompanyService _companyService;
        private readonly IAuthService _authService;

        public CompanyController(ICompanyService companyService, IAuthService authService)
        {
            _companyService = companyService;
            _authService = authService;
        }

        // Vista principal
        public async Task<IActionResult> Index()
        {
            try
            {
                Console.WriteLine("🔍 [CompanyController] Index() - Iniciando...");
                
                // Obtener usuario actual y sus asignaciones
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    Console.WriteLine("❌ [CompanyController] Index() - Usuario no autenticado");
                    return View(new List<Company>());
                }

                var userId = Guid.Parse(userIdClaim.Value);
                var currentUser = await _authService.GetCurrentUserAsync(User);
                
                if (currentUser == null || currentUser.Branch == null)
                {
                    Console.WriteLine("❌ [CompanyController] Index() - Usuario o sucursal no encontrado");
                    return View(new List<Company>());
                }

                Console.WriteLine($"✅ [CompanyController] Index() - Usuario: {currentUser.Email}, CompanyId: {currentUser.Branch.CompanyId}");

                // Filtrar compañías por la compañía del usuario actual
                var companies = new List<Company>();
                if (currentUser.Branch.CompanyId.HasValue)
                {
                    var company = await _companyService.GetByIdAsync(currentUser.Branch.CompanyId.Value);
                    if (company != null)
                    {
                        companies.Add(company);
                    }
                }
                
                Console.WriteLine($"📊 [CompanyController] Index() - Compañías encontradas: {companies.Count}");
                return View(companies);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [CompanyController] Index() - Error: {ex.Message}");
                return View(new List<Company>());
            }
        }

        // Obtener compañías del usuario actual (JSON)
        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            try
            {
                Console.WriteLine("🔍 [CompanyController] GetCompanies() - Iniciando...");
                
                // Obtener usuario actual y sus asignaciones
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    Console.WriteLine("❌ [CompanyController] GetCompanies() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var userId = Guid.Parse(userIdClaim.Value);
                var currentUser = await _authService.GetCurrentUserAsync(User);
                
                if (currentUser == null || currentUser.Branch == null)
                {
                    Console.WriteLine("❌ [CompanyController] GetCompanies() - Usuario o sucursal no encontrado");
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }

                Console.WriteLine($"✅ [CompanyController] GetCompanies() - Usuario: {currentUser.Email}, CompanyId: {currentUser.Branch.CompanyId}");

                // Filtrar compañías por la compañía del usuario actual
                var companies = new List<Company>();
                if (currentUser.Branch.CompanyId.HasValue)
                {
                    var company = await _companyService.GetByIdAsync(currentUser.Branch.CompanyId.Value);
                    if (company != null)
                    {
                        companies.Add(company);
                    }
                }
                
                var data = companies.Select(c => new {
                    id = c.Id,
                    name = c.Name,
                    legalId = c.LegalId,
                    taxId = c.TaxId,
                    address = c.Address,
                    phone = c.Phone,
                    email = c.Email,
                    isActive = c.IsActive,
                    createdAt = c.CreatedAt,
                    updatedAt = c.UpdatedAt,
                    createdBy = c.CreatedBy,
                    updatedBy = c.UpdatedBy
                }).ToList();
                
                Console.WriteLine($"📤 [CompanyController] GetCompanies() - Enviando {data.Count} compañías");
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [CompanyController] GetCompanies() - Error: {ex.Message}");
                return Json(new { success = false, message = $"Error al cargar compañías: {ex.Message}" });
            }
        }

        // Obtener compañía por ID (solo la del usuario actual)
        [HttpGet]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                Console.WriteLine($"🔍 [CompanyController] Get() - Iniciando para ID: {id}");
                
                // Obtener usuario actual y sus asignaciones
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    Console.WriteLine("❌ [CompanyController] Get() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var userId = Guid.Parse(userIdClaim.Value);
                var currentUser = await _authService.GetCurrentUserAsync(User);
                
                if (currentUser == null || currentUser.Branch == null)
                {
                    Console.WriteLine("❌ [CompanyController] Get() - Usuario o sucursal no encontrado");
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }

                Console.WriteLine($"✅ [CompanyController] Get() - Usuario: {currentUser.Email}, CompanyId: {currentUser.Branch.CompanyId}");

                // Verificar que el ID solicitado sea el de la compañía del usuario actual
                if (!currentUser.Branch.CompanyId.HasValue || currentUser.Branch.CompanyId.Value != id)
                {
                    Console.WriteLine($"⚠️ [CompanyController] Get() - Acceso denegado: Usuario intenta acceder a compañía {id} pero pertenece a {currentUser.Branch.CompanyId}");
                    return Json(new { success = false, message = "No tienes permisos para acceder a esta compañía" });
                }

                var company = await _companyService.GetByIdAsync(id);
                if (company == null)
                {
                    Console.WriteLine($"❌ [CompanyController] Get() - Compañía no encontrada: {id}");
                    return Json(new { success = false, message = "Compañía no encontrada" });
                }
                
                Console.WriteLine($"✅ [CompanyController] Get() - Compañía encontrada: {company.Name}");
                return Json(new { success = true, data = new {
                    id = company.Id,
                    name = company.Name,
                    legalId = company.LegalId,
                    taxId = company.TaxId,
                    address = company.Address,
                    phone = company.Phone,
                    email = company.Email,
                    isActive = company.IsActive,
                    createdAt = company.CreatedAt,
                    updatedAt = company.UpdatedAt,
                    createdBy = company.CreatedBy,
                    updatedBy = company.UpdatedBy
                }});
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [CompanyController] Get() - Error: {ex.Message}");
                return Json(new { success = false, message = $"Error al cargar compañía: {ex.Message}" });
            }
        }

        // Crear compañía
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Company model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return Json(new { success = false, message = "El nombre es requerido" });
            
            if (string.IsNullOrWhiteSpace(model.LegalId))
                return Json(new { success = false, message = "El ID legal es requerido" });
            
            // Validar que el legal_id no esté duplicado
            var existingCompany = await _companyService.GetByLegalIdAsync(model.LegalId);
            if (existingCompany != null)
                return Json(new { success = false, message = "Ya existe una compañía con este ID legal" });
            
            // ✅ NUEVO: Obtener usuario actual para tracking
            var currentUser = await _authService.GetCurrentUserAsync(User);
            model.CreatedBy = currentUser?.Email ?? "Sistema";
            
            // Remover asignación manual de fecha - se maneja en el servicio o BD
            var created = await _companyService.CreateAsync(model);
            return Json(new { success = true, data = created });
        }

        // Editar compañía (solo la del usuario actual)
        [HttpPut]
        public async Task<IActionResult> Edit(Guid id, [FromBody] Company model)
        {
            try
            {
                Console.WriteLine($"🔍 [CompanyController] Edit() - Iniciando para ID: {id}");
                
                if (id != model.Id)
                {
                    Console.WriteLine($"⚠️ [CompanyController] Edit() - ID no coincide: {id} vs {model.Id}");
                    return Json(new { success = false, message = "ID no coincide" });
                }
                
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    Console.WriteLine($"⚠️ [CompanyController] Edit() - Nombre requerido");
                    return Json(new { success = false, message = "El nombre es requerido" });
                }
                
                // Obtener usuario actual y sus asignaciones
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    Console.WriteLine("❌ [CompanyController] Edit() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var userId = Guid.Parse(userIdClaim.Value);
                var currentUser = await _authService.GetCurrentUserAsync(User);
                
                if (currentUser == null || currentUser.Branch == null)
                {
                    Console.WriteLine("❌ [CompanyController] Edit() - Usuario o sucursal no encontrado");
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }

                Console.WriteLine($"✅ [CompanyController] Edit() - Usuario: {currentUser.Email}, CompanyId: {currentUser.Branch.CompanyId}");

                // Verificar que el ID solicitado sea el de la compañía del usuario actual
                if (!currentUser.Branch.CompanyId.HasValue || currentUser.Branch.CompanyId.Value != id)
                {
                    Console.WriteLine($"⚠️ [CompanyController] Edit() - Acceso denegado: Usuario intenta editar compañía {id} pero pertenece a {currentUser.Branch.CompanyId}");
                    return Json(new { success = false, message = "No tienes permisos para editar esta compañía" });
                }
                
                // ✅ NUEVO: Obtener usuario actual para tracking
                model.UpdatedBy = currentUser?.Email ?? "Sistema";
                
                await _companyService.UpdateAsync(model);
                Console.WriteLine($"✅ [CompanyController] Edit() - Compañía editada exitosamente: {model.Name}");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [CompanyController] Edit() - Error: {ex.Message}");
                return Json(new { success = false, message = $"Error al editar compañía: {ex.Message}" });
            }
        }

        // Eliminar compañía (solo la del usuario actual)
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                Console.WriteLine($"🔍 [CompanyController] Delete() - Iniciando para ID: {id}");
                
                // Obtener usuario actual y sus asignaciones
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    Console.WriteLine("❌ [CompanyController] Delete() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var userId = Guid.Parse(userIdClaim.Value);
                var currentUser = await _authService.GetCurrentUserAsync(User);
                
                if (currentUser == null || currentUser.Branch == null)
                {
                    Console.WriteLine("❌ [CompanyController] Delete() - Usuario o sucursal no encontrado");
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }

                Console.WriteLine($"✅ [CompanyController] Delete() - Usuario: {currentUser.Email}, CompanyId: {currentUser.Branch.CompanyId}");

                // Verificar que el ID solicitado sea el de la compañía del usuario actual
                if (!currentUser.Branch.CompanyId.HasValue || currentUser.Branch.CompanyId.Value != id)
                {
                    Console.WriteLine($"⚠️ [CompanyController] Delete() - Acceso denegado: Usuario intenta eliminar compañía {id} pero pertenece a {currentUser.Branch.CompanyId}");
                    return Json(new { success = false, message = "No tienes permisos para eliminar esta compañía" });
                }
                
                await _companyService.DeleteAsync(id);
                Console.WriteLine($"✅ [CompanyController] Delete() - Compañía eliminada exitosamente: {id}");
                return Json(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"❌ [CompanyController] Delete() - InvalidOperationException: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [CompanyController] Delete() - Error: {ex.Message}");
                return Json(new { success = false, message = "Error interno del servidor. Por favor intenta nuevamente." });
            }
        }

        // Obtener compañía con sucursales
        [HttpGet]
        public async Task<IActionResult> GetCompanyWithBranches(Guid id)
        {
            var company = await _companyService.GetCompanyWithBranchesAsync(id);
            if (company == null)
                return Json(new { success = false, message = "Compañía no encontrada" });
            return Json(new { success = true, data = company });
        }

        // Obtener compañías con sucursales activas
        [HttpGet]
        public async Task<IActionResult> GetCompaniesWithActiveBranches()
        {
            var companies = await _companyService.GetCompaniesWithActiveBranchesAsync();
            return Json(new { success = true, data = companies });
        }

        // Obtener compañía por LegalId
        [HttpGet]
        public async Task<IActionResult> GetByLegalId(string legalId)
        {
            var company = await _companyService.GetByLegalIdAsync(legalId);
            if (company == null)
                return Json(new { success = false, message = "Compañía no encontrada" });
            return Json(new { success = true, data = company });
        }
    }
} 