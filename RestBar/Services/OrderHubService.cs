using Microsoft.AspNetCore.SignalR;
using RestBar.Hubs;
using RestBar.Models;
using System.Threading.Tasks;

namespace RestBar.Services
{
    public class OrderHubService : IOrderHubService
    {
        private readonly IHubContext<OrderHub> _hubContext;

        public OrderHubService(IHubContext<OrderHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyOrderStatusChanged(Guid orderId, OrderStatus newStatus)
        {
            await _hubContext.Clients.Group($"order_{orderId}")
                .SendAsync("OrderStatusChanged", orderId, newStatus.ToString());
        }

        public async Task NotifyOrderItemStatusChanged(Guid orderId, Guid orderItemId, OrderItemStatus newStatus)
        {
            await _hubContext.Clients.Group($"order_{orderId}")
                .SendAsync("OrderItemStatusChanged", orderId, orderItemId, newStatus.ToString());
        }

        public async Task NotifyOrderItemUpdated(Guid orderId, Guid orderItemId, Guid productId, string productName, string newStatus, string timestamp)
        {
            await _hubContext.Clients.Group($"order_{orderId}")
                .SendAsync("OrderItemUpdated", new {
                    ItemId = orderItemId,
                    ProductId = productId,
                    ProductName = productName,
                    NewStatus = newStatus,
                    Timestamp = timestamp
                });
        }

        public async Task NotifyNewOrder(Guid orderId, string tableNumber)
        {
            await _hubContext.Clients.Group("kitchen")
                .SendAsync("NewOrder", orderId, tableNumber);
        }

        public async Task NotifyOrderCancelled(Guid orderId)
        {
            await _hubContext.Clients.Group($"order_{orderId}")
                .SendAsync("OrderCancelled", orderId);
        }

        public async Task NotifyTableStatusChanged(Guid tableId, string newStatus)
        {
            await _hubContext.Clients.Group($"table_{tableId}")
                .SendAsync("TableStatusChanged", tableId, newStatus);
            
            await _hubContext.Clients.Group("table_all")
                .SendAsync("TableStatusChanged", tableId, newStatus);
        }

        public async Task NotifyKitchenUpdate()
        {
            await _hubContext.Clients.Group("kitchen")
                .SendAsync("KitchenUpdate");
        }

        public async Task NotifyPaymentProcessed(Guid orderId, decimal amount, string method, bool isFullyPaid)
        {
            await _hubContext.Clients.Group($"order_{orderId}")
                .SendAsync("PaymentProcessed", orderId, amount, method, isFullyPaid);
        }
    }
} 