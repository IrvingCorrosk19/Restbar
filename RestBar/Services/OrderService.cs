using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using RestBar.ViewModel;
using RestBar.Services;
using System.Text.Json;

namespace RestBar.Services
{
    public class OrderService : BaseTrackingService, IOrderService
    {
        private readonly IProductService _productService;
        private readonly IOrderHubService _orderHubService;
        private readonly IUserAssignmentService _userAssignmentService;
        private readonly IGlobalLoggingService _loggingService;
        private readonly IInventoryOperationsService _inventoryOps;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            RestBarContext context,
            IProductService productService,
            IOrderHubService orderHubService,
            IUserAssignmentService userAssignmentService,
            IGlobalLoggingService loggingService,
            IInventoryOperationsService inventoryOps,
            IHttpContextAccessor httpContextAccessor,
            ILogger<OrderService> logger)
            : base(context, httpContextAccessor)
        {
            _productService = productService;
            _orderHubService = orderHubService;
            _userAssignmentService = userAssignmentService;
            _loggingService = loggingService;
            _inventoryOps = inventoryOps;
            _logger = logger;
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
            order.OpenedAt = DateTime.UtcNow; // ✅ Fecha específica de apertura de orden
            
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
                var hasPendingItems = order.OrderItems.Any(oi => oi.Status == OrderItemStatus.Pending);
                var hasPreparingItems = order.OrderItems.Any(oi => oi.Status == OrderItemStatus.Preparing);
                var hasReadyItems = order.OrderItems.Any(oi => oi.Status == OrderItemStatus.Ready);
                var allItemsReady = order.OrderItems.All(oi => oi.Status == OrderItemStatus.Ready);

                Console.WriteLine($"[OrderService] Estado de items - Pending: {hasPendingItems}, Preparing: {hasPreparingItems}, Ready: {hasReadyItems}, AllReady: {allItemsReady}");

                if (hasPreparingItems || hasPendingItems)
                {
                    // 🎯 LOG ESTRATÉGICO: MESA EN PREPARACIÓN
                    Console.WriteLine($"🚀 [OrderService] UpdateAsync() - MESA EN PREPARACIÓN - Mesa {order.Table.TableNumber}");
                    order.Table.Status = TableStatus.EnPreparacion;
                    Console.WriteLine($"[OrderService] Mesa {order.Table.TableNumber} cambió a EN PREPARACIÓN");
                }
                else if (allItemsReady && order.OrderItems.Any())
                {
                    // 🎯 LOG ESTRATÉGICO: MESA PARA PAGO
                    Console.WriteLine($"🚀 [OrderService] UpdateAsync() - MESA PARA PAGO - Mesa {order.Table.TableNumber}");
                    order.Table.Status = TableStatus.ParaPago;
                    Console.WriteLine($"[OrderService] Mesa {order.Table.TableNumber} cambió a PARA PAGO");
                }
                else if (hasReadyItems)
                {
                    order.Table.Status = TableStatus.Servida;
                    Console.WriteLine($"[OrderService] Mesa {order.Table.TableNumber} cambió a SERVIDA");
                }
                else
                {
                    order.Table.Status = TableStatus.Ocupada;
                    Console.WriteLine($"[OrderService] Mesa {order.Table.TableNumber} cambió a OCUPADA");
                }
                await _context.SaveChangesAsync();
            }
            return order;
        }

        public async Task UpdateAsync(Order order)
        {
            try
            {
                Console.WriteLine($"🔍 [OrderService] UpdateAsync() - Actualizando orden: {order.OrderNumber} (ID: {order.Id})");
                
                // Buscar si hay una entidad con el mismo ID siendo rastreada
                var existingEntity = _context.ChangeTracker.Entries<Order>()
                    .FirstOrDefault(e => e.Entity.Id == order.Id);

                if (existingEntity != null)
                {
                    // Detach la entidad existente para evitar conflictos
                    existingEntity.State = EntityState.Detached;
                }

                // ✅ Usar SetUpdatedTracking para establecer campos de auditoría de actualización
                SetUpdatedTracking(order);
                
                Console.WriteLine($"✅ [OrderService] UpdateAsync() - Campos actualizados: UpdatedBy={order.UpdatedBy}, UpdatedAt={order.UpdatedAt}");

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"✅ [OrderService] UpdateAsync() - Orden actualizada exitosamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [OrderService] UpdateAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [OrderService] UpdateAsync() - StackTrace: {ex.StackTrace}");
                throw new ApplicationException("Error al actualizar la orden en la base de datos.", ex);
            }
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
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.PreparedByStation)
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
                order.ClosedAt = DateTime.UtcNow; // ✅ Fecha específica de cierre de orden
                
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
            
            // ✅ NUEVO: Notificar nueva orden a cocina
            var table = await _context.Tables.FindAsync(order.TableId);
            if (table != null)
            {
                Console.WriteLine($"🔍 [OrderService] SendToKitchenAsync() - Enviando notificación de nueva orden a cocina");
                Console.WriteLine($"📋 [OrderService] SendToKitchenAsync() - Mesa: {table.TableNumber}, OrderId: {order.Id}");
                await _orderHubService.NotifyNewOrder(order.Id, table.TableNumber);
                Console.WriteLine($"✅ [OrderService] SendToKitchenAsync() - Notificación enviada exitosamente");
            }
            else
            {
                Console.WriteLine($"⚠️ [OrderService] SendToKitchenAsync() - No se encontró la mesa con ID: {order.TableId}");
            }
            
            // Recargar la orden para obtener el estado final
            var finalOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == order.Id);
            
            Console.WriteLine($"[OrderService] Estado final de orden: {finalOrder?.Status}");
            Console.WriteLine($"[OrderService] SendToKitchenAsync completado");

            await _loggingService.LogOrderActivityAsync(
                action: AuditAction.UPDATE.ToString(),
                description: $"Pedido enviado a cocina - Mesa {table?.TableNumber ?? "N/A"}, Items: {dto.Items?.Count ?? 0}",
                orderId: (finalOrder ?? order).Id,
                newValues: new { dto.TableId, UserId = userId, ItemCount = dto.Items?.Count ?? 0 });
            
            return finalOrder ?? order;
        }

        public async Task<List<KitchenOrderViewModel>> GetKitchenOrdersAsync(Guid? branchId = null, Guid? companyId = null, int page = 1, int pageSize = 50)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 50;
                if (pageSize > 200) pageSize = 200;

                _logger.LogInformation("[KDS] GetKitchenOrdersAsync - branchId={BranchId}, companyId={CompanyId}, page={Page}", branchId, companyId, page);

                var ordersQuery = _context.Orders
                    .AsNoTracking()
                    .Where(o => o.Status == OrderStatus.SentToKitchen ||
                               o.Status == OrderStatus.Preparing      ||
                               o.Status == OrderStatus.Ready          ||
                               o.Status == OrderStatus.ReadyToPay);

                if (branchId.HasValue)
                    ordersQuery = ordersQuery.Where(o => o.BranchId == branchId.Value);
                if (companyId.HasValue)
                    ordersQuery = ordersQuery.Where(o => o.CompanyId == companyId.Value);

                var orders = await ordersQuery
                    .OrderByDescending(o => o.Priority)
                    .ThenBy(o => o.IsVip ? 0 : 1)
                    .ThenBy(o => o.OpenedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Include(o => o.Table)
                        .ThenInclude(t => t!.Area)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.PreparedByStation)
                    .ToListAsync();

                _logger.LogInformation("[KDS] GetKitchenOrdersAsync - {Count} órdenes activas encontradas", orders.Count);

                // ─── Mapeo a ViewModel ───────────────────────────────────────────────────
                // ELIMINADO: filtro hard-coded a "cocina/kitchen/bar".
                // Ahora se incluyen ítems de CUALQUIER tipo de estación y también los
                // ítems sin estación asignada (PreparedByStationId == null).
                // El filtro por estación se realiza en StationOrders() usando los IDs
                // reales de estación obtenidos de la base de datos.
                var result = orders
                    .Select(order =>
                    {
                        // Solo ítems con producto válido y no cancelados
                        var activeItems = order.OrderItems
                            .Where(oi => oi.Product != null
                                         && oi.Status != OrderItemStatus.Cancelled)
                            .ToList();

                        // Ítems que aún no están listos (pendientes o en preparación)
                        var itemsToShow = activeItems
                            .Where(oi => oi.Status != OrderItemStatus.Ready)
                            .Select(oi => new KitchenOrderItemViewModel
                            {
                                ItemId        = oi.Id,
                                ProductName   = oi.Product!.Name,
                                Quantity      = oi.Quantity,
                                Notes         = oi.Notes,
                                Status        = oi.Status.ToString(),
                                KitchenStatus = oi.KitchenStatus.ToString(),
                                // StationId permite filtrar por ID real (sin strings mágicos)
                                StationId     = oi.PreparedByStationId,
                                StationName   = oi.PreparedByStation?.Type ?? string.Empty,
                                StationDisplayName = oi.PreparedByStation?.Name ?? string.Empty
                            })
                            .ToList();

                        if (activeItems.Count == 0)
                            _logger.LogWarning("[KDS] Orden {OrderId} no tiene ítems activos", order.Id);

                        return new KitchenOrderViewModel
                        {
                            OrderId        = order.Id,
                            TableNumber    = order.Table?.TableNumber ?? "Delivery",
                            TableAreaId    = order.Table?.AreaId,
                            TableAreaName  = order.Table?.Area?.Name,
                            BranchId       = order.BranchId,
                            OpenedAt       = order.OpenedAt,
                            Items          = itemsToShow,
                            TotalItems     = activeItems.Count,
                            PendingItems   = activeItems.Count(i => i.Status == OrderItemStatus.Pending),
                            ReadyItems     = activeItems.Count(i => i.Status == OrderItemStatus.Ready),
                            PreparingItems = activeItems.Count(i => i.Status == OrderItemStatus.Preparing),
                            Notes          = activeItems
                                .Where(oi => !string.IsNullOrWhiteSpace(oi.Notes))
                                .Select(oi => oi.Notes)
                                .FirstOrDefault()
                        };
                    })
                    .Where(k => k.Items.Any())                // Sólo órdenes con ítems pendientes
                    .OrderByDescending(k => k.OpenedAt)
                    .ToList();

                _logger.LogInformation("[KDS] GetKitchenOrdersAsync - {Count} órdenes con ítems pendientes retornadas", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[KDS] GetKitchenOrdersAsync - error inesperado");
                throw; // Re-lanza para que el caller maneje con lista vacía si corresponde
            }
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
                .Where(o => o.TableId == tableId &&
                    o.Status != OrderStatus.Cancelled &&
                    o.Status != OrderStatus.Completed &&
                    o.OrderItems.Any(oi => oi.Status != OrderItemStatus.Cancelled))
                .OrderByDescending(o => o.OpenedAt)
                .FirstOrDefaultAsync();

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
                
                // Recalcular el total de la orden con todos los items individuales (menos descuento global)
                order.TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice - oi.Discount) - order.DiscountAmount;
            }

            return order;
        }

        // ✅ MEJORADO: Agregar items a una orden existente con reducción de inventario
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



            try
            {
                foreach (var itemDto in items)
                {
                    var product = await _productService.GetByIdAsync(itemDto.ProductId);
                    if (product == null)
                        continue;

                    if (product.Price == null || product.Price <= 0)
                        throw new InvalidOperationException($"El producto '{product.Name}' no tiene precio configurado.");



                    if (!string.IsNullOrEmpty(itemDto.Notes) && itemDto.Notes.Length > 200)
                        throw new InvalidOperationException("El comentario no puede superar los 200 caracteres.");



                    var newItem = new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = product.Price,
                        Discount = itemDto.Discount ?? 0,
                        Notes = itemDto.Notes,
                        // ✅ NUEVO: Establecer campos multi-tenant desde la orden
                        CompanyId = order.CompanyId,
                        BranchId = order.BranchId
                    };
                    
                    // ✅ NUEVO: Establecer campos de auditoría usando BaseTrackingService
                    SetCreatedTracking(newItem);
                    
                    order.OrderItems.Add(newItem);
                }

                // Cambiar el estado de la orden a Pending cuando se agreguen nuevos items
                // Esto indica que hay nuevos items pendientes de envío a cocina
                order.Status = OrderStatus.Pending;
                Console.WriteLine($"[OrderService] AddItemsToOrderAsync - Estado de la orden cambiado de {previousStatus} a {order.Status}");

                // P1-FIX-04: Retry ante DbUpdateConcurrencyException (Version token puede incrementar por pagos concurrentes)
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    _logger.LogWarning("[P1-FIX-04] Concurrencia detectada en AddItemsToOrderAsync para orden {OrderId}. Reintentando...", order.Id);
                    try
                    {
                        await _context.Entry(order).ReloadAsync();
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        throw new InvalidOperationException("La orden fue modificada simultáneamente. Por favor, intente de nuevo.");
                    }
                }

                order.TotalAmount = order.OrderItems.Sum(oi => (oi.Quantity * oi.UnitPrice) - oi.Discount);

                // Notificar a cocina y a los clientes
                await _orderHubService.NotifyOrderStatusChanged(order.Id, order.Status);
                await _orderHubService.NotifyKitchenUpdate();

                return order;
            }
            catch (Exception ex)
            {
                // ✅ NUEVO: Rollback - restaurar inventario de los items que ya se procesaron
                Console.WriteLine($"[OrderService] ERROR en AddItemsToOrderAsync: {ex.Message}");
                Console.WriteLine($"[OrderService] Realizando rollback del inventario...");
                

                
                throw; // Re-lanzar la excepción original
            }
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

                // P0-FIX-01: Bloquear cancelación si totalPaid > newTotal para evitar saldo negativo
                var totalPaidForCancel = await _context.Payments
                    .Where(p => p.OrderId == orderId && !p.IsVoided)
                    .SumAsync(p => p.Amount);

                if (totalPaidForCancel > 0)
                {
                    var newTotal = order.OrderItems
                        .Where(oi => oi.Id != itemToRemove.Id)
                        .Sum(oi => oi.Quantity * oi.UnitPrice - oi.Discount);

                    if (totalPaidForCancel > newTotal + 0.01m)
                    {
                        throw new InvalidOperationException(
                            $"No se puede cancelar el ítem porque el pago registrado (${totalPaidForCancel:F2}) supera el nuevo total (${newTotal:F2}). Anule primero el pago excedente.");
                    }
                }

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
                    // P1-FIX-03: Cancelar la orden en lugar de borrarla para preservar auditoría
                    _logger.LogInformation("[P1-FIX-03] Orden {OrderId} quedó vacía. Cambiando a Cancelled en lugar de eliminar.", order.Id);

                    // Actualizar estado de la mesa si existe
                    if (order.Table != null)
                    {
                        Console.WriteLine($"[OrderService] Actualizando estado de la mesa a Disponible...");
                        order.Table.Status = TableStatus.Disponible;
                    }
                    else
                    {
                        Console.WriteLine($"[OrderService] WARNING: No se encontró mesa asociada a la orden");
                    }

                    // Cancelar orden (NO borrar) para preservar auditoría y FK references
                    order.Status = OrderStatus.Cancelled;
                    order.ClosedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    // 🔄 NOTIFICAR CAMBIO DE ESTADO DE MESA VIA SIGNALR
                    if (order.Table != null)
                    {
                        await _orderHubService.NotifyTableStatusChanged(order.Table.Id, order.Table.Status.ToString());
                    }

                    // 📡 NOTIFICAR CANCELACIÓN DE ORDEN VIA SIGNALR
                    await _orderHubService.NotifyOrderStatusChanged(order.Id, OrderStatus.Cancelled);

                    // Retornar null para indicar que la orden quedó vacía
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
                        .ThenInclude(oi => oi.Product)
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
                    
                    // Verificar si la orden quedó vacía — OBS-1: Cancelar en lugar de borrar (auditoría)
                    if (order.OrderItems.Count == 0)
                    {
                        _logger.LogInformation("[OBS-1] Orden {OrderId} quedó vacía. Cancelando (no borrar).", order.Id);
                        if (order.Table != null)
                            order.Table.Status = TableStatus.Disponible;
                        order.Status = OrderStatus.Cancelled;
                        order.ClosedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        if (order.Table != null)
                            await _orderHubService.NotifyTableStatusChanged(order.Table.Id, order.Table.Status.ToString());
                        await _orderHubService.NotifyOrderStatusChanged(order.Id, OrderStatus.Cancelled);
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine($"[OrderService] Actualizando cantidad de {itemToUpdate.Quantity} a {newQuantity}");
                    
                    // ✅ NUEVO: Actualizar campos de auditoría del item
                    SetUpdatedTracking(itemToUpdate);
                    Console.WriteLine($"[OrderService] Campos de auditoría actualizados: UpdatedBy={itemToUpdate.UpdatedBy}, UpdatedAt={itemToUpdate.UpdatedAt}");
                    
                    // Actualizar la cantidad
                    itemToUpdate.Quantity = newQuantity;
                    await _context.SaveChangesAsync();
                    
                    Console.WriteLine($"[OrderService] ✅ Cantidad actualizada exitosamente");
                    
                    // ✅ NUEVO: Recalcular TotalAmount de la orden después de actualizar cantidad
                    order.TotalAmount = order.OrderItems.Sum(oi => (oi.Quantity * oi.UnitPrice) - oi.Discount);
                    SetUpdatedTracking(order);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"[OrderService] TotalAmount recalculado: ${order.TotalAmount:F2}");
                }

                // Notificar a cocina y a los clientes
                await _orderHubService.NotifyOrderStatusChanged(order.Id, order.Status);
                
                // ✅ NUEVO: Notificar cambio específico del item
                await _orderHubService.NotifyOrderItemStatusChanged(order.Id, itemToUpdate.Id, itemToUpdate.Status);
                
                // ✅ NUEVO: Notificar cambio de estado de mesa si existe
                if (order.Table != null)
                {
                    await _orderHubService.NotifyTableStatusChanged(order.Table.Id, order.Table.Status.ToString());
                    Console.WriteLine($"[OrderService] Notificación de mesa enviada: {order.Table.Status}");
                }
                
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
                    
                    // Verificar si la orden quedó vacía — OBS-1: Cancelar en lugar de borrar (auditoría)
                    if (order.OrderItems.Count == 0)
                    {
                        _logger.LogInformation("[OBS-1] Orden {OrderId} quedó vacía. Cancelando (no borrar).", order.Id);
                        if (order.Table != null)
                            order.Table.Status = TableStatus.Disponible;
                        order.Status = OrderStatus.Cancelled;
                        order.ClosedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        if (order.Table != null)
                            await _orderHubService.NotifyTableStatusChanged(order.Table.Id, order.Table.Status.ToString());
                        await _orderHubService.NotifyOrderStatusChanged(order.Id, OrderStatus.Cancelled);
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
                    
                    // Verificar si la orden quedó vacía — OBS-1: Cancelar en lugar de borrar (auditoría)
                    if (order.OrderItems.Count == 0)
                    {
                        _logger.LogInformation("[OBS-1] Orden {OrderId} quedó vacía. Cancelando (no borrar).", order.Id);
                        if (order.Table != null)
                            order.Table.Status = TableStatus.Disponible;
                        order.Status = OrderStatus.Cancelled;
                        order.ClosedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        if (order.Table != null)
                            await _orderHubService.NotifyTableStatusChanged(order.Table.Id, order.Table.Status.ToString());
                        await _orderHubService.NotifyOrderStatusChanged(order.Id, OrderStatus.Cancelled);
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

        /// <summary>Cancelar orden. Regla: no se puede cancelar una orden ya Completed.</summary>
        public async Task CancelOrderAsync(Guid orderId, Guid? userId, string? reason = null, Guid? supervisorId = null)
        {
            if (orderId == Guid.Empty)
                throw new ArgumentException("OrderId es requerido.", nameof(orderId));

            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new KeyNotFoundException($"No se encontró la orden con ID {orderId}");

            if (order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("La orden ya está cancelada.");

            if (order.Status == OrderStatus.Completed)
                throw new InvalidOperationException("No se puede cancelar una orden ya completada y pagada.");

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var orderPayments = await _context.Payments
                        .Where(p => p.OrderId == orderId && !p.IsVoided)
                        .ToListAsync();
                    foreach (var payment in orderPayments)
                        payment.IsVoided = true;

                    order.Status = OrderStatus.Cancelled;
                    order.ClosedAt = DateTime.UtcNow;
                    order.Version++;

                    var productNames = string.Join(", ", order.OrderItems.Select(oi => oi.Product?.Name ?? "Producto"));
                    _context.OrderCancellationLogs.Add(new OrderCancellationLog
                    {
                        Id = Guid.NewGuid(),
                        OrderId = orderId,
                        UserId = userId,
                        SupervisorId = supervisorId,
                        Reason = reason ?? "Cancelación por usuario",
                        Date = DateTime.UtcNow,
                        Products = productNames
                    });

                    if (order.Table != null)
                    {
                        var activeOrdersForTable = await _context.Orders
                            .Where(o => o.TableId == order.TableId && o.Id != orderId &&
                                o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Completed)
                            .CountAsync();
                        if (activeOrdersForTable == 0)
                            order.Table.Status = TableStatus.Disponible;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            // Restaurar inventario solo de ítems no cancelados (ítems ya cancelados no tienen stock a restaurar)
            foreach (var item in order.OrderItems.Where(oi => oi.Status != OrderItemStatus.Cancelled))
            {
                if (item.ProductId == null || item.ProductId == Guid.Empty) continue;
                try
                {
                    await _inventoryOps.RestoreInventoryForCancelAsync(
                        item.ProductId.Value,
                        item.Quantity,
                        item.PreparedByStationId,
                        order.BranchId,
                        order.CompanyId,
                        orderId,
                        userId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[OrderService] RestoreStock falló para item {item.Product?.Name}: {ex.Message}");
                }
            }

            await _orderHubService.NotifyOrderCancelled(orderId);
            await _orderHubService.NotifyOrderStatusChanged(orderId, OrderStatus.Cancelled);
            if (order.Table != null)
                await _orderHubService.NotifyTableStatusChanged(order.Table.Id, order.Table.Status.ToString());

            await _loggingService.LogOrderActivityAsync(
                action: AuditAction.DELETE.ToString(),
                description: $"Orden cancelada: {reason ?? "Sin motivo"}",
                orderId: orderId,
                newValues: new
                {
                    userId,
                    supervisorId,
                    reason,
                    Products = string.Join(", ", order.OrderItems.Select(oi => oi.Product?.Name ?? "Producto"))
                });
        }

        // ✅ NUEVO: Método para marcar mesa como ocupada cuando se selecciona
        public async Task<bool> SetTableOccupiedAsync(Guid tableId)
        {
            try
            {
                Console.WriteLine($"🔍 [OrderService] SetTableOccupiedAsync() - INICIANDO - tableId: {tableId}");
                
                Console.WriteLine($"📋 [OrderService] SetTableOccupiedAsync() - Buscando mesa en base de datos...");
                var table = await _context.Tables.FindAsync(tableId);
                if (table == null)
                {
                    Console.WriteLine($"❌ [OrderService] SetTableOccupiedAsync() - ERROR: Mesa no encontrada con ID {tableId}");
                    return false;
                }
                
                Console.WriteLine($"📋 [OrderService] SetTableOccupiedAsync() - Mesa encontrada: {table.TableNumber}, Estado actual: {table.Status}");

                // Solo cambiar a ocupada si está disponible
                if (table.Status == TableStatus.Disponible)
                {
                    Console.WriteLine($"🔄 [OrderService] SetTableOccupiedAsync() - Cambiando estado de Disponible a Ocupada...");
                    table.Status = TableStatus.Ocupada;
                    
                    Console.WriteLine($"💾 [OrderService] SetTableOccupiedAsync() - Guardando cambios en base de datos...");
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"✅ [OrderService] SetTableOccupiedAsync() - Mesa {table.TableNumber} marcada como OCUPADA en BD");
                    
                    // ✅ NUEVO: Enviar notificación SignalR para sincronizar otras vistas
                    Console.WriteLine($"📡 [OrderService] SetTableOccupiedAsync() - Enviando notificación SignalR para mesa {table.TableNumber}...");
                    Console.WriteLine($"📋 [OrderService] SetTableOccupiedAsync() - TableId: {table.Id}, Status: {table.Status.ToString()}");
                    
                    await _orderHubService.NotifyTableStatusChanged(table.Id, table.Status.ToString());
                    
                    Console.WriteLine($"✅ [OrderService] SetTableOccupiedAsync() - COMPLETADO - Notificación SignalR enviada exitosamente");
                    Console.WriteLine($"📊 [OrderService] SetTableOccupiedAsync() - Mesa {table.TableNumber} ahora está OCUPADA y notificada");
                    
                    return true;
                }
                else
                {
                    Console.WriteLine($"⚠️ [OrderService] SetTableOccupiedAsync() - Mesa {table.TableNumber} ya está en estado {table.Status}, no se cambió");
                    // Idempotente: estados operativos cuentan como éxito
                    if (table.Status is TableStatus.Ocupada or TableStatus.EnPreparacion or TableStatus.ParaPago)
                        return true;
                    // Intentar liberar estados residuales y marcar ocupada
                    table.Status = TableStatus.Ocupada;
                    await _context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [OrderService] SetTableOccupiedAsync() - ERROR: {ex.Message}");
                Console.WriteLine($"🔍 [OrderService] SetTableOccupiedAsync() - StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<Order> MoveOrderToTableAsync(Guid orderId, Guid targetTableId, Guid? userId)
        {
            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId)
                ?? throw new KeyNotFoundException($"No se encontró la orden con ID {orderId}");

            if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Completed)
                throw new InvalidOperationException("No se puede mover una orden cancelada o completada");

            var oldTableId = order.TableId;
            var targetTable = await _context.Tables.FirstOrDefaultAsync(t => t.Id == targetTableId)
                ?? throw new KeyNotFoundException("Mesa destino no encontrada");

            if (!targetTable.IsActive)
                throw new InvalidOperationException("La mesa destino no está activa");

            if (order.BranchId != null && targetTable.BranchId != order.BranchId)
                throw new InvalidOperationException("La mesa destino pertenece a otra sucursal");

            if (order.CompanyId != null && targetTable.CompanyId != order.CompanyId)
                throw new InvalidOperationException("La mesa destino pertenece a otra empresa");

            if (oldTableId != targetTableId)
            {
                var hasActiveOrder = await _context.Orders
                    .Include(o => o.OrderItems)
                    .AnyAsync(o => o.TableId == targetTableId
                        && o.Id != orderId
                        && o.Status != OrderStatus.Cancelled
                        && o.Status != OrderStatus.Completed
                        && o.OrderItems.Any(oi => oi.Status != OrderItemStatus.Cancelled));

                if (hasActiveOrder)
                    throw new InvalidOperationException("La mesa destino ya tiene una orden activa");

                if (targetTable.Status != TableStatus.Disponible && targetTable.Status != TableStatus.Ocupada)
                    throw new InvalidOperationException($"La mesa destino no está disponible (estado: {targetTable.Status})");
            }

            order.TableId = targetTableId;
            targetTable.Status = TableStatus.Ocupada;
            order.UpdatedAt = DateTime.UtcNow;
            order.Version++;

            // Re-enrutar ítems pendientes según área de mesa destino
            var newAreaId = targetTable.AreaId;
            foreach (var item in order.OrderItems.Where(oi => oi.Status != OrderItemStatus.Cancelled && oi.Status != OrderItemStatus.Served && oi.ProductId.HasValue))
            {
                var bestStation = await _productService.FindBestStationForProductAsync(
                    item.ProductId!.Value, item.Quantity, order.BranchId, newAreaId);
                if (bestStation.HasValue && bestStation != item.PreparedByStationId)
                {
                    var oldStation = item.PreparedByStationId;
                    if (oldStation.HasValue && item.Product?.TrackInventory == true)
                    {
                        try
                        {
                            await _inventoryOps.TransferStockBetweenStationsAsync(
                                item.ProductId.Value, oldStation.Value, bestStation.Value,
                                item.Quantity, order.BranchId, order.CompanyId, userId, "MoveToTable");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "[MoveToTable] No se pudo transferir stock ítem {ItemId}", item.Id);
                        }
                    }
                    item.PreparedByStationId = bestStation;
                }
            }

            await _context.SaveChangesAsync();

            if (oldTableId.HasValue && oldTableId != targetTableId)
            {
                var oldTable = await _context.Tables.FindAsync(oldTableId.Value);
                if (oldTable != null)
                {
                    var hasOtherOrders = await _context.Orders.AnyAsync(o =>
                        o.TableId == oldTableId
                        && o.Status != OrderStatus.Cancelled
                        && o.Status != OrderStatus.Completed);

                    if (!hasOtherOrders)
                    {
                        oldTable.Status = TableStatus.Disponible;
                        await _context.SaveChangesAsync();
                        await _orderHubService.NotifyTableStatusChanged(oldTable.Id, oldTable.Status.ToString());
                    }
                }
            }

            await _orderHubService.NotifyTableStatusChanged(targetTable.Id, targetTable.Status.ToString());
            await _orderHubService.NotifyOrderStatusChanged(order.Id, order.Status);

            return order;
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

                // Verificar el estado actual de los items
                var allItems = order.OrderItems.ToList();
                var readyItems = allItems.Where(oi => oi.Status == OrderItemStatus.Ready).Count();
                var pendingItems = allItems.Where(oi => oi.Status == OrderItemStatus.Pending).Count();
                var preparingItems = allItems.Where(oi => oi.Status == OrderItemStatus.Preparing).Count();
                var totalItems = allItems.Count;

                Console.WriteLine($"[OrderService] Estado items - Listos: {readyItems}, Pendientes: {pendingItems}, Preparándose: {preparingItems}, Total: {totalItems}");

                // Si todos los items están listos y hay items en la orden
                if (readyItems == totalItems && totalItems > 0)
                {
                    // 🎯 LOG ESTRATÉGICO: TODOS LOS ITEMS LISTOS
                    Console.WriteLine($"🚀 [OrderService] CheckAndUpdateTableStatusAsync() - TODOS LOS ITEMS LISTOS - Verificando otras órdenes");
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
                        // 🎯 LOG ESTRATÉGICO: MESA PARA PAGO (NO HAY MÁS ÓRDENES)
                        Console.WriteLine($"🚀 [OrderService] CheckAndUpdateTableStatusAsync() - MESA PARA PAGO - Mesa {order.Table.TableNumber} - No hay más órdenes pendientes");
                        order.Table.Status = TableStatus.ParaPago;
                        Console.WriteLine($"[OrderService] Estado de mesa actualizado a: {order.Table.Status}");
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        Console.WriteLine($"[OrderService] Hay órdenes pendientes, manteniendo estado actual de la mesa");
                    }
                }
                else if (pendingItems > 0 || preparingItems > 0)
                {
                    // Si hay items pendientes o en preparación, asegurar que la mesa esté en EnPreparacion
                    if (order.Table.Status != TableStatus.EnPreparacion)
                    {
                        order.Table.Status = TableStatus.EnPreparacion;
                        Console.WriteLine($"[OrderService] Mesa {order.Table.TableNumber} cambió a EN PREPARACIÓN - Hay items pendientes/preparándose");
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        Console.WriteLine($"[OrderService] Mesa ya está en EN PREPARACIÓN");
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
                    
                    // ✅ NUEVO: Obtener CompanyId y BranchId de la orden
                    var orderForItem = await _context.Orders.FindAsync(orderId);
                    if (orderForItem != null)
                    {
                        newItem.CompanyId = orderForItem.CompanyId;
                        newItem.BranchId = orderForItem.BranchId;
                    }
                    
                    // ✅ NUEVO: Establecer campos de auditoría usando BaseTrackingService
                    SetCreatedTracking(newItem);

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
                    await _orderHubService.NotifyTableStatusChanged(order.Table.Id, order.Table.Status.ToString());
                }

                // Actualizar el estado de la mesa según los ítems de la orden
                if (order.Table != null)
                {
                    var hasPendingOrPreparing = order.OrderItems.Any(oi =>
                        oi.Status == OrderItemStatus.Pending || oi.Status == OrderItemStatus.Preparing);

                    if (hasPendingOrPreparing)
                    {
                        order.Table.Status = TableStatus.EnPreparacion;
                    }
                    else if (order.OrderItems.All(oi => oi.Status == OrderItemStatus.Ready))
                    {
                        order.Table.Status = TableStatus.ParaPago;
                    }
                    else
                    {
                        order.Table.Status = TableStatus.Ocupada;
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

        public async Task<Order> AddOrUpdateOrderWithPendingItemsAsync(SendOrderDto dto, Guid? userId)
        {
            // Buscar orden activa
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.TableId == dto.TableId &&
                    (o.Status == OrderStatus.SentToKitchen || o.Status == OrderStatus.ReadyToPay || o.Status == OrderStatus.Preparing));

            if (order == null)
            {
                // ✅ Obtener CompanyId y BranchId del usuario actual
                Guid? companyId = null;
                Guid? branchId = null;
                
                if (userId.HasValue)
                {
                    var user = await _context.Users
                        .Include(u => u.Branch)
                        .FirstOrDefaultAsync(u => u.Id == userId.Value);
                    
                    if (user != null)
                    {
                        branchId = user.BranchId;
                        companyId = user.Branch?.CompanyId;
                        Console.WriteLine($"🔍 [OrderService] Usuario encontrado - CompanyId: {companyId}, BranchId: {branchId}");
                    }
                }
                
                // ✅ Si no se obtuvo del usuario, intentar desde claims
                if (!companyId.HasValue || !branchId.HasValue)
                {
                    var httpContext = _httpContextAccessor?.HttpContext;
                    if (httpContext?.User?.Identity?.IsAuthenticated == true)
                    {
                        var companyIdClaim = httpContext.User.FindFirst("CompanyId")?.Value;
                        var branchIdClaim = httpContext.User.FindFirst("BranchId")?.Value;
                        
                        if (!string.IsNullOrEmpty(companyIdClaim) && Guid.TryParse(companyIdClaim, out var parsedCompanyId))
                            companyId = parsedCompanyId;
                        
                        if (!string.IsNullOrEmpty(branchIdClaim) && Guid.TryParse(branchIdClaim, out var parsedBranchId))
                            branchId = parsedBranchId;
                        
                        Console.WriteLine($"🔍 [OrderService] Claims obtenidos - CompanyId: {companyId}, BranchId: {branchId}");
                    }
                }
                
                // ✅ Generar OrderNumber único
                var orderNumber = await GenerateOrderNumberAsync(companyId);
                Console.WriteLine($"🔍 [OrderService] OrderNumber generado: {orderNumber}");
                
                // Crear nueva orden
                Console.WriteLine($"🔍 [OrderService] Creando nueva orden en estado SentToKitchen");
                order = new Order
                {
                    Id = Guid.NewGuid(),
                    OrderNumber = orderNumber, // ✅ OrderNumber generado
                    TableId = dto.TableId,
                    UserId = userId,
                    OrderType = (OrderType)Enum.Parse(typeof(OrderType), dto.OrderType),
                    Status = OrderStatus.SentToKitchen,  // Estado inicial garantizado
                    OpenedAt = DateTime.UtcNow, // ✅ Fecha específica de apertura de orden
                    TotalAmount = 0,
                    CompanyId = companyId, // ✅ CompanyId establecido
                    BranchId = branchId // ✅ BranchId establecido
                };
                
                // ✅ Establecer campos de auditoría
                SetCreatedTracking(order);
                
                _context.Orders.Add(order);
                Console.WriteLine($"✅ [OrderService] Nueva orden creada con ID: {order.Id}, OrderNumber: {order.OrderNumber}, CompanyId: {order.CompanyId}, BranchId: {order.BranchId}");
            }
            else
            {
                // ✅ MEJORADO: Validar estado de orden y pagos antes de agregar items
                Console.WriteLine($"[OrderService] Orden existente encontrada - Status actual: {order.Status}");
                
                // ✅ NUEVO: Verificar si la orden tiene pagos
                var totalPaid = await _context.Payments
                    .Where(p => p.OrderId == order.Id && !p.IsVoided)
                    .SumAsync(p => p.Amount);
                
                Console.WriteLine($"[OrderService] Total pagado en orden: ${totalPaid:F2}");

                // P0-FIX-02: Si la orden está completamente pagada, crear NUEVA orden para la misma mesa
                var currentOrderTotal = order.OrderItems
                    .Where(oi => oi.Status != OrderItemStatus.Cancelled)
                    .Sum(oi => oi.Quantity * oi.UnitPrice - oi.Discount);

                if (totalPaid > 0 && totalPaid >= currentOrderTotal - 0.01m)
                {
                    _logger.LogWarning("[P0-FIX-02] Orden {OrderId} completamente pagada (pagado={TotalPaid}, total={CurrentTotal}). Creando nueva orden para mesa {TableId}.",
                        order.Id, totalPaid, currentOrderTotal, dto.TableId);

                    var newOrderNumber = await GenerateOrderNumberAsync(order.CompanyId);
                    var newOrder = new Order
                    {
                        Id = Guid.NewGuid(),
                        OrderNumber = newOrderNumber,
                        TableId = dto.TableId,
                        UserId = userId,
                        OrderType = (OrderType)Enum.Parse(typeof(OrderType), dto.OrderType),
                        Status = OrderStatus.SentToKitchen,
                        OpenedAt = DateTime.UtcNow,
                        TotalAmount = 0,
                        CompanyId = order.CompanyId,
                        BranchId = order.BranchId
                    };
                    SetCreatedTracking(newOrder);
                    _context.Orders.Add(newOrder);
                    order = newOrder; // continuar el método con la nueva orden
                    _logger.LogInformation("[P0-FIX-02] Nueva orden {OrderId} (#{OrderNumber}) creada para mesa {TableId}.", order.Id, order.OrderNumber, dto.TableId);
                }
                else
                {
                // OBS-3: Órdenes en estado cerrado (Completed, Served, Cancelled) nunca se reutilizan; crear siempre nueva
                if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Served || order.Status == OrderStatus.Cancelled)
                {
                    _logger.LogWarning("[OBS-3] Orden {OrderId} en estado {Status}. Creando nueva orden para mesa {TableId}.", order.Id, order.Status, dto.TableId);
                    var newOrderNumberObs3 = await GenerateOrderNumberAsync(order.CompanyId);
                    var newOrderObs3 = new Order
                    {
                        Id = Guid.NewGuid(),
                        OrderNumber = newOrderNumberObs3,
                        TableId = dto.TableId,
                        UserId = userId,
                        OrderType = (OrderType)Enum.Parse(typeof(OrderType), dto.OrderType),
                        Status = OrderStatus.SentToKitchen,
                        OpenedAt = DateTime.UtcNow,
                        TotalAmount = 0,
                        CompanyId = order.CompanyId,
                        BranchId = order.BranchId
                    };
                    SetCreatedTracking(newOrderObs3);
                    _context.Orders.Add(newOrderObs3);
                    order = newOrderObs3;
                    _logger.LogInformation("[OBS-3] Nueva orden {OrderId} (#{OrderNumber}) creada para mesa {TableId}.", order.Id, order.OrderNumber, dto.TableId);
                }
                else if (order.Status == OrderStatus.ReadyToPay || order.Status == OrderStatus.Ready)
                {
                    Console.WriteLine($"[OrderService] Orden existente en estado {order.Status}, cambiando a SentToKitchen por nuevos items");
                    order.Status = OrderStatus.SentToKitchen;
                    if (totalPaid > 0)
                        Console.WriteLine($"⚠️ [OrderService] ADVERTENCIA: Orden tiene pagos parciales (${totalPaid:F2}), pero se agregan nuevos items");
                }
                else if (order.Status == OrderStatus.Preparing)
                {
                    Console.WriteLine($"[OrderService] Orden existente en estado {order.Status}, manteniendo estado actual");
                }
                else if (order.Status == OrderStatus.Pending)
                {
                    Console.WriteLine($"[OrderService] Orden existente en estado {order.Status}, cambiando a SentToKitchen");
                    order.Status = OrderStatus.SentToKitchen;
                }
                else
                {
                    Console.WriteLine($"[OrderService] Orden existente en estado {order.Status}, cambiando a SentToKitchen");
                    order.Status = OrderStatus.SentToKitchen;
                }
                Console.WriteLine($"[OrderService] Estado final de orden: {order.Status}");
                SetUpdatedTracking(order);
                Console.WriteLine($"✅ [OrderService] AddOrUpdateOrderWithPendingItemsAsync() - Orden existente actualizada: UpdatedBy={order.UpdatedBy}, UpdatedAt={order.UpdatedAt}");
                } // end else (orden no completamente pagada)
            }

            decimal total = 0;
            Guid? tableAreaId = null;
            if (dto.TableId != Guid.Empty)
            {
                var orderTable = await _context.Tables.AsNoTracking().FirstOrDefaultAsync(t => t.Id == dto.TableId);
                tableAreaId = orderTable?.AreaId;
                Console.WriteLine($"[OrderService] Área de mesa para enrutamiento: {tableAreaId}");
            }

            // Procesar cada item individualmente
            Console.WriteLine($"[OrderService] Procesando {dto.Items.Count} items individualmente");
            // Verificar items duplicados en el DTO
            var duplicateIds = dto.Items.GroupBy(i => i.Id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicateIds.Any())
            {
                Console.WriteLine($"[OrderService] ⚠️ ADVERTENCIA: IDs duplicados en DTO: {string.Join(", ", duplicateIds)}");
            }

            try
            {
                foreach (var itemDto in dto.Items)
                {
                    Console.WriteLine($"[OrderService] Procesando item: ProductId={itemDto.ProductId}, Quantity={itemDto.Quantity}, Status={itemDto.Status}, DTO_ID={itemDto.Id}");
                    var product = await _productService.GetByIdAsync(itemDto.ProductId);
                    if (product == null)
                    {
                        Console.WriteLine($"[OrderService] Producto no encontrado: {itemDto.ProductId}");
                        continue;
                    }

                    // Verificar si ya existe un item con el mismo ID en la base de datos
                    var existingItem = await _context.OrderItems.FindAsync(itemDto.Id);
                    if (existingItem != null)
                    {
                        Console.WriteLine($"[OrderService] ⚠️ ADVERTENCIA: Ya existe un item con ID {itemDto.Id} en la base de datos, saltando...");
                        continue;
                    }
                    // Verificar si el item ya está siendo trackeado en el contexto actual
                    var trackedItem = _context.ChangeTracker.Entries<OrderItem>()
                        .Where(e => e.Entity.Id == itemDto.Id)
                        .FirstOrDefault();
                    if (trackedItem != null)
                    {
                        Console.WriteLine($"[OrderService] ⚠️ ADVERTENCIA: Item con ID {itemDto.Id} ya está siendo trackeado en el contexto, saltando...");
                        continue;
                    }

                    // ✅ NUEVO: Verificar stock disponible antes de crear el item
                    Console.WriteLine($"🔍 [OrderService] Verificando stock para producto: {product.Name}, Cantidad requerida: {itemDto.Quantity}");
                    
                    if (product.TrackInventory)
                    {
                        var hasStock = await _productService.HasStockAvailableAsync(product.Id, itemDto.Quantity, order.BranchId);
                        if (!hasStock)
                        {
                            var availableStock = await _productService.GetAvailableStockAsync(product.Id, order.BranchId);
                            Console.WriteLine($"❌ [OrderService] Stock insuficiente para {product.Name}. Disponible: {availableStock}, Requerido: {itemDto.Quantity}");
                            throw new InvalidOperationException($"Stock insuficiente para {product.Name}. Disponible: {availableStock}, Requerido: {itemDto.Quantity}");
                        }
                        Console.WriteLine($"✅ [OrderService] Stock disponible confirmado para {product.Name}");
                    }

                    // ✅ NUEVO: Prioridad de asignación de estación:
                    // 1. Si el admin seleccionó una estación manualmente (SelectedStationId), usarla
                    // 2. Si el salonero tiene estación asignada, usarla
                    // 3. Si no, usar la lógica del producto (mejor estación con stock o estación predeterminada)
                    Guid? assignedStationId = null;
                    
                    // Prioridad 1: Estación seleccionada manualmente por admin
                    if (dto.SelectedStationId.HasValue)
                    {
                        assignedStationId = dto.SelectedStationId.Value;
                        Console.WriteLine($"✅ [OrderService] Usando estación seleccionada manualmente por admin para {product.Name}: {assignedStationId.Value}");
                    }
                    else
                    {
                        // Prioridad 2: Obtener la estación del salonero (waiter) desde UserAssignment
                        Guid? waiterStationId = null;
                        if (userId.HasValue)
                        {
                            var userAssignment = await _userAssignmentService.GetActiveByUserIdAsync(userId.Value);
                            if (userAssignment != null && userAssignment.StationId.HasValue)
                            {
                                waiterStationId = userAssignment.StationId.Value;
                                Console.WriteLine($"✅ [OrderService] Estación del salonero encontrada: {waiterStationId.Value}");
                            }
                            else
                            {
                                Console.WriteLine($"⚠️ [OrderService] El salonero no tiene una estación asignada en UserAssignment");
                            }
                        }

                        if (waiterStationId.HasValue)
                        {
                            // Usar la estación del salonero
                            assignedStationId = waiterStationId;
                            Console.WriteLine($"✅ [OrderService] Usando estación del salonero para {product.Name}: {assignedStationId.Value}");
                            
                            // Validar que el producto pueda ser preparado en esta estación
                            if (product.TrackInventory)
                            {
                                // Verificar stock en la estación del salonero
                                var stationStock = await _productService.GetStockInStationAsync(
                                    product.Id, 
                                    assignedStationId.Value, 
                                    order.BranchId);
                                
                                if (stationStock < itemDto.Quantity && !product.AllowNegativeStock)
                                {
                                    Console.WriteLine($"⚠️ [OrderService] Stock insuficiente en la estación del salonero. Stock disponible: {stationStock}, Requerido: {itemDto.Quantity}");
                                    // No lanzar error, continuar con la asignación pero notificar
                                }
                            }
                        }
                        else
                        {
                            // Prioridad 3: Si el salonero no tiene estación asignada, usar la lógica anterior del producto
                            Console.WriteLine($"⚠️ [OrderService] Salonero sin estación asignada, usando lógica de producto");
                            
                            // ✅ NUEVO: Encontrar la mejor estación basada en stock disponible
                            var bestStationId = await _productService.FindBestStationForProductAsync(
                                product.Id, 
                                itemDto.Quantity, 
                                order.BranchId,
                                tableAreaId);
                            
                            // Si no se encontró estación con stock suficiente y no se permite stock negativo, lanzar error
                            if (!bestStationId.HasValue && product.TrackInventory && !product.AllowNegativeStock)
                            {
                                Console.WriteLine($"❌ [OrderService] No hay estación disponible con stock suficiente para {product.Name}");
                                throw new InvalidOperationException($"No hay estación disponible con stock suficiente para {product.Name}");
                            }

                            // Usar estación encontrada (ya no hay estación predeterminada en producto)
                            assignedStationId = bestStationId;
                            if (assignedStationId.HasValue)
                            {
                                Console.WriteLine($"✅ [OrderService] Estación asignada para {product.Name}: {assignedStationId.Value}");
                            }
                            else
                            {
                                Console.WriteLine($"⚠️ [OrderService] No se asignó estación para {product.Name}");
                            }
                        }
                    }

                    if (!assignedStationId.HasValue)
                    {
                        throw new InvalidOperationException(
                            $"El producto '{product.Name}' no tiene estación de preparación configurada para esta área/sucursal. Configure ProductStockAssignment.");
                    }

                    // Crear un OrderItem individual para cada item del DTO
                    var newItem = new OrderItem
                    {
                        Id = itemDto.Id != Guid.Empty ? itemDto.Id : Guid.NewGuid(),
                        OrderId = order.Id,
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = product.Price,
                        Discount = itemDto.Discount ?? 0,
                        Notes = itemDto.Notes,
                        KitchenStatus = KitchenStatus.Pending,
                        Status = !string.IsNullOrEmpty(itemDto.Status)
                            ? Enum.Parse<OrderItemStatus>(itemDto.Status, ignoreCase: true)
                            : OrderItemStatus.Pending,
                        // ✅ NUEVO: Asignar estación basada en stock disponible
                        PreparedByStationId = assignedStationId,
                        AddedByUserId = userId,
                        // ✅ NUEVO: Establecer campos multi-tenant desde la orden
                        CompanyId = order.CompanyId,
                        BranchId = order.BranchId
                    };
                    
                    // ✅ NUEVO: Establecer campos de auditoría usando BaseTrackingService
                    SetCreatedTracking(newItem);
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
                    
                    // ✅ NUEVO: Reducir stock del producto después de agregar el item exitosamente
                    if (product.TrackInventory && assignedStationId.HasValue)
                    {
                        try
                        {
                            await _inventoryOps.DeductInventoryForSaleAsync(
                                product.Id,
                                itemDto.Quantity,
                                assignedStationId.Value,
                                order.BranchId,
                                order.CompanyId,
                                order.Id,
                                userId);
                            Console.WriteLine($"✅ [OrderService] Stock reducido para {product.Name}: -{itemDto.Quantity}");
                        }
                        catch (Exception stockEx)
                        {
                            Console.WriteLine($"❌ [OrderService] Error al reducir stock para {product.Name}: {stockEx.Message}");
                        }
                    }
                    else if (product.TrackInventory)
                    {
                        try
                        {
                            await _inventoryOps.DeductInventoryForSaleAsync(
                                product.Id,
                                itemDto.Quantity,
                                null,
                                order.BranchId,
                                order.CompanyId,
                                order.Id,
                                userId);
                            Console.WriteLine($"✅ [OrderService] Stock global reducido para {product.Name}: -{itemDto.Quantity}");
                        }
                        catch (Exception stockEx)
                        {
                            Console.WriteLine($"❌ [OrderService] Error al reducir stock global para {product.Name}: {stockEx.Message}");
                        }
                    }
                    
                    total += (newItem.Quantity * newItem.UnitPrice) - newItem.Discount;
                }
                await _context.SaveChangesAsync();
                Console.WriteLine($"[OrderService] ✅ Orden guardada exitosamente con {dto.Items.Count} items");
                
                // ✅ NUEVO: Recalcular TotalAmount después de agregar items (una sola vez)
                order.TotalAmount = order.OrderItems.Sum(oi => (oi.Quantity * oi.UnitPrice) - oi.Discount);
                await _context.SaveChangesAsync();
                Console.WriteLine($"[OrderService] TotalAmount recalculado: ${order.TotalAmount:F2}");
                
                // Si la orden es nueva y order.Table es null, cargar la mesa asociada
                if (order.Table == null && order.TableId != Guid.Empty)
                {
                    order.Table = await _context.Tables.FindAsync(order.TableId);
                }
                
                // ✅ MEJORADO: Actualizar el estado de la mesa según los ítems de la orden
                if (order.Table != null)
                {
                    // ✅ NUEVO: Considerar todos los items (antiguos y nuevos)
                    var allItems = order.OrderItems.ToList();
                    var hasPendingOrPreparing = allItems.Any(oi =>
                        oi.Status == OrderItemStatus.Pending || oi.Status == OrderItemStatus.Preparing);
                    var allItemsReady = allItems.All(oi => oi.Status == OrderItemStatus.Ready);
                    var hasReadyItems = allItems.Any(oi => oi.Status == OrderItemStatus.Ready);
                    
                    if (hasPendingOrPreparing)
                    {
                        order.Table.Status = TableStatus.EnPreparacion;
                        Console.WriteLine($"[OrderService] Mesa {order.Table.TableNumber} cambiada a EnPreparacion (hay items pendientes/preparándose)");
                    }
                    else if (allItemsReady && allItems.Any())
                    {
                        order.Table.Status = TableStatus.ParaPago;
                        Console.WriteLine($"[OrderService] Mesa {order.Table.TableNumber} cambiada a ParaPago (todos los items listos)");
                    }
                    else if (hasReadyItems)
                    {
                        order.Table.Status = TableStatus.Servida;
                        Console.WriteLine($"[OrderService] Mesa {order.Table.TableNumber} cambiada a Servida (hay items listos pero no todos)");
                    }
                    else
                    {
                        order.Table.Status = TableStatus.Ocupada;
                        Console.WriteLine($"[OrderService] Mesa {order.Table.TableNumber} cambiada a Ocupada");
                    }
                    
                    await _context.SaveChangesAsync();
                    // Notificar cambio de estado de mesa vía SignalR
                    await _orderHubService.NotifyTableStatusChanged(order.Table.Id, order.Table.Status.ToString());
                    Console.WriteLine($"[OrderService] Notificación de mesa enviada: {order.Table.Status}");
                }
                
                // ✅ NUEVO: Notificar cambios de orden vía SignalR
                await _orderHubService.NotifyOrderStatusChanged(order.Id, order.Status);
                await _orderHubService.NotifyKitchenUpdate();
                
                return order;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderService] ERROR en AddOrUpdateOrderWithPendingItemsAsync: {ex.Message}");
                throw; // Re-lanzar la excepción original
            }
        }

        // NUEVO: Enviar a cocina solo los ítems Pending, marcarlos como Sent y notificar
        public async Task<List<OrderItem>> SendPendingItemsToKitchenAsync(Guid orderId)
        {
            _logger.LogInformation("[KDS] SendPendingItemsToKitchenAsync - orderId: {OrderId}", orderId);

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new KeyNotFoundException($"Orden {orderId} no encontrada en SendPendingItemsToKitchenAsync");

            // Garantizar estado correcto en la orden ANTES de persistir
            if (order.Status != OrderStatus.SentToKitchen)
            {
                _logger.LogInformation("[KDS] Orden {OrderId} en estado {From} → cambiando a SentToKitchen", orderId, order.Status);
                order.Status = OrderStatus.SentToKitchen;
            }

            // Seleccionar sólo los ítems que aún no fueron enviados a cocina
            var pendingItems = order.OrderItems
                .Where(oi => oi.KitchenStatus == KitchenStatus.Pending)
                .ToList();

            _logger.LogInformation("[KDS] {Count} ítems con KitchenStatus=Pending encontrados en orden {OrderId}", pendingItems.Count, orderId);

            foreach (var item in pendingItems)
            {
                // Solo KitchenStatus cambia a Sent.
                // OrderItemStatus permanece en Pending hasta que la estación
                // confirme inicio de preparación (MarkItemAsPreparingAsync).
                item.KitchenStatus = KitchenStatus.Sent;
                item.SentAt        = DateTime.UtcNow;
            }

            // ─── PERSISTIR ANTES DE NOTIFICAR ──────────────────────────────────────
            // Garantía: el estado en DB refleja la realidad antes de que el cliente
            // reciba la notificación SignalR (elimina "estados fantasma" en la UI).
            await _context.SaveChangesAsync();
            _logger.LogInformation("[KDS] Estado persistido en DB para {Count} ítems de orden {OrderId}", pendingItems.Count, orderId);

            // ─── NOTIFICACIONES SignalR ─────────────────────────────────────────────
            // Notificamos el estado REAL persistido (item.Status = Pending),
            // NO un estado futuro hipotético como Preparing.
            foreach (var item in pendingItems)
            {
                await _orderHubService.NotifyOrderItemStatusChanged(order.Id, item.Id, item.Status);
            }

            await _orderHubService.NotifyOrderStatusChanged(order.Id, order.Status);
            await _orderHubService.NotifyKitchenUpdate();

            _logger.LogInformation("[KDS] SendPendingItemsToKitchenAsync completado - orden {OrderId} en {Status}", orderId, order.Status);
            return pendingItems;
        }

        // NUEVO: Marcar ítem como listo
        public async Task MarkItemAsReadyAsync(Guid orderId, Guid itemId, Guid? deliveredByUserId = null)
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
                item.PreparedAt = DateTime.UtcNow; // ✅ Fecha específica de preparación
                if (deliveredByUserId.HasValue)
                    item.DeliveredByUserId = deliveredByUserId.Value;
                
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
                var timestamp = DateTime.UtcNow.ToString("HH:mm:ss");
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
                    await _orderHubService.NotifyTableStatusChanged(order.Table.Id, order.Table.Status.ToString());
                    Console.WriteLine($"[OrderService] Notificación de mesa enviada: {order.Table.Status}");
                }
                
                Console.WriteLine($"[OrderService] MarkItemAsReadyAsync completado exitosamente");

                await _loggingService.LogOrderActivityAsync(
                    action: AuditAction.UPDATE.ToString(),
                    description: $"Ítem entregado/listo: {item.Product?.Name}",
                    orderId: orderId,
                    newValues: new { itemId, DeliveredByUserId = deliveredByUserId, item.Status });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderService] ERROR en MarkItemAsReadyAsync: {ex.Message}");
                Console.WriteLine($"[OrderService] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        // Cambiar estación de un ítem (re-enrutamiento)
        public async Task<OrderItem> UpdateItemStationAsync(Guid orderId, Guid itemId, Guid newStationId, Guid? userId = null)
        {
            var orderItem = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .Include(oi => oi.PreparedByStation)
                .FirstOrDefaultAsync(oi => oi.Id == itemId && oi.OrderId == orderId)
                ?? throw new KeyNotFoundException($"Item {itemId} no encontrado en la orden {orderId}");

            if (orderItem.Order?.Status is OrderStatus.Cancelled or OrderStatus.Completed)
                throw new InvalidOperationException("No se puede cambiar estación en una orden cerrada");

            if (orderItem.Status == OrderItemStatus.Cancelled)
                throw new InvalidOperationException("No se puede cambiar estación de un ítem cancelado");

            var newStation = await _context.Stations.AsNoTracking().FirstOrDefaultAsync(s => s.Id == newStationId && s.IsActive)
                ?? throw new KeyNotFoundException("Estación destino no encontrada o inactiva");

            if (orderItem.Order?.BranchId != null && newStation.BranchId != orderItem.Order.BranchId)
                throw new InvalidOperationException("La estación destino pertenece a otra sucursal");

            if (orderItem.ProductId.HasValue)
            {
                var canPrepare = await _context.ProductStockAssignments.AnyAsync(sa =>
                    sa.ProductId == orderItem.ProductId.Value &&
                    sa.StationId == newStationId &&
                    sa.IsActive &&
                    (orderItem.Order!.BranchId == null || sa.BranchId == orderItem.Order.BranchId));

                if (!canPrepare)
                    throw new InvalidOperationException($"El producto no está configurado para la estación '{newStation.Name}'");
            }

            var oldStationId = orderItem.PreparedByStationId;
            orderItem.PreparedByStationId = newStationId;
            orderItem.UpdatedAt = DateTime.UtcNow;
            orderItem.Order!.Version++;

            if (oldStationId.HasValue && oldStationId != newStationId && orderItem.ProductId.HasValue && orderItem.Product?.TrackInventory == true)
            {
                try
                {
                    await _inventoryOps.TransferStockBetweenStationsAsync(
                        orderItem.ProductId.Value,
                        oldStationId.Value,
                        newStationId,
                        orderItem.Quantity,
                        orderItem.Order.BranchId,
                        orderItem.Order.CompanyId,
                        userId,
                        "UpdateItemStation");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[UpdateItemStation] Transferencia de stock falló para ítem {ItemId}", itemId);
                }
            }

            await _context.SaveChangesAsync();

            await _orderHubService.NotifyOrderItemStatusChanged(orderId, itemId, orderItem.Status);
            await _orderHubService.NotifyKitchenUpdate();

            _logger.LogInformation("[OrderService] Item {ItemId} reasignado de estación {Old} a {New} por usuario {UserId}",
                itemId, oldStationId, newStationId, userId);

            return orderItem;
        }

        // ✅ NUEVO: Cancelar item de orden
        public async Task CancelOrderItemAsync(Guid orderId, Guid itemId, Guid? userId = null, string? userRole = null, Guid? supervisorId = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var orderItem = await _context.OrderItems
                    .Include(oi => oi.Order)
                        .ThenInclude(o => o.Branch)
                    .Include(oi => oi.Product)
                    .FirstOrDefaultAsync(oi => oi.Id == itemId && oi.OrderId == orderId);

                if (orderItem == null)
                    throw new KeyNotFoundException($"Item {itemId} no encontrado en la orden {orderId}");

                if (orderItem.Status == OrderItemStatus.Cancelled)
                    throw new InvalidOperationException("El ítem ya está cancelado");

                if (orderItem.Order?.Status == OrderStatus.Completed)
                    throw new InvalidOperationException("No se puede cancelar un ítem de una orden ya completada y pagada");

                var inKitchen = orderItem.KitchenStatus is KitchenStatus.Sent or KitchenStatus.Ready
                    || orderItem.Status is OrderItemStatus.Preparing or OrderItemStatus.Ready or OrderItemStatus.Served;
                var isSupervisor = userRole is "supervisor" or "manager" or "admin" or "superadmin";
                if (inKitchen && !isSupervisor && !supervisorId.HasValue)
                    throw new UnauthorizedAccessException("Se requiere autorización de supervisor para cancelar ítems ya enviados a cocina.");

                orderItem.Status = OrderItemStatus.Cancelled;
                orderItem.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                if (orderItem.ProductId.HasValue && orderItem.ProductId != Guid.Empty)
                {
                    try
                    {
                        await _inventoryOps.RestoreInventoryForCancelAsync(
                            orderItem.ProductId.Value,
                            orderItem.Quantity,
                            orderItem.PreparedByStationId,
                            orderItem.Order?.BranchId,
                            orderItem.Order?.CompanyId,
                            orderId,
                            supervisorId ?? userId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[OrderService] No se pudo restaurar stock para item {ItemId}", itemId);
                    }
                }

                await _loggingService.LogOrderActivityAsync(
                    AuditAction.DELETE.ToString(),
                    $"Ítem cancelado: {orderItem.Product?.Name}",
                    orderId,
                    newValues: new { itemId, supervisorId, userId, userRole });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ✅ NUEVO: Marcar item como preparando
        public async Task MarkItemAsPreparingAsync(Guid orderId, Guid itemId)
        {
            try
            {
                Console.WriteLine($"🔍 [OrderService] MarkItemAsPreparingAsync() - Iniciando...");
                Console.WriteLine($"📋 [OrderService] MarkItemAsPreparingAsync() - OrderId: {orderId}, ItemId: {itemId}");
                
                var orderItem = await _context.OrderItems
                    .Include(oi => oi.Order)
                    .Include(oi => oi.Product)
                    .FirstOrDefaultAsync(oi => oi.Id == itemId && oi.OrderId == orderId);
                
                if (orderItem == null) 
                {
                    Console.WriteLine($"⚠️ [OrderService] MarkItemAsPreparingAsync() - Item no encontrado con ID {itemId}");
                    throw new Exception("Item no encontrado");
                }
                
                // Marcar como preparando
                orderItem.Status = OrderItemStatus.Preparing;
                orderItem.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ [OrderService] MarkItemAsPreparingAsync() - Item marcado como preparando exitosamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [OrderService] MarkItemAsPreparingAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [OrderService] MarkItemAsPreparingAsync() - StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetPendingPaymentOrdersAsync()
        {
            try
            {
                Console.WriteLine($"[OrderService] GetPendingPaymentOrdersAsync iniciado");
                
                var orders = await _context.Orders
                    .Include(o => o.Table)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Include(o => o.Payments)
                    .Where(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.ReadyToPay)
                    .ToListAsync();

                var pendingOrders = new List<Order>();
                
                foreach (var order in orders)
                {
                    var totalAmount = order.OrderItems?.Sum(oi => oi.Quantity * oi.UnitPrice) ?? 0;
                    var paidAmount = order.Payments?.Where(p => p.IsVoided != true).Sum(p => p.Amount) ?? 0;
                    
                    if (paidAmount < totalAmount)
                    {
                        pendingOrders.Add(order);
                    }
                }

                Console.WriteLine($"[OrderService] ✅ Encontradas {pendingOrders.Count} órdenes con pagos pendientes");
                return pendingOrders;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderService] ERROR en GetPendingPaymentOrdersAsync: {ex.Message}");
                Console.WriteLine($"[OrderService] Stack trace: {ex.StackTrace}");
                throw;
            }
        }
        
        // ✅ NUEVO: Generar número de orden único
        private async Task<string> GenerateOrderNumberAsync(Guid? companyId)
        {
            try
            {
                Console.WriteLine($"🔍 [OrderService] GenerateOrderNumberAsync() - Generando número de orden...");
                
                // Obtener el último número de orden para esta compañía
                int lastOrderNumber = 0;
                
                var query = _context.Orders.AsQueryable();
                
                // Si hay CompanyId, filtrar por compañía
                if (companyId.HasValue)
                {
                    query = query.Where(o => o.CompanyId == companyId.Value);
                    Console.WriteLine($"🔍 [OrderService] GenerateOrderNumberAsync() - Filtrando por CompanyId: {companyId.Value}");
                }
                
                // Obtener el último OrderNumber numérico
                var lastOrder = await query
                    .Where(o => !string.IsNullOrEmpty(o.OrderNumber) && 
                                o.OrderNumber.All(char.IsDigit))
                    .OrderByDescending(o => o.OrderNumber)
                    .FirstOrDefaultAsync();
                
                if (lastOrder != null && int.TryParse(lastOrder.OrderNumber, out var parsedNumber))
                {
                    lastOrderNumber = parsedNumber;
                    Console.WriteLine($"🔍 [OrderService] GenerateOrderNumberAsync() - Último número encontrado: {lastOrderNumber}");
                }
                
                // Incrementar y generar nuevo número
                var newOrderNumber = (lastOrderNumber + 1).ToString().PadLeft(6, '0');
                Console.WriteLine($"✅ [OrderService] GenerateOrderNumberAsync() - Nuevo número generado: {newOrderNumber}");
                
                return newOrderNumber;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [OrderService] GenerateOrderNumberAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [OrderService] GenerateOrderNumberAsync() - StackTrace: {ex.StackTrace}");
                
                // Fallback: usar timestamp como número de orden
                var fallbackNumber = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                Console.WriteLine($"⚠️ [OrderService] GenerateOrderNumberAsync() - Usando número de fallback: {fallbackNumber}");
                return fallbackNumber;
            }
        }

        public async Task<Order> ApplyOrderDiscountAsync(Guid orderId, string discountType, decimal discountValue, string? reason, Guid? userId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId)
                ?? throw new KeyNotFoundException($"Orden {orderId} no encontrada");

            if (order.Status is OrderStatus.Completed or OrderStatus.Cancelled)
                throw new InvalidOperationException("No se puede aplicar descuento a una orden cerrada.");

            var subtotal = order.OrderItems
                .Where(oi => oi.Status != OrderItemStatus.Cancelled)
                .Sum(oi => oi.Quantity * oi.UnitPrice - oi.Discount);

            decimal discountAmount = discountType.Equals("percentage", StringComparison.OrdinalIgnoreCase)
                ? Math.Round(subtotal * discountValue / 100m, 2)
                : discountValue;

            if (discountAmount < 0) discountAmount = 0;
            if (discountAmount > subtotal) discountAmount = subtotal;

            order.DiscountType = discountType;
            order.DiscountAmount = discountAmount;
            order.DiscountReason = reason;
            order.TotalAmount = subtotal - discountAmount;
            order.Version++;
            SetUpdatedTracking(order);

            await _context.SaveChangesAsync();

            await _loggingService.LogOrderActivityAsync(
                AuditAction.UPDATE.ToString(),
                $"Descuento aplicado: ${discountAmount:F2}",
                orderId,
                newValues: new { discountType, discountValue, discountAmount, reason, userId });

            return order;
        }
    }
} 