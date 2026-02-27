using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RestBar.Helpers;
using RestBar.Interfaces;
using RestBar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestBar.Controllers
{
    /// <summary>
    /// Controlador para gestionar asignaciones de stock de productos a estaciones
    /// </summary>
    [Authorize(Policy = "ProductAccess")]
    public class ProductStockAssignmentController : Controller
    {
        private readonly IProductStockAssignmentService _assignmentService;
        private readonly IProductService _productService;
        private readonly IStationService _stationService;
        private readonly IAreaService _areaService;
        private readonly ILogger<ProductStockAssignmentController>? _logger;

        public ProductStockAssignmentController(
            IProductStockAssignmentService assignmentService,
            IProductService productService,
            IStationService stationService,
            IAreaService areaService,
            ILogger<ProductStockAssignmentController>? logger = null)
        {
            _assignmentService = assignmentService;
            _productService = productService;
            _stationService = stationService;
            _areaService = areaService;
            _logger = logger;
        }

        // GET: ProductStockAssignment
        public async Task<IActionResult> Index()
        {
            try
            {
                Console.WriteLine("🔍 [ProductStockAssignmentController] Index() - Iniciando carga de asignaciones...");
                
                // Obtener el usuario actual para filtrar por multi-tenant
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    Console.WriteLine("❌ [ProductStockAssignmentController] Index() - Usuario no autenticado");
                    return RedirectToAction("Login", "Account");
                }

                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(Guid.Parse(userIdClaim.Value));
                if (currentUser == null || currentUser.Branch == null)
                {
                    Console.WriteLine("❌ [ProductStockAssignmentController] Index() - Usuario o sucursal no encontrado");
                    return RedirectToAction("Login", "Account");
                }

                var assignments = await _assignmentService.GetAllAsync(currentUser.BranchId);
                
                Console.WriteLine($"✅ [ProductStockAssignmentController] Index() - Total asignaciones: {assignments.Count()}");
                return View(assignments);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductStockAssignmentController] Index() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [ProductStockAssignmentController] Index() - StackTrace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "Error al cargar las asignaciones de stock";
                return View(new List<ProductStockAssignment>());
            }
        }

        // GET: ProductStockAssignment/GetAssignments
        [HttpGet]
        public async Task<IActionResult> GetAssignments(Guid? productId = null, Guid? stationId = null)
        {
            try
            {
                Console.WriteLine($"🔍 [ProductStockAssignmentController] GetAssignments() - ProductId: {productId}, StationId: {stationId}");
                
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(Guid.Parse(userIdClaim.Value));
                if (currentUser?.Branch == null)
                {
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }

                IEnumerable<ProductStockAssignment> assignments;

                if (productId.HasValue)
                {
                    assignments = await _assignmentService.GetByProductIdAsync(productId.Value, currentUser.BranchId);
                }
                else if (stationId.HasValue)
                {
                    assignments = await _assignmentService.GetByStationIdAsync(stationId.Value, currentUser.BranchId);
                }
                else
                {
                    assignments = await _assignmentService.GetAllAsync(currentUser.BranchId);
                }

                var dataArray = assignments.Select(a => new
                {
                    id = a.Id,
                    productId = a.ProductId,
                    productName = a.Product?.Name ?? "N/A",
                    stationId = a.StationId,
                    stationName = a.Station?.Name ?? "N/A",
                    stock = a.Stock,
                    minStock = a.MinStock,
                    priority = a.Priority,
                    isActive = a.IsActive
                }).ToList();

                Console.WriteLine($"✅ [ProductStockAssignmentController] GetAssignments() - Total asignaciones: {dataArray.Count}");
                if (dataArray.Count == 0)
                {
                    Console.WriteLine("⚠️ [ProductStockAssignmentController] GetAssignments() - No se encontraron asignaciones para los filtros solicitados");
                }
                return Json(new { success = true, data = dataArray });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductStockAssignmentController] GetAssignments() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [ProductStockAssignmentController] GetAssignments() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Crea una nueva asignación de stock
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductStockAssignment assignment)
        {
            try
            {
                if (assignment == null)
                {
                    LoggingHelper.LogWarning(_logger, nameof(ProductStockAssignmentController), nameof(Create), 
                        "Asignación nula recibida");
                    return BadRequest(new { success = false, message = "Los datos de la asignación son requeridos" });
                }

                LoggingHelper.LogParams(_logger, nameof(ProductStockAssignmentController), nameof(Create), 
                    $"ProductId: {assignment.ProductId}, StationId: {assignment.StationId}, Stock: {assignment.Stock}");
                
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    LoggingHelper.LogWarning(_logger, nameof(ProductStockAssignmentController), nameof(Create), 
                        $"ModelState inválido: {string.Join(", ", errors)}");
                    return BadRequest(new { success = false, message = "Datos inválidos", errors });
                }

                var created = await _assignmentService.CreateAsync(assignment);
                
                LoggingHelper.LogSuccess(_logger, nameof(ProductStockAssignmentController), nameof(Create), 
                    $"Asignación creada exitosamente: {created.Id}");
                
                return CreatedAtAction(nameof(GetAssignments), new { productId = created.ProductId }, new { 
                    success = true, 
                    data = new 
                    { 
                        id = created.Id,
                        productId = created.ProductId,
                        stationId = created.StationId,
                        stock = created.Stock,
                        minStock = created.MinStock,
                        priority = created.Priority,
                        isActive = created.IsActive
                    } 
                });
            }
            catch (ArgumentException ex)
            {
                LoggingHelper.LogWarning(_logger, nameof(ProductStockAssignmentController), nameof(Create), 
                    $"Error de validación: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                LoggingHelper.LogWarning(_logger, nameof(ProductStockAssignmentController), nameof(Create), 
                    $"Error de validación: {ex.Message}");
                return Conflict(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                LoggingHelper.LogError(_logger, nameof(ProductStockAssignmentController), nameof(Create), ex, 
                    "Error inesperado al crear asignación");
                return StatusCode(500, new { success = false, message = "Error interno al crear la asignación" });
            }
        }

        /// <summary>
        /// Actualiza una asignación existente
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> Update(Guid id, [FromBody] ProductStockAssignment assignment)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest(new { success = false, message = "El ID es requerido" });
                }

                if (assignment == null)
                {
                    LoggingHelper.LogWarning(_logger, nameof(ProductStockAssignmentController), nameof(Update), 
                        "Asignación nula recibida");
                    return BadRequest(new { success = false, message = "Los datos de la asignación son requeridos" });
                }

                LoggingHelper.LogInfo(_logger, nameof(ProductStockAssignmentController), nameof(Update), $"Id: {id}");
                
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    LoggingHelper.LogWarning(_logger, nameof(ProductStockAssignmentController), nameof(Update), 
                        $"ModelState inválido: {string.Join(", ", errors)}");
                    return BadRequest(new { success = false, message = "Datos inválidos", errors });
                }

                var updated = await _assignmentService.UpdateAsync(id, assignment);
                
                LoggingHelper.LogSuccess(_logger, nameof(ProductStockAssignmentController), nameof(Update), 
                    $"Asignación actualizada exitosamente: {updated.Id}");
                
                return Ok(new { 
                    success = true, 
                    data = new 
                    { 
                        id = updated.Id,
                        productId = updated.ProductId,
                        stationId = updated.StationId,
                        stock = updated.Stock,
                        minStock = updated.MinStock,
                        priority = updated.Priority,
                        isActive = updated.IsActive
                    } 
                });
            }
            catch (ArgumentException ex)
            {
                LoggingHelper.LogWarning(_logger, nameof(ProductStockAssignmentController), nameof(Update), 
                    $"Error de validación: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                LoggingHelper.LogWarning(_logger, nameof(ProductStockAssignmentController), nameof(Update), 
                    $"Asignación no encontrada: {ex.Message}");
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                LoggingHelper.LogError(_logger, nameof(ProductStockAssignmentController), nameof(Update), ex, 
                    "Error inesperado al actualizar asignación");
                return StatusCode(500, new { success = false, message = "Error interno al actualizar la asignación" });
            }
        }

        /// <summary>
        /// Elimina una asignación por su ID
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest(new { success = false, message = "El ID es requerido" });
                }

                LoggingHelper.LogInfo(_logger, nameof(ProductStockAssignmentController), nameof(Delete), $"Id: {id}");
                
                var deleted = await _assignmentService.DeleteAsync(id);
                
                if (deleted)
                {
                    LoggingHelper.LogSuccess(_logger, nameof(ProductStockAssignmentController), nameof(Delete), 
                        $"Asignación eliminada exitosamente: {id}");
                    return Ok(new { success = true, message = "Asignación eliminada exitosamente" });
                }
                else
                {
                    LoggingHelper.LogWarning(_logger, nameof(ProductStockAssignmentController), nameof(Delete), 
                        $"Asignación no encontrada: {id}");
                    return NotFound(new { success = false, message = "Asignación no encontrada" });
                }
            }
            catch (ArgumentException ex)
            {
                LoggingHelper.LogWarning(_logger, nameof(ProductStockAssignmentController), nameof(Delete), 
                    $"Error de validación: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                LoggingHelper.LogError(_logger, nameof(ProductStockAssignmentController), nameof(Delete), ex, 
                    "Error inesperado al eliminar asignación");
                return StatusCode(500, new { success = false, message = "Error interno al eliminar la asignación" });
            }
        }
    }
}

