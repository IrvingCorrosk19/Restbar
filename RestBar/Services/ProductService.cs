using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestBar.Services
{
    public class ProductService : IProductService
    {
        private readonly RestBarContext _context;

        public ProductService(RestBarContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.Include(p => p.Category).Include(p => p.Station).OrderBy(p => p.Name).ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _context.Products.Include(p => p.Category).Include(p => p.Station).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> CreateAsync(Product product)
        {
         
            try
            {
                if (product == null)
                    throw new ArgumentNullException(nameof(product), "El producto no puede ser null.");

                product.Id = Guid.NewGuid();
                product.CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return product;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProductService] Error en CreateAsync: {ex.Message}");
                throw; // Deja que el controlador maneje la excepci�n
            }
        }

        public async Task<Product> UpdateAsync(Guid id, Product product)
        {
            var existing = await _context.Products.FindAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Producto con ID {id} no encontrado");
            // Actualizar campos
            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.Cost = product.Cost;
            existing.TaxRate = product.TaxRate;
            existing.Unit = product.Unit;
            existing.ImageUrl = product.ImageUrl;
            existing.IsActive = product.IsActive;
            existing.CategoryId = product.CategoryId;
            existing.StationId = product.StationId;
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return false;
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Product>> GetByCategoryIdAsync(Guid categoryId)
        {
            return await _context.Products
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive == true)
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<Product?> GetProductWithInventoryAsync(Guid id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Inventories)
                    .ThenInclude(i => i.Branch)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product?> GetProductWithModifiersAsync(Guid id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Modifiers)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _context.Products
                .Where(p => p.Price >= minPrice && p.Price <= maxPrice && p.IsActive == true)
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            return await _context.Products
                .Where(p => (p.Name.Contains(searchTerm) || 
                           (p.Description != null && p.Description.Contains(searchTerm))) && 
                           p.IsActive == true)
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetActiveProductsForViewBagAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetProductsWithDetailsAsync(Guid? categoryId = null)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Station)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            return await query
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    price = p.Price,
                    imageUrl = p.ImageUrl,
                    categoryId = p.CategoryId,
                    categoryName = p.Category.Name,
                    stationId = p.StationId,
                    stationName = p.Station.Name
                })
                .ToListAsync();
        }
    }
} 