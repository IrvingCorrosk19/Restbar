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
            Console.WriteLine($"🔍 ENTRADA: NotifyOrderStatusChanged() - OrderId: {orderId}, Status: {newStatus}");
            // 🎯 LOG ESTRATÉGICO: NOTIFICACIÓN ENVIADA
            Console.WriteLine($"🚀 [OrderHubService] NotifyOrderStatusChanged() - NOTIFICACIÓN ENVIADA - OrderId: {orderId}, Status: {newStatus}");
            
            await _hubContext.Clients.Group($"order_{orderId}")
                .SendAsync("OrderStatusChanged", orderId, newStatus.ToString());
        }

        public async Task NotifyOrderItemStatusChanged(Guid orderId, Guid orderItemId, OrderItemStatus newStatus)
        {
            try
            {
                Console.WriteLine($"🔍 [OrderHubService] NotifyOrderItemStatusChanged() - Iniciando notificación...");
                Console.WriteLine($"📋 [OrderHubService] NotifyOrderItemStatusChanged() - OrderId: {orderId}, ItemId: {orderItemId}, Status: {newStatus}");
                
                var data = new {
                    OrderId = orderId,
                    ItemId = orderItemId,
                    Status = newStatus.ToString(),
                    Message = newStatus == OrderItemStatus.Cancelled ? 
                        "🗑️ Item eliminado de la orden" : 
                        $"✅ Item actualizado a {newStatus}",
                    Type = newStatus == OrderItemStatus.Cancelled ? "item_deleted" : "item_updated",
                    Timestamp = DateTime.UtcNow
                };
                
                await _hubContext.Clients.Group($"order_{orderId}")
                    .SendAsync("OrderItemStatusChanged", data);
                
                // ✅ NUEVO: También notificar al grupo de cocina
                await _hubContext.Clients.Group("kitchen")
                    .SendAsync("OrderItemStatusChanged", data);
                
                // ✅ NUEVO: Notificar también a Order/Index cuando se elimina un item
                if (newStatus == OrderItemStatus.Cancelled)
                {
                    await _hubContext.Clients.Group("orders")
                        .SendAsync("OrderItemStatusChanged", data);
                    Console.WriteLine($"📤 [OrderHubService] Notificación de item eliminado enviada al grupo 'orders'");
                }
                
                Console.WriteLine($"✅ [OrderHubService] NotifyOrderItemStatusChanged() - Notificación enviada exitosamente");
                Console.WriteLine($"📤 [OrderHubService] NotifyOrderItemStatusChanged() - Grupos: order_{orderId}, kitchen");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [OrderHubService] NotifyOrderItemStatusChanged() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [OrderHubService] NotifyOrderItemStatusChanged() - StackTrace: {ex.StackTrace}");
                throw;
            }
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
            try
            {
                Console.WriteLine($"🔍 [OrderHubService] NotifyNewOrder() - Iniciando notificación...");
                Console.WriteLine($"📋 [OrderHubService] NotifyNewOrder() - OrderId: {orderId}, TableNumber: {tableNumber}");
                
                var data = new { 
                    OrderId = orderId, 
                    TableNumber = tableNumber,
                    Message = $"🆕 Nueva orden recibida para Mesa {tableNumber}",
                    Type = "new_order",
                    Timestamp = DateTime.UtcNow
                };
                Console.WriteLine($"📤 [OrderHubService] NotifyNewOrder() - Enviando datos: {System.Text.Json.JsonSerializer.Serialize(data)}");
                
                await _hubContext.Clients.Group("kitchen")
                    .SendAsync("NewOrder", data);
                
                // ✅ NUEVO: También notificar al grupo de órdenes (Order/Index)
                await _hubContext.Clients.Group("orders")
                    .SendAsync("NewOrder", data);
                
                Console.WriteLine($"✅ [OrderHubService] NotifyNewOrder() - Notificación enviada exitosamente a grupos 'kitchen' y 'orders'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [OrderHubService] NotifyNewOrder() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [OrderHubService] NotifyNewOrder() - StackTrace: {ex.StackTrace}");
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
                Console.WriteLine($"🔍 [OrderHubService] NotifyOrderCompleted() - Iniciando notificación...");
                Console.WriteLine($"📋 [OrderHubService] NotifyOrderCompleted() - OrderId: {orderId}, TableNumber: {tableNumber}");
                
                var data = new { 
                    OrderId = orderId, 
                    TableNumber = tableNumber,
                    Message = $"✅ Orden completada para Mesa {tableNumber}",
                    Type = "order_completed",
                    Timestamp = DateTime.UtcNow
                };
                Console.WriteLine($"📤 [OrderHubService] NotifyOrderCompleted() - Enviando datos: {System.Text.Json.JsonSerializer.Serialize(data)}");
                
                // ✅ NUEVO: Notificar a todos los grupos relevantes
                await _hubContext.Clients.Group("kitchen")
                    .SendAsync("OrderCompleted", data);
                
                await _hubContext.Clients.Group("orders")
                    .SendAsync("OrderCompleted", data);
                
                await _hubContext.Clients.Group($"order_{orderId}")
                    .SendAsync("OrderCompleted", data);
                
                Console.WriteLine($"✅ [OrderHubService] NotifyOrderCompleted() - Notificación enviada exitosamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [OrderHubService] NotifyOrderCompleted() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [OrderHubService] NotifyOrderCompleted() - StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task NotifyTableStatusChanged(Guid tableId, string newStatus)
        {
            Console.WriteLine($"🔍 [OrderHubService] NotifyTableStatusChanged() - INICIANDO - TableId: {tableId}, Status: {newStatus}");
            try
            {
                // 🎯 LOG ESTRATÉGICO: NOTIFICACIÓN DE MESA ENVIADA
                Console.WriteLine($"🚀 [OrderHubService] NotifyTableStatusChanged() - NOTIFICACIÓN DE MESA ENVIADA - TableId: {tableId}, Status: {newStatus}");
                
                Console.WriteLine($"📋 [OrderHubService] NotifyTableStatusChanged() - Preparando datos para envío...");
                Console.WriteLine($"📋 [OrderHubService] NotifyTableStatusChanged() - TableId: {tableId}, NewStatus: {newStatus}");
                
                var message = newStatus switch
                {
                    "EnPreparacion" => $"👨‍🍳 Mesa cambió a EN PREPARACIÓN - Cocina trabajando",
                    "ParaPago" => $"💰 Mesa cambió a PARA PAGO - Lista para cobrar",
                    "Ocupada" => $"👥 Mesa cambió a OCUPADA - Clientes atendidos",
                    "Disponible" => $"✅ Mesa cambió a DISPONIBLE - Libre para nuevos clientes",
                    "Servida" => $"🍽️ Mesa cambió a SERVIDA - Pedido entregado",
                    _ => $"🔄 Mesa cambió de estado a {newStatus}"
                };

                var data = new {
                    TableId = tableId,
                    NewStatus = newStatus,
                    Message = message,
                    Type = "table_status_changed",
                    Timestamp = DateTime.UtcNow
                };
                
                Console.WriteLine($"📤 [OrderHubService] NotifyTableStatusChanged() - Enviando a grupo específico de mesa: table_{tableId}");
                // ✅ CORREGIDO: Enviar notificación a TODOS los grupos relevantes
                await _hubContext.Clients.Group($"table_{tableId}")
                    .SendAsync("TableStatusChanged", data);
                Console.WriteLine($"✅ [OrderHubService] NotifyTableStatusChanged() - Enviado a grupo table_{tableId}");
                
                Console.WriteLine($"📤 [OrderHubService] NotifyTableStatusChanged() - Enviando a grupo general de mesas: table_all");
                await _hubContext.Clients.Group("table_all")
                    .SendAsync("TableStatusChanged", data);
                Console.WriteLine($"✅ [OrderHubService] NotifyTableStatusChanged() - Enviado a grupo table_all");
                
                // ✅ NUEVO: Enviar también a grupos de órdenes y cocina
                Console.WriteLine($"📤 [OrderHubService] NotifyTableStatusChanged() - Enviando a grupo de órdenes: orders");
                await _hubContext.Clients.Group("orders")
                    .SendAsync("TableStatusChanged", data);
                Console.WriteLine($"✅ [OrderHubService] NotifyTableStatusChanged() - Enviado a grupo orders");
                
                Console.WriteLine($"📤 [OrderHubService] NotifyTableStatusChanged() - Enviando a grupo de cocina: kitchen");
                await _hubContext.Clients.Group("kitchen")
                    .SendAsync("TableStatusChanged", data);
                Console.WriteLine($"✅ [OrderHubService] NotifyTableStatusChanged() - Enviado a grupo kitchen");
                
                Console.WriteLine($"✅ [OrderHubService] NotifyTableStatusChanged() - COMPLETADO - Notificación enviada exitosamente a todos los grupos");
                Console.WriteLine($"📊 [OrderHubService] NotifyTableStatusChanged() - Grupos enviados: table_{tableId}, table_all, orders, kitchen");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [OrderHubService] NotifyTableStatusChanged() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [OrderHubService] NotifyTableStatusChanged() - StackTrace: {ex.StackTrace}");
                throw;
            }
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

        // ✅ NUEVO: Métodos para notificar cambios de stock
        public async Task NotifyStockUpdated(Guid productId, string productName, decimal newStock)
        {
            await _hubContext.Clients.Group("stock_updates")
                .SendAsync("StockUpdated", new {
                    ProductId = productId,
                    ProductName = productName,
                    NewStock = newStock,
                    Timestamp = DateTime.UtcNow // ✅ Fecha específica de notificación
                });
        }

        public async Task NotifyStockReduced(Guid productId, string productName, decimal oldStock, decimal newStock, decimal quantityReduced)
        {
            await _hubContext.Clients.Group("stock_updates")
                .SendAsync("StockReduced", new {
                    ProductId = productId,
                    ProductName = productName,
                    OldStock = oldStock,
                    NewStock = newStock,
                    QuantityReduced = quantityReduced,
                    Timestamp = DateTime.UtcNow // ✅ Fecha específica de notificación
                });
        }
    }
} 