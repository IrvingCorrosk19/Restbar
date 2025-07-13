using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestBar.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly RestBarContext _context;

        public CategoryService(RestBarContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(Guid id)
        {
            return await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            category.Id = Guid.NewGuid();
            category.IsActive = true;
            
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            
            return category;
        }

        public async Task<Category> UpdateCategoryAsync(Guid id, Category category)
        {
            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
                throw new KeyNotFoundException($"Categoría con ID {id} no encontrada");

            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;
            existingCategory.IsActive = category.IsActive;

            await _context.SaveChangesAsync();
            return existingCategory;
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            try
            {
                Console.WriteLine("🔍 [CategoryService] GetActiveCategoriesAsync() - Iniciando consulta de categorías activas...");
                
                var categories = await _context.Categories
                    .Where(c => c.IsActive)
                    .ToListAsync();
                
                Console.WriteLine($"✅ [CategoryService] GetActiveCategoriesAsync() - Categorías activas encontradas: {categories.Count}");
                
                foreach (var category in categories)
                {
                    Console.WriteLine($"📋 [CategoryService] GetActiveCategoriesAsync() - Categoría: ID={category.Id}, Name={category.Name}, IsActive={category.IsActive}");
                }
                
                return categories;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [CategoryService] GetActiveCategoriesAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [CategoryService] GetActiveCategoriesAsync() - StackTrace: {ex.StackTrace}");
                throw;
            }
        }
    }
} 