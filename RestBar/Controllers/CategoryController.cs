using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestBar.Models;
using RestBar.Services;
using System;
using System.Threading.Tasks;
using System.Linq;
using RestBar.Interfaces;

namespace RestBar.Controllers
{
    [Authorize(Policy = "SystemConfig")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IAreaService _areaService;

        public CategoryController(ICategoryService categoryService, IProductService productService, IAreaService areaService)
        {
            _categoryService = categoryService;
            _productService = productService;
            _areaService = areaService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                Console.WriteLine("🔍 [CategoryController] Index() - Iniciando carga de categorías...");
                
                // Obtener el usuario actual para filtrar por multi-tenant
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    Console.WriteLine("❌ [CategoryController] Index() - Usuario no autenticado");
                    return RedirectToAction("Login", "Account");
                }

                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(Guid.Parse(userIdClaim.Value));
                if (currentUser == null || currentUser.Branch == null)
                {
                    Console.WriteLine("❌ [CategoryController] Index() - Usuario o sucursal no encontrado");
                    return RedirectToAction("Login", "Account");
                }

                var allCategories = await _categoryService.GetAllCategoriesAsync();
                
                // Filtrar categorías: mostrar las de la sucursal actual O las que no tienen BranchId asignado (legacy)
                var filteredCategories = allCategories.Where(c => 
                    c.BranchId == currentUser.BranchId || 
                    c.BranchId == null
                ).ToList();
                
                Console.WriteLine($"✅ [CategoryController] Index() - Usuario: {currentUser.Email}");
                Console.WriteLine($"🏢 [CategoryController] Index() - Compañía: {currentUser.Branch.CompanyId}");
                Console.WriteLine($"🏪 [CategoryController] Index() - Sucursal: {currentUser.BranchId}");
                Console.WriteLine($"📊 [CategoryController] Index() - Total categorías: {allCategories.Count()}");
                Console.WriteLine($"📊 [CategoryController] Index() - Categorías filtradas: {filteredCategories.Count}");

                return View(filteredCategories);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [CategoryController] Index() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [CategoryController] Index() - StackTrace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "Error al cargar las categorías";
                return View(new List<Category>());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                await _categoryService.CreateCategoryAsync(category);
                TempData["SuccessMessage"] = "Categoría creada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _categoryService.UpdateCategoryAsync(id, category);
                    TempData["SuccessMessage"] = "Categoría actualizada exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var exist = await _productService.GetByCategoryIdAsync(id);

            if (exist != null && exist.Any())
            {
                TempData["SuccessMessage"] = "La categoría está asignada a uno o más productos y no puede ser eliminada.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _categoryService.DeleteCategoryAsync(id);
            if (!result)
            {
                return NotFound();
            }
            TempData["SuccessMessage"] = "Categoría eliminada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromForm] Category category)
        {
            try
            {
                Console.WriteLine("🔍 [CategoryController] CreateAjax() - Iniciando creación de categoría...");
                
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("⚠️ [CategoryController] CreateAjax() - Datos inválidos");
                    return Json(new { success = false, message = "Datos inválidos" });
                }

                // Obtener el usuario actual para auditoría y multi-tenant
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var userNameClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Name);
                
                if (userIdClaim == null)
                {
                    Console.WriteLine("❌ [CategoryController] CreateAjax() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(Guid.Parse(userIdClaim.Value));
                if (currentUser == null || currentUser.Branch == null)
                {
                    Console.WriteLine("❌ [CategoryController] CreateAjax() - Usuario o sucursal no encontrado");
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }

                // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService
                category.CreatedBy = userNameClaim?.Value ?? currentUser.Email;
                category.UpdatedBy = userNameClaim?.Value ?? currentUser.Email;
                
                // Asignar CompanyId y BranchId del usuario actual
                category.CompanyId = currentUser.Branch.CompanyId;
                category.BranchId = currentUser.BranchId;

                Console.WriteLine($"✅ [CategoryController] CreateAjax() - Usuario: {currentUser.Email}");
                Console.WriteLine($"🏢 [CategoryController] CreateAjax() - Compañía: {currentUser.Branch.CompanyId}");
                Console.WriteLine($"🏪 [CategoryController] CreateAjax() - Sucursal: {currentUser.BranchId}");
                Console.WriteLine($"📝 [CategoryController] CreateAjax() - Categoría: {category.Name}");
                Console.WriteLine($"👤 [CategoryController] CreateAjax() - Creado por: {category.CreatedBy}");
                Console.WriteLine($"🕒 [CategoryController] CreateAjax() - Creado en: {category.CreatedAt}");

                var created = await _categoryService.CreateCategoryAsync(category);
                
                Console.WriteLine($"✅ [CategoryController] CreateAjax() - Categoría creada exitosamente: {created.Id}");

                return Json(new { 
                    success = true, 
                    data = new { 
                        id = created.Id, 
                        name = created.Name,
                        createdAt = created.CreatedAt,
                        createdBy = created.CreatedBy
                    } 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [CategoryController] CreateAjax() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [CategoryController] CreateAjax() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = "Error al crear la categoría" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                Console.WriteLine("🔍 [CategoryController] GetCategories() - Iniciando carga de categorías...");
                
                // Obtener el usuario actual para filtrar por multi-tenant
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    Console.WriteLine("❌ [CategoryController] GetCategories() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(Guid.Parse(userIdClaim.Value));
                if (currentUser == null || currentUser.Branch == null)
                {
                    Console.WriteLine("❌ [CategoryController] GetCategories() - Usuario o sucursal no encontrado");
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }

                var allCategories = await _categoryService.GetActiveCategoriesAsync();
                
                // Filtrar categorías por la sucursal del usuario actual
                var filteredCategories = allCategories.Where(c => c.BranchId == currentUser.BranchId).ToList();
                
                Console.WriteLine($"✅ [CategoryController] GetCategories() - Usuario: {currentUser.Email}");
                Console.WriteLine($"🏢 [CategoryController] GetCategories() - Compañía: {currentUser.Branch.CompanyId}");
                Console.WriteLine($"🏪 [CategoryController] GetCategories() - Sucursal: {currentUser.BranchId}");
                Console.WriteLine($"📊 [CategoryController] GetCategories() - Categorías encontradas: {filteredCategories.Count}");

                var data = filteredCategories.Select(c => new { id = c.Id, name = c.Name }).ToList();
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [CategoryController] GetCategories() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [CategoryController] GetCategories() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = "Error al cargar categorías" });
            }
        }

        [HttpPost]
        [Route("Category/EditAjax/{id}")]
        public async Task<IActionResult> EditAjax(Guid id, [FromForm] Category category)
        {
            try
            {
                Console.WriteLine("🔍 [CategoryController] EditAjax() - Iniciando actualización de categoría...");
                
                if (id != category.Id)
                {
                    Console.WriteLine("⚠️ [CategoryController] EditAjax() - ID de categoría no válido");
                    return Json(new { success = false, message = "ID de categoría no válido" });
                }

                if (!ModelState.IsValid)
                {
                    Console.WriteLine("⚠️ [CategoryController] EditAjax() - Datos inválidos");
                    return Json(new { success = false, message = "Datos inválidos" });
                }

                // Obtener el usuario actual para auditoría
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var userNameClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Name);
                
                if (userIdClaim == null)
                {
                    Console.WriteLine("❌ [CategoryController] EditAjax() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(Guid.Parse(userIdClaim.Value));
                if (currentUser == null || currentUser.Branch == null)
                {
                    Console.WriteLine("❌ [CategoryController] EditAjax() - Usuario o sucursal no encontrado");
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }

                // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService
                category.UpdatedBy = userNameClaim?.Value ?? currentUser.Email;
                
                // Mantener CompanyId y BranchId del usuario actual
                category.CompanyId = currentUser.Branch.CompanyId;
                category.BranchId = currentUser.BranchId;

                Console.WriteLine($"✅ [CategoryController] EditAjax() - Usuario: {currentUser.Email}");
                Console.WriteLine($"🏢 [CategoryController] EditAjax() - Compañía: {currentUser.Branch.CompanyId}");
                Console.WriteLine($"🏪 [CategoryController] EditAjax() - Sucursal: {currentUser.BranchId}");
                Console.WriteLine($"📝 [CategoryController] EditAjax() - Categoría: {category.Name}");
                Console.WriteLine($"👤 [CategoryController] EditAjax() - Actualizado por: {category.UpdatedBy}");
                Console.WriteLine($"🕒 [CategoryController] EditAjax() - Actualizado en: {category.UpdatedAt}");

                var updated = await _categoryService.UpdateCategoryAsync(id, category);
                
                Console.WriteLine($"✅ [CategoryController] EditAjax() - Categoría actualizada exitosamente: {updated.Id}");

                return Json(new { 
                    success = true, 
                    message = "Categoría actualizada correctamente",
                    data = new { 
                        id = updated.Id, 
                        name = updated.Name,
                        updatedAt = updated.UpdatedAt,
                        updatedBy = updated.UpdatedBy
                    }
                });
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("❌ [CategoryController] EditAjax() - Categoría no encontrada");
                return Json(new { success = false, message = "Categoría no encontrada" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [CategoryController] EditAjax() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [CategoryController] EditAjax() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = "Error interno del servidor" });
            }
        }
    }
} 