using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestBar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RestBar.ViewModel;
using RestBar.Interfaces;

namespace RestBar.Controllers
{
    [Authorize(Policy = "ProductAccess")]
    public class ProductController : Controller
    {
        private readonly RestBarContext _context;
        private readonly ILogger<ProductController> _logger;
        private readonly ICategoryService _categoryService;
        private readonly IStationService _stationService;
        private readonly IProductService _productService;
        private readonly IAreaService _areaService;

        public ProductController(RestBarContext context, ILogger<ProductController> logger, ICategoryService categoryService, IStationService stationService, IProductService productService, IAreaService areaService)
        {
            _context = context;
            _logger = logger;
            _categoryService = categoryService;
            _stationService = stationService;
            _productService = productService;
            _areaService = areaService;
        }

        // GET: Product
        public async Task<IActionResult> Index()
        {
            try
            {
                Console.WriteLine("🔍 [ProductController] Index() - Iniciando carga de productos...");
                
                // Obtener el usuario actual para filtrar por multi-tenant
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    Console.WriteLine("❌ [ProductController] Index() - Usuario no autenticado");
                    return RedirectToAction("Login", "Account");
                }

                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(Guid.Parse(userIdClaim.Value));
                if (currentUser == null || currentUser.Branch == null)
                {
                    Console.WriteLine("❌ [ProductController] Index() - Usuario o sucursal no encontrado");
                    return RedirectToAction("Login", "Account");
                }

                var allProducts = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Station)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
                
                // Filtrar productos por la sucursal del usuario actual
                var filteredProducts = allProducts.Where(p => 
                    p.BranchId == currentUser.BranchId || 
                    p.BranchId == null
                ).ToList();
                
                Console.WriteLine($"✅ [ProductController] Index() - Usuario: {currentUser.Email}");
                Console.WriteLine($"🏢 [ProductController] Index() - Compañía: {currentUser.Branch.CompanyId}");
                Console.WriteLine($"🏪 [ProductController] Index() - Sucursal: {currentUser.BranchId}");
                Console.WriteLine($"📊 [ProductController] Index() - Total productos: {allProducts.Count()}");
                Console.WriteLine($"📊 [ProductController] Index() - Productos filtrados: {filteredProducts.Count}");
                
                return View(filteredProducts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductController] Index() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [ProductController] Index() - StackTrace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "Error al cargar los productos";
                return View(new List<Product>());
            }
        }

        // GET: Product/GetProducts
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                Console.WriteLine("🔍 [ProductController] GetProducts() - Iniciando carga de productos...");
                
                // Obtener el usuario actual para filtrar por multi-tenant
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    Console.WriteLine("❌ [ProductController] GetProducts() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(Guid.Parse(userIdClaim.Value));
                if (currentUser == null || currentUser.Branch == null)
                {
                    Console.WriteLine("❌ [ProductController] GetProducts() - Usuario o sucursal no encontrado");
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }

                var allProducts = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Station)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
                
                // Filtrar productos por la sucursal del usuario actual
                var filteredProducts = allProducts.Where(p => 
                    p.BranchId == currentUser.BranchId || 
                    p.BranchId == null
                ).ToList();
                
                Console.WriteLine($"✅ [ProductController] GetProducts() - Usuario: {currentUser.Email}");
                Console.WriteLine($"🏢 [ProductController] GetProducts() - Compañía: {currentUser.Branch.CompanyId}");
                Console.WriteLine($"🏪 [ProductController] GetProducts() - Sucursal: {currentUser.BranchId}");
                Console.WriteLine($"📊 [ProductController] GetProducts() - Productos encontrados: {filteredProducts.Count}");

                return Json(new { success = true, data = filteredProducts });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductController] GetProducts() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [ProductController] GetProducts() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = "Error al obtener los productos" });
            }
        }

        // GET: Product/GetCategories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                Console.WriteLine("🔍 [ProductController] GetCategories() - Iniciando carga de categorías...");
                
                // Obtener el usuario actual para filtrar por multi-tenant
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    Console.WriteLine("❌ [ProductController] GetCategories() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(Guid.Parse(userIdClaim.Value));
                if (currentUser == null || currentUser.Branch == null)
                {
                    Console.WriteLine("❌ [ProductController] GetCategories() - Usuario o sucursal no encontrado");
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }

                var allCategories = await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                
                // Filtrar categorías por la sucursal del usuario actual
                var filteredCategories = allCategories.Where(c => 
                    c.BranchId == currentUser.BranchId || 
                    c.BranchId == null
                ).ToList();
                
                Console.WriteLine($"✅ [ProductController] GetCategories() - Usuario: {currentUser.Email}");
                Console.WriteLine($"🏢 [ProductController] GetCategories() - Compañía: {currentUser.Branch.CompanyId}");
                Console.WriteLine($"🏪 [ProductController] GetCategories() - Sucursal: {currentUser.BranchId}");
                Console.WriteLine($"📊 [ProductController] GetCategories() - Categorías encontradas: {filteredCategories.Count}");
                Console.WriteLine($"🔍 [ProductController] GetCategories() - Estructura de datos:");
                foreach (var cat in filteredCategories.Take(3))
                {
                    Console.WriteLine($"  - ID: {cat.Id}, Name: {cat.Name}, BranchId: {cat.BranchId}");
                }

                return Json(new { success = true, data = filteredCategories });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductController] GetCategories() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [ProductController] GetCategories() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = "Error al obtener las categorías" });
            }
        }

        // POST: Product/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Datos inválidos" });
                }

                // Obtener el usuario actual para auditoría y multi-tenant
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var userNameClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Name);
                
                if (userIdClaim == null)
                {
                    Console.WriteLine("❌ [ProductController] Create() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(Guid.Parse(userIdClaim.Value));
                if (currentUser == null || currentUser.Branch == null)
                {
                    Console.WriteLine("❌ [ProductController] Create() - Usuario o sucursal no encontrado");
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }

                var product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    Cost = model.Cost,
                    TaxRate = model.TaxRate,
                    Unit = model.Unit,
                    ImageUrl = model.ImageUrl,
                    IsActive = model.IsActive,
                    CategoryId = model.CategoryId,
                    StationId = model.StationId,
                    // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService
                    CreatedBy = userNameClaim?.Value ?? currentUser.Email,
                    UpdatedBy = userNameClaim?.Value ?? currentUser.Email,
                    CompanyId = currentUser.Branch.CompanyId,
                    BranchId = currentUser.BranchId
                };

                Console.WriteLine($"✅ [ProductController] Create() - Usuario: {currentUser.Email}");
                Console.WriteLine($"🏢 [ProductController] Create() - Compañía: {currentUser.Branch.CompanyId}");
                Console.WriteLine($"🏪 [ProductController] Create() - Sucursal: {currentUser.BranchId}");
                Console.WriteLine($"📝 [ProductController] Create() - Producto: {product.Name}");
                Console.WriteLine($"👤 [ProductController] Create() - Creado por: {product.CreatedBy}");
                Console.WriteLine($"🕒 [ProductController] Create() - Creado en: {product.CreatedAt}");

                var created = await _productService.CreateAsync(product);
                return Json(new { success = true, data = created });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear producto");
                return Json(new { success = false, message = "Error al crear el producto" });
            }
        }


        // PUT: Product/Edit/5
        [HttpPut]
        public async Task<IActionResult> Edit(Guid id, [FromBody] ProductEditViewModel model)
        {
            try
            {
                if (id != model.Id)
                {
                    return Json(new { success = false, message = "ID no coincide" });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return Json(new { 
                        success = false, 
                        message = "Datos inválidos", 
                        errors = errors 
                    });
                }

                var existingProduct = await _context.Products.FindAsync(id);
                if (existingProduct == null)
                {
                    return Json(new { success = false, message = "Producto no encontrado" });
                }

                // Obtener el usuario actual para auditoría y multi-tenant
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var userNameClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Name);
                
                if (userIdClaim == null)
                {
                    Console.WriteLine("❌ [ProductController] Edit() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(Guid.Parse(userIdClaim.Value));
                if (currentUser == null || currentUser.Branch == null)
                {
                    Console.WriteLine("❌ [ProductController] Edit() - Usuario o sucursal no encontrado");
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }

                // Actualizar las propiedades
                existingProduct.Name = model.Name;
                existingProduct.Description = model.Description;
                existingProduct.Price = model.Price;
                existingProduct.Cost = model.Cost;
                existingProduct.TaxRate = model.TaxRate;
                existingProduct.Unit = model.Unit;
                existingProduct.ImageUrl = model.ImageUrl;
                existingProduct.IsActive = model.IsActive;
                existingProduct.CategoryId = model.CategoryId;
                existingProduct.StationId = model.StationId;
                // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService
                existingProduct.UpdatedBy = userNameClaim?.Value ?? currentUser.Email;
                existingProduct.CompanyId = currentUser.Branch.CompanyId;
                existingProduct.BranchId = currentUser.BranchId;

                Console.WriteLine($"✅ [ProductController] Edit() - Usuario: {currentUser.Email}");
                Console.WriteLine($"🏢 [ProductController] Edit() - Compañía: {currentUser.Branch.CompanyId}");
                Console.WriteLine($"🏪 [ProductController] Edit() - Sucursal: {currentUser.BranchId}");
                Console.WriteLine($"📝 [ProductController] Edit() - Producto: {existingProduct.Name}");
                Console.WriteLine($"👤 [ProductController] Edit() - Actualizado por: {existingProduct.UpdatedBy}");

                // ✅ Usar el servicio para actualizar (aplica SetUpdatedTracking)
                await _productService.UpdateAsync(id, existingProduct);

                return Json(new { success = true, data = existingProduct });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(model.Id))
                {
                    return Json(new { success = false, message = "Producto no encontrado" });
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar producto");
                return Json(new { success = false, message = "Error al editar el producto" });
            }
        }

        // DELETE: Product/Delete/5
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return Json(new { success = false, message = "Producto no encontrado" });
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar producto");
                return Json(new { success = false, message = "Error al eliminar el producto" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategoryAjax([FromForm] Category category)
        {
            try
            {
                Console.WriteLine("🔍 [ProductController] CreateCategoryAjax() - Iniciando creación de categoría...");
                
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("⚠️ [ProductController] CreateCategoryAjax() - ModelState inválido");
                    return Json(new { success = false, message = "Datos inválidos" });
                }

                var created = await _categoryService.CreateCategoryAsync(category);
                
                Console.WriteLine($"✅ [ProductController] CreateCategoryAjax() - Categoría creada exitosamente: {created.Name}");
                return Json(new { success = true, data = new { id = created.Id, name = created.Name } });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"⚠️ [ProductController] CreateCategoryAjax() - Error de validación: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductController] CreateCategoryAjax() - Error inesperado: {ex.Message}");
                Console.WriteLine($"🔍 [ProductController] CreateCategoryAjax() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = "Error interno al crear la categoría" });
            }
        }

        [HttpPost]
       // [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStationAjax([FromForm] Station station)
        {
            try
            {
                Console.WriteLine("🔍 [ProductController] CreateStationAjax() - Iniciando creación de estación...");
                
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("⚠️ [ProductController] CreateStationAjax() - ModelState inválido");
                    return Json(new { success = false, message = "Datos inválidos" });
                }

                var created = await _stationService.CreateStationAsync(station);
                
                Console.WriteLine($"✅ [ProductController] CreateStationAjax() - Estación creada exitosamente: {created.Name}");
                return Json(new { success = true, data = new { id = created.Id, name = created.Name, type = created.Type } });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"⚠️ [ProductController] CreateStationAjax() - Error de validación: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductController] CreateStationAjax() - Error inesperado: {ex.Message}");
                Console.WriteLine($"🔍 [ProductController] CreateStationAjax() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = "Error interno al crear la estación" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Station)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    return Json(new { success = false, message = "Producto no encontrado" });
                }

                // Asegurarse de que los valores numéricos sean serializados correctamente
                var response = new
                {
                    success = true,
                    data = new
                    {
                        product.Id,
                        product.Name,
                        product.Description,
                        product.Price,
                        product.Cost,
                        product.TaxRate,
                        product.Unit,
                        product.ImageUrl,
                        product.IsActive,
                        product.CategoryId,
                        product.StationId,
                        Category = product.Category != null ? new { product.Category.Id, product.Category.Name } : null,
                        Station = product.Station != null ? new { product.Station.Id, product.Station.Name, product.Station.Type } : null
                    }
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener producto con ID {ProductId}", id);
                return Json(new { success = false, message = "Error al obtener el producto" });
            }
        }

        private bool ProductExists(Guid id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
} 