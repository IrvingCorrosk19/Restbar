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
            var companies = await _companyService.GetAllAsync();
            return View(companies);
        }

        // Obtener todas las compañías (JSON)
        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await _companyService.GetAllAsync();
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
            return Json(new { success = true, data });
        }

        // Obtener compañía por ID
        [HttpGet]
        public async Task<IActionResult> Get(Guid id)
        {
            var company = await _companyService.GetByIdAsync(id);
            if (company == null)
                return Json(new { success = false, message = "Compañía no encontrada" });
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

        // Editar compañía
        [HttpPut]
        public async Task<IActionResult> Edit(Guid id, [FromBody] Company model)
        {
            if (id != model.Id)
                return Json(new { success = false, message = "ID no coincide" });
            if (string.IsNullOrWhiteSpace(model.Name))
                return Json(new { success = false, message = "El nombre es requerido" });
            
            // ✅ NUEVO: Obtener usuario actual para tracking
            var currentUser = await _authService.GetCurrentUserAsync(User);
            model.UpdatedBy = currentUser?.Email ?? "Sistema";
            
            await _companyService.UpdateAsync(model);
            return Json(new { success = true });
        }

        // Eliminar compañía
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _companyService.DeleteAsync(id);
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