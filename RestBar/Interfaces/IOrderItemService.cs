using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface IOrderItemService
    {
        Task<IEnumerable<OrderItem>> GetAllAsync();
        Task<OrderItem?> GetByIdAsync(Guid id);
        Task<OrderItem> CreateAsync(OrderItem orderItem);
        Task UpdateAsync(OrderItem orderItem);
        Task DeleteAsync(Guid id);

        // Métodos adicionales específicos para OrderItem
        Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId);
        Task<IEnumerable<OrderItem>> GetByProductIdAsync(Guid productId);
        Task<decimal> GetOrderTotalAsync(Guid orderId);
        Task<OrderItem?> GetOrderItemWithDetailsAsync(Guid id);
        Task<IEnumerable<OrderItem>> GetByQuantityRangeAsync(decimal minQuantity, decimal maxQuantity);
        Task<IEnumerable<OrderItem>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<IEnumerable<OrderItem>> GetDiscountedItemsAsync();
        
        // Métodos para operaciones específicas del controlador
        Task<OrderItem?> GetOrderItemWithOrderAndProductAsync(Guid id);
        Task<Guid?> DeleteOrderItemAndGetOrderIdAsync(Guid orderItemId);
    }
} 