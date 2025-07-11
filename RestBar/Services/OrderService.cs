using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using RestBar.ViewModel;
using RestBar.Services;
using System.Text.Json;

namespace RestBar.Services
{
    public class OrderService : IOrderService
    {
        private readonly RestBarContext _context;
        private readonly IProductService _productService;
        private readonly IOrderHubService _orderHubService;

        public OrderService(RestBarContext context, IProductService productService, IOrderHubService orderHubService)
        {
            _context = context;
            _productService = productService;
            _orderHubService = orderHubService;
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(Guid id)
        {
            return await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.Customer)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order> CreateAsync(Order order)
        {
            order.OpenedAt = DateTime.UtcNow;
            
            // Validación de desarrollo para asegurar que las fechas sean UTC
            if (order.OpenedAt.HasValue && order.OpenedAt.Value.Kind == DateTimeKind.Unspecified)
                throw new InvalidOperationException("OpenedAt no debe ser Unspecified para columnas timestamp with time zone");
            
            order.Status = OrderStatus.Pending;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Si la orden es nueva y order.Table es null, cargar la mesa asociada
            if (order.Table == null && order.TableId != Guid.Empty)
            {
                order.Table = await _context.Tables.FindAsync(order.TableId);
            }

            // Actualizar el estado de la mesa según los ítems de la orden
            if (order.Table != null)
            {
                var hasPendingOrPreparing = order.OrderItems.Any(oi =>
                    oi.Status == OrderItemStatus.Pending || oi.Status == OrderItemStatus.Preparing);

                if (hasPendingOrPreparing)
                {
                    order.Table.Status = TableStatus.EnPreparacion.ToString();
                }
                else if (order.OrderItems.All(oi => oi.Status == OrderItemStatus.Ready))
                {
                    order.Table.Status = TableStatus.ParaPago.ToString();
                }
                else
                {
                    order.Table.Status = TableStatus.Ocupada.ToString();
                }
                await _context.SaveChangesAsync();
            }
            return order;
        }

        public async Task UpdateAsync(Order order)
        {
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Order>> GetByTableIdAsync(Guid tableId)
        {
            return await _context.Orders
                .Where(o => o.TableId == tableId)
                .Include(o => o.Customer)
                .Include(o => o.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId)
        {
            return await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.Table)
                .Include(o => o.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Table)
                .Include(o => o.Customer)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)
        {
            return await _context.Orders
                .Where(o => o.Status == status)
                .Include(o => o.Table)
                .Include(o => o.Customer)
                .Include(o => o.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOpenOrdersAsync()
        {
            return await _context.Orders
                .Where(o => o.Status == OrderStatus.Pending)
                .Include(o => o.Table)
                .Include(o => o.Customer)
                .Include(o => o.User)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderWithDetailsAsync(Guid id)
        {
            Console.WriteLine($"[OrderService] GetOrderWithDetailsAsync iniciado - orderId: {id}");
            
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Table)
                    .Include(o => o.Customer)
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);
                
                Console.WriteLine($"[OrderService] GetOrderWithDetailsAsync resultado: {(order != null ? "SÍ" : "NO")}");
                if (order != null)
                {
                    Console.WriteLine($"[OrderService] Orden encontrada - Status: {order.Status}, Items: {order.OrderItems?.Count ?? 0}");
                }
                
                return order;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderService] Exception en GetOrderWithDetailsAsync: {ex.Message}");
                Console.WriteLine($"[OrderService] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<Order?> GetOrderWithPaymentsAsync(Guid id)
        {
            return await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<decimal> CalculateOrderTotalAsync(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return 0;

            decimal total = order.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice);
            order.TotalAmount = total;
            await _context.SaveChangesAsync();
            return total;
        }

        public async Task CloseOrderAsync(Guid id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.Status = OrderStatus.Completed;
                order.ClosedAt = DateTime.UtcNow;
                
                // Validación de desarrollo para asegurar que las fechas sean UTC
                if (order.ClosedAt.HasValue && order.ClosedAt.Value.Kind == DateTimeKind.Unspecified)
                    throw new InvalidOperationException("ClosedAt no debe ser Unspecified para columnas timestamp with time zone");
                
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Order> SendToKitchenAsync(SendOrderDto dto, Guid? userId)
        {
            Console.WriteLine($"[OrderService] SendToKitchenAsync iniciado");
            Console.WriteLine($"[OrderService] TableId: {dto.TableId}, OrderType: {dto.OrderType}, Items: {dto.Items?.Count ?? 0}");
            
            // Crear o actualizar orden (ya establece SentToKitchen)
            var order = await AddOrUpdateOrderWithPendingItemsAsync(dto, userId);
            Console.WriteLine($"[OrderService] Orden procesada - Status: {order.Status}");
            
            // Enviar items pendientes a cocina
            await SendPendingItemsToKitchenAsync(order.Id);
            
            // Recargar la orden para obtener el estado final
            var finalOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == order.Id);
            
            Console.WriteLine($"[OrderService] Estado final de orden: {finalOrder?.Status}");
            Console.WriteLine($"[OrderService] SendToKitchenAsync completado");
            
            return finalOrder ?? order;
        }

        public async Task<List<KitchenOrderViewModel>> GetKitchenOrdersAsync()
        {
            // Traer pedidos abiertos con sus items y productos y mesa
            var orders = await _context.Orders
                .Where(o => o.Status == OrderStatus.SentToKitchen || 
                           o.Status == OrderStatus.Preparing || 
                           o.Status == OrderStatus.Ready)
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Station)
                .ToListAsync();

            // Solo los items de cocina
            var kitchenOrders = orders
                .Select(order => {
                    var allKitchenItems = order.OrderItems
                        .Where(oi => oi.Product != null && oi.Product.Station != null && oi.Product.Station.Type.ToLower() == "cocina")
                        .ToList();
                    
                    var pendingItems = allKitchenItems
                        .Where(oi => oi.Status == OrderItemStatus.Pending)
                        .ToList();
                    
                    var readyItems = allKitchenItems
                        .Where(oi => oi.Status == OrderItemStatus.Ready)
                        .ToList();
                    
                    var preparingItems = allKitchenItems
                        .Where(oi => oi.Status == OrderItemStatus.Preparing)
                        .ToList();
                    
                    return new KitchenOrderViewModel
                    {
                        OrderId = order.Id,
                        TableNumber = order.Table != null ? order.Table.TableNumber : "Delivery",
                        OpenedAt = order.OpenedAt,
                        // Mostrar solo items pendientes para cocina
                        Items = pendingItems
                            .Select(oi => new KitchenOrderItemViewModel
                            {
                                ProductName = oi.Product.Name,
                                Quantity = oi.Quantity,
                                Notes = oi.Notes
                            }).ToList(),
                        // Información adicional sobre el estado de la orden
                        TotalItems = allKitchenItems.Count,
                        PendingItems = pendingItems.Count,
                        ReadyItems = readyItems.Count,
                        PreparingItems = preparingItems.Count,
                        Notes = order.OrderItems
                            .Where(oi => !string.IsNullOrWhiteSpace(oi.Notes))
                            .Select(oi => oi.Notes)
                            .FirstOrDefault()
                    };
                })
                .Where(k => k.Items.Any()) // Solo mostrar órdenes con items pendientes
                .OrderByDescending(k => k.OpenedAt)
                .ToList();

            return kitchenOrders;
        }

        public async Task<bool> OrderExistsAsync(Guid id)
        {
            return await _context.Orders.AnyAsync(e => e.Id == id);
        }

        public async Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(Guid orderId)
        {
            Console.WriteLine($"[OrderService] GetOrderItemsByOrderIdAsync iniciado - orderId: {orderId}");
            
            try
            {
                var items = await _context.OrderItems
                    .Include(oi => oi.Product)
                    .Include(oi => oi.PreparedByStation)
                    .Where(oi => oi.OrderId == orderId)
                    .ToListAsync();
                
                Console.WriteLine($"[OrderService] GetOrderItemsByOrderIdAsync resultado: {items.Count} items");
                return items;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderService] Exception en GetOrderItemsByOrderIdAsync: {ex.Message}");
                throw;
            }
        }

        // Obtener orden activa por mesa (que no esté cancelada o completada)
        public async Task<Order?> GetActiveOrderByTableAsync(Guid tableId)
        {
            Console.WriteLine($"[OrderService] GetActiveOrderByTableAsync iniciado - TableId: {tableId}");
            
            // Primero, buscar todas las órdenes para esta mesa para diagnosticar
            var allOrdersForTable = await _context.Orders
                .Where(o => o.TableId == tableId)
                .ToListAsync();
            
            Console.WriteLine($"[OrderService] Total de órdenes encontradas para la mesa: {allOrdersForTable.Count}");
            foreach (var orderInfo in allOrdersForTable)
            {
                Console.WriteLine($"[OrderService] Orden ID: {orderInfo.Id}, Status: {orderInfo.Status}, Items: {orderInfo.OrderItems?.Count ?? 0}");
            }
            
            // Filtrar órdenes que NO estén canceladas ni completadas
            var activeOrders = allOrdersForTable.Where(o => o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Completed).ToList();
            Console.WriteLine($"[OrderService] Órdenes activas (no canceladas ni completadas): {activeOrders.Count}");
            foreach (var activeOrder in activeOrders)
            {
                Console.WriteLine($"[OrderService] Orden activa - ID: {activeOrder.Id}, Status: {activeOrder.Status}");
            }
            
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.PreparedByStation)
                .FirstOrDefaultAsync(o => o.TableId == tableId && 
                    o.Status != OrderStatus.Cancelled && 
                    o.Status != OrderStatus.Completed);

            Console.WriteLine($"[OrderService] Orden activa encontrada: {(order != null ? "SÍ" : "NO")}");
            if (order != null)
            {
                Console.WriteLine($"[OrderService] Orden activa - ID: {order.Id}, Status: {order.Status}, Items: {order.OrderItems.Count}");
            }

            if (order != null)
            {
                Console.WriteLine($"[OrderService] Items originales (sin agrupar): {order.OrderItems.Count}");
                foreach (var item in order.OrderItems)
                {
                    Console.WriteLine($"[OrderService] Item individual: ID={item.Id}, ProductId={item.ProductId}, ProductName={item.Product?.Name}, Quantity={item.Quantity}, Status={item.Status}, KitchenStatus={item.KitchenStatus}, Station={item.PreparedByStation?.Name}, PreparedAt={item.PreparedAt}");
                }

                // ✅ NO AGRUPAR: Mantener cada OrderItem individual
                // Cada item mantiene su propia cantidad y estado
                Console.WriteLine($"[OrderService] Items mantenidos individualmente: {order.OrderItems.Count}");
                
                // Recalcular el total de la orden con todos los items individuales
                order.TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice);
            }

            return order;
        }

        // Agregar items a una orden existente
        public async Task<Order> AddItemsToOrderAsync(Guid orderId, List<OrderItemDto> items)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new KeyNotFoundException($"No se encontró la orden con ID {orderId}");

            // Verificar que la orden no esté cancelada o completada
            if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Completed)
                throw new InvalidOperationException("No se pueden agregar items a una orden cancelada o completada");

            // Guardar el estado anterior de la orden para logging
            var previousStatus = order.Status;
            Console.WriteLine($"[OrderService] AddItemsToOrderAsync - Estado anterior de la orden: {previousStatus}");

            foreach (var itemDto in items)
            {
                var product = await _productService.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                    continue;

                if (product.Price == null || product.Price <= 0)
                    throw new InvalidOperationException($"El producto '{product.Name}' no tiene precio configurado.");

                if (product.Stock != null && product.Stock <= 0)
                    throw new InvalidOperationException($"El producto '{product.Name}' está agotado.");

                if (!string.IsNullOrEmpty(itemDto.Notes) && itemDto.Notes.Length > 200)
                    throw new InvalidOperationException("El comentario no puede superar los 200 caracteres.");

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price,
                    Discount = itemDto.Discount ?? 0,
                    Notes = itemDto.Notes
                });
            }

            // Cambiar el estado de la orden a Pending cuando se agreguen nuevos items
            // Esto indica que hay nuevos items pendientes de envío a cocina
            order.Status = OrderStatus.Pending;
            Console.WriteLine($"[OrderService] AddItemsToOrderAsync - Estado de la orden cambiado de {previousStatus} a {order.Status}");

            await _context.SaveChangesAsync();

            order.TotalAmount = order.OrderItems.Sum(oi => (oi.Quantity * oi.UnitPrice) - (oi.Discount ?? 0));

            // Notificar a cocina y a los clientes
            await _orderHubService.NotifyOrderStatusChanged(order.Id, order.Status);
            await _orderHubService.NotifyKitchenUpdate();

            return order;
        }

        // Eliminar item específico de una orden
        public async Task<Order> RemoveItemFromOrderAsync(Guid orderId, Guid productId, string? status = null, Guid? itemId = null)
        {
            try
            {
                Console.WriteLine($"[OrderService] RemoveItemFromOrderAsync iniciado - OrderId: {orderId}, ProductId: {productId}, Status: {status}");
                
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.Table) // Incluir la mesa para poder actualizar su estado
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                Console.WriteLine($"[OrderService] Orden encontrada: {(order != null ? "SÍ" : "NO")}");

                if (order == null)
                {
                    Console.WriteLine($"[OrderService] ERROR: No se encontró la orden con ID {orderId}");
                    throw new KeyNotFoundException($"No se encontró la orden con ID {orderId}");
                }

                Console.WriteLine($"[OrderService] Estado de la orden: {order.Status}");
                Console.WriteLine($"[OrderService] Cantidad de items en la orden: {order.OrderItems.Count}");
                Console.WriteLine($"[OrderService] Mesa asociada: {(order.Table != null ? $"ID={order.Table.Id}, Estado={order.Table.Status}" : "NO")}");

                // Verificar que la orden no esté cancelada o completada
                if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Completed)
                {
                    Console.WriteLine($"[OrderService] ERROR: Orden cancelada o completada - Estado: {order.Status}");
                    throw new InvalidOperationException("No se pueden eliminar items de una orden cancelada o completada");
                }

                // Buscar el item específico por ItemId (preferido) o por ProductId y Status
                OrderItem? itemToRemove = null;
                
                if (itemId.HasValue && itemId.Value != Guid.Empty)
                {
                    // Si tenemos el ItemId específico, usarlo para búsqueda precisa
                    itemToRemove = order.OrderItems.FirstOrDefault(oi => oi.Id == itemId.Value);
                    Console.WriteLine($"[OrderService] Buscando por ItemId específico: {itemId.Value}");
                }
                
                if (itemToRemove == null)
                {
                    // Fallback: buscar por ProductId y Status
                    var itemsWithSameProductAndStatus = order.OrderItems.Where(oi => 
                        oi.ProductId == productId && 
                        (status == null || oi.Status.ToString() == status)
                    ).ToList();
                    
                    Console.WriteLine($"[OrderService] Items encontrados con ProductId {productId} y Status {status}: {itemsWithSameProductAndStatus.Count}");
                    
                    if (itemsWithSameProductAndStatus.Count > 1)
                    {
                        Console.WriteLine($"[OrderService] ⚠️ ADVERTENCIA: Múltiples items encontrados con mismo ProductId y Status");
                        foreach (var item in itemsWithSameProductAndStatus)
                        {
                            Console.WriteLine($"[OrderService]   - Item ID: {item.Id}, Quantity: {item.Quantity}, Created: {item.Id}");
                        }
                        Console.WriteLine($"[OrderService] ❌ ERROR: No se puede eliminar item específico sin ItemId");
                        throw new InvalidOperationException($"Se encontraron múltiples items con ProductId {productId} y Status {status}. Se requiere ItemId específico para eliminación.");
                    }
                    
                    itemToRemove = itemsWithSameProductAndStatus.FirstOrDefault();
                    Console.WriteLine($"[OrderService] Buscando por ProductId y Status como fallback");
                }
                
                // Log detallado de todos los items del mismo producto para debugging
                var allItemsWithSameProduct = order.OrderItems.Where(oi => oi.ProductId == productId).ToList();
                Console.WriteLine($"[OrderService] Items encontrados con ProductId {productId}: {allItemsWithSameProduct.Count}");
                foreach (var item in allItemsWithSameProduct)
                {
                    Console.WriteLine($"[OrderService]   - Item ID: {item.Id}, Status: {item.Status}, Quantity: {item.Quantity}");
                }
                
                Console.WriteLine($"[OrderService] Item a eliminar encontrado: {(itemToRemove != null ? "SÍ" : "NO")}");
                
                if (itemToRemove != null)
                {
                    Console.WriteLine($"[OrderService] Detalles del item: ProductId={itemToRemove.ProductId}, Status={itemToRemove.Status}, Quantity={itemToRemove.Quantity}, UnitPrice={itemToRemove.UnitPrice}");
                }

                if (itemToRemove == null)
                {
                    Console.WriteLine($"[OrderService] ERROR: No se encontró el producto con ID {productId} y status {status} en la orden");
                    throw new KeyNotFoundException($"No se encontró el producto con ID {productId} y status {status} en la orden");
                }

                Console.WriteLine($"[OrderService] Intentando eliminar item del contexto...");
                
                // Eliminar el item del contexto
                _context.OrderItems.Remove(itemToRemove);
                
                Console.WriteLine($"[OrderService] Item removido del contexto, guardando cambios...");
                await _context.SaveChangesAsync();
                
                // Recargar la orden con todos los items (por si el contexto no está actualizado)
                await _context.Entry(order).Collection(o => o.OrderItems).LoadAsync();
                
                Console.WriteLine($"[OrderService] Cambios guardados exitosamente");
                Console.WriteLine($"[OrderService] Items restantes en la orden: {order.OrderItems.Count}");
                
                // Verificar si la orden quedó vacía después de eliminar el item
                if (order.OrderItems.Count == 0)
                {
                    Console.WriteLine($"[OrderService] La orden quedó vacía, eliminando orden completa...");
                    
                    // Actualizar estado de la mesa si existe
                    if (order.Table != null)
                    {
                        Console.WriteLine($"[OrderService] Actualizando estado de la mesa a Disponible...");
                        Console.WriteLine($"[OrderService] Estado anterior de la mesa: {order.Table.Status}");
                        order.Table.Status = TableStatus.Disponible.ToString();
                        Console.WriteLine($"[OrderService] Nuevo estado de la mesa: {order.Table.Status}");
                    }
                    else
                    {
                        Console.WriteLine($"[OrderService] WARNING: No se encontró mesa asociada a la orden");
                    }
                    
                    // Eliminar la orden completa
                    _context.Orders.Remove(order);
                    await _context.SaveChangesAsync();
                    
                    Console.WriteLine($"[OrderService] Orden eliminada completamente");
                    
                    // Retornar null para indicar que la orden fue eliminada
                    return null;
                }
                
                // Notificar a cocina y a los clientes
                await _orderHubService.NotifyOrderStatusChanged(order.Id, order.Status);
                await _orderHubService.NotifyKitchenUpdate();
                return order;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderService] ERROR en RemoveItemFromOrderAsync: {ex.Message}");
                Console.WriteLine($"[OrderService] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        // ✅ NUEVO: Actualizar cantidad de un item específico por ItemId
        public async Task<Order> UpdateItemQuantityByIdAsync(Guid orderId, Guid itemId, decimal newQuantity)
        {
            try
            {
                Console.WriteLine($"[OrderService] UpdateItemQuantityByIdAsync iniciado - OrderId: {orderId}, ItemId: {itemId}, NewQuantity: {newQuantity}");
                
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.Table)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    throw new KeyNotFoundException($"No se encontró la orden con ID {orderId}");

                Console.WriteLine($"[OrderService] Mesa asociada: {(order.Table != null ? $"ID={order.Table.Id}, Estado={order.Table.Status}" : "NO")}");

                // Verificar que la orden no esté cancelada o completada
                if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Completed)
                    throw new InvalidOperationException("No se pueden modificar items de una orden cancelada o completada");

                // ✅ Buscar el item específico por ItemId
                var itemToUpdate = order.OrderItems.FirstOrDefault(oi => oi.Id == itemId);
                
                if (itemToUpdate == null)
                {
                    var availableItemIds = order.OrderItems.Select(oi => oi.Id).ToList();
                    throw new KeyNotFoundException($"No se encontró el item con ID {itemId} en la orden. Items disponibles: {string.Join(", ", availableItemIds)}");
                }

                Console.WriteLine($"[OrderService] Item encontrado: {itemToUpdate.Product?.Name}, Cantidad actual: {itemToUpdate.Quantity}");

                if (newQuantity <= 0)
                {
                    Console.WriteLine($"[OrderService] Cantidad <= 0, eliminando item...");
                    // Si la cantidad es 0 o menor, eliminar el item
                    _context.OrderItems.Remove(itemToUpdate);
                    
                    await _context.SaveChangesAsync();
                    
                    Console.WriteLine($"[OrderService] Item eliminado, items restantes: {order.OrderItems.Count}");
                    
                    // Verificar si la orden quedó vacía
                    if (order.OrderItems.Count == 0)
                    {
                        Console.WriteLine($"[OrderService] La orden quedó vacía, eliminando orden completa...");
                        
                        // Actualizar estado de la mesa si existe
                        if (order.Table != null)
                        {
                            Console.WriteLine($"[OrderService] Actualizando estado de la mesa a Disponible...");
                            order.Table.Status = TableStatus.Disponible.ToString();
                        }
                        
                        // Eliminar la orden completa
                        _context.Orders.Remove(order);
                        await _context.SaveChangesAsync();
                        
                        Console.WriteLine($"[OrderService] Orden eliminada completamente");
                        
                        // Retornar null para indicar que la orden fue eliminada
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine($"[OrderService] Actualizando cantidad de {itemToUpdate.Quantity} a {newQuantity}");
                    // Actualizar la cantidad
                    itemToUpdate.Quantity = newQuantity;
                    await _context.SaveChangesAsync();
                    
                    Console.WriteLine($"[OrderService] ✅ Cantidad actualizada exitosamente");
                }

                // Notificar a cocina y a los clientes
                await _orderHubService.NotifyOrderStatusChanged(order.Id, order.Status);
                await _orderHubService.NotifyKitchenUpdate();
                return order;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderService] ERROR en UpdateItemQuantityByIdAsync: {ex.Message}");
                Console.WriteLine($"[OrderService] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<Order> UpdateItemQuantityAsync(Guid orderId, Guid productId, decimal newQuantity, string? status = null)
        {
            try
            {
                Console.WriteLine($"[OrderService] UpdateItemQuantityAsync iniciado - OrderId: {orderId}, ProductId: {productId}, NewQuantity: {newQuantity}, Status: {status}");
                
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.Table) // Incluir la mesa para poder actualizar su estado
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    throw new KeyNotFoundException($"No se encontró la orden con ID {orderId}");

                Console.WriteLine($"[OrderService] Mesa asociada: {(order.Table != null ? $"ID={order.Table.Id}, Estado={order.Table.Status}" : "NO")}");

                // Verificar que la orden no esté cancelada o completada
                if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Completed)
                    throw new InvalidOperationException("No se pueden modificar items de una orden cancelada o completada");

                // Buscar el item específico por ProductId y Status
                var itemToUpdate = order.OrderItems.FirstOrDefault(oi => 
                    oi.ProductId == productId && 
                    (string.IsNullOrEmpty(status) || oi.Status.ToString() == status)
                );
                
                if (itemToUpdate == null)
                {
                    var availableItems = order.OrderItems.Where(oi => oi.ProductId == productId).ToList();
                    var statuses = availableItems.Select(oi => oi.Status.ToString()).Distinct();
                    throw new KeyNotFoundException($"No se encontró el producto con ID {productId} y status '{status}' en la orden. Status disponibles: {string.Join(", ", statuses)}");
                }

                if (newQuantity <= 0)
                {
                    Console.WriteLine($"[OrderService] Cantidad <= 0, eliminando item...");
                    // Si la cantidad es 0 o menor, eliminar el item
                    _context.OrderItems.Remove(itemToUpdate);
                    
                    await _context.SaveChangesAsync();
                    
                    Console.WriteLine($"[OrderService] Item eliminado, items restantes: {order.OrderItems.Count}");
                    
                    // Verificar si la orden quedó vacía
                    if (order.OrderItems.Count == 0)
                    {
                        Console.WriteLine($"[OrderService] La orden quedó vacía, eliminando orden completa...");
                        
                        // Actualizar estado de la mesa si existe
                        if (order.Table != null)
                        {
                            Console.WriteLine($"[OrderService] Actualizando estado de la mesa a Disponible...");
                            Console.WriteLine($"[OrderService] Estado anterior de la mesa: {order.Table.Status}");
                            order.Table.Status = TableStatus.Disponible.ToString();
                            Console.WriteLine($"[OrderService] Nuevo estado de la mesa: {order.Table.Status}");
                        }
                        else
                        {
                            Console.WriteLine($"[OrderService] WARNING: No se encontró mesa asociada a la orden");
                        }
                        
                        // Eliminar la orden completa
                        _context.Orders.Remove(order);
                        await _context.SaveChangesAsync();
                        
                        Console.WriteLine($"[OrderService] Orden eliminada completamente");
                        
                        // Retornar null para indicar que la orden fue eliminada
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine($"[OrderService] Actualizando cantidad a: {newQuantity}");
                    // Actualizar la cantidad
                    itemToUpdate.Quantity = newQuantity;
                    await _context.SaveChangesAsync();
                }

                // Notificar a cocina y a los clientes
                await _orderHubService.NotifyOrderStatusChanged(order.Id, order.Status);
                await _orderHubService.NotifyKitchenUpdate();
                return order;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderService] ERROR en UpdateItemQuantityAsync: {ex.Message}");
                Console.WriteLine($"[OrderService] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        // Actualizar item completo (cantidad y notas) en una orden
        public async Task<Order> UpdateItemAsync(Guid orderId, Guid productId, decimal newQuantity, string? notes, string? status = null)
        {
            try
            {
                Console.WriteLine($"[OrderService] UpdateItemAsync iniciado - OrderId: {orderId}, ProductId: {productId}, NewQuantity: {newQuantity}, Notes: {notes}, Status: {status}");
                
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.Table) // Incluir la mesa para poder actualizar su estado
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    throw new KeyNotFoundException($"No se encontró la orden con ID {orderId}");

                Console.WriteLine($"[OrderService] Mesa asociada: {(order.Table != null ? $"ID={order.Table.Id}, Estado={order.Table.Status}" : "NO")}");

                // Verificar que la orden no esté cancelada o completada
                if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Completed)
                    throw new InvalidOperationException("No se pueden modificar items de una orden cancelada o completada");

                // Buscar el item específico por ProductId y Status
                var itemToUpdate = order.OrderItems.FirstOrDefault(oi => 
                    oi.ProductId == productId && 
                    (status == null || oi.Status.ToString() == status)
                );
                
                if (itemToUpdate == null)
                    throw new KeyNotFoundException($"No se encontró el producto con ID {productId} y status {status} en la orden");

                if (newQuantity <= 0)
                {
                    Console.WriteLine($"[OrderService] Cantidad <= 0, eliminando item...");
                    // Si la cantidad es 0 o menor, eliminar el item
                    _context.OrderItems.Remove(itemToUpdate);
                    
                    await _context.SaveChangesAsync();
                    
                    Console.WriteLine($"[OrderService] Item eliminado, items restantes: {order.OrderItems.Count}");
                    
                    // Verificar si la orden quedó vacía
                    if (order.OrderItems.Count == 0)
                    {
                        Console.WriteLine($"[OrderService] La orden quedó vacía, eliminando orden completa...");
                        
                        // Actualizar estado de la mesa si existe
                        if (order.Table != null)
                        {
                            Console.WriteLine($"[OrderService] Actualizando estado de la mesa a Disponible...");
                            Console.WriteLine($"[OrderService] Estado anterior de la mesa: {order.Table.Status}");
                            order.Table.Status = TableStatus.Disponible.ToString();
                            Console.WriteLine($"[OrderService] Nuevo estado de la mesa: {order.Table.Status}");
                        }
                        else
                        {
                            Console.WriteLine($"[OrderService] WARNING: No se encontró mesa asociada a la orden");
                        }
                        
                        // Eliminar la orden completa
                        _context.Orders.Remove(order);
                        await _context.SaveChangesAsync();
                        
                        Console.WriteLine($"[OrderService] Orden eliminada completamente");
                        
                        // Retornar null para indicar que la orden fue eliminada
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine($"[OrderService] Actualizando cantidad a: {newQuantity} y notas a: {notes}");
                    // Actualizar la cantidad y notas
                    itemToUpdate.Quantity = newQuantity;
                    itemToUpdate.Notes = notes;
                    await _context.SaveChangesAsync();
                }

                return order;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderService] ERROR en UpdateItemAsync: {ex.Message}");
                Console.WriteLine($"[OrderService] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        // Cancelar orden
        public async Task CancelOrderAsync(Guid orderId, Guid? userId, string? reason = null, Guid? supervisorId = null)
        {
            try
            {
                Console.WriteLine($"[OrderService] CancelOrderAsync iniciado");
                Console.WriteLine($"[OrderService] orderId: {orderId}, userId: {userId}, reason: {reason}, supervisorId: {supervisorId}");
                
                var order = await _context.Orders
                    .Include(o => o.Table)
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    Console.WriteLine($"[OrderService] ERROR: Orden no encontrada con ID {orderId}");
                    throw new KeyNotFoundException($"No se encontró la orden con ID {orderId}");
                }

                Console.WriteLine($"[OrderService] Orden encontrada - Status actual: {order.Status}");
                Console.WriteLine($"[OrderService] Mesa asociada: {(order.Table != null ? $"ID={order.Table.Id}, Estado={order.Table.Status}" : "NO")}");

                // Marcar la orden como cancelada
                order.Status = OrderStatus.Cancelled;
                order.ClosedAt = DateTime.UtcNow;
                
                // Validación de desarrollo para asegurar que las fechas sean UTC
                if (order.ClosedAt.HasValue && order.ClosedAt.Value.Kind == DateTimeKind.Unspecified)
                    throw new InvalidOperationException("ClosedAt no debe ser Unspecified para columnas timestamp with time zone");

                // Crear log de cancelación
                var cancellationLog = new OrderCancellationLog
                {
                    OrderId = orderId,
                    UserId = userId,
                    SupervisorId = supervisorId,
                    Reason = reason ?? "Cancelación por usuario",
                    Date = DateTime.UtcNow,
                    Products = string.Join(", ", order.OrderItems.Select(oi => oi.Product?.Name ?? "Producto desconocido"))
                };

                _context.OrderCancellationLogs.Add(cancellationLog);

                // Actualizar el estado de la mesa si existe
                if (order.Table != null)
                {
                    Console.WriteLine($"[OrderService] Verificando si hay otras órdenes activas para la mesa {order.Table.Id}");
                    
                    // Verificar si hay otras órdenes activas para esta mesa
                    var activeOrdersForTable = await _context.Orders
                        .Where(o => o.TableId == order.TableId && 
                                   o.Id != orderId && 
                                   (o.Status == OrderStatus.Pending || 
                                    o.Status == OrderStatus.SentToKitchen || 
                                    o.Status == OrderStatus.Preparing ||
                                    o.Status == OrderStatus.Ready))
                        .CountAsync();
                    
                    Console.WriteLine($"[OrderService] Órdenes activas para la mesa: {activeOrdersForTable}");
                    
                    // Si no hay más órdenes activas, cambiar el estado de la mesa a disponible
                    if (activeOrdersForTable == 0)
                    {
                        order.Table.Status = TableStatus.Disponible.ToString();
                        Console.WriteLine($"[OrderService] Estado de mesa actualizado a: {order.Table.Status}");
                    }
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"[OrderService] Orden cancelada exitosamente");

                // Notificar cambios vía SignalR
                await _orderHubService.NotifyOrderCancelled(orderId);
                await _orderHubService.NotifyOrderStatusChanged(orderId, order.Status);
                if (order.Table != null)
                {
                    await _orderHubService.NotifyTableStatusChanged(order.Table.Id, order.Table.Status);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderService] ERROR en CancelOrderAsync: {ex.Message}");
                Console.WriteLine($"[OrderService] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        // Método para verificar y actualizar el estado de la mesa cuando todos los items estén listos
        public async Task CheckAndUpdateTableStatusAsync(Guid orderId)
        {
            try
            {
                Console.WriteLine($"[OrderService] CheckAndUpdateTableStatusAsync iniciado - orderId: {orderId}");
                
                var order = await _context.Orders
                    .Include(o => o.Table)
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    Console.WriteLine($"[OrderService] ERROR: Orden no encontrada con ID {orderId}");
                    return;
                }

                if (order.Table == null)
                {
                    Console.WriteLine($"[OrderService] No hay mesa asociada a la orden");
                    return;
                }

                // Verificar si todos los items de la orden están listos
                var allItems = order.OrderItems.ToList();
                var readyItems = allItems.Where(oi => oi.Status == OrderItemStatus.Ready).Count();
                var totalItems = allItems.Count;

                Console.WriteLine($"[OrderService] Items listos: {readyItems}/{totalItems}");

                // Si todos los items están listos y hay items en la orden
                if (readyItems == totalItems && totalItems > 0)
                {
                    Console.WriteLine($"[OrderService] Todos los items están listos, verificando otras órdenes de la mesa");
                    
                    // Verificar si hay otras órdenes pendientes para esta mesa
                    var pendingOrdersForTable = await _context.Orders
                        .Where(o => o.TableId == order.TableId && 
                                   o.Id != orderId && 
                                   (o.Status == OrderStatus.SentToKitchen || 
                                    o.Status == OrderStatus.Preparing ||
                                    o.Status == OrderStatus.Ready))
                        .CountAsync();
                    
                    Console.WriteLine($"[OrderService] Órdenes pendientes para la mesa: {pendingOrdersForTable}");
                    
                    // Si no hay más órdenes pendientes, cambiar el estado de la mesa
                    if (pendingOrdersForTable == 0)
                    {
                        order.Table.Status = TableStatus.ParaPago.ToString();
                        Console.WriteLine($"[OrderService] Estado de mesa actualizado a: {order.Table.Status}");
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        Console.WriteLine($"[OrderService] Hay órdenes pendientes, manteniendo estado actual de la mesa");
                    }
                }
                else
                {
                    Console.WriteLine($"[OrderService] No todos los items están listos, manteniendo estado actual de la mesa");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderService] ERROR en CheckAndUpdateTableStatusAsync: {ex.Message}");
                Console.WriteLine($"[OrderService] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        // Actualizar orden completa - eliminar todos los items existentes y crear los nuevos
        public async Task<Order> UpdateOrderCompleteAsync(Guid orderId, List<UpdateOrderItemDto> items)
        {
            try
            {
                Console.WriteLine($"[OrderService] UpdateOrderCompleteAsync iniciado");
                Console.WriteLine($"[OrderService] orderId: {orderId}, items count: {items?.Count}");
                
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.Table)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    Console.WriteLine($"[OrderService] ERROR: Orden no encontrada con ID {orderId}");
                    throw new KeyNotFoundException($"No se encontró la orden con ID {orderId}");
                }

                Console.WriteLine($"[OrderService] Orden encontrada - Status actual: {order.Status}");
                Console.WriteLine($"[OrderService] Items existentes: {order.OrderItems.Count}");

                // Eliminar todos los items existentes
                Console.WriteLine($"[OrderService] Eliminando todos los items existentes...");
                _context.OrderItems.RemoveRange(order.OrderItems);
                await _context.SaveChangesAsync();
                Console.WriteLine($"[OrderService] Items existentes eliminados");

                // Crear los nuevos items
                Console.WriteLine($"[OrderService] Creando nuevos items...");
                decimal totalAmount = 0;

                foreach (var itemDto in items)
                {
                    var product = await _productService.GetByIdAsync(itemDto.ProductId);
                    if (product == null)
                    {
                        Console.WriteLine($"[OrderService] WARNING: Producto no encontrado con ID {itemDto.ProductId}");
                        continue;
                    }

                    var newItem = new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = orderId,
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = product.Price,
                        Discount = 0,
                        Notes = itemDto.Notes,
                        Status = OrderItemStatus.Pending,
                        PreparedAt = null,
                        PreparedByStationId = null
                    };

                    _context.OrderItems.Add(newItem);
                    totalAmount += newItem.Quantity * newItem.UnitPrice;
                    
                    Console.WriteLine($"[OrderService] Item creado: {product.Name} x {itemDto.Quantity} = ${newItem.Quantity * newItem.UnitPrice}");
                }

                // Actualizar el total de la orden
                order.TotalAmount = totalAmount;
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"[OrderService] Orden actualizada - Total: ${totalAmount}");
                Console.WriteLine($"[OrderService] Nuevos items creados: {items.Count}");

                // Notificar cambios vía SignalR
                await _orderHubService.NotifyOrderStatusChanged(orderId, order.Status);
                if (order.Table != null)
                {
                    await _orderHubService.NotifyTableStatusChanged(order.Table.Id, order.Table.Status);
                }

                // Actualizar el estado de la mesa según los ítems de la orden
                if (order.Table != null)
                {
                    var hasPendingOrPreparing = order.OrderItems.Any(oi =>
                        oi.Status == OrderItemStatus.Pending || oi.Status == OrderItemStatus.Preparing);

                    if (hasPendingOrPreparing)
                    {
                        order.Table.Status = TableStatus.EnPreparacion.ToString();
                    }
                    else if (order.OrderItems.All(oi => oi.Status == OrderItemStatus.Ready))
                    {
                        order.Table.Status = TableStatus.ParaPago.ToString();
                    }
                    else
                    {
                        order.Table.Status = TableStatus.Ocupada.ToString();
                    }
                    await _context.SaveChangesAsync();
                }

                return order;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderService] ERROR en UpdateOrderCompleteAsync: {ex.Message}");
                Console.WriteLine($"[OrderService] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        // NUEVO: Crear o actualizar orden, solo agrega nuevos ítems con KitchenStatus=Pending
        public async Task<Order> AddOrUpdateOrderWithPendingItemsAsync(SendOrderDto dto, Guid? userId)
        {
            // Buscar orden activa
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.TableId == dto.TableId &&
                    (o.Status == OrderStatus.SentToKitchen || o.Status == OrderStatus.ReadyToPay || o.Status == OrderStatus.Preparing));

            if (order == null)
            {
                // Crear nueva orden
                Console.WriteLine($"[OrderService] Creando nueva orden en estado SentToKitchen");
                order = new Order
                {
                    Id = Guid.NewGuid(),
                    TableId = dto.TableId,
                    UserId = userId,
                    OrderType = (OrderType)Enum.Parse(typeof(OrderType), dto.OrderType),
                    Status = OrderStatus.SentToKitchen,  // ✅ Estado inicial garantizado
                    OpenedAt = DateTime.UtcNow,
                    TotalAmount = 0
                };
                _context.Orders.Add(order);
                Console.WriteLine($"[OrderService] Nueva orden creada con ID: {order.Id}");
            }
            else
            {
                // ✅ LÓGICA MEJORADA: Asegurar que la orden esté en SentToKitchen
                Console.WriteLine($"[OrderService] Orden existente encontrada - Status actual: {order.Status}");
                
                if (order.Status == OrderStatus.ReadyToPay || order.Status == OrderStatus.Ready)
                {
                    Console.WriteLine($"[OrderService] Orden existente en estado {order.Status}, cambiando a SentToKitchen por nuevos items");
                    order.Status = OrderStatus.SentToKitchen;
                }
                else if (order.Status == OrderStatus.Preparing)
                {
                    // Si ya está en preparación, mantener el estado
                    Console.WriteLine($"[OrderService] Orden existente en estado {order.Status}, manteniendo estado actual");
                }
                else if (order.Status == OrderStatus.Pending)
                {
                    // Si está pendiente, cambiar a SentToKitchen
                    Console.WriteLine($"[OrderService] Orden existente en estado {order.Status}, cambiando a SentToKitchen");
                    order.Status = OrderStatus.SentToKitchen;
                }
                else
                {
                    // Para cualquier otro estado, asegurar que esté en SentToKitchen
                    Console.WriteLine($"[OrderService] Orden existente en estado {order.Status}, cambiando a SentToKitchen");
                    order.Status = OrderStatus.SentToKitchen;
                }
                
                Console.WriteLine($"[OrderService] Estado final de orden: {order.Status}");
            }

            decimal total = 0;
            
            // ✅ NO AGRUPAR: Procesar cada item individualmente
            Console.WriteLine($"[OrderService] Procesando {dto.Items.Count} items individualmente");
            
            // ✅ Verificar items duplicados en el DTO
            var duplicateIds = dto.Items.GroupBy(i => i.Id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicateIds.Any())
            {
                Console.WriteLine($"[OrderService] ⚠️ ADVERTENCIA: IDs duplicados en DTO: {string.Join(", ", duplicateIds)}");
            }
            
            foreach (var itemDto in dto.Items)
            {
                Console.WriteLine($"[OrderService] Procesando item: ProductId={itemDto.ProductId}, Quantity={itemDto.Quantity}, Status={itemDto.Status}, DTO_ID={itemDto.Id}");
                
                var product = await _productService.GetByIdAsync(itemDto.ProductId);
                if (product == null) 
                {
                    Console.WriteLine($"[OrderService] Producto no encontrado: {itemDto.ProductId}");
                    continue;
                }

                // ✅ Verificar si ya existe un item con el mismo ID en la base de datos
                var existingItem = await _context.OrderItems.FindAsync(itemDto.Id);
                if (existingItem != null)
                {
                    Console.WriteLine($"[OrderService] ⚠️ ADVERTENCIA: Ya existe un item con ID {itemDto.Id} en la base de datos, saltando...");
                    continue;
                }
                
                // ✅ Verificar si el item ya está siendo trackeado en el contexto actual
                var trackedItem = _context.ChangeTracker.Entries<OrderItem>()
                    .Where(e => e.Entity.Id == itemDto.Id)
                    .FirstOrDefault();
                    
                if (trackedItem != null)
                {
                    Console.WriteLine($"[OrderService] ⚠️ ADVERTENCIA: Item con ID {itemDto.Id} ya está siendo trackeado en el contexto, saltando...");
                    continue;
                }

                // ✅ Crear un OrderItem individual para cada item del DTO
                var newItem = new OrderItem
                {
                    Id = itemDto.Id != Guid.Empty ? itemDto.Id : Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,  // ✅ Cantidad individual del item
                    UnitPrice = product.Price,
                    Discount = itemDto.Discount ?? 0,
                    Notes = itemDto.Notes,
                    KitchenStatus = KitchenStatus.Pending,
                    Status = !string.IsNullOrEmpty(itemDto.Status)
                        ? Enum.Parse<OrderItemStatus>(itemDto.Status, ignoreCase: true)
                        : OrderItemStatus.Pending
                };
                
                Console.WriteLine($"[OrderService] Intentando agregar item con ID: {newItem.Id}");
                Console.WriteLine($"[OrderService] Item individual creado: {product.Name} x {itemDto.Quantity} = ${newItem.Quantity * newItem.UnitPrice}");
                
                try
                {
                    _context.OrderItems.Add(newItem);
                    Console.WriteLine($"[OrderService] ✅ Item agregado exitosamente al contexto");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[OrderService] ❌ ERROR al agregar item: {ex.Message}");
                    Console.WriteLine($"[OrderService] Stack trace: {ex.StackTrace}");
                    throw;
                }
                
                total += (newItem.Quantity * newItem.UnitPrice) - (newItem.Discount ?? 0);
            }
            order.TotalAmount += total;
            await _context.SaveChangesAsync();

            // Si la orden es nueva y order.Table es null, cargar la mesa asociada
            if (order.Table == null && order.TableId != Guid.Empty)
            {
                order.Table = await _context.Tables.FindAsync(order.TableId);
            }

            // Actualizar el estado de la mesa según los ítems de la orden
            if (order.Table != null)
            {
                var hasPendingOrPreparing = order.OrderItems.Any(oi =>
                    oi.Status == OrderItemStatus.Pending || oi.Status == OrderItemStatus.Preparing);

                if (hasPendingOrPreparing)
                {
                    order.Table.Status = TableStatus.EnPreparacion.ToString();
                }
                else if (order.OrderItems.All(oi => oi.Status == OrderItemStatus.Ready))
                {
                    order.Table.Status = TableStatus.ParaPago.ToString();
                }
                else
                {
                    order.Table.Status = TableStatus.Ocupada.ToString();
                }
                await _context.SaveChangesAsync();
                
                // Notificar cambio de estado de mesa vía SignalR
                await _orderHubService.NotifyTableStatusChanged(order.Table.Id, order.Table.Status);
                Console.WriteLine($"[OrderService] Notificación de mesa enviada: {order.Table.Status}");
            }
            return order;
        }

        // NUEVO: Enviar a cocina solo los ítems Pending, marcarlos como Sent y notificar
        public async Task<List<OrderItem>> SendPendingItemsToKitchenAsync(Guid orderId)
        {
            Console.WriteLine($"[OrderService] SendPendingItemsToKitchenAsync iniciado - orderId: {orderId}");
            
            var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) throw new Exception("Orden no encontrada");
            
            Console.WriteLine($"[OrderService] Orden encontrada - Status actual: {order.Status}");
            
            // Asegurar que la orden esté en estado SentToKitchen
            if (order.Status != OrderStatus.SentToKitchen)
            {
                Console.WriteLine($"[OrderService] Cambiando estado de orden de {order.Status} a SentToKitchen");
                order.Status = OrderStatus.SentToKitchen;
            }
            
            var pendingItems = order.OrderItems.Where(oi => oi.KitchenStatus == KitchenStatus.Pending).ToList();
            Console.WriteLine($"[OrderService] Items pendientes encontrados: {pendingItems.Count}");
            
            foreach (var item in pendingItems)
            {
                item.KitchenStatus = KitchenStatus.Sent;
                item.SentAt = DateTime.UtcNow;
                Console.WriteLine($"[OrderService] Item {item.Product?.Name} marcado como enviado a cocina");
            }
            
            await _context.SaveChangesAsync();
            Console.WriteLine($"[OrderService] Cambios guardados en base de datos");
            
            // Notificar a cocina vía SignalR
            foreach (var item in pendingItems)
            {
                await _orderHubService.NotifyOrderItemStatusChanged(order.Id, item.Id, OrderItemStatus.Preparing);
            }
            
            // Notificar cambio de estado de la orden
            await _orderHubService.NotifyOrderStatusChanged(order.Id, order.Status);
            await _orderHubService.NotifyKitchenUpdate();
            
            Console.WriteLine($"[OrderService] Notificaciones SignalR enviadas");
            Console.WriteLine($"[OrderService] SendPendingItemsToKitchenAsync completado - Orden en estado: {order.Status}");
            
            return pendingItems;
        }

        // NUEVO: Marcar ítem como listo
        public async Task MarkItemAsReadyAsync(Guid orderId, Guid itemId)
        {
            try
            {
                Console.WriteLine($"[OrderService] MarkItemAsReadyAsync iniciado");
                Console.WriteLine($"[OrderService] orderId: {orderId}, itemId: {itemId}");
                
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.Table)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderId);
                
                if (order == null) 
                {
                    Console.WriteLine($"[OrderService] ERROR: Orden no encontrada con ID {orderId}");
                    throw new Exception("Orden no encontrada");
                }
                
                var item = order.OrderItems.FirstOrDefault(oi => oi.Id == itemId);
                if (item == null) 
                {
                    Console.WriteLine($"[OrderService] ERROR: Item no encontrado con ID {itemId}");
                    throw new Exception("Item no encontrado");
                }
                
                Console.WriteLine($"[OrderService] Orden encontrada - Status actual: {order.Status}");
                Console.WriteLine($"[OrderService] Item a marcar: {item.Product?.Name}, KitchenStatus actual: {item.KitchenStatus}");
                Console.WriteLine($"[OrderService] Mesa asociada: {(order.Table != null ? $"ID={order.Table.Id}, Estado={order.Table.Status}" : "NO")}");
                
                // Marcar el item como listo
                item.KitchenStatus = KitchenStatus.Ready;
                item.Status = OrderItemStatus.Ready;
                item.PreparedAt = DateTime.UtcNow;
                
                Console.WriteLine($"[OrderService] Item marcado como listo: {item.Product?.Name}");
                
                // Verificar si todos los items de la orden están listos
                var allItems = order.OrderItems.ToList();
                var readyItems = allItems.Where(oi => oi.KitchenStatus == KitchenStatus.Ready).Count();
                var totalItems = allItems.Count;
                
                Console.WriteLine($"[OrderService] Items listos: {readyItems}/{totalItems}");
                
                // Si todos los items están listos, cambiar el estado de la orden
                if (readyItems == totalItems && totalItems > 0)
                {
                    Console.WriteLine($"[OrderService] Todos los items están listos, cambiando estado de orden a ReadyToPay");
                    order.Status = OrderStatus.ReadyToPay;
                }
                
                await _context.SaveChangesAsync();
                Console.WriteLine($"[OrderService] Cambios guardados en base de datos");
                
                // Verificar y actualizar el estado de la mesa
                await CheckAndUpdateTableStatusAsync(orderId);
                
                // Notificar cambios vía SignalR
                Console.WriteLine($"[OrderService] Enviando notificaciones SignalR...");
                
                // Notificación detallada del item actualizado
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                await _orderHubService.NotifyOrderItemUpdated(
                    order.Id, 
                    item.Id, 
                    item.ProductId ?? Guid.Empty,
                    item.Product?.Name ?? "Producto desconocido", 
                    "Ready", 
                    timestamp
                );
                
                await _orderHubService.NotifyOrderItemStatusChanged(order.Id, item.Id, OrderItemStatus.Ready);
                await _orderHubService.NotifyOrderStatusChanged(order.Id, order.Status);
                
                if (order.Table != null)
                {
                    await _orderHubService.NotifyTableStatusChanged(order.Table.Id, order.Table.Status);
                    Console.WriteLine($"[OrderService] Notificación de mesa enviada: {order.Table.Status}");
                }
                
                Console.WriteLine($"[OrderService] MarkItemAsReadyAsync completado exitosamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderService] ERROR en MarkItemAsReadyAsync: {ex.Message}");
                Console.WriteLine($"[OrderService] Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
} 