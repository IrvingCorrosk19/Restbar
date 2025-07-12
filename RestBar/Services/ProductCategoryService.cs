using Microsoft.EntityFrameworkCore;

using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly RestBarContext _context;

        public ProductCategoryService(RestBarContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductCategory>> GetAllAsync()
        {
            return await _context.ProductCategories
                .Include(pc => pc.Products)
                .ToListAsync();
        }

        public async Task<ProductCategory?> GetByIdAsync(Guid id)
        {
            return await _context.ProductCategories
                .Include(pc => pc.Products)
                .FirstOrDefaultAsync(pc => pc.Id == id);
        }

        public async Task<ProductCategory> CreateAsync(ProductCategory category)
        {
            _context.ProductCategories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task UpdateAsync(ProductCategory category)
        {
            try
            {
                // Buscar si hay una entidad con el mismo ID siendo rastreada
                var existingEntity = _context.ChangeTracker.Entries<ProductCategory>()
                    .FirstOrDefault(e => e.Entity.Id == category.Id);

                if (existingEntity != null)
                {
                    // Detach la entidad existente para evitar conflictos
                    existingEntity.State = EntityState.Detached;
                }

                // Usar Update para manejar automáticamente el tracking
                _context.ProductCategories.Update(category);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al actualizar la categoría de producto en la base de datos.", ex);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var category = await _context.ProductCategories.FindAsync(id);
            if (category != null)
            {
                _context.ProductCategories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ProductCategory?> GetByNameAsync(string name)
        {
            return await _context.ProductCategories
                .Include(pc => pc.Products)
                .FirstOrDefaultAsync(pc => pc.Name == name);
        }

        public async Task<ProductCategory?> GetCategoryWithProductsAsync(Guid id)
        {
            return await _context.ProductCategories
                .Include(pc => pc.Products)
                .FirstOrDefaultAsync(pc => pc.Id == id);
        }

        public async Task<IEnumerable<ProductCategory>> GetCategoriesWithProductsAsync()
        {
            return await _context.ProductCategories
                .Include(pc => pc.Products)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductCategory>> SearchCategoriesAsync(string searchTerm)
        {
            return await _context.ProductCategories
                .Include(pc => pc.Products)
                .Where(pc => pc.Name.Contains(searchTerm) || 
                            (pc.Description != null && pc.Description.Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task<int> GetProductCountAsync(Guid categoryId)
        {
            return await _context.ProductCategories
                .Where(pc => pc.Id == categoryId)
                .Select(pc => pc.Products.Count)
                .FirstOrDefaultAsync();
        }
    }
} 