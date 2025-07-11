using Microsoft.AspNetCore.Mvc;
using RestBar.Interfaces;
using RestBar.Models;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace RestBar.Controllers
{
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
            var branches = await _branchService.GetAllAsync();
            var data = branches.Select(b => new { id = b.Id, name = b.Name }).ToList();
            return Json(new { success = true, data });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Area model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return Json(new { success = false, message = "El nombre es requerido" });
            if (model.BranchId == null)
                return Json(new { success = false, message = "La sucursal es requerida" });
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