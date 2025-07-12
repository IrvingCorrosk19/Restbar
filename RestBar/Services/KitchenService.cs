using Microsoft.EntityFrameworkCore;
using RestBar.Models;
using RestBar.ViewModel;

namespace RestBar.Services
{
    public class KitchenService : IKitchenService
    {
        private readonly RestBarContext _context;
        private readonly IOrderHubService _orderHubService;

        public KitchenService(RestBarContext context, IOrderHubService orderHubService)
        {
            _context = context;
            _orderHubService = orderHubService;
        }

        public async Task<List<KitchenOrderViewModel>> GetPendingOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.Status == OrderStatus.SentToKitchen)
                .OrderBy(o => o.OpenedAt)
                .Select(o => new KitchenOrderViewModel
                {
                    OrderId = o.Id,
                    TableNumber = o.Table != null ? o.Table.TableNumber : "Sin mesa",
                    OpenedAt = o.OpenedAt,
                    OrderType = o.OrderType,
                    Notes = o.Notes,
                    Items = o.OrderItems.Select(i => new KitchenOrderItemViewModel
                    {
                        ProductName = i.Product.Name,
                        Quantity = i.Quantity,
                        Notes = i.Notes
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<List<KitchenOrderViewModel>> GetPendingOrdersByStationTypeAsync(string stationType)
        {
            if (string.IsNullOrEmpty(stationType))
            {
                return new List<KitchenOrderViewModel>();
            }

            return await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Station)
                .Where(o => o.Status == OrderStatus.SentToKitchen || o.Status == OrderStatus.Preparing)
                .OrderBy(o => o.OpenedAt)
                .Select(o => new KitchenOrderViewModel
                {
                    OrderId = o.Id,
                    TableNumber = o.Table != null ? o.Table.TableNumber : "Sin mesa",
                    OpenedAt = o.OpenedAt,
                    OrderType = o.OrderType,
                    Notes = o.Notes,
                    OrderStatus = o.Status.ToString(),
                    Items = o.OrderItems
                        .Where(i => i.Product != null && i.Product.Station != null && 
                                   i.Product.Station.Type.ToLower() == stationType.ToLower() &&
                                   (i.KitchenStatus == KitchenStatus.Pending || i.KitchenStatus == KitchenStatus.Sent))
                        .Select(i => new KitchenOrderItemViewModel
                        {
                            ItemId = i.Id,
                            ProductName = i.Product.Name,
                            Quantity = i.Quantity,
                            Notes = i.Notes,
                            Status = i.Status.ToString(),
                            KitchenStatus = i.KitchenStatus.ToString(),
                            StationName = i.Product.Station.Name
                        }).ToList(),
                    ReadyItemsCount = o.OrderItems.Count(oi => oi.KitchenStatus == KitchenStatus.Ready),
                    TotalItemsCount = o.OrderItems.Count,
                    TotalItems = o.OrderItems.Count,
                    PendingItems = o.OrderItems.Count(oi => oi.KitchenStatus == KitchenStatus.Pending),
                    ReadyItems = o.OrderItems.Count(oi => oi.KitchenStatus == KitchenStatus.Ready),
                    PreparingItems = o.OrderItems.Count(oi => oi.KitchenStatus == KitchenStatus.Sent)
                })
                .Where(vm => vm.Items.Any())
                .ToListAsync();
        }

        public async Task MarkOrderAsReadyAsync(Guid orderId)
        {
            Console.WriteLine($"[KitchenService] MarkOrderAsReadyAsync iniciado - orderId: {orderId}");
            
            var order = await _context.Orders
                .Include(o => o.Table)
                .FirstOrDefaultAsync(o => o.Id == orderId);
                
            if (order == null)
            {
                Console.WriteLine($"[KitchenService] Error: Orden no encontrada con ID {orderId}");
                throw new KeyNotFoundException($"No se encontró la orden con ID {orderId}");
            }

            Console.WriteLine($"[KitchenService] Orden encontrada - Status actual: {order.Status}");
            Console.WriteLine($"[KitchenService] TableId: {order.TableId}");

            order.Status = OrderStatus.Ready;
            order.ClosedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            
            // Validación de desarrollo para asegurar que las fechas sean UTC
            if (order.ClosedAt.HasValue && order.ClosedAt.Value.Kind == DateTimeKind.Unspecified)
                throw new InvalidOperationException("ClosedAt no debe ser Unspecified para columnas timestamp with time zone");
            
            Console.WriteLine($"[KitchenService] Status actualizado a: {order.Status}");
            Console.WriteLine($"[KitchenService] ClosedAt establecido a: {order.ClosedAt}");
            
            // Actualizar el estado de la mesa si existe
            if (order.Table != null)
            {
                Console.WriteLine($"[KitchenService] Mesa encontrada - Status actual: {order.Table.Status}");
                
                // Verificar si hay otras órdenes pendientes para esta mesa
                var pendingOrdersForTable = await _context.Orders
                    .Where(o => o.TableId == order.TableId && 
                               o.Id != orderId && 
                               (o.Status == OrderStatus.SentToKitchen || o.Status == OrderStatus.Preparing))
                    .CountAsync();
                
                Console.WriteLine($"[KitchenService] Órdenes pendientes para la mesa: {pendingOrdersForTable}");
                
                // Si no hay más órdenes pendientes, cambiar el estado de la mesa
                if (pendingOrdersForTable == 0)
                {
                    order.Table.Status = TableStatus.ParaPago.ToString();
                    Console.WriteLine($"[KitchenService] Estado de mesa actualizado a: {order.Table.Status}");
                }
            }
            
            Console.WriteLine($"[KitchenService] Guardando cambios en la base de datos...");
            await _context.SaveChangesAsync();
            Console.WriteLine($"[KitchenService] Cambios guardados exitosamente");
        }

        // Nuevo método para marcar items específicos como listos por estación
        public async Task MarkItemsAsReadyByStationAsync(Guid orderId, string stationType)
        {
            Console.WriteLine($"[KitchenService] MarkItemsAsReadyByStationAsync iniciado");
            Console.WriteLine($"[KitchenService] orderId: {orderId}, stationType: {stationType}");
            
            // Obtener la estación
            var station = await _context.Stations
                .FirstOrDefaultAsync(s => s.Type.ToLower() == stationType.ToLower());
                
            if (station == null)
            {
                Console.WriteLine($"[KitchenService] Error: Estación no encontrada con tipo {stationType}");
                throw new KeyNotFoundException($"No se encontró la estación con tipo {stationType}");
            }

            Console.WriteLine($"[KitchenService] Estación encontrada: {station.Name} (ID: {station.Id})");
            
            // Obtener la orden con todos sus items
            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Station)
                .FirstOrDefaultAsync(o => o.Id == orderId);
                
            if (order == null)
            {
                Console.WriteLine($"[KitchenService] Error: Orden no encontrada con ID {orderId}");
                throw new KeyNotFoundException($"No se encontró la orden con ID {orderId}");
            }

            Console.WriteLine($"[KitchenService] Orden encontrada - Status actual: {order.Status}");
            Console.WriteLine($"[KitchenService] Total de items en la orden: {order.OrderItems.Count}");
            
            // Log detallado de todos los items
            foreach (var item in order.OrderItems)
            {
                Console.WriteLine($"[KitchenService] Item: ID={item.Id}, Product={item.Product?.Name}, Status={item.Status}, Station={item.Product?.Station?.Type}");
            }
            
            // Marcar como listos solo los items de esta estación
            var itemsToMark = order.OrderItems
                .Where(oi => oi.Product != null && 
                            oi.Product.Station != null && 
                            oi.Product.Station.Type.ToLower() == stationType.ToLower() &&
                            oi.Status == OrderItemStatus.Pending)
                .ToList();

            Console.WriteLine($"[KitchenService] Items a marcar como listos: {itemsToMark.Count}");
            
            // Log detallado de los items que se van a marcar
            foreach (var item in itemsToMark)
            {
                Console.WriteLine($"[KitchenService] Item a marcar: ID={item.Id}, Product={item.Product?.Name}, Status={item.Status}");
            }
            
            foreach (var item in itemsToMark)
            {
                item.Status = OrderItemStatus.Ready;
                item.PreparedByStationId = station.Id;
                item.PreparedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                
                // Validación de desarrollo para asegurar que las fechas sean UTC
                if (item.PreparedAt.HasValue && item.PreparedAt.Value.Kind == DateTimeKind.Unspecified)
                    throw new InvalidOperationException("PreparedAt no debe ser Unspecified para columnas timestamp with time zone");
                
                Console.WriteLine($"[KitchenService] Item marcado como listo: {item.Product?.Name}");
            }

            // Verificar si todos los items de la orden están listos
            var allItems = order.OrderItems.ToList();
            var readyItems = allItems.Where(oi => oi.Status == OrderItemStatus.Ready).Count();
            var totalItems = allItems.Count;

            Console.WriteLine($"[KitchenService] Items listos: {readyItems}/{totalItems}");

            // Si todos los items están listos, marcar la orden como lista
            if (readyItems == totalItems && totalItems > 0)
            {
                Console.WriteLine($"[KitchenService] Todos los items están listos, marcando orden como lista");
                order.Status = OrderStatus.Ready;
                order.ClosedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                
                // Validación de desarrollo para asegurar que las fechas sean UTC
                if (order.ClosedAt.HasValue && order.ClosedAt.Value.Kind == DateTimeKind.Unspecified)
                    throw new InvalidOperationException("ClosedAt no debe ser Unspecified para columnas timestamp with time zone");
                
                // Actualizar el estado de la mesa usando el método auxiliar
                await UpdateTableStatusIfAllItemsReadyAsync(order);
            }
            else
            {
                Console.WriteLine($"[KitchenService] No todos los items están listos, orden permanece en estado actual: {order.Status}");
                
                // Si la orden estaba en Pending y ahora tiene items listos, cambiar a SentToKitchen
                if (order.Status == OrderStatus.Pending && readyItems > 0)
                {
                    Console.WriteLine($"[KitchenService] Orden cambiando de Pending a SentToKitchen");
                    order.Status = OrderStatus.SentToKitchen;
                }
                
                // Aún así, verificar si la mesa necesita actualización
                await UpdateTableStatusIfAllItemsReadyAsync(order);
            }
            
            Console.WriteLine($"[KitchenService] Guardando cambios en la base de datos...");
            await _context.SaveChangesAsync();
            Console.WriteLine($"[KitchenService] Cambios guardados exitosamente");

            // Notificar cambios vía SignalR
            foreach (var item in itemsToMark)
            {
                await _orderHubService.NotifyOrderItemStatusChanged(orderId, item.Id, item.Status);
            }
            await _orderHubService.NotifyOrderStatusChanged(orderId, order.Status);
            if (order.Table != null)
            {
                await _orderHubService.NotifyTableStatusChanged(order.Table.Id, order.Table.Status);
            }
            await _orderHubService.NotifyKitchenUpdate();
        }

        // Método auxiliar para verificar y actualizar el estado de la mesa
        private async Task UpdateTableStatusIfAllItemsReadyAsync(Order order)
        {
            try
            {
                Console.WriteLine($"[KitchenService] UpdateTableStatusIfAllItemsReadyAsync iniciado");
                Console.WriteLine($"[KitchenService] OrderId: {order.Id}, TableId: {order.TableId}");
                
                if (order.Table == null)
                {
                    Console.WriteLine($"[KitchenService] No hay mesa asociada a la orden");
                    return;
                }

                // Verificar si todos los items de la orden están listos
                var allItems = order.OrderItems.ToList();
                var readyItems = allItems.Where(oi => oi.Status == OrderItemStatus.Ready).Count();
                var totalItems = allItems.Count;

                Console.WriteLine($"[KitchenService] Items listos: {readyItems}/{totalItems}");

                // Si todos los items están listos y hay items en la orden
                if (readyItems == totalItems && totalItems > 0)
                {
                    Console.WriteLine($"[KitchenService] Todos los items están listos, verificando otras órdenes de la mesa");
                    
                    // Verificar si hay otras órdenes pendientes para esta mesa
                    var pendingOrdersForTable = await _context.Orders
                        .Where(o => o.TableId == order.TableId && 
                                   o.Id != order.Id && 
                                   (o.Status == OrderStatus.SentToKitchen || o.Status == OrderStatus.Preparing))
                        .CountAsync();
                    
                    Console.WriteLine($"[KitchenService] Órdenes pendientes para la mesa: {pendingOrdersForTable}");
                    
                    // Si no hay más órdenes pendientes, cambiar el estado de la mesa
                    if (pendingOrdersForTable == 0)
                    {
                        order.Table.Status = TableStatus.ParaPago.ToString();
                        Console.WriteLine($"[KitchenService] Estado de mesa actualizado a: {order.Table.Status}");
                    }
                    else
                    {
                        Console.WriteLine($"[KitchenService] Hay órdenes pendientes, manteniendo estado actual de la mesa");
                    }
                }
                else
                {
                    Console.WriteLine($"[KitchenService] No todos los items están listos, manteniendo estado actual de la mesa");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[KitchenService] ERROR en UpdateTableStatusIfAllItemsReadyAsync: {ex.Message}");
                throw;
            }
        }

        // Método para marcar un item específico como listo
        public async Task MarkSpecificItemAsReadyAsync(Guid orderId, Guid itemId)
        {
            Console.WriteLine($"[KitchenService] MarkSpecificItemAsReadyAsync iniciado");
            Console.WriteLine($"[KitchenService] orderId: {orderId}, itemId: {itemId}");
            
            // Obtener la orden con todos sus items
            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Station)
                .FirstOrDefaultAsync(o => o.Id == orderId);
                
            if (order == null)
            {
                Console.WriteLine($"[KitchenService] Error: Orden no encontrada con ID {orderId}");
                throw new KeyNotFoundException($"No se encontró la orden con ID {orderId}");
            }

            Console.WriteLine($"[KitchenService] Orden encontrada - Status actual: {order.Status}");
            
            // Buscar el item específico
            var itemToMark = order.OrderItems.FirstOrDefault(oi => oi.Id == itemId);
            
            if (itemToMark == null)
            {
                Console.WriteLine($"[KitchenService] Error: Item no encontrado con ID {itemId}");
                throw new KeyNotFoundException($"No se encontró el item con ID {itemId}");
            }

            Console.WriteLine($"[KitchenService] Item encontrado: {itemToMark.Product?.Name}, Status actual: {itemToMark.Status}");
            
            // Verificar que el item esté en estado Pending
            if (itemToMark.Status != OrderItemStatus.Pending)
            {
                Console.WriteLine($"[KitchenService] Error: Item no está en estado Pending, estado actual: {itemToMark.Status}");
                throw new InvalidOperationException($"El item no está en estado Pending, estado actual: {itemToMark.Status}");
            }

            // Obtener la estación del producto
            var station = itemToMark.Product?.Station;
            if (station == null)
            {
                Console.WriteLine($"[KitchenService] Error: El producto no tiene estación asignada");
                throw new InvalidOperationException("El producto no tiene estación asignada");
            }

            Console.WriteLine($"[KitchenService] Estación del producto: {station.Name} (ID: {station.Id})");
            
            // Marcar el item como listo
            itemToMark.Status = OrderItemStatus.Ready;
            itemToMark.PreparedByStationId = station.Id;
            itemToMark.PreparedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            
            // Validación de desarrollo para asegurar que las fechas sean UTC
            if (itemToMark.PreparedAt.HasValue && itemToMark.PreparedAt.Value.Kind == DateTimeKind.Unspecified)
                throw new InvalidOperationException("PreparedAt no debe ser Unspecified para columnas timestamp with time zone");
            
            Console.WriteLine($"[KitchenService] Item marcado como listo: {itemToMark.Product?.Name}");

            // Verificar si todos los items de la orden están listos
            var allItems = order.OrderItems.ToList();
            var readyItems = allItems.Where(oi => oi.Status == OrderItemStatus.Ready).Count();
            var totalItems = allItems.Count;

            Console.WriteLine($"[KitchenService] Items listos: {readyItems}/{totalItems}");

            // Si todos los items están listos, marcar la orden como lista
            if (readyItems == totalItems && totalItems > 0)
            {
                Console.WriteLine($"[KitchenService] Todos los items están listos, marcando orden como lista");
                order.Status = OrderStatus.Ready;
                order.ClosedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                
                // Validación de desarrollo para asegurar que las fechas sean UTC
                if (order.ClosedAt.HasValue && order.ClosedAt.Value.Kind == DateTimeKind.Unspecified)
                    throw new InvalidOperationException("ClosedAt no debe ser Unspecified para columnas timestamp with time zone");
                
                // Actualizar el estado de la mesa usando el método auxiliar
                await UpdateTableStatusIfAllItemsReadyAsync(order);
            }
            else
            {
                Console.WriteLine($"[KitchenService] No todos los items están listos, orden permanece en estado actual: {order.Status}");
                
                // Si la orden estaba en Pending y ahora tiene items listos, cambiar a SentToKitchen
                if (order.Status == OrderStatus.Pending && readyItems > 0)
                {
                    Console.WriteLine($"[KitchenService] Orden cambiando de Pending a SentToKitchen");
                    order.Status = OrderStatus.SentToKitchen;
                }
                
                // Aún así, verificar si la mesa necesita actualización
                await UpdateTableStatusIfAllItemsReadyAsync(order);
            }
            
            Console.WriteLine($"[KitchenService] Guardando cambios en la base de datos...");
            await _context.SaveChangesAsync();
            Console.WriteLine($"[KitchenService] Cambios guardados exitosamente");
        }
    }
} 