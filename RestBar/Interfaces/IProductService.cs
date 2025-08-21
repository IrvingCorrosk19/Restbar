using RestBar.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestBar.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(Guid id);
        Task<Product> CreateAsync(Product product);
        Task<Product> UpdateAsync(Guid id, Product product);
        Task<bool> DeleteAsync(Guid id);


        // Métodos adicionales específicos para Product
        Task<IEnumerable<Product>> GetByCategoryIdAsync(Guid categoryId);
        Task<IEnumerable<Product>> GetActiveProductsAsync();

        Task<Product?> GetProductWithModifiersAsync(Guid id);
        Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
        
        // Método para obtener productos con formato específico para ViewBag
        Task<IEnumerable<Product>> GetActiveProductsForViewBagAsync();
        
        // Método para obtener productos con detalles completos
        Task<IEnumerable<object>> GetProductsWithDetailsAsync(Guid? categoryId = null);
    }
}