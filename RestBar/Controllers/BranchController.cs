using Microsoft.AspNetCore.Mvc;
using RestBar.Interfaces;
using RestBar.Models;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace RestBar.Controllers
{
    public class BranchController : Controller
    {
        private readonly IBranchService _branchService;

        public BranchController(IBranchService branchService)
        {
            _branchService = branchService;
        }

        public async Task<IActionResult> Index()
        {
            var branches = await _branchService.GetAllAsync();
            return View(branches);
        }

        [HttpGet]
        public async Task<IActionResult> GetBranches()
        {
            var branches = await _branchService.GetAllAsync();
            var data = branches.Select(b => new {
                id = b.Id,
                name = b.Name,
                address = b.Address,
                phone = b.Phone,
                isActive = b.IsActive,
                createdAt = b.CreatedAt
            }).ToList();
            return Json(new { success = true, data });
        }

        [HttpGet]
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
            if (string.IsNullOrWhiteSpace(model.Name))
                return Json(new { success = false, message = "El nombre es requerido" });
            if (model.CompanyId == null)
                return Json(new { success = false, message = "La compañía es requerida" });
            model.CreatedAt = DateTime.Now;
            var created = await _branchService.CreateAsync(model);
            return Json(new { success = true, data = created });
        }

        [HttpPut]
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
        public async Task<IActionResult> Delete(Guid id)
        {
            await _branchService.DeleteAsync(id);
            return Json(new { success = true });
        }
    }
} 