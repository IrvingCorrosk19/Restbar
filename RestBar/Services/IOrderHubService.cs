using RestBar.Models;
using System.Threading.Tasks;

namespace RestBar.Services
{
    public interface IOrderHubService
    {
        Task NotifyOrderStatusChanged(Guid orderId, OrderStatus newStatus);
        Task NotifyOrderItemStatusChanged(Guid orderId, Guid orderItemId, OrderItemStatus newStatus);
        Task NotifyOrderItemUpdated(Guid orderId, Guid orderItemId, Guid productId, string productName, string newStatus, string timestamp);
        Task NotifyNewOrder(Guid orderId, string tableNumber);
        Task NotifyOrderCancelled(Guid orderId);
        Task NotifyOrderCompleted(Guid orderId, string tableNumber);
        Task NotifyTableStatusChanged(Guid tableId, string newStatus);
        Task NotifyKitchenUpdate();
        Task NotifyPaymentProcessed(Guid orderId, decimal amount, string method, bool isFullyPaid);
        
        // Notificación dirigida a una estación específica por tipo (ej. "kitchen", "bar")
        // Envía evento "KitchenUpdate" al grupo "station_{stationType}".
        // Usar cuando el tipo de estación del ítem actualizado sea conocido.
        Task NotifyStationUpdate(string stationType);

        // Métodos para notificar cambios de stock
        Task NotifyStockUpdated(Guid productId, string productName, decimal newStock);
        Task NotifyStockReduced(Guid productId, string productName, decimal oldStock, decimal newStock, decimal quantityReduced);
    }
} 