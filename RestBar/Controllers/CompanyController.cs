using Microsoft.AspNetCore.Mvc;
using RestBar.Interfaces;
using RestBar.Models;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace RestBar.Controllers
{
    public class CompanyController : Controller
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
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
                createdAt = c.CreatedAt
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
                createdAt = company.CreatedAt
            }});
        }

        // Crear compañía
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Company model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return Json(new { success = false, message = "El nombre es requerido" });
            if (model.CreatedAt == null)
                model.CreatedAt = DateTime.Now;
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
            await _companyService.UpdateAsync(model);
            return Json(new { success = true });
        }

        // Eliminar compañía
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _companyService.DeleteAsync(id);
            return Json(new { success = true });
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