using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestBar.Interfaces;
using RestBar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestBar.Controllers
{
    [Authorize(Policy = "ProductAccess")]
    public class ProductStockAssignmentController : Controller
    {
        private readonly IProductStockAssignmentService _assignmentService;
        private readonly IProductService _productService;
        private readonly IStationService _stationService;
        private readonly IAreaService _areaService;

        public ProductStockAssignmentController(
            IProductStockAssignmentService assignmentService,
            IProductService productService,
            IStationService stationService,
            IAreaService areaService)
        {
            _assignmentService = assignmentService;
            _productService = productService;
            _stationService = stationService;
            _areaService = areaService;
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
                return Json(new { success = true, data = dataArray });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductStockAssignmentController] GetAssignments() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [ProductStockAssignmentController] GetAssignments() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: ProductStockAssignment/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductStockAssignment assignment)
        {
            try
            {
                Console.WriteLine($"🔍 [ProductStockAssignmentController] Create() - ProductId: {assignment.ProductId}, StationId: {assignment.StationId}, Stock: {assignment.Stock}");
                
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("⚠️ [ProductStockAssignmentController] Create() - ModelState inválido");
                    return Json(new { success = false, message = "Datos inválidos" });
                }

                var created = await _assignmentService.CreateAsync(assignment);
                
                Console.WriteLine($"✅ [ProductStockAssignmentController] Create() - Asignación creada exitosamente: {created.Id}");
                return Json(new { 
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
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"⚠️ [ProductStockAssignmentController] Create() - Error de validación: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductStockAssignmentController] Create() - Error inesperado: {ex.Message}");
                Console.WriteLine($"🔍 [ProductStockAssignmentController] Create() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = "Error interno al crear la asignación" });
            }
        }

        // PUT: ProductStockAssignment/Update
        [HttpPut]
        public async Task<IActionResult> Update(Guid id, [FromBody] ProductStockAssignment assignment)
        {
            try
            {
                Console.WriteLine($"🔍 [ProductStockAssignmentController] Update() - Id: {id}");
                
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("⚠️ [ProductStockAssignmentController] Update() - ModelState inválido");
                    return Json(new { success = false, message = "Datos inválidos" });
                }

                var updated = await _assignmentService.UpdateAsync(id, assignment);
                
                Console.WriteLine($"✅ [ProductStockAssignmentController] Update() - Asignación actualizada exitosamente: {updated.Id}");
                return Json(new { 
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
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"⚠️ [ProductStockAssignmentController] Update() - Asignación no encontrada: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductStockAssignmentController] Update() - Error inesperado: {ex.Message}");
                Console.WriteLine($"🔍 [ProductStockAssignmentController] Update() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = "Error interno al actualizar la asignación" });
            }
        }

        // DELETE: ProductStockAssignment/Delete
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                Console.WriteLine($"🔍 [ProductStockAssignmentController] Delete() - Id: {id}");
                
                var deleted = await _assignmentService.DeleteAsync(id);
                
                if (deleted)
                {
                    Console.WriteLine($"✅ [ProductStockAssignmentController] Delete() - Asignación eliminada exitosamente: {id}");
                    return Json(new { success = true, message = "Asignación eliminada exitosamente" });
                }
                else
                {
                    Console.WriteLine($"⚠️ [ProductStockAssignmentController] Delete() - Asignación no encontrada: {id}");
                    return Json(new { success = false, message = "Asignación no encontrada" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductStockAssignmentController] Delete() - Error inesperado: {ex.Message}");
                Console.WriteLine($"🔍 [ProductStockAssignmentController] Delete() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = "Error interno al eliminar la asignación" });
            }
        }
    }
}

