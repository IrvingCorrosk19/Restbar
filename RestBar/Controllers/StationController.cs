using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RestBar.Controllers
{
    [Authorize(Roles = "admin,manager")]
    public class StationController : Controller
    {
        private readonly IStationService _stationService;
        private readonly IAreaService _areaService;
        private readonly IBranchService _branchService;
        private readonly IAuthService _authService;

        public StationController(IStationService stationService, IAreaService areaService, IBranchService branchService, IAuthService authService)
        {
            _stationService = stationService;
            _areaService = areaService;
            _branchService = branchService;
            _authService = authService;
        }

        // GET: Station
        public async Task<IActionResult> Index()
        {
            var stations = await _stationService.GetAllStationsAsync();
            return View(stations);
        }

        // GET: Station/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (!id.HasValue)
                return NotFound();

            var station = await _stationService.GetStationByIdAsync(id.Value);
            if (station == null)
                return NotFound();

            return View(station);
        }

        // GET: Station/Details/5 (AJAX)
        [HttpGet]
        public async Task<IActionResult> DetailsAjax(Guid id)
        {
            var station = await _stationService.GetStationByIdAsync(id);
            if (station == null)
                return Json(new { success = false, message = "Estación no encontrada" });

            var detailsHtml = $@"
                <div class='row'>
                    <div class='col-md-6'>
                        <dl class='row'>
                            <dt class='col-sm-4'>ID:</dt>
                            <dd class='col-sm-8'>{station.Id}</dd>
                            <dt class='col-sm-4'>Nombre:</dt>
                            <dd class='col-sm-8'>{station.Name}</dd>
                            <dt class='col-sm-4'>Tipo:</dt>
                            <dd class='col-sm-8'>
                                <span class='badge bg-info'>{station.Type}</span>
                            </dd>
                            <dt class='col-sm-4'>Área:</dt>
                            <dd class='col-sm-8'>
                                <span class='badge bg-secondary'>{(station.Area?.Name ?? "No asignada")}</span>
                            </dd>
                            <dt class='col-sm-4'>Estado:</dt>
                            <dd class='col-sm-8'>
                                <span class='badge {(station.IsActive ? "bg-success" : "bg-danger")}'>
                                    {(station.IsActive ? "Activa" : "Inactiva")}
                                </span>
                            </dd>
                            <dt class='col-sm-4'>Productos:</dt>
                            <dd class='col-sm-8'>
                                <span class='badge bg-warning'>{station.Products.Count} producto(s)</span>
                            </dd>
                        </dl>
                    </div>
                    <div class='col-md-6'>
                        {(station.Products.Any() ? $@"
                            <h5>Productos de esta Estación:</h5>
                            <div class='table-responsive'>
                                <table class='table table-sm table-striped'>
                                    <thead>
                                        <tr>
                                            <th>Producto</th>
                                            <th>Precio</th>
                                            <th>Estado</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {string.Join("", station.Products.OrderBy(p => p.Name).Select(p => $@"
                                            <tr>
                                                <td>{p.Name}</td>
                                                <td>{p.Price:C}</td>
                                                <td>
                                                    <span class='badge {(p.IsActive == true ? "bg-success" : "bg-danger")}'>
                                                        {(p.IsActive == true ? "Activo" : "Inactivo")}
                                                    </span>
                                                </td>
                                            </tr>
                                        "))}
                                    </tbody>
                                </table>
                            </div>
                        " : @"
                            <div class='alert alert-info'>
                                <i class='fas fa-info-circle'></i>
                                Esta estación no tiene productos asociados.
                            </div>
                        ")}
                    </div>
                </div>";

            return Content(detailsHtml, "text/html");
        }

        // GET: Station/Create
        public async Task<IActionResult> Create()
        {
            await PopulateAreasDropdown();
            return View();
        }

        // POST: Station/Create
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Name,Type,Icon,AreaId,CompanyId,BranchId,IsActive")] Station station)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Convertir string vacío a null para Icon
                    if (string.IsNullOrWhiteSpace(station.Icon))
                        station.Icon = null;
                    
                    await _stationService.CreateStationAsync(station);
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            
            await PopulateAreasDropdown(station.AreaId);
            return View(station);
        }

        // POST: Station/Create (AJAX)
        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromForm] Station station)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new
                {
                    success = false,
                    message = "Datos inválidos",
                    errors
                });
            }

            try
            {
                if (station.Id == Guid.Empty)
                    station.Id = Guid.NewGuid();

                // Convertir string vacío a null para Icon
                if (string.IsNullOrWhiteSpace(station.Icon))
                    station.Icon = null;

                var created = await _stationService.CreateStationAsync(station);

                return Json(new
                {
                    success = true,
                    message = "Estación creada correctamente",
                    data = new { 
                        id = created.Id, 
                        name = created.Name, 
                        type = created.Type,
                        areaId = created.AreaId,
                        areaName = created.Area?.Name,
                        isActive = created.IsActive,
                        icon = created.Icon
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error interno del servidor" });
            }
        }

        // GET: Station/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (!id.HasValue)
                return NotFound();

            var station = await _stationService.GetStationByIdAsync(id.Value);
            if (station == null)
                return NotFound();

            await PopulateAreasDropdown(station.AreaId);
            return View(station);
        }

        // POST: Station/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Type,Icon,AreaId,CompanyId,BranchId,IsActive")] Station station)
        {
            if (id != station.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Convertir string vacío a null para Icon
                    if (string.IsNullOrWhiteSpace(station.Icon))
                        station.Icon = null;
                    
                    await _stationService.UpdateStationAsync(id, station);
                    return RedirectToAction(nameof(Index));
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            
            await PopulateAreasDropdown(station.AreaId);
            return View(station);
        }

        // POST: Station/Edit/5 (AJAX)
        [HttpPost]
        [Route("Station/EditAjax/{id}")]
        public async Task<IActionResult> EditAjax(Guid id, [FromForm] Station station)
        {
            if (id != station.Id)
                return Json(new { success = false, message = "ID de estación no válido" });

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Datos inválidos" });

            try
            {
                // Convertir string vacío a null para Icon
                if (string.IsNullOrWhiteSpace(station.Icon))
                    station.Icon = null;

                var updated = await _stationService.UpdateStationAsync(id, station);
                return Json(new { 
                    success = true, 
                    message = "Estación actualizada correctamente",
                    data = new { 
                        id = updated.Id, 
                        name = updated.Name, 
                        type = updated.Type,
                        areaId = updated.AreaId,
                        areaName = updated.Area?.Name,
                        isActive = updated.IsActive,
                        icon = updated.Icon
                    }
                });
            }
            catch (KeyNotFoundException)
            {
                return Json(new { success = false, message = "Estación no encontrada" });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error interno del servidor" });
            }
        }

        // GET: Station/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (!id.HasValue)
                return NotFound();

            var station = await _stationService.GetStationByIdAsync(id.Value);
            if (station == null)
                return NotFound();

            // Verificar si tiene productos asociados
            var hasProducts = await _stationService.StationHasProductsAsync(id.Value);
            var productCount = await _stationService.GetProductCountAsync(id.Value);
            
            ViewBag.HasProducts = hasProducts;
            ViewBag.ProductCount = productCount;

            return View(station);
        }

        // POST: Station/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var result = await _stationService.DeleteStationAsync(id);
                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return NotFound();
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // POST: Station/Delete/5 (AJAX)
        [HttpPost]
        [Route("Station/DeleteAjax/{id}")]
        public async Task<IActionResult> DeleteAjax(Guid id)
        {
            try
            {
                var result = await _stationService.DeleteStationAsync(id);
                if (result)
                {
                    return Json(new { success = true, message = "Estación eliminada correctamente" });
                }
                else
                {
                    return Json(new { success = false, message = "Estación no encontrada" });
                }
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error interno del servidor" });
            }
        }

        // API Endpoints
        [HttpGet]
        public async Task<IActionResult> GetStations()
        {
            var stations = await _stationService.GetAllStationsAsync();
            var data = stations.Select(s => new { 
                id = s.Id, 
                name = s.Name, 
                type = s.Type,
                areaId = s.AreaId,
                areaName = s.Area?.Name,
                isActive = s.IsActive,
                icon = s.Icon,
                productCount = s.Products.Count
            }).ToList();
            return Json(new { success = true, data });
        }

        [HttpGet]
        public async Task<IActionResult> GetStationTypes()
        {
            var types = await _stationService.GetDistinctStationTypesAsync();
            return Json(new { success = true, data = types });
        }

        [HttpGet]
        [Route("Station/GetStationById/{id}")]
        public async Task<IActionResult> GetStationById(Guid id)
        {
            var station = await _stationService.GetStationByIdAsync(id);
            if (station == null)
                return Json(new { success = false, message = "Estación no encontrada" });

            var data = new
            {
                id = station.Id,
                name = station.Name,
                type = station.Type,
                areaId = station.AreaId,
                areaName = station.Area?.Name,
                companyId = station.CompanyId,
                companyName = station.Company?.Name,
                branchId = station.BranchId,
                branchName = station.Branch?.Name,
                isActive = station.IsActive,
                icon = station.Icon,
                productCount = station.Products.Count
            };

            return Json(new { success = true, data });
        }

        [HttpGet]
        [Route("Station/GetAreas")]
        public async Task<IActionResult> GetAreas()
        {
            var areas = await _areaService.GetAllAsync();
            var data = areas.Select(a => new {
                id = a.Id,
                name = a.Name,
                description = a.Description
            }).ToList();
            return Json(new { success = true, data });
        }

        // Métodos auxiliares
        private async Task PopulateAreasDropdown(Guid? selectedAreaId = null)
        {
            var areas = await _areaService.GetAllAsync();
            ViewBag.AreaId = new SelectList(areas, "Id", "Name", selectedAreaId);
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
                    userCompanyId = currentUser.Branch.CompanyId,
                    totalBranchesInDb = branches.Count(),
                    filteredBranchesCount = userCompanyBranches.Count(),
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
                return Json(new { success = false, message = "Error al obtener las sucursales" });
            }
        }

        [HttpGet]
        [Route("Station/GetAreasByBranch/{branchId}")]
        public async Task<IActionResult> GetAreasByBranch(Guid branchId)
        {
            try
            {
                var areas = await _areaService.GetAllAsync();
                var branchAreas = areas
                    .Where(a => a.BranchId == branchId)
                    .Select(a => new { id = a.Id, name = a.Name })
                    .ToList();

                // Agregar información de depuración
                var debugInfo = new
                {
                    requestedBranchId = branchId,
                    totalAreasInDb = areas.Count(),
                    filteredAreasCount = branchAreas.Count(),
                    allAreas = areas.Select(a => new { id = a.Id, name = a.Name, branchId = a.BranchId }).ToList()
                };

                return Json(new { 
                    success = true, 
                    data = branchAreas,
                    debug = debugInfo
                }, new System.Text.Json.JsonSerializerOptions
                {
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener las áreas de la sucursal" });
            }
        }
    }
} 