using RestBar.Models;
using RestBar.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestBar.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(Guid id);
        Task<Order> CreateAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(Guid id);

        // Métodos adicionales específicos para Order
        Task<IEnumerable<Order>> GetByTableIdAsync(Guid tableId);
        Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId);
        Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status);
        Task<IEnumerable<Order>> GetOpenOrdersAsync();
        Task<Order?> GetOrderWithDetailsAsync(Guid id);
        Task<Order?> GetOrderWithPaymentsAsync(Guid id);
        Task<decimal> CalculateOrderTotalAsync(Guid id);
        Task CloseOrderAsync(Guid id);
        Task<Order> SendToKitchenAsync(SendOrderDto dto, Guid? userId);
        
        // NUEVOS: Métodos para la lógica de KitchenStatus
        Task<Order> AddOrUpdateOrderWithPendingItemsAsync(SendOrderDto dto, Guid? userId);
        Task<List<OrderItem>> SendPendingItemsToKitchenAsync(Guid orderId);
        Task MarkItemAsReadyAsync(Guid orderId, Guid itemId);
        Task CancelOrderItemAsync(Guid orderId, Guid itemId);
        Task MarkItemAsPreparingAsync(Guid orderId, Guid itemId);
        
        // Métodos para KitchenOrders y validación
        Task<List<KitchenOrderViewModel>> GetKitchenOrdersAsync();
        Task<bool> OrderExistsAsync(Guid id);
        
        // Métodos para manejar órdenes activas
        Task<Order?> GetActiveOrderByTableAsync(Guid tableId);
        Task<Order> AddItemsToOrderAsync(Guid orderId, List<OrderItemDto> items);
        Task<Order> RemoveItemFromOrderAsync(Guid orderId, Guid productId, string? status = null, Guid? itemId = null);
        Task<Order> UpdateItemQuantityAsync(Guid orderId, Guid productId, decimal newQuantity, string? status = null);
        Task<Order> UpdateItemQuantityByIdAsync(Guid orderId, Guid itemId, decimal newQuantity);
        Task<Order> UpdateItemAsync(Guid orderId, Guid productId, decimal newQuantity, string? notes, string? status = null);
        Task CancelOrderAsync(Guid orderId, Guid? userId, string? reason = null, Guid? supervisorId = null);
        Task CheckAndUpdateTableStatusAsync(Guid orderId);
        Task<Order> UpdateOrderCompleteAsync(Guid orderId, List<UpdateOrderItemDto> items);
        Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(Guid orderId);
        Task<bool> SetTableOccupiedAsync(Guid tableId);
        
        // Método para obtener órdenes con pagos pendientes
        Task<IEnumerable<Order>> GetPendingPaymentOrdersAsync();
    }
} 