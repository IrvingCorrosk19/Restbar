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

        // ✅ MÉTODOS DE INVENTARIO
        /// <summary>
        /// Obtiene el stock disponible total de un producto (stock global + stock por estaciones)
        /// </summary>
        Task<decimal> GetAvailableStockAsync(Guid productId, Guid? branchId = null);

        /// <summary>
        /// Obtiene el stock disponible de un producto en una estación específica
        /// </summary>
        Task<decimal> GetStockInStationAsync(Guid productId, Guid stationId, Guid? branchId = null);

        /// <summary>
        /// Encuentra la mejor estación para asignar un producto basándose en stock disponible y prioridad
        /// </summary>
        Task<Guid?> FindBestStationForProductAsync(Guid productId, decimal requiredQuantity, Guid? branchId = null);

        /// <summary>
        /// Reduce el stock de un producto (global o por estación)
        /// </summary>
        Task<bool> ReduceStockAsync(Guid productId, decimal quantity, Guid? stationId = null, Guid? branchId = null);

        /// <summary>
        /// Restaura stock de un producto (al cancelar una orden)
        /// </summary>
        Task<bool> RestoreStockAsync(Guid productId, decimal quantity, Guid? stationId = null, Guid? branchId = null);

        /// <summary>
        /// Verifica si un producto tiene stock suficiente
        /// </summary>
        Task<bool> HasStockAvailableAsync(Guid productId, decimal quantity, Guid? branchId = null);
    }
}