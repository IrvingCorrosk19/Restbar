using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestBar.Services
{
    public class CategoryService : BaseTrackingService, ICategoryService
    {
        public CategoryService(RestBarContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
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
            try
            {
                Console.WriteLine($"🔍 [CategoryService] CreateCategoryAsync() - Iniciando creación de categoría: {category.Name}");
                
                // ✅ Obtener usuario actual para CompanyId y BranchId
                var userIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    var user = await _context.Users
                        .Include(u => u.Branch)
                        .FirstOrDefaultAsync(u => u.Id == userId);
                    
                    if (user != null && user.Branch != null)
                    {
                        category.CompanyId = user.Branch.CompanyId;
                        category.BranchId = user.BranchId;
                        Console.WriteLine($"✅ [CategoryService] CreateCategoryAsync() - Asignando CompanyId: {category.CompanyId}, BranchId: {category.BranchId}");
                    }
                }
                
                // ✅ Generar ID si no existe
                if (category.Id == Guid.Empty)
                {
                    category.Id = Guid.NewGuid();
                }
                
                category.IsActive = true;
                
                // ✅ Usar SetCreatedTracking para establecer todos los campos de auditoría
                SetCreatedTracking(category);
                
                // Si el controlador ya estableció CreatedBy, mantenerlo
                var existingCreatedBy = category.CreatedBy;
                if (!string.IsNullOrWhiteSpace(existingCreatedBy))
                {
                    category.CreatedBy = existingCreatedBy;
                    category.UpdatedBy = existingCreatedBy;
                }
                
                Console.WriteLine($"✅ [CategoryService] CreateCategoryAsync() - Campos establecidos: CreatedBy={category.CreatedBy}, CreatedAt={category.CreatedAt}, UpdatedAt={category.UpdatedAt}");
                
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"✅ [CategoryService] CreateCategoryAsync() - Categoría creada exitosamente: {category.Name} (ID: {category.Id})");
                return category;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [CategoryService] CreateCategoryAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [CategoryService] CreateCategoryAsync() - StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<Category> UpdateCategoryAsync(Guid id, Category category)
        {
            try
            {
                Console.WriteLine($"🔍 [CategoryService] UpdateCategoryAsync() - Actualizando categoría: {category.Name} (ID: {id})");
                
                var existingCategory = await _context.Categories.FindAsync(id);
                if (existingCategory == null)
                    throw new KeyNotFoundException($"Categoría con ID {id} no encontrada");

                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;
                existingCategory.IsActive = category.IsActive;

                // ✅ Usar SetUpdatedTracking para establecer campos de auditoría de actualización
                SetUpdatedTracking(existingCategory);
                
                Console.WriteLine($"✅ [CategoryService] UpdateCategoryAsync() - Campos actualizados: UpdatedBy={existingCategory.UpdatedBy}, UpdatedAt={existingCategory.UpdatedAt}");

                await _context.SaveChangesAsync();
                
                Console.WriteLine($"✅ [CategoryService] UpdateCategoryAsync() - Categoría actualizada exitosamente");
                return existingCategory;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [CategoryService] UpdateCategoryAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [CategoryService] UpdateCategoryAsync() - StackTrace: {ex.StackTrace}");
                throw;
            }
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