using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RestBar.Hubs;
using RestBar.Models;
using System.Threading.Tasks;

namespace RestBar.Services
{
    public class OrderHubService : IOrderHubService
    {
        private readonly IHubContext<OrderHub> _hubContext;
        private readonly ILogger<OrderHubService> _logger;

        public OrderHubService(IHubContext<OrderHub> hubContext, ILogger<OrderHubService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task NotifyOrderStatusChanged(Guid orderId, OrderStatus newStatus)
        {
            _logger.LogInformation("[SignalR] NotifyOrderStatusChanged - OrderId: {OrderId}, Status: {Status}", orderId, newStatus);
            await _hubContext.Clients.Group($"order_{orderId}")
                .SendAsync("OrderStatusChanged", orderId, newStatus.ToString());
        }

        public async Task NotifyOrderItemStatusChanged(Guid orderId, Guid orderItemId, OrderItemStatus newStatus)
        {
            try
            {
                _logger.LogDebug("[SignalR] NotifyOrderItemStatusChanged - OrderId: {OrderId}, ItemId: {ItemId}, Status: {Status}",
                    orderId, orderItemId, newStatus);

                var data = new
                {
                    OrderId = orderId,
                    ItemId = orderItemId,
                    Status = newStatus.ToString(),
                    Message = newStatus == OrderItemStatus.Cancelled
                        ? "🗑️ Item eliminado de la orden"
                        : $"✅ Item actualizado a {newStatus}",
                    Type = newStatus == OrderItemStatus.Cancelled ? "item_deleted" : "item_updated",
                    Timestamp = DateTime.UtcNow
                };

                // Notificar al grupo específico de la orden
                await _hubContext.Clients.Group($"order_{orderId}")
                    .SendAsync("OrderItemStatusChanged", data);

                // Notificar al grupo legacy "kitchen" (broadcast general a todas las estaciones)
                await _hubContext.Clients.Group("kitchen")
                    .SendAsync("OrderItemStatusChanged", data);

                // Si el item se cancela, notificar también a Order/Index
                if (newStatus == OrderItemStatus.Cancelled)
                {
                    await _hubContext.Clients.Group("orders")
                        .SendAsync("OrderItemStatusChanged", data);
                }

                _logger.LogDebug("[SignalR] NotifyOrderItemStatusChanged - notificación enviada a order_{OrderId} + kitchen", orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SignalR] NotifyOrderItemStatusChanged - error para OrderId: {OrderId}, ItemId: {ItemId}",
                    orderId, orderItemId);
                throw;
            }
        }

        public async Task NotifyOrderItemUpdated(Guid orderId, Guid orderItemId, Guid productId, string productName, string newStatus, string timestamp)
        {
            await _hubContext.Clients.Group($"order_{orderId}")
                .SendAsync("OrderItemUpdated", new
                {
                    ItemId = orderItemId,
                    ProductId = productId,
                    ProductName = productName,
                    NewStatus = newStatus,
                    Timestamp = timestamp
                });
        }

        public async Task NotifyNewOrder(Guid orderId, string tableNumber)
        {
            try
            {
                _logger.LogInformation("[SignalR] NotifyNewOrder - OrderId: {OrderId}, Mesa: {TableNumber}", orderId, tableNumber);

                var data = new
                {
                    OrderId = orderId,
                    TableNumber = tableNumber,
                    Message = $"🆕 Nueva orden recibida para Mesa {tableNumber}",
                    Type = "new_order",
                    Timestamp = DateTime.UtcNow
                };

                // Notificar al grupo legacy "kitchen" — recibido por todas las vistas de estación (aún unidas a "kitchen")
                await _hubContext.Clients.Group("kitchen")
                    .SendAsync("NewOrder", data);

                // Notificar al grupo de órdenes (Order/Index)
                await _hubContext.Clients.Group("orders")
                    .SendAsync("NewOrder", data);

                _logger.LogDebug("[SignalR] NotifyNewOrder - enviado a grupos 'kitchen' y 'orders'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SignalR] NotifyNewOrder - error para OrderId: {OrderId}", orderId);
                throw;
            }
        }

        public async Task NotifyOrderCancelled(Guid orderId)
        {
            await _hubContext.Clients.Group($"order_{orderId}")
                .SendAsync("OrderCancelled", orderId);
        }

        public async Task NotifyOrderCompleted(Guid orderId, string tableNumber)
        {
            try
            {
                _logger.LogInformation("[SignalR] NotifyOrderCompleted - OrderId: {OrderId}, Mesa: {TableNumber}", orderId, tableNumber);

                var data = new
                {
                    OrderId = orderId,
                    TableNumber = tableNumber,
                    Message = $"✅ Orden completada para Mesa {tableNumber}",
                    Type = "order_completed",
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.Group("kitchen").SendAsync("OrderCompleted", data);
                await _hubContext.Clients.Group("orders").SendAsync("OrderCompleted", data);
                await _hubContext.Clients.Group($"order_{orderId}").SendAsync("OrderCompleted", data);

                _logger.LogDebug("[SignalR] NotifyOrderCompleted - enviado a kitchen, orders, order_{OrderId}", orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SignalR] NotifyOrderCompleted - error para OrderId: {OrderId}", orderId);
                throw;
            }
        }

        public async Task NotifyTableStatusChanged(Guid tableId, string newStatus)
        {
            try
            {
                _logger.LogInformation("[SignalR] NotifyTableStatusChanged - TableId: {TableId}, Status: {Status}", tableId, newStatus);

                var message = newStatus switch
                {
                    "EnPreparacion" => $"👨‍🍳 Mesa cambió a EN PREPARACIÓN - Cocina trabajando",
                    "ParaPago"      => $"💰 Mesa cambió a PARA PAGO - Lista para cobrar",
                    "Ocupada"       => $"👥 Mesa cambió a OCUPADA - Clientes atendidos",
                    "Disponible"    => $"✅ Mesa cambió a DISPONIBLE - Libre para nuevos clientes",
                    "Servida"       => $"🍽️ Mesa cambió a SERVIDA - Pedido entregado",
                    _               => $"🔄 Mesa cambió de estado a {newStatus}"
                };

                var data = new
                {
                    TableId = tableId,
                    NewStatus = newStatus,
                    Message = message,
                    Type = "table_status_changed",
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.Group($"table_{tableId}").SendAsync("TableStatusChanged", data);
                await _hubContext.Clients.Group("table_all").SendAsync("TableStatusChanged", data);
                await _hubContext.Clients.Group("orders").SendAsync("TableStatusChanged", data);
                await _hubContext.Clients.Group("kitchen").SendAsync("TableStatusChanged", data);

                _logger.LogDebug("[SignalR] NotifyTableStatusChanged - enviado a table_{TableId}, table_all, orders, kitchen", tableId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SignalR] NotifyTableStatusChanged - error para TableId: {TableId}", tableId);
                throw;
            }
        }

        public async Task NotifyKitchenUpdate()
        {
            // Difusión general: llega a cualquier estación unida al grupo "kitchen"
            await _hubContext.Clients.Group("kitchen").SendAsync("KitchenUpdate");
        }

        /// <summary>
        /// Envía evento "KitchenUpdate" al grupo específico de la estación indicada
        /// (ej. "station_kitchen", "station_bar"). Usar cuando se conoce el tipo de
        /// estación del ítem actualizado, para evitar refrescos innecesarios en otras estaciones.
        /// </summary>
        public async Task NotifyStationUpdate(string stationType)
        {
            if (string.IsNullOrWhiteSpace(stationType))
            {
                // Sin tipo de estación conocido → broadcast al grupo legacy "kitchen"
                await NotifyKitchenUpdate();
                return;
            }

            var groupName = $"station_{stationType.ToLower().Trim()}";
            _logger.LogDebug("[SignalR] NotifyStationUpdate - enviando KitchenUpdate a grupo '{GroupName}'", groupName);
            await _hubContext.Clients.Group(groupName).SendAsync("KitchenUpdate");
        }

        public async Task NotifyPaymentProcessed(Guid orderId, decimal amount, string method, bool isFullyPaid)
        {
            await _hubContext.Clients.Group($"order_{orderId}")
                .SendAsync("PaymentProcessed", orderId, amount, method, isFullyPaid);
        }

        // Notificaciones de stock
        public async Task NotifyStockUpdated(Guid productId, string productName, decimal newStock)
        {
            await _hubContext.Clients.Group("stock_updates")
                .SendAsync("StockUpdated", new
                {
                    ProductId = productId,
                    ProductName = productName,
                    NewStock = newStock,
                    Timestamp = DateTime.UtcNow
                });
        }

        public async Task NotifyStockReduced(Guid productId, string productName, decimal oldStock, decimal newStock, decimal quantityReduced)
        {
            await _hubContext.Clients.Group("stock_updates")
                .SendAsync("StockReduced", new
                {
                    ProductId = productId,
                    ProductName = productName,
                    OldStock = oldStock,
                    NewStock = newStock,
                    QuantityReduced = quantityReduced,
                    Timestamp = DateTime.UtcNow
                });
        }

        public async Task NotifyCashSessionChanged(Guid sessionId, Guid registerId, string status)
        {
            var payload = new { SessionId = sessionId, RegisterId = registerId, Status = status, Timestamp = DateTime.UtcNow };
            await _hubContext.Clients.Group($"cash_register_{registerId}").SendAsync("CashSessionChanged", payload);
            await _hubContext.Clients.Group("cash_dashboard").SendAsync("CashSessionChanged", payload);
        }

        public async Task NotifyCashMovement(Guid sessionId, Guid registerId, string movementType, decimal amount)
        {
            var payload = new { SessionId = sessionId, RegisterId = registerId, MovementType = movementType, Amount = amount, Timestamp = DateTime.UtcNow };
            await _hubContext.Clients.Group($"cash_register_{registerId}").SendAsync("CashMovement", payload);
            await _hubContext.Clients.Group("cash_dashboard").SendAsync("CashMovement", payload);
        }
    }
}
