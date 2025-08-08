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
    [Authorize(Roles = "admin")]
    public class BranchController : Controller
    {
        private readonly IBranchService _branchService;
        private readonly IAuthService _authService;

        public BranchController(IBranchService branchService, IAuthService authService)
        {
            _branchService = branchService;
            _authService = authService;
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Branch model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Name))
                    return Json(new { success = false, message = "El nombre es requerido" });

                // Obtener la compañía del usuario logueado
                var currentUser = await _authService.GetCurrentUserAsync(User);
                if (currentUser?.Branch?.CompanyId == null)
                {
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                // Asignar automáticamente la compañía del usuario logueado
                model.CompanyId = currentUser.Branch.CompanyId.Value;
                model.CreatedAt = DateTime.UtcNow;
                
                var created = await _branchService.CreateAsync(model);
                return Json(new { success = true, data = created });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al crear la sucursal" });
            }
        }

        [HttpPut]
        [Route("Branch/Edit/{id}")]
        public async Task<IActionResult> Edit(Guid id, [FromBody] Branch model)
        {
            try
            {
                if (id != model.Id)
                    return Json(new { success = false, message = "ID no coincide" });
                if (string.IsNullOrWhiteSpace(model.Name))
                    return Json(new { success = false, message = "El nombre es requerido" });

                // Obtener la compañía del usuario logueado
                var currentUser = await _authService.GetCurrentUserAsync(User);
                if (currentUser?.Branch?.CompanyId == null)
                {
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                // Asignar automáticamente la compañía del usuario logueado
                model.CompanyId = currentUser.Branch.CompanyId.Value;
                
                await _branchService.UpdateAsync(model);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al actualizar la sucursal" });
            }
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