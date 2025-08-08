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
        private readonly IAuthService _authService;

        public AreaController(IAreaService areaService, IBranchService branchService, IAuthService authService)
        {
            _areaService = areaService;
            _branchService = branchService;
            _authService = authService;
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
        public async Task<IActionResult> GetUserCompany()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync(User);
                if (currentUser?.Branch?.CompanyId == null)
                {
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                return Json(new { 
                    success = true, 
                    data = new { 
                        companyId = currentUser.Branch.CompanyId,
                        companyName = currentUser.Branch.Company?.Name
                    } 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener la compañía del usuario" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBranchesByUserCompany()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync(User);
                if (currentUser?.Branch?.CompanyId == null)
                {
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                var branches = await _branchService.GetAllAsync();

                var userCompanyBranches = branches
                    .Where(b => b.CompanyId == currentUser.Branch.CompanyId)
                    .Select(b => new { id = b.Id, name = b.Name })
                    .ToList();

                // Agregar información de depuración
                var debugInfo = new
                {
                    currentUserCompanyId = currentUser.Branch.CompanyId,
                    totalBranches = branches.Count(),
                    filteredBranches = userCompanyBranches.Count,
                    allBranches = branches.Select(b => new { id = b.Id, name = b.Name, companyId = b.CompanyId }).ToList()
                };

                return Json(new { 
                    success = true, 
                    data = userCompanyBranches,
                    debug = debugInfo
                }, new System.Text.Json.JsonSerializerOptions
                {
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener las sucursales", error = ex.Message });
            }
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
            try
            {
                if (string.IsNullOrWhiteSpace(model.Name))
                    return Json(new { success = false, message = "El nombre es requerido" });
                if (model.BranchId == null)
                    return Json(new { success = false, message = "La sucursal es requerida" });

                // Verificar que la sucursal pertenece a la compañía del usuario
                var currentUser = await _authService.GetCurrentUserAsync(User);
                if (currentUser?.Branch?.CompanyId == null)
                {
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                var branch = await _branchService.GetByIdAsync(model.BranchId.Value);
                if (branch == null || branch.CompanyId != currentUser.Branch.CompanyId)
                {
                    return Json(new { success = false, message = "La sucursal seleccionada no es válida para su compañía" });
                }

                var created = await _areaService.CreateAsync(model);
                return Json(new { success = true, data = created });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al crear el área" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Edit(Guid id, [FromBody] Area model)
        {
            try
            {
                if (id != model.Id)
                    return Json(new { success = false, message = "ID no coincide" });
                if (string.IsNullOrWhiteSpace(model.Name))
                    return Json(new { success = false, message = "El nombre es requerido" });
                if (model.BranchId == null)
                    return Json(new { success = false, message = "La sucursal es requerida" });

                // Verificar que la sucursal pertenece a la compañía del usuario
                var currentUser = await _authService.GetCurrentUserAsync(User);
                if (currentUser?.Branch?.CompanyId == null)
                {
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                var branch = await _branchService.GetByIdAsync(model.BranchId.Value);
                if (branch == null || branch.CompanyId != currentUser.Branch.CompanyId)
                {
                    return Json(new { success = false, message = "La sucursal seleccionada no es válida para su compañía" });
                }

                await _areaService.UpdateAsync(model);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al actualizar el área" });
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