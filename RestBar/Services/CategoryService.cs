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
                .Include(c => c.Company)
                .Include(c => c.Branch)
                .ToListAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(Guid id)
        {
            return await _context.Categories
                .Include(c => c.Products)
                .Include(c => c.Company)
                .Include(c => c.Branch)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            Console.WriteLine("=== INICIO CreateCategoryAsync ===");
            Console.WriteLine($"Categoría recibida - ID: {category.Id}, Name: {category.Name}");
            Console.WriteLine($"CompanyId: {category.CompanyId}, BranchId: {category.BranchId}");
            Console.WriteLine($"CreatedAt: {category.CreatedAt}, IsActive: {category.IsActive}");
            
            try
            {
                category.Id = Guid.NewGuid();
                category.IsActive = true;
                
                Console.WriteLine($"Categoría procesada - ID: {category.Id}, IsActive: {category.IsActive}");
                
                _context.Categories.Add(category);
                Console.WriteLine("✅ Categoría agregada al contexto");
                
                await _context.SaveChangesAsync();
                Console.WriteLine("✅ Cambios guardados en la base de datos");
                
                Console.WriteLine($"✅ Categoría creada exitosamente - ID: {category.Id}, Name: {category.Name}");
                return category;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en CreateCategoryAsync: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                // Obtener más detalles del error interno
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"❌ Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner StackTrace: {ex.InnerException.StackTrace}");
                }
                
                throw;
            }
        }

        public async Task<Category> UpdateCategoryAsync(Guid id, Category category)
        {
            Console.WriteLine("=== INICIO UpdateCategoryAsync ===");
            Console.WriteLine($"ID de categoría a actualizar: {id}");
            Console.WriteLine($"Categoría recibida - Name: {category.Name}, UpdatedAt: {category.UpdatedAt}, UpdatedBy: {category.UpdatedBy}");
            
            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
            {
                Console.WriteLine("❌ Categoría no encontrada");
                throw new KeyNotFoundException($"Categoría con ID {id} no encontrada");
            }

            Console.WriteLine($"Categoría existente encontrada - Name: {existingCategory.Name}");
            
            // Actualizar campos básicos
            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;
            existingCategory.IsActive = category.IsActive;
            existingCategory.CompanyId = category.CompanyId;
            existingCategory.BranchId = category.BranchId;
            
            // Actualizar campos de auditoría
            existingCategory.UpdatedAt = category.UpdatedAt;
            existingCategory.UpdatedBy = category.UpdatedBy;
            
            Console.WriteLine($"Categoría actualizada - UpdatedAt: {existingCategory.UpdatedAt}, UpdatedBy: {existingCategory.UpdatedBy}");

            await _context.SaveChangesAsync();
            Console.WriteLine("✅ Cambios guardados en la base de datos");
            
            // Cargar las referencias después de guardar
            await _context.Entry(existingCategory).Reference(c => c.Company).LoadAsync();
            await _context.Entry(existingCategory).Reference(c => c.Branch).LoadAsync();
            
            Console.WriteLine($"✅ Categoría actualizada exitosamente - ID: {existingCategory.Id}, Name: {existingCategory.Name}");
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
            return await _context.Categories
                .Where(c => c.IsActive)
                .Include(c => c.Company)
                .Include(c => c.Branch)
                .ToListAsync();
        }
    }
} 