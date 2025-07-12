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

        public CategoryController(ICategoryService categoryService, IProductService productService)
        {
            _categoryService = categoryService;
            _productService = productService;
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
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Datos inválidos" });

            try
            {
                var created = await _categoryService.CreateCategoryAsync(category);
                return Json(new { success = true, data = new { id = created.Id, name = created.Name } });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al crear la categoría" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            var data = categories.Select(c => new { id = c.Id, name = c.Name }).ToList();
            return Json(new { success = true, data });
        }
    }
} 