using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestBar.Models;
using RestBar.Services;
using System;
using System.Threading.Tasks;
using System.Linq;
using RestBar.Interfaces;
using System.Text.Json.Serialization;

namespace RestBar.Controllers
{
    [Authorize(Policy = "SystemConfig")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IBranchService _branchService;
        private readonly IAuthService _authService;

        public CategoryController(ICategoryService categoryService, IProductService productService, IBranchService branchService, IAuthService authService)
        {
            _categoryService = categoryService;
            _productService = productService;
            _branchService = branchService;
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,CompanyId,BranchId,IsActive")] Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _authService.GetCurrentUserAsync(User);
                    if (currentUser?.Branch?.CompanyId == null)
                    {
                        TempData["ErrorMessage"] = "No se pudo determinar la compañía del usuario";
                        return RedirectToAction(nameof(Index));
                    }

                    category.CompanyId = currentUser.Branch.CompanyId.Value;
                    category.CreatedAt = DateTime.UtcNow;
                    
                    await _categoryService.CreateCategoryAsync(category);
                    TempData["SuccessMessage"] = "Categoría creada exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error al crear la categoría";
                    return RedirectToAction(nameof(Index));
                }
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
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Description,CompanyId,BranchId,IsActive")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _authService.GetCurrentUserAsync(User);
                    if (currentUser?.Branch?.CompanyId == null)
                    {
                        TempData["ErrorMessage"] = "No se pudo determinar la compañía del usuario";
                        return RedirectToAction(nameof(Index));
                    }

                    category.CompanyId = currentUser.Branch.CompanyId.Value;
                    category.UpdatedAt = DateTime.UtcNow;
                    category.UpdatedBy = currentUser.Id;
                    
                    await _categoryService.UpdateCategoryAsync(id, category);
                    TempData["SuccessMessage"] = "Categoría actualizada exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error al actualizar la categoría";
                    return RedirectToAction(nameof(Index));
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
            Console.WriteLine("=== INICIO CreateAjax ===");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState no es válido");
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine($"Errores de validación: {string.Join(", ", errors)}");
                return Json(new { success = false, message = "Datos inválidos", errors = errors });
            }

            try
            {
                Console.WriteLine("🔍 Obteniendo usuario actual...");
                var currentUser = await _authService.GetCurrentUserAsync(User);
                Console.WriteLine($"Usuario obtenido: {currentUser?.Email ?? "NULL"}");
                Console.WriteLine($"Branch: {currentUser?.Branch?.Name ?? "NULL"}");
                Console.WriteLine($"CompanyId: {currentUser?.Branch?.CompanyId?.ToString() ?? "NULL"}");
                
                if (currentUser?.Branch?.CompanyId == null)
                {
                    Console.WriteLine("❌ No se pudo determinar la compañía del usuario");
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                Console.WriteLine("📝 Asignando datos a la categoría...");
                Console.WriteLine($"Categoría recibida - Name: {category.Name}, Description: {category.Description}, BranchId: {category.BranchId}");
                
                category.CompanyId = currentUser.Branch.CompanyId.Value;
                category.CreatedAt = DateTime.UtcNow;
                category.CreatedBy = currentUser.Id;
                
                Console.WriteLine($"Categoría procesada - CompanyId: {category.CompanyId}, CreatedAt: {category.CreatedAt}");
                
                Console.WriteLine("💾 Guardando categoría en la base de datos...");
                var created = await _categoryService.CreateCategoryAsync(category);
                Console.WriteLine($"✅ Categoría creada exitosamente - ID: {created.Id}, Name: {created.Name}");
                
                return Json(new { success = true, data = new { id = created.Id, name = created.Name } });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al crear la categoría: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = "Error al crear la categoría", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            var data = categories.Select(c => new { id = c.Id, name = c.Name }).ToList();
            return Json(new { success = true, data });
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

        [HttpPost]
        public async Task<IActionResult> EditAjax(Guid id, [FromForm] Category category)
        {
            Console.WriteLine("=== INICIO EditAjax ===");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            Console.WriteLine($"Category ID: {id}");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState no es válido");
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine($"Errores de validación: {string.Join(", ", errors)}");
                return Json(new { success = false, message = "Datos inválidos", errors = errors });
            }

            try
            {
                Console.WriteLine("🔍 Obteniendo usuario actual...");
                var currentUser = await _authService.GetCurrentUserAsync(User);
                Console.WriteLine($"Usuario obtenido: {currentUser?.Email ?? "NULL"}");
                Console.WriteLine($"Branch: {currentUser?.Branch?.Name ?? "NULL"}");
                Console.WriteLine($"CompanyId: {currentUser?.Branch?.CompanyId?.ToString() ?? "NULL"}");
                
                if (currentUser?.Branch?.CompanyId == null)
                {
                    Console.WriteLine("❌ No se pudo determinar la compañía del usuario");
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                Console.WriteLine("📝 Asignando datos a la categoría...");
                Console.WriteLine($"Categoría recibida - Name: {category.Name}, Description: {category.Description}, BranchId: {category.BranchId}");
                
                category.CompanyId = currentUser.Branch.CompanyId.Value;
                category.UpdatedAt = DateTime.UtcNow;
                category.UpdatedBy = currentUser.Id;
                
                Console.WriteLine($"Categoría procesada - CompanyId: {category.CompanyId}, UpdatedAt: {category.UpdatedAt}, UpdatedBy: {category.UpdatedBy}");
                
                Console.WriteLine("💾 Actualizando categoría en la base de datos...");
                var updated = await _categoryService.UpdateCategoryAsync(id, category);
                Console.WriteLine($"✅ Categoría actualizada exitosamente - ID: {updated.Id}, Name: {updated.Name}");
                
                return Json(new { success = true, data = new { id = updated.Id, name = updated.Name } });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al actualizar la categoría: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"❌ Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner StackTrace: {ex.InnerException.StackTrace}");
                }
                
                return Json(new { success = false, message = "Error al actualizar la categoría", error = ex.Message });
            }
        }
    }
} 