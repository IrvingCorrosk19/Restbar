using Microsoft.AspNetCore.Mvc;
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
    public class ProductController : Controller
    {
        private readonly RestBarContext _context;
        private readonly ILogger<ProductController> _logger;
        private readonly ICategoryService _categoryService;
        private readonly IStationService _stationService;
        private readonly IProductService _productService;

        public ProductController(RestBarContext context, ILogger<ProductController> logger, ICategoryService categoryService, IStationService stationService, IProductService productService)
        {
            _context = context;
            _logger = logger;
            _categoryService = categoryService;
            _stationService = stationService;
            _productService = productService;
        }

        // GET: Product
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToListAsync();
            return View(products);
        }

        // GET: Product/GetProducts
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                return Json(new { success = true, data = products });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos");
                return Json(new { success = false, message = "Error al obtener los productos" });
            }
        }

        // GET: Product/GetCategories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                return Json(new { success = true, data = categories });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías");
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
                    CreatedAt = DateTime.UtcNow
                };

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
               // existingProduct.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

                _context.Update(existingProduct);
                await _context.SaveChangesAsync();

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
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Datos inválidos" });

            var created = await _categoryService.CreateCategoryAsync(category);
            return Json(new { success = true, data = new { id = created.Id, name = created.Name } });
        }

        [HttpPost]
       // [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStationAjax([FromForm] Station station)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Datos inválidos" });

            var created = await _stationService.CreateStationAsync(station);
            return Json(new { success = true, data = new { id = created.Id, name = created.Name, type = created.Type } });
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