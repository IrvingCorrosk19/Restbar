using Microsoft.AspNetCore.Mvc;
using RestBar.Interfaces;
using System;
using System.Threading.Tasks;
using System.Linq;
using RestBar.Models;

namespace RestBar.Controllers
{
    public class TableController : Controller
    {
        private readonly ITableService _tableService;
        private readonly IAreaService _areaService;

        public TableController(ITableService tableService, IAreaService areaService)
        {
            _tableService = tableService;
            _areaService = areaService;
        }

        public async Task<IActionResult> Index()
        {
            var tables = await _tableService.GetAllAsync();
            return View(tables);
        }

        [HttpGet]
        public async Task<IActionResult> GetTables()
        {
            var tables = await _tableService.GetAllAsync();
            var data = tables.Select(t => new {
                id = t.Id,
                tableNumber = t.TableNumber,
                capacity = t.Capacity,
                status = t.Status,
                isActive = t.IsActive,
                areaId = t.AreaId,
                areaName = t.Area != null ? t.Area.Name : null
            }).ToList();
            return Json(new { success = true, data });
        }

        [HttpGet]
        public async Task<IActionResult> GetAreas()
        {
            var areas = await _areaService.GetAllAsync();
            var data = areas.Select(a => new { id = a.Id, name = a.Name }).ToList();
            return Json(new { success = true, data });
        }

        [HttpGet]
        public async Task<IActionResult> Get(Guid id)
        {
            var table = await _tableService.GetByIdAsync(id);
            if (table == null)
                return Json(new { success = false, message = "Mesa no encontrada" });
            return Json(new { success = true, data = new {
                id = table.Id,
                tableNumber = table.TableNumber,
                capacity = table.Capacity,
                status = table.Status,
                isActive = table.IsActive,
                areaId = table.AreaId
            }});
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Table model)
        {
            if (string.IsNullOrWhiteSpace(model.TableNumber))
                return Json(new { success = false, message = "El número de mesa es requerido" });
            if (model.Capacity == null || model.Capacity <= 0)
                return Json(new { success = false, message = "La capacidad debe ser mayor a 0" });
            if (model.AreaId == null)
                return Json(new { success = false, message = "El área es requerida" });
            var created = await _tableService.CreateAsync(model);
            return Json(new { success = true, data = created });
        }

        [HttpPut]
        public async Task<IActionResult> Edit(Guid id, [FromBody] Table model)
        {
            if (id != model.Id)
                return Json(new { success = false, message = "ID no coincide" });
            if (string.IsNullOrWhiteSpace(model.TableNumber))
                return Json(new { success = false, message = "El número de mesa es requerido" });
            if (model.Capacity == null || model.Capacity <= 0)
                return Json(new { success = false, message = "La capacidad debe ser mayor a 0" });
            if (model.AreaId == null)
                return Json(new { success = false, message = "El área es requerida" });
            await _tableService.UpdateAsync(model);
            return Json(new { success = true });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _tableService.DeleteAsync(id);
            return Json(new { success = true });
        }
    }
} 