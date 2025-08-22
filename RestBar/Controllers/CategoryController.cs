using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestBar.Models;
using RestBar.Services;
using System;
using System.Threading.Tasks;
using System.Linq;
using RestBar.Interfaces;
using System.Security.Claims;

namespace RestBar.Controllers
{
    [Authorize(Policy = "SystemConfig")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IAreaService _areaService;
        private readonly ICompanyService _companyService;
        private readonly IBranchService _branchService;

        public CategoryController(ICategoryService categoryService, IProductService productService, 
            IAreaService areaService, ICompanyService companyService, IBranchService branchService)
        {
            _categoryService = categoryService;
            _productService = productService;
            _areaService = areaService;
            _companyService = companyService;
            _branchService = branchService;
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
        public async Task<IActionResult> Create(Category category)
        {
            try
            {
                Console.WriteLine("🔍 [CategoryController] Create() - Iniciando creación de categoría...");
                
                if (ModelState.IsValid)
                {
                    // Obtener usuario actual y asignar CompanyId y BranchId
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
                    {
                        var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(userId);
                        if (currentUser != null)
                        {
                            category.CompanyId = currentUser.CompanyId;
                            category.BranchId = currentUser.BranchId;
                            Console.WriteLine($"✅ [CategoryController] Create() - Asignado CompanyId: {category.CompanyId}, BranchId: {category.BranchId}");
                        }
                    }

                    await _categoryService.CreateCategoryAsync(category);
                    Console.WriteLine($"✅ [CategoryController] Create() - Categoría creada exitosamente: {category.Name}");
                    TempData["SuccessMessage"] = "Categoría creada exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    Console.WriteLine($"❌ [CategoryController] Create() - ModelState inválido: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [CategoryController] Create() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [CategoryController] Create() - StackTrace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "Error al crear la categoría.";
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
                Console.WriteLine("🔍 [CategoryController] CreateAjax() - Iniciando creación de categoría via AJAX...");
                
                if (!ModelState.IsValid)
                {
                    Console.WriteLine($"❌ [CategoryController] CreateAjax() - ModelState inválido: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
                    return Json(new { success = false, message = "Datos inválidos" });
                }

                // Obtener usuario actual y asignar CompanyId y BranchId
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(userId);
                    if (currentUser != null)
                    {
                        category.CompanyId = currentUser.CompanyId;
                        category.BranchId = currentUser.BranchId;
                        Console.WriteLine($"✅ [CategoryController] CreateAjax() - Asignado CompanyId: {category.CompanyId}, BranchId: {category.BranchId}");
                    }
                }

                var created = await _categoryService.CreateCategoryAsync(category);
                Console.WriteLine($"✅ [CategoryController] CreateAjax() - Categoría creada exitosamente: {created.Name}");
                return Json(new { success = true, data = new { id = created.Id, name = created.Name } });
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
                Console.WriteLine("🔍 [CategoryController] GetCategories() - Iniciando carga de categorías del usuario actual...");
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    Console.WriteLine("⚠️ [CategoryController] GetCategories() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado", data = new List<object>() });
                }

                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(userId);
                if (currentUser == null)
                {
                    Console.WriteLine("⚠️ [CategoryController] GetCategories() - Usuario no encontrado");
                    return Json(new { success = false, message = "Usuario no encontrado", data = new List<object>() });
                }

                Console.WriteLine($"👤 [CategoryController] GetCategories() - Usuario: {currentUser.FullName ?? currentUser.Email}");
                Console.WriteLine($"🏢 [CategoryController] GetCategories() - CompanyId: {currentUser.CompanyId}");
                Console.WriteLine($"🏪 [CategoryController] GetCategories() - BranchId: {currentUser.BranchId}");

                var categories = await _categoryService.GetActiveCategoriesByCompanyAndBranchAsync(
                    currentUser.CompanyId.Value, currentUser.BranchId.Value);

                Console.WriteLine($"📊 [CategoryController] GetCategories() - Total categorías filtradas: {categories.Count()}");
                
                var data = categories.Select(c => new { id = c.Id, name = c.Name }).ToList();
                Console.WriteLine($"✅ [CategoryController] GetCategories() - Enviando {data.Count} categorías");
                
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [CategoryController] GetCategories() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [CategoryController] GetCategories() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error al cargar categorías: {ex.Message}", data = new List<object>() });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            try
            {
                Console.WriteLine("🔍 [CategoryController] GetCompanies() - Iniciando carga de compañía del usuario actual...");
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    Console.WriteLine("⚠️ [CategoryController] GetCompanies() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado", data = new List<object>() });
                }

                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(userId);
                if (currentUser == null)
                {
                    Console.WriteLine("⚠️ [CategoryController] GetCompanies() - Usuario no encontrado");
                    return Json(new { success = false, message = "Usuario no encontrado", data = new List<object>() });
                }

                if (!currentUser.CompanyId.HasValue)
                {
                    Console.WriteLine("⚠️ [CategoryController] GetCompanies() - Usuario no tiene compañía asignada");
                    return Json(new { success = false, message = "Usuario no tiene compañía asignada", data = new List<object>() });
                }

                var company = await _companyService.GetByIdAsync(currentUser.CompanyId.Value);
                if (company == null)
                {
                    Console.WriteLine("⚠️ [CategoryController] GetCompanies() - Compañía no encontrada");
                    return Json(new { success = false, message = "Compañía no encontrada", data = new List<object>() });
                }

                Console.WriteLine($"✅ [CategoryController] GetCompanies() - Compañía encontrada: {company.Name} (ID: {company.Id})");
                var data = new List<object> { new { id = company.Id, name = company.Name } };
                var response = new { success = true, data };
                Console.WriteLine($"📤 [CategoryController] GetCompanies() - Enviando respuesta: {System.Text.Json.JsonSerializer.Serialize(response)}");
                return Json(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [CategoryController] GetCompanies() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [CategoryController] GetCompanies() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error al cargar compañía: {ex.Message}", data = new List<object>() });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBranches()
        {
            try
            {
                Console.WriteLine("🔍 [CategoryController] GetBranches() - Iniciando carga de sucursal del usuario actual...");
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    Console.WriteLine("⚠️ [CategoryController] GetBranches() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado", data = new List<object>() });
                }

                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(userId);
                if (currentUser == null)
                {
                    Console.WriteLine("⚠️ [CategoryController] GetBranches() - Usuario no encontrado");
                    return Json(new { success = false, message = "Usuario no encontrado", data = new List<object>() });
                }

                if (!currentUser.BranchId.HasValue)
                {
                    Console.WriteLine("⚠️ [CategoryController] GetBranches() - Usuario no tiene sucursal asignada");
                    return Json(new { success = false, message = "Usuario no tiene sucursal asignada", data = new List<object>() });
                }

                var branch = await _branchService.GetByIdAsync(currentUser.BranchId.Value);
                if (branch == null)
                {
                    Console.WriteLine("⚠️ [CategoryController] GetBranches() - Sucursal no encontrada");
                    return Json(new { success = false, message = "Sucursal no encontrada", data = new List<object>() });
                }

                Console.WriteLine($"✅ [CategoryController] GetBranches() - Sucursal encontrada: {branch.Name} (ID: {branch.Id})");
                var data = new List<object> { new { id = branch.Id, name = branch.Name } };
                var response = new { success = true, data };
                Console.WriteLine($"📤 [CategoryController] GetBranches() - Enviando respuesta: {System.Text.Json.JsonSerializer.Serialize(response)}");
                return Json(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [CategoryController] GetBranches() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [CategoryController] GetBranches() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error al cargar sucursal: {ex.Message}", data = new List<object>() });
            }
        }
    }
} 