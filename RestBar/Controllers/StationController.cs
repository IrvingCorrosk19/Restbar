using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace RestBar.Controllers
{
    [Authorize(Roles = "admin,manager")]
    public class StationController : Controller
    {
        private readonly IStationService _stationService;
        private readonly IAreaService _areaService;
        private readonly ICompanyService _companyService;
        private readonly IBranchService _branchService;

        public StationController(IStationService stationService, IAreaService areaService, ICompanyService companyService, IBranchService branchService)
        {
            _stationService = stationService;
            _areaService = areaService;
            _companyService = companyService;
            _branchService = branchService;
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
        public async Task<IActionResult> Create([Bind("Name,Type,Icon,AreaId,IsActive")] Station station)
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
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Type,Icon,AreaId,IsActive")] Station station)
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
            try
            {
                Console.WriteLine("🔍 [StationController] GetAreas() - Iniciando carga de áreas del usuario actual...");

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    Console.WriteLine("⚠️ [StationController] GetAreas() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado", data = new List<object>() });
                }

                // Obtener el usuario actual
                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(userId);
                if (currentUser == null)
                {
                    Console.WriteLine("⚠️ [StationController] GetAreas() - Usuario no encontrado");
                    return Json(new { success = false, message = "Usuario no encontrado", data = new List<object>() });
                }

                Console.WriteLine($"👤 [StationController] GetAreas() - Usuario: {currentUser.FullName ?? currentUser.Email}");
                Console.WriteLine($"🏢 [StationController] GetAreas() - CompanyId: {currentUser.CompanyId}");
                Console.WriteLine($"🏪 [StationController] GetAreas() - BranchId: {currentUser.BranchId}");

                // Filtrar áreas por compañía y sucursal del usuario
                var areas = await _areaService.GetAllAsync();
                var filteredAreas = areas.Where(a => 
                    a.CompanyId == currentUser.CompanyId && 
                    a.BranchId == currentUser.BranchId
                ).ToList();

                Console.WriteLine($"📊 [StationController] GetAreas() - Total áreas: {areas.Count()}");
                Console.WriteLine($"📊 [StationController] GetAreas() - Áreas filtradas: {filteredAreas.Count}");

                var data = filteredAreas.Select(a => new {
                    id = a.Id,
                    name = a.Name,
                    description = a.Description
                }).ToList();

                Console.WriteLine($"✅ [StationController] GetAreas() - Enviando {data.Count} áreas");
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [StationController] GetAreas() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [StationController] GetAreas() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error al cargar áreas: {ex.Message}", data = new List<object>() });
            }
        }

        // Métodos auxiliares
        private async Task PopulateAreasDropdown(Guid? selectedAreaId = null)
        {
            var areas = await _areaService.GetAllAsync();
            ViewBag.AreaId = new SelectList(areas, "Id", "Name", selectedAreaId);
        }

        // GET: Station/GetCurrentUserData
        [HttpGet]
        public async Task<IActionResult> GetCurrentUserData()
        {
            try
            {
                Console.WriteLine("🔍 [StationController] GetCurrentUserData() - Obteniendo datos del usuario actual...");

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var userRoleClaim = User.FindFirst(ClaimTypes.Role);

                Console.WriteLine($"👤 [StationController] GetCurrentUserData() - UserId: {userIdClaim?.Value ?? "NULL"}");
                Console.WriteLine($"👤 [StationController] GetCurrentUserData() - UserRole: {userRoleClaim?.Value ?? "NULL"}");

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    Console.WriteLine("⚠️ [StationController] GetCurrentUserData() - Usuario no autenticado o ID inválido");
                    return Json(new { success = false, message = "Usuario no autenticado", data = new { companyId = (Guid?)null, branchId = (Guid?)null } });
                }

                // Obtener el usuario actual con sus asignaciones
                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(userId);

                if (currentUser == null)
                {
                    Console.WriteLine("⚠️ [StationController] GetCurrentUserData() - Usuario no encontrado en la base de datos");
                    return Json(new { success = false, message = "Usuario no encontrado", data = new { companyId = (Guid?)null, branchId = (Guid?)null } });
                }

                Console.WriteLine($"✅ [StationController] GetCurrentUserData() - Usuario encontrado: {currentUser.FullName ?? currentUser.Email}");
                Console.WriteLine($"🏢 [StationController] GetCurrentUserData() - CompanyId: {currentUser.CompanyId}");
                Console.WriteLine($"🏪 [StationController] GetCurrentUserData() - BranchId: {currentUser.BranchId}");

                return Json(new {
                    success = true,
                    data = new {
                        companyId = currentUser.CompanyId,
                        branchId = currentUser.BranchId,
                        userName = currentUser.FullName ?? currentUser.Email,
                        userRole = userRoleClaim?.Value
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [StationController] GetCurrentUserData() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [StationController] GetCurrentUserData() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error al obtener datos del usuario: {ex.Message}", data = new { companyId = (Guid?)null, branchId = (Guid?)null } });
            }
        }

        // GET: Station/GetCompanies
        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            try
            {
                Console.WriteLine("🔍 [StationController] GetCompanies() - Iniciando carga de compañía del usuario actual...");

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    Console.WriteLine("⚠️ [StationController] GetCompanies() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado", data = new List<object>() });
                }

                // Obtener el usuario actual
                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(userId);
                if (currentUser == null)
                {
                    Console.WriteLine("⚠️ [StationController] GetCompanies() - Usuario no encontrado");
                    return Json(new { success = false, message = "Usuario no encontrado", data = new List<object>() });
                }

                if (!currentUser.CompanyId.HasValue)
                {
                    Console.WriteLine("⚠️ [StationController] GetCompanies() - Usuario no tiene compañía asignada");
                    return Json(new { success = false, message = "Usuario no tiene compañía asignada", data = new List<object>() });
                }

                // Obtener la compañía del usuario
                var company = await _companyService.GetByIdAsync(currentUser.CompanyId.Value);
                if (company == null)
                {
                    Console.WriteLine("⚠️ [StationController] GetCompanies() - Compañía no encontrada");
                    return Json(new { success = false, message = "Compañía no encontrada", data = new List<object>() });
                }

                Console.WriteLine($"✅ [StationController] GetCompanies() - Compañía encontrada: {company.Name} (ID: {company.Id})");

                var data = new List<object> { new { id = company.Id, name = company.Name } };
                var response = new { success = true, data };
                Console.WriteLine($"📤 [StationController] GetCompanies() - Enviando respuesta: {System.Text.Json.JsonSerializer.Serialize(response)}");

                return Json(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [StationController] GetCompanies() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [StationController] GetCompanies() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error al cargar compañía: {ex.Message}", data = new List<object>() });
            }
        }

        // GET: Station/GetBranches
        [HttpGet]
        public async Task<IActionResult> GetBranches()
        {
            try
            {
                Console.WriteLine("🔍 [StationController] GetBranches() - Iniciando carga de sucursal del usuario actual...");

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    Console.WriteLine("⚠️ [StationController] GetBranches() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado", data = new List<object>() });
                }

                // Obtener el usuario actual
                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(userId);
                if (currentUser == null)
                {
                    Console.WriteLine("⚠️ [StationController] GetBranches() - Usuario no encontrado");
                    return Json(new { success = false, message = "Usuario no encontrado", data = new List<object>() });
                }

                if (!currentUser.BranchId.HasValue)
                {
                    Console.WriteLine("⚠️ [StationController] GetBranches() - Usuario no tiene sucursal asignada");
                    return Json(new { success = false, message = "Usuario no tiene sucursal asignada", data = new List<object>() });
                }

                // Obtener la sucursal del usuario
                var branch = await _branchService.GetByIdAsync(currentUser.BranchId.Value);
                if (branch == null)
                {
                    Console.WriteLine("⚠️ [StationController] GetBranches() - Sucursal no encontrada");
                    return Json(new { success = false, message = "Sucursal no encontrada", data = new List<object>() });
                }

                Console.WriteLine($"✅ [StationController] GetBranches() - Sucursal encontrada: {branch.Name} (ID: {branch.Id})");

                var data = new List<object> { new { id = branch.Id, name = branch.Name } };
                var response = new { success = true, data };
                Console.WriteLine($"📤 [StationController] GetBranches() - Enviando respuesta: {System.Text.Json.JsonSerializer.Serialize(response)}");

                return Json(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [StationController] GetBranches() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [StationController] GetBranches() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error al cargar sucursal: {ex.Message}", data = new List<object>() });
            }
        }
    }
} 