using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestBar.Services
{
    public class ProductService : BaseTrackingService, IProductService
    {
        public ProductService(RestBarContext context, IHttpContextAccessor httpContextAccessor) 
            : base(context, httpContextAccessor)
        {
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
                Console.WriteLine($"🔍 [ProductService] CreateAsync() - Iniciando creación de producto: {product.Name}");
                
                if (product == null)
                    throw new ArgumentNullException(nameof(product), "El producto no puede ser null.");

                // ✅ Obtener usuario actual para CompanyId y BranchId
                var userIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    var user = await _context.Users
                        .Include(u => u.Branch)
                        .FirstOrDefaultAsync(u => u.Id == userId);
                    
                    if (user != null && user.Branch != null)
                    {
                        product.CompanyId = user.Branch.CompanyId;
                        product.BranchId = user.BranchId;
                        Console.WriteLine($"✅ [ProductService] CreateAsync() - Asignando CompanyId: {product.CompanyId}, BranchId: {product.BranchId}");
                    }
                }

                // ✅ Generar ID si no existe
                if (product.Id == Guid.Empty)
                {
                    product.Id = Guid.NewGuid();
                }
                
                // ✅ Usar SetCreatedTracking para establecer todos los campos de auditoría
                SetCreatedTracking(product);
                
                // Si el controlador ya estableció CreatedBy, mantenerlo
                var existingCreatedBy = product.CreatedBy;
                if (!string.IsNullOrWhiteSpace(existingCreatedBy))
                {
                    product.CreatedBy = existingCreatedBy;
                    product.UpdatedBy = existingCreatedBy;
                }
                
                Console.WriteLine($"✅ [ProductService] CreateAsync() - Campos establecidos: CreatedBy={product.CreatedBy}, CreatedAt={product.CreatedAt}, UpdatedAt={product.UpdatedAt}");

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ [ProductService] CreateAsync() - Producto creado exitosamente: {product.Name} (ID: {product.Id})");
                return product;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductService] CreateAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [ProductService] CreateAsync() - StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<Product> UpdateAsync(Guid id, Product product)
        {
            try
            {
                Console.WriteLine($"🔍 [ProductService] UpdateAsync() - Actualizando producto: {product.Name} (ID: {id})");
                
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
                
                // ✅ Usar SetUpdatedTracking para establecer campos de auditoría de actualización
                SetUpdatedTracking(existing);
                
                Console.WriteLine($"✅ [ProductService] UpdateAsync() - Campos actualizados: UpdatedBy={existing.UpdatedBy}, UpdatedAt={existing.UpdatedAt}");

                await _context.SaveChangesAsync();
                
                Console.WriteLine($"✅ [ProductService] UpdateAsync() - Producto actualizado exitosamente");
                return existing;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductService] UpdateAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [ProductService] UpdateAsync() - StackTrace: {ex.StackTrace}");
                throw;
            }
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
                    stationName = p.Station.Name,

                    taxRate = p.TaxRate  // ✅ AGREGADO: Campo taxRate
                })
                .ToListAsync();
        }
    }
} 