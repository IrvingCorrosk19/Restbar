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

        public StationController(IStationService stationService, IAreaService areaService)
        {
            _stationService = stationService;
            _areaService = areaService;
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

            var activeAssignments = station.StockAssignments
                .Where(sa => sa.IsActive)
                .OrderBy(sa => sa.Product?.Name)
                .ToList();

            var assignmentsRows = string.Join("", activeAssignments.Select(sa =>
            {
                var product = sa.Product;
                var productName = product?.Name ?? "Producto no disponible";
                var productPrice = product != null ? product.Price.ToString("C") : "-";
                var productStatusBadge = product?.IsActive == true
                    ? "<span class='badge bg-success'>Activo</span>"
                    : "<span class='badge bg-danger'>Inactivo</span>";

                return $@"
                    <tr>
                        <td>{productName}</td>
                        <td>{sa.Stock:N2}</td>
                        <td>{productPrice}</td>
                        <td>{productStatusBadge}</td>
                    </tr>";
            }));

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
                                {(activeAssignments.Any()
                                    ? $"<span class='badge bg-warning'>{activeAssignments.Count} producto(s)</span>"
                                    : "<span class='badge bg-secondary'>Sin productos</span>")}
                            </dd>
                        </dl>
                    </div>
                    <div class='col-md-6'>
                        {(activeAssignments.Any() ? $@"
                            <h5>Productos de esta Estación:</h5>
                            <div class='table-responsive'>
                                <table class='table table-sm table-striped'>
                                    <thead>
                                        <tr>
                                            <th>Producto</th>
                                            <th>Stock en estación</th>
                                            <th>Precio</th>
                                            <th>Estado</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {assignmentsRows}
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
            try
            {
                Console.WriteLine("🔍 [StationController] CreateAjax() - Iniciando creación de estación...");
                
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    Console.WriteLine("⚠️ [StationController] CreateAjax() - Errores de validación:", errors);
                    return Json(new
                    {
                        success = false,
                        message = "Datos inválidos",
                        errors
                    });
                }

                // Obtener el usuario actual para auditoría
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var userNameClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Name);
                
                if (userIdClaim == null)
                {
                    Console.WriteLine("❌ [StationController] CreateAjax() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(Guid.Parse(userIdClaim.Value));
                if (currentUser == null || currentUser.Branch == null)
                {
                    Console.WriteLine("❌ [StationController] CreateAjax() - Usuario o sucursal no encontrado");
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }

                if (station.Id == Guid.Empty)
                    station.Id = Guid.NewGuid();

                // Convertir string vacío a null para Icon
                if (string.IsNullOrWhiteSpace(station.Icon))
                    station.Icon = null;

                // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService
                station.CreatedBy = userNameClaim?.Value ?? currentUser.Email;
                station.UpdatedBy = userNameClaim?.Value ?? currentUser.Email;
                
                // Asignar CompanyId y BranchId del usuario actual
                station.CompanyId = currentUser.Branch.CompanyId;
                station.BranchId = currentUser.BranchId;

                Console.WriteLine($"✅ [StationController] CreateAjax() - Usuario: {currentUser.Email}");
                Console.WriteLine($"🏢 [StationController] CreateAjax() - Compañía: {currentUser.Branch.CompanyId}");
                Console.WriteLine($"🏪 [StationController] CreateAjax() - Sucursal: {currentUser.BranchId}");
                Console.WriteLine($"📝 [StationController] CreateAjax() - Estación: {station.Name}");
                Console.WriteLine($"👤 [StationController] CreateAjax() - Creado por: {station.CreatedBy}");
                Console.WriteLine($"🕒 [StationController] CreateAjax() - Creado en: {station.CreatedAt}");

                var created = await _stationService.CreateStationAsync(station);

                Console.WriteLine($"✅ [StationController] CreateAjax() - Estación creada exitosamente: {created.Id}");

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
                        icon = created.Icon,
                        createdAt = created.CreatedAt,
                        createdBy = created.CreatedBy
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"❌ [StationController] CreateAjax() - Error de operación: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"❌ [StationController] CreateAjax() - Error de argumento: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [StationController] CreateAjax() - Error interno: {ex.Message}");
                Console.WriteLine($"🔍 [StationController] CreateAjax() - StackTrace: {ex.StackTrace}");
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
                
                // ✅ NUEVO: Obtener usuario actual para tracking
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var userNameClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Name) ?? 
                                   User.FindFirst(System.Security.Claims.ClaimTypes.Email);
                
                if (userIdClaim != null)
                {
                    var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(Guid.Parse(userIdClaim.Value));
                    if (currentUser != null)
                    {
                        station.UpdatedBy = userNameClaim?.Value ?? currentUser.Email;
                    }
                }
                    
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
            try
            {
                Console.WriteLine("🔍 [StationController] EditAjax() - Iniciando actualización de estación...");
                
                if (id != station.Id)
                {
                    Console.WriteLine("⚠️ [StationController] EditAjax() - ID de estación no válido");
                    return Json(new { success = false, message = "ID de estación no válido" });
                }

                if (!ModelState.IsValid)
                {
                    Console.WriteLine("⚠️ [StationController] EditAjax() - Datos inválidos");
                    return Json(new { success = false, message = "Datos inválidos" });
                }

                // Obtener el usuario actual para auditoría
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var userNameClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Name);
                
                if (userIdClaim == null)
                {
                    Console.WriteLine("❌ [StationController] EditAjax() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(Guid.Parse(userIdClaim.Value));
                if (currentUser == null || currentUser.Branch == null)
                {
                    Console.WriteLine("❌ [StationController] EditAjax() - Usuario o sucursal no encontrado");
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }

                // Convertir string vacío a null para Icon
                if (string.IsNullOrWhiteSpace(station.Icon))
                    station.Icon = null;

                // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService
                station.UpdatedBy = userNameClaim?.Value ?? currentUser.Email;
                
                // Mantener CompanyId y BranchId del usuario actual
                station.CompanyId = currentUser.Branch.CompanyId;
                station.BranchId = currentUser.BranchId;

                Console.WriteLine($"✅ [StationController] EditAjax() - Usuario: {currentUser.Email}");
                Console.WriteLine($"🏢 [StationController] EditAjax() - Compañía: {currentUser.Branch.CompanyId}");
                Console.WriteLine($"🏪 [StationController] EditAjax() - Sucursal: {currentUser.BranchId}");
                Console.WriteLine($"📝 [StationController] EditAjax() - Estación: {station.Name}");
                Console.WriteLine($"👤 [StationController] EditAjax() - Actualizado por: {station.UpdatedBy}");
                Console.WriteLine($"🕒 [StationController] EditAjax() - Actualizado en: {station.UpdatedAt}");

                var updated = await _stationService.UpdateStationAsync(id, station);
                
                Console.WriteLine($"✅ [StationController] EditAjax() - Estación actualizada exitosamente: {updated.Id}");

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
                        icon = updated.Icon,
                        updatedAt = updated.UpdatedAt,
                        updatedBy = updated.UpdatedBy
                    }
                });
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("❌ [StationController] EditAjax() - Estación no encontrada");
                return Json(new { success = false, message = "Estación no encontrada" });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"❌ [StationController] EditAjax() - Error de operación: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"❌ [StationController] EditAjax() - Error de argumento: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [StationController] EditAjax() - Error interno: {ex.Message}");
                Console.WriteLine($"🔍 [StationController] EditAjax() - StackTrace: {ex.StackTrace}");
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
            try
            {
                Console.WriteLine("🔍 [StationController] GetStations() - Iniciando...");
                
                var stations = await _stationService.GetAllStationsAsync();
                var data = stations.Select(s => new { 
                    id = s.Id, 
                    name = s.Name, 
                    type = s.Type,
                    areaId = s.AreaId,
                    areaName = s.Area?.Name,
                    isActive = s.IsActive,
                    icon = s.Icon,
                    productCount = s.StockAssignments?.Count(sa => sa.IsActive) ?? 0
                }).ToList();
                
                Console.WriteLine($"✅ [StationController] GetStations() - Total estaciones: {data.Count}");
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [StationController] GetStations() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [StationController] GetStations() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = ex.Message });
            }
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
                productCount = station.StockAssignments.Count(sa => sa.IsActive)
            };

            return Json(new { success = true, data });
        }

        [HttpGet]
        [Route("Station/GetAreas")]
        public async Task<IActionResult> GetAreas()
        {
            try
            {
                Console.WriteLine("🔍 [StationController] GetAreas() - Iniciando carga de áreas...");
                
                // Obtener el usuario actual con sus asignaciones
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    Console.WriteLine("❌ [StationController] GetAreas() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }
                
                var userId = Guid.Parse(userIdClaim.Value);
                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(userId);
                
                if (currentUser == null || currentUser.Branch == null)
                {
                    Console.WriteLine("❌ [StationController] GetAreas() - Usuario o sucursal no encontrado");
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }
                
                Console.WriteLine($"✅ [StationController] GetAreas() - Usuario actual: {currentUser.Email}");
                Console.WriteLine($"🏢 [StationController] GetAreas() - Compañía: {currentUser.Branch?.Company?.Name}");
                Console.WriteLine($"🏪 [StationController] GetAreas() - Sucursal: {currentUser.Branch?.Name}");
                
                // Obtener áreas de la sucursal del usuario actual
                var areas = await _areaService.GetAllAsync();
                var filteredAreas = areas.Where(a => a.BranchId == currentUser.BranchId).ToList();
                
                Console.WriteLine($"📊 [StationController] GetAreas() - Áreas encontradas: {filteredAreas.Count}");
                
                var data = filteredAreas.Select(a => new {
                    id = a.Id,
                    name = a.Name,
                    description = a.Description
                }).ToList();
                
                Console.WriteLine($"📤 [StationController] GetAreas() - Enviando {data.Count} áreas");
                Console.WriteLine($"📤 [StationController] GetAreas() - Datos: {System.Text.Json.JsonSerializer.Serialize(data)}");
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [StationController] GetAreas() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [StationController] GetAreas() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error al cargar áreas: {ex.Message}" });
            }
        }

        // Métodos auxiliares
        private async Task PopulateAreasDropdown(Guid? selectedAreaId = null)
        {
            var areas = await _areaService.GetAllAsync();
            ViewBag.AreaId = new SelectList(areas, "Id", "Name", selectedAreaId);
        }
    }
} 