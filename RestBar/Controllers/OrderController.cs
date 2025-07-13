using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using RestBar.Services;
using System;
using System.Threading.Tasks;
using System.Linq;
using RestBar.ViewModel;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;

namespace RestBar.Controllers
{
    [Authorize] // Requiere autenticación para todos los métodos
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IOrderItemService _orderItemService;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly ITableService _tableService;
        private readonly IAreaService _areaService;
        private readonly ICustomerService _customerService;
        private readonly IUserService _userService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderService orderService,
            IOrderItemService orderItemService,
            ICategoryService categoryService,
            IProductService productService,
            ITableService tableService,
            IAreaService areaService,
            ICustomerService customerService,
            IUserService userService,
            ILogger<OrderController> logger
        )
        {
            _orderService = orderService;
            _orderItemService = orderItemService;
            _categoryService = categoryService;
            _productService = productService;
            _tableService = tableService;
            _areaService = areaService;
            _customerService = customerService;
            _userService = userService;
            _logger = logger;
        }

        // GET: Order
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllAsync();
            ViewBag.Tables = await _tableService.GetTablesForViewBagAsync();
            ViewBag.Customers = await _customerService.GetAllAsync();
            ViewBag.Products = await _productService.GetActiveProductsForViewBagAsync();
            return View(orders);
        }

        // GET: Order/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var order = await _orderService.GetOrderWithDetailsAsync(id.Value);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Order/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Tables = await _tableService.GetTablesForViewBagAsync();
            ViewBag.Customers = await _customerService.GetAllAsync();
            ViewBag.Users = await _userService.GetAllAsync();
            ViewBag.Products = await _productService.GetActiveProductsForViewBagAsync();
            return View();
        }

        // POST: Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TableId,CustomerId,UserId,OrderType")] Order order)
        {
            if (ModelState.IsValid)
            {
                order.Id = Guid.NewGuid();
                await _orderService.CreateAsync(order);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Tables = await _tableService.GetTablesForViewBagAsync();
            ViewBag.Customers = await _customerService.GetAllAsync();
            ViewBag.Users = await _userService.GetAllAsync();
            return View(order);
        }

        // GET: Order/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var order = await _orderService.GetByIdAsync(id.Value);
            if (order == null)
            {
                return NotFound();
            }

            ViewBag.Tables = await _tableService.GetTablesForViewBagAsync();
            ViewBag.Customers = await _customerService.GetAllAsync();
            ViewBag.Users = await _userService.GetAllAsync();
            return View(order);
        }

        // POST: Order/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,TableId,CustomerId,UserId,OrderType,Status,TotalAmount")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _orderService.UpdateAsync(order);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _orderService.OrderExistsAsync(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Tables = await _tableService.GetTablesForViewBagAsync();
            ViewBag.Customers = await _customerService.GetAllAsync();
            ViewBag.Users = await _userService.GetAllAsync();
            return View(order);
        }

        // GET: Order/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var order = await _orderService.GetOrderWithDetailsAsync(id.Value);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Order/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _orderService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: Order/AddItem/5
        public async Task<IActionResult> AddItem(Guid? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var order = await _orderService.GetByIdAsync(id.Value);
            if (order == null)
            {
                return NotFound();
            }

            ViewBag.Products = await _productService.GetActiveProductsForViewBagAsync();
            return View(new OrderItem { OrderId = id.Value });
        }

        // POST: Order/AddItem/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(Guid id, [Bind("ProductId,Quantity,UnitPrice")] OrderItem orderItem)
        {
            if (ModelState.IsValid)
            {
                orderItem.OrderId = id;
                orderItem.Id = Guid.NewGuid();
                await _orderItemService.CreateAsync(orderItem);
                await _orderService.CalculateOrderTotalAsync(id);
                return RedirectToAction(nameof(Details), new { id });
            }

            ViewBag.Products = await _productService.GetActiveProductsForViewBagAsync();
            return View(orderItem);
        }

        // GET: Order/RemoveItem/5
        public async Task<IActionResult> RemoveItem(Guid? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var orderItem = await _orderItemService.GetOrderItemWithOrderAndProductAsync(id.Value);

            if (orderItem == null)
            {
                return NotFound();
            }

            return View(orderItem);
        }

        // POST: Order/RemoveItem/5
        [HttpPost, ActionName("RemoveItem")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItemConfirmed(Guid id)
        {
            var orderId = await _orderItemService.DeleteOrderItemAndGetOrderIdAsync(id);
                if (orderId.HasValue)
                {
                    await _orderService.CalculateOrderTotalAsync(orderId.Value);
            }
            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        // GET: Order/Close/5
        public async Task<IActionResult> Close(Guid? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var order = await _orderService.GetOrderWithPaymentsAsync(id.Value);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Order/Close/5
        [HttpPost, ActionName("Close")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseConfirmed(Guid id)
        {
            await _orderService.CloseOrderAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: Order/ByTable/5
        public async Task<IActionResult> ByTable(Guid tableId)
        {
            var orders = await _orderService.GetByTableIdAsync(tableId);
            return View("Index", orders);
        }

        // GET: Order/ByCustomer/5
        public async Task<IActionResult> ByCustomer(Guid customerId)
        {
            var orders = await _orderService.GetByCustomerIdAsync(customerId);
            return View("Index", orders);
        }

        // GET: Order/ByUser/5
        public async Task<IActionResult> ByUser(Guid userId)
        {
            var orders = await _orderService.GetByUserIdAsync(userId);
            return View("Index", orders);
        }

        // GET: Order/ByStatus/5
        public async Task<IActionResult> ByStatus(OrderStatus status)
        {
            var orders = await _orderService.GetByStatusAsync(status);
            return View("Index", orders);
        }

        // GET: Order/Open
        public async Task<IActionResult> Open()
        {
            var orders = await _orderService.GetOpenOrdersAsync();
            return View("Index", orders);
        }

        // --- ENDPOINTS PARA EL FRONT POS ---

        // GET: Order/GetActiveCategories
        [HttpGet]
        public async Task<IActionResult> GetActiveCategories()
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            var data = categories.Select(c => new { id = c.Id, name = c.Name });
            return Json(new { success = true, data });
        }

        // GET: Order/GetProductsByCategory/{categoryId}
        [HttpGet]
        public async Task<IActionResult> GetProductsByCategory(Guid categoryId)
        {
            var products = await _productService.GetByCategoryIdAsync(categoryId);
            var data = products.Select(p => new {
                id = p.Id,
                name = p.Name,
                price = p.Price,
                imageUrl = p.ImageUrl
            });
            return Json(new { success = true, data });
        }

        // GET: Order/GetActiveTables
        [HttpGet]
        public async Task<IActionResult> GetActiveTables()
        {
            try
            {
                // Verificar autenticación
                _logger.LogInformation($"[GetActiveTables] User authenticated: {User.Identity.IsAuthenticated}");
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.FindFirst("UserId")?.Value;
                    var userRole = User.FindFirst("UserRole")?.Value;
                    _logger.LogInformation($"[GetActiveTables] UserId: {userId}, Role: {userRole}");
                }
                else
                {
                    _logger.LogWarning("[GetActiveTables] Usuario no autenticado");
                    return Unauthorized(new { error = "Usuario no autenticado" });
                }

                var tables = await _tableService.GetActiveTablesAsync();
                var tableData = tables.Select(t => new
                {
                    t.Id,
                    Number = t.TableNumber,
                    t.Status,
                    AreaName = t.Area?.Name,
                    t.Capacity
                });
                return Json(tableData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mesas activas");
                return StatusCode(500, new { error = "Error al obtener mesas activas" });
            }
        }

        // GET: Order/GetAreas
        [HttpGet]
        public async Task<IActionResult> GetAreas()
        {
            var areas = await _areaService.GetAllAsync();
            var data = areas.Select(a => new { id = a.Id, name = a.Name });
            return Json(new { success = true, data });
        }

        // GET: Order/GetCategories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                var data = categories.Select(c => new { id = c.Id, name = c.Name });
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías");
                return StatusCode(500, new { success = false, error = "Error al obtener categorías" });
            }
        }

        // GET: Order/GetProducts
        [HttpGet]
        public async Task<IActionResult> GetProducts(Guid? categoryId)
        {
            try
            {
                var products = await _productService.GetProductsWithDetailsAsync(categoryId);
                return Json(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos");
                return StatusCode(500, new { error = "Error al obtener productos" });
            }
        }

        // POST: Order/SendToKitchen
        [HttpPost]
        public async Task<IActionResult> SendToKitchen([FromBody] SendOrderDto dto)
        {
            try
            {
                Console.WriteLine($"[OrderController] SendToKitchen iniciado");
                Console.WriteLine($"[OrderController] TableId: {dto.TableId}, Items: {dto.Items?.Count ?? 0}");
                
                // ✅ LOGGING DETALLADO DE ITEMS RECIBIDOS
                Console.WriteLine($"[OrderController] === DETALLE DE ITEMS RECIBIDOS ===");
                if (dto.Items != null)
                {
                    foreach (var item in dto.Items)
                    {
                        Console.WriteLine($"[OrderController] Item: ID={item.Id}, ProductId={item.ProductId}, Quantity={item.Quantity}, Status={item.Status}");
                    }
                }
                Console.WriteLine($"[OrderController] === FIN DETALLE DE ITEMS ===");
                
                // Validar que TableId no sea Guid.Empty
                if (dto.TableId == Guid.Empty)
                {
                    Console.WriteLine($"[OrderController] ERROR: TableId es Guid.Empty");
                    _logger.LogWarning("SendToKitchen: TableId es Guid.Empty");
                    return BadRequest(new { error = "Debe seleccionar una mesa antes de enviar la orden." });
                }

                // Obtener userId del usuario autenticado
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    Console.WriteLine($"[OrderController] ERROR: Usuario no autenticado o UserId inválido");
                    _logger.LogWarning("SendToKitchen: Usuario no autenticado o UserId inválido");
                    return BadRequest(new { error = "Usuario no autenticado." });
                }

                Console.WriteLine($"[OrderController] Usuario autenticado - UserId: {userId}");
                Console.WriteLine($"[OrderController] Llamando a SendToKitchenAsync...");
                
                var order = await _orderService.SendToKitchenAsync(dto, userId);
                
                Console.WriteLine($"[OrderController] Orden procesada - ID: {order.Id}, Status: {order.Status}");
                
                // Verificar que la orden esté en estado SentToKitchen
                if (order.Status != OrderStatus.SentToKitchen)
                {
                    Console.WriteLine($"[OrderController] ADVERTENCIA: Orden no está en SentToKitchen, Status actual: {order.Status}");
                }
                else
                {
                    Console.WriteLine($"[OrderController] ✅ Orden correctamente en estado SentToKitchen");
                }
                
                var response = new { 
                    orderId = order.Id, 
                    status = order.Status.ToString(),
                    message = "Orden enviada a cocina exitosamente"
                };
                
                Console.WriteLine($"[OrderController] Respuesta enviada: {System.Text.Json.JsonSerializer.Serialize(response)}");
                Console.WriteLine($"[OrderController] SendToKitchen completado");
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderController] ERROR en SendToKitchen: {ex.Message}");
                _logger.LogError(ex, "Error al enviar orden a cocina");
                return BadRequest(new { error = ex.Message });
            }
        }

        // POST: Order/MarkItemReady
        [HttpPost]
        public async Task<IActionResult> MarkItemReady([FromBody] MarkItemReadyDto dto)
        {
            try
            {
                await _orderService.MarkItemAsReadyAsync(dto.OrderId, dto.ItemId);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET: Order/GetActiveOrder/{tableId}
        [HttpGet]
        public async Task<IActionResult> GetActiveOrder(Guid tableId)
        {
            try
            {
                _logger.LogInformation($"GetActiveOrder llamado con tableId: {tableId}");
                
                var order = await _orderService.GetActiveOrderByTableAsync(tableId);
                
                // Solo considerar activa la orden si tiene items
                if (order == null || order.OrderItems == null || !order.OrderItems.Any())
                {
                    _logger.LogInformation($"No se encontró orden activa para tableId: {tableId}");
                    return Json(new { hasActiveOrder = false });
                }

                _logger.LogInformation($"Orden encontrada - ID: {order.Id}, Items: {order.OrderItems.Count}");
                
                // 🔍 LOG DETALLADO DE ITEMS ANTES DEL MAPEO
                _logger.LogInformation("=== ITEMS ANTES DEL MAPEO ===");
                foreach (var item in order.OrderItems)
                {
                    _logger.LogInformation($"Item: ID={item.Id}, ProductId={item.ProductId}, ProductName={item.Product?.Name}, Quantity={item.Quantity}, Status={item.Status}, KitchenStatus={item.KitchenStatus}");
                }
                _logger.LogInformation("=== FIN ITEMS ANTES DEL MAPEO ===");

                var orderItems = order.OrderItems.Select(oi => new
                {
                    id = oi.Id,
                    productId = oi.ProductId,
                    productName = oi.Product?.Name,
                    quantity = oi.Quantity,
                    unitPrice = oi.UnitPrice,
                    total = oi.Quantity * oi.UnitPrice,
                    status = oi.Status.ToString(),
                    kitchenStatus = oi.KitchenStatus.ToString(),
                    preparedAt = oi.PreparedAt,
                    preparedByStation = oi.PreparedByStation?.Name,
                    notes = oi.Notes
                }).ToList();

                // 🔍 LOG DETALLADO DE ITEMS DESPUÉS DEL MAPEO
                _logger.LogInformation("=== ITEMS DESPUÉS DEL MAPEO ===");
                foreach (var item in orderItems)
                {
                    _logger.LogInformation($"Item mapeado: ID={item.id}, ProductId={item.productId}, ProductName={item.productName}, Quantity={item.quantity}, Status={item.status}, KitchenStatus={item.kitchenStatus}");
                }
                _logger.LogInformation("=== FIN ITEMS DESPUÉS DEL MAPEO ===");

                var result = new
                {
                    hasActiveOrder = true,
                    orderId = order.Id,
                    status = order.Status.ToString(),
                    openedAt = order.OpenedAt,
                    items = orderItems,
                    totalAmount = order.TotalAmount
                };

                _logger.LogInformation($"Enviando respuesta con {orderItems.Count} items");
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener orden activa");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // POST: Order/AddItems
        [HttpPost]
        public async Task<IActionResult> AddItems([FromBody] AddItemsDto dto)
        {
            try
            {
                var order = await _orderService.AddItemsToOrderAsync(dto.OrderId, dto.Items);
                
                // Recalcular total
                await _orderService.CalculateOrderTotalAsync(order.Id);

                return Ok(new
                {
                    success = true,
                    message = "Items agregados exitosamente",
                    orderId = order.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar items a la orden");
                return BadRequest(new { error = ex.Message });
            }
        }

        // POST: Order/Cancel
        [HttpPost]
        public async Task<IActionResult> Cancel([FromBody] CancelOrderDto dto)
        {
            try
            {
                Console.WriteLine($"[OrderController] Cancel endpoint llamado");
                Console.WriteLine($"[OrderController] Request recibido - dto: {dto}");
                Console.WriteLine($"[OrderController] dto?.OrderId: {dto?.OrderId}");
                Console.WriteLine($"[OrderController] dto?.UserId: {dto?.UserId}");
                Console.WriteLine($"[OrderController] dto?.SupervisorId: {dto?.SupervisorId}");
                Console.WriteLine($"[OrderController] dto?.Reason: {dto?.Reason}");
                
                // Validar que dto no sea null
                if (dto == null)
                {
                    Console.WriteLine($"[OrderController] ERROR: dto es null");
                    _logger.LogWarning("Cancel: dto es null");
                    return BadRequest(new { error = "Datos de entrada inválidos - dto es null" });
                }

                Console.WriteLine($"[OrderController] dto validado correctamente");

                // Validar que OrderId no sea vacío
                if (dto.OrderId == Guid.Empty)
                {
                    Console.WriteLine($"[OrderController] ERROR: OrderId es vacío");
                    _logger.LogWarning("Cancel: OrderId es vacío");
                    return BadRequest(new { error = "OrderId es requerido" });
                }

                Console.WriteLine($"[OrderController] OrderId validado: {dto.OrderId}");

                // Obtener userId del contexto de autenticación
                Guid? userId = null;
                var userIdClaim = User.FindFirst("UserId")?.Value;
                
                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out Guid parsedUserId))
                {
                    userId = parsedUserId;
                    Console.WriteLine($"[OrderController] UserId obtenido del contexto de autenticación: {userId}");
                }
                else
                {
                    Console.WriteLine($"[OrderController] No se pudo obtener UserId del contexto de autenticación");
                }

                Console.WriteLine($"[OrderController] Llamando a CancelOrderAsync...");
                Console.WriteLine($"[OrderController] Parámetros: orderId={dto.OrderId}, userId={userId}, reason={dto.Reason}, supervisorId={dto.SupervisorId}");
                
                await _orderService.CancelOrderAsync(dto.OrderId, userId, dto.Reason, dto.SupervisorId);
                
                Console.WriteLine($"[OrderController] CancelOrderAsync completado exitosamente");
                Console.WriteLine($"[OrderController] Orden cancelada exitosamente para orderId: {dto?.OrderId}");
                
                return Ok(new
                {
                    success = true,
                    message = "Orden cancelada exitosamente"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderController] ERROR al cancelar orden: {ex.Message}");
                Console.WriteLine($"[OrderController] Tipo de excepción: {ex.GetType().Name}");
                Console.WriteLine($"[OrderController] Stack trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[OrderController] Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"[OrderController] Inner exception type: {ex.InnerException.GetType().Name}");
                }
                
                _logger.LogError(ex, "Error al cancelar orden");
                return BadRequest(new { error = ex.Message });
            }
        }

        public class CancelOrderDto
        {
            public Guid OrderId { get; set; }
            public string? UserId { get; set; }  // Cambiado a string para ser más flexible
            public string? Reason { get; set; }
            public Guid? SupervisorId { get; set; }
        }

        // POST: Order/RemoveItemFromOrder
        [HttpPost]
        public async Task<IActionResult> RemoveItemFromOrder([FromBody] RemoveItemDto dto)
        {
            try
            {
                Console.WriteLine($"[OrderController] RemoveItemFromOrder endpoint llamado");
                Console.WriteLine($"[OrderController] Request body: OrderId={dto?.OrderId}, ProductId={dto?.ProductId}, Status={dto?.Status}");
                
                _logger.LogInformation($"RemoveItemFromOrder llamado con OrderId: {dto?.OrderId}, ProductId: {dto?.ProductId}, Status: {dto?.Status}");

                if (dto == null)
                {
                    Console.WriteLine($"[OrderController] ERROR: dto es null");
                    _logger.LogWarning("RemoveItemFromOrder: dto es null");
                    return BadRequest(new { error = "Datos de entrada inválidos" });
                }

                if (dto.OrderId == Guid.Empty || dto.ProductId == Guid.Empty)
                {
                    Console.WriteLine($"[OrderController] ERROR: OrderId o ProductId vacío - OrderId: {dto.OrderId}, ProductId: {dto.ProductId}");
                    _logger.LogWarning($"RemoveItemFromOrder: OrderId o ProductId vacío - OrderId: {dto.OrderId}, ProductId: {dto.ProductId}");
                    return BadRequest(new { error = "OrderId y ProductId son requeridos" });
                }

                Console.WriteLine($"[OrderController] Llamando a RemoveItemFromOrderAsync...");
                _logger.LogInformation($"RemoveItemFromOrder: Llamando a RemoveItemFromOrderAsync");
                var order = await _orderService.RemoveItemFromOrderAsync(dto.OrderId, dto.ProductId, dto.Status, dto.ItemId);
                
                // Si la orden fue eliminada completamente (quedó vacía)
                if (order == null)
                {
                    Console.WriteLine($"[OrderController] Orden eliminada completamente (quedó vacía)");
                    _logger.LogInformation($"RemoveItemFromOrder: Orden eliminada completamente");
                    return Ok(new
                    {
                        success = true,
                        message = "Item eliminado y orden cerrada (quedó vacía)",
                        orderDeleted = true,
                        orderId = dto.OrderId
                    });
                }
                
                Console.WriteLine($"[OrderController] RemoveItemFromOrderAsync completado, recalculando total...");
                _logger.LogInformation($"RemoveItemFromOrder: Item eliminado, recalculando total");
                // Recalcular total
                await _orderService.CalculateOrderTotalAsync(order.Id);

                Console.WriteLine($"[OrderController] Operación completada exitosamente");
                _logger.LogInformation($"RemoveItemFromOrder: Operación completada exitosamente");
                return Ok(new
                {
                    success = true,
                    message = "Item eliminado exitosamente",
                    orderDeleted = false,
                    orderId = order.Id
                });
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"[OrderController] KeyNotFoundException: {ex.Message}");
                _logger.LogWarning(ex, "Item no encontrado para eliminar");
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"[OrderController] InvalidOperationException: {ex.Message}");
                _logger.LogWarning(ex, "Operación inválida al eliminar item");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderController] Exception inesperada: {ex.Message}");
                Console.WriteLine($"[OrderController] Stack trace: {ex.StackTrace}");
                _logger.LogError(ex, "Error inesperado al eliminar item de la orden");
                return StatusCode(500, new { error = "Error interno del servidor al eliminar el item" });
            }
        }

        // POST: Order/UpdateItemQuantityInOrder
        [HttpPost]
        public async Task<IActionResult> UpdateItemQuantityInOrder([FromBody] UpdateItemQuantityDto dto)
        {
            try
            {
                Console.WriteLine($"[OrderController] UpdateItemQuantityInOrder iniciado");
                Console.WriteLine($"[OrderController] Request body: OrderId={dto?.OrderId}, ProductId={dto?.ProductId}, Quantity={dto?.Quantity}, Status={dto?.Status}");
                
                _logger.LogInformation($"UpdateItemQuantityInOrder llamado con OrderId: {dto?.OrderId}, ProductId: {dto?.ProductId}, Quantity: {dto?.Quantity}, Status: {dto?.Status}");

                if (dto == null)
                {
                    Console.WriteLine($"[OrderController] ERROR: dto es null");
                    _logger.LogWarning("UpdateItemQuantityInOrder: dto es null");
                    return BadRequest(new { error = "Datos de entrada inválidos" });
                }

                if (dto.OrderId == Guid.Empty || dto.ProductId == Guid.Empty)
                {
                    Console.WriteLine($"[OrderController] ERROR: OrderId o ProductId vacío - OrderId: {dto.OrderId}, ProductId: {dto.ProductId}");
                    _logger.LogWarning($"UpdateItemQuantityInOrder: OrderId o ProductId vacío - OrderId: {dto.OrderId}, ProductId: {dto.ProductId}");
                    return BadRequest(new { error = "OrderId y ProductId son requeridos" });
                }

                if (dto.Quantity < 0)
                {
                    Console.WriteLine($"[OrderController] ERROR: Cantidad negativa - Quantity: {dto.Quantity}");
                    _logger.LogWarning($"UpdateItemQuantityInOrder: Cantidad negativa - Quantity: {dto.Quantity}");
                    return BadRequest(new { error = "La cantidad no puede ser negativa" });
                }

                // ✅ NUEVO: Usar el método que busca por ItemId específico
                Console.WriteLine($"[OrderController] Llamando a UpdateItemQuantityByIdAsync...");
                _logger.LogInformation($"UpdateItemQuantityInOrder: Llamando a UpdateItemQuantityByIdAsync");
                
                // ✅ Si se proporciona ItemId, usarlo directamente
                if (dto.ItemId.HasValue && dto.ItemId != Guid.Empty)
                {
                    Console.WriteLine($"[OrderController] Usando ItemId proporcionado: {dto.ItemId}");
                    var updatedOrder = await _orderService.UpdateItemQuantityByIdAsync(dto.OrderId, dto.ItemId.Value, dto.Quantity);
                    
                    // Si la cantidad es 0, la orden fue eliminada completamente
                    if (dto.Quantity == 0)
                    {
                        Console.WriteLine($"[OrderController] Cantidad es 0, orden eliminada completamente");
                        _logger.LogInformation($"UpdateItemQuantityInOrder: Orden eliminada completamente");
                        return Ok(new
                        {
                            success = true,
                            message = "Item eliminado y orden cerrada (quedó vacía)",
                            orderDeleted = true,
                            orderId = dto.OrderId
                        });
                    }
                    
                    Console.WriteLine($"[OrderController] UpdateItemQuantityByIdAsync completado exitosamente");
                    _logger.LogInformation($"UpdateItemQuantityInOrder: Cantidad actualizada exitosamente");
                    
                    return Ok(new
                    {
                        success = true,
                        message = "Cantidad actualizada exitosamente",
                        orderDeleted = false,
                        orderId = updatedOrder.Id
                    });
                }
                
                // ✅ Buscar el item específico por ProductId y Status para obtener su ItemId (fallback)
                var order = await _orderService.GetByIdAsync(dto.OrderId);
                if (order == null)
                {
                    Console.WriteLine($"[OrderController] ERROR: Orden no encontrada");
                    return NotFound(new { error = "Orden no encontrada" });
                }

                var item = order.OrderItems.FirstOrDefault(oi => 
                    oi.ProductId == dto.ProductId && 
                    (string.IsNullOrEmpty(dto.Status) || oi.Status.ToString() == dto.Status)
                );

                if (item == null)
                {
                    Console.WriteLine($"[OrderController] ERROR: Item no encontrado con ProductId: {dto.ProductId}, Status: {dto.Status}");
                    return NotFound(new { error = "Item no encontrado" });
                }

                Console.WriteLine($"[OrderController] Item encontrado - ID: {item.Id}, ProductId: {item.ProductId}, Status: {item.Status}");
                
                // ✅ Usar el nuevo método con ItemId específico
                var updatedOrderFallback = await _orderService.UpdateItemQuantityByIdAsync(dto.OrderId, item.Id, dto.Quantity);
                
                // Si la cantidad es 0, la orden fue eliminada completamente
                if (dto.Quantity == 0)
                {
                    Console.WriteLine($"[OrderController] Cantidad es 0, orden eliminada completamente");
                    _logger.LogInformation($"UpdateItemQuantityInOrder: Orden eliminada completamente");
                    return Ok(new
                    {
                        success = true,
                        message = "Item eliminado y orden cerrada (quedó vacía)",
                        orderDeleted = true,
                        orderId = dto.OrderId
                    });
                }
                
                Console.WriteLine($"[OrderController] UpdateItemQuantityByIdAsync completado exitosamente");
                _logger.LogInformation($"UpdateItemQuantityInOrder: Cantidad actualizada exitosamente");
                
                return Ok(new
                {
                    success = true,
                    message = "Cantidad actualizada exitosamente",
                    orderDeleted = false,
                    orderId = updatedOrderFallback.Id
                });
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"[OrderController] KeyNotFoundException: {ex.Message}");
                _logger.LogError(ex, "UpdateItemQuantityInOrder: Orden o producto no encontrado");
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"[OrderController] InvalidOperationException: {ex.Message}");
                _logger.LogError(ex, "UpdateItemQuantityInOrder: Operación inválida");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderController] Exception inesperada: {ex.Message}");
                Console.WriteLine($"[OrderController] Stack trace: {ex.StackTrace}");
                _logger.LogError(ex, "UpdateItemQuantityInOrder: Error inesperado");
                return StatusCode(500, new { error = "Error interno del servidor al actualizar la cantidad" });
            }
        }

        // POST: Order/UpdateItemInOrder
        [HttpPost]
        public async Task<IActionResult> UpdateItemInOrder([FromBody] UpdateItemDto dto)
        {
            try
            {
                Console.WriteLine($"[OrderController] UpdateItemInOrder iniciado");
                Console.WriteLine($"[OrderController] Request body: OrderId={dto?.OrderId}, ProductId={dto?.ProductId}, Quantity={dto?.Quantity}, Notes={dto?.Notes}, Status={dto?.Status}");
                
                _logger.LogInformation($"UpdateItemInOrder llamado con OrderId: {dto?.OrderId}, ProductId: {dto?.ProductId}, Quantity: {dto?.Quantity}, Notes: {dto?.Notes}, Status: {dto?.Status}");

                if (dto == null)
                {
                    Console.WriteLine($"[OrderController] ERROR: dto es null");
                    _logger.LogWarning("UpdateItemInOrder: dto es null");
                    return BadRequest(new { error = "Datos de entrada inválidos" });
                }

                if (dto.OrderId == Guid.Empty || dto.ProductId == Guid.Empty)
                {
                    Console.WriteLine($"[OrderController] ERROR: OrderId o ProductId vacío - OrderId: {dto.OrderId}, ProductId: {dto.ProductId}");
                    _logger.LogWarning($"UpdateItemInOrder: OrderId o ProductId vacío - OrderId: {dto.OrderId}, ProductId: {dto.ProductId}");
                    return BadRequest(new { error = "OrderId y ProductId son requeridos" });
                }

                if (dto.Quantity <= 0)
                {
                    Console.WriteLine($"[OrderController] ERROR: Cantidad inválida - Quantity: {dto.Quantity}");
                    _logger.LogWarning($"UpdateItemInOrder: Cantidad inválida - Quantity: {dto.Quantity}");
                    return BadRequest(new { error = "La cantidad debe ser mayor a 0" });
                }

                Console.WriteLine($"[OrderController] Llamando a UpdateItemAsync...");
                _logger.LogInformation($"UpdateItemInOrder: Llamando a UpdateItemAsync");
                var order = await _orderService.UpdateItemAsync(dto.OrderId, dto.ProductId, dto.Quantity, dto.Notes, dto.Status);

                if (order == null)
                {
                    Console.WriteLine($"[OrderController] Orden eliminada completamente");
                    _logger.LogInformation($"UpdateItemInOrder: Orden eliminada completamente");
                    return Ok(new
                    {
                        success = true,
                        message = "Item eliminado y orden cerrada (quedó vacía)",
                        orderDeleted = true,
                        orderId = dto.OrderId
                    });
                }

                Console.WriteLine($"[OrderController] UpdateItemAsync completado exitosamente");
                _logger.LogInformation($"UpdateItemInOrder: Item actualizado exitosamente");
                
                return Ok(new
                {
                    success = true,
                    message = "Item actualizado exitosamente",
                    orderId = order.Id,
                    totalAmount = order.TotalAmount
                });
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"[OrderController] KeyNotFoundException: {ex.Message}");
                _logger.LogError(ex, "UpdateItemInOrder: Orden o producto no encontrado");
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"[OrderController] InvalidOperationException: {ex.Message}");
                _logger.LogError(ex, "UpdateItemInOrder: Operación inválida");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderController] Exception: {ex.Message}");
                Console.WriteLine($"[OrderController] Stack trace: {ex.StackTrace}");
                _logger.LogError(ex, "UpdateItemInOrder: Error inesperado");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        // POST: Order/CheckAndUpdateTableStatus
        [HttpPost]
        public async Task<IActionResult> CheckAndUpdateTableStatus([FromBody] CheckTableStatusDto dto)
        {
            try
            {
                Console.WriteLine($"[OrderController] CheckAndUpdateTableStatus iniciado");
                Console.WriteLine($"[OrderController] Request body: OrderId={dto?.OrderId}");
                
                if (dto == null)
                {
                    Console.WriteLine($"[OrderController] ERROR: dto es null");
                    return BadRequest(new { error = "Datos de entrada inválidos" });
                }

                if (dto.OrderId == Guid.Empty)
                {
                    Console.WriteLine($"[OrderController] ERROR: OrderId vacío");
                    return BadRequest(new { error = "OrderId es requerido" });
                }

                Console.WriteLine($"[OrderController] Llamando a CheckAndUpdateTableStatusAsync...");
                await _orderService.CheckAndUpdateTableStatusAsync(dto.OrderId);
                
                Console.WriteLine($"[OrderController] CheckAndUpdateTableStatusAsync completado exitosamente");
                return Ok(new
                {
                    success = true,
                    message = "Estado de mesa verificado y actualizado"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderController] Exception: {ex.Message}");
                Console.WriteLine($"[OrderController] Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        public class CheckTableStatusDto
        {
            public Guid OrderId { get; set; }
        }

        // GET: Order/GetOrderStatus/{orderId}
        [HttpGet("Order/GetOrderStatus/{orderId}")]
        public async Task<IActionResult> GetOrderStatus(string orderId)
        {
            try
            {
                Console.WriteLine($"[OrderController] GetOrderStatus iniciado - orderId: {orderId}");

                if (!Guid.TryParse(orderId, out var guidOrderId))
                {
                    Console.WriteLine("[OrderController] ERROR: orderId no es un Guid válido");
                    return Json(new { success = false, error = "ID de orden inválido" });
                }

                // Primero verificar si la orden existe de forma simple
                var orderExists = await _orderService.OrderExistsAsync(guidOrderId);
                Console.WriteLine($"[OrderController] Orden existe (verificación simple): {orderExists}");
                
                if (!orderExists)
                {
                    Console.WriteLine($"[OrderController] ERROR: Orden no existe en la base de datos");
                    return Json(new { success = false, error = "Orden no encontrada" });
                }
                
                var order = await _orderService.GetOrderWithDetailsAsync(guidOrderId);
                
                Console.WriteLine($"[OrderController] GetOrderWithDetailsAsync resultado: {(order != null ? "SÍ" : "NO")}");
                
                if (order == null)
                {
                    Console.WriteLine($"[OrderController] ERROR: Orden no encontrada con GetOrderWithDetailsAsync pero existe en la base de datos");
                    
                    // Intentar obtener la orden de forma más simple
                    var simpleOrder = await _orderService.GetByIdAsync(guidOrderId);
                    Console.WriteLine($"[OrderController] Búsqueda simple resultado: {(simpleOrder != null ? "SÍ" : "NO")}");
                    if (simpleOrder != null)
                    {
                        Console.WriteLine($"[OrderController] Orden encontrada de forma simple - Status: {simpleOrder.Status}");
                        
                        // Intentar obtener los items de forma separada
                        var orderItemsFromService = await _orderService.GetOrderItemsByOrderIdAsync(guidOrderId);
                        Console.WriteLine($"[OrderController] Items obtenidos por separado: {orderItemsFromService?.Count ?? 0}");
                        
                        object itemsData;
                        if (orderItemsFromService != null)
                        {
                            itemsData = orderItemsFromService.Select(oi => new
                            {
                                id = oi.Id,
                                productId = oi.ProductId,
                                productName = oi.Product?.Name ?? "Producto desconocido",
                                quantity = oi.Quantity,
                                unitPrice = oi.UnitPrice,
                                status = oi.Status.ToString(),
                                kitchenStatus = oi.KitchenStatus.ToString(),
                                preparedAt = oi.PreparedAt,
                                preparedByStation = oi.PreparedByStation?.Name,
                                notes = oi.Notes
                            }).ToList();
                        }
                        else
                        {
                            itemsData = new List<object>();
                        }
                        
                        return Json(new { 
                            success = true, 
                            orderId = simpleOrder.Id,
                            status = simpleOrder.Status.ToString(),
                            totalAmount = simpleOrder.TotalAmount,
                            items = itemsData
                        });
                    }
                    
                    return Json(new { success = false, error = "Orden no encontrada" });
                }

                Console.WriteLine($"[OrderController] Orden encontrada - Status: {order.Status}, Items: {order.OrderItems?.Count ?? 0}");

                var orderItems = order.OrderItems.Select(oi => new
                {
                    id = oi.Id,
                    productId = oi.ProductId,
                    productName = oi.Product?.Name,
                    quantity = oi.Quantity,
                    unitPrice = oi.UnitPrice,
                    total = oi.Quantity * oi.UnitPrice,
                    status = oi.Status.ToString(),
                    kitchenStatus = oi.KitchenStatus.ToString(),
                    preparedAt = oi.PreparedAt,
                    preparedByStation = oi.PreparedByStation?.Name,
                    notes = oi.Notes
                }).ToList();

                Console.WriteLine($"[OrderController] Items procesados: {orderItems.Count}");

                return Json(new
                {
                    success = true,
                    orderId = order.Id,
                    status = order.Status.ToString(),
                    totalAmount = order.TotalAmount,
                    items = orderItems
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderController] Exception en GetOrderStatus: {ex.Message}");
                Console.WriteLine($"[OrderController] Stack trace: {ex.StackTrace}");
                _logger.LogError(ex, "Error al obtener estado de la orden");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        // POST: Order/UpdateOrderComplete
        [HttpPost]
        public async Task<IActionResult> UpdateOrderComplete([FromBody] UpdateOrderCompleteDto dto)
        {
            try
            {
                Console.WriteLine($"[OrderController] UpdateOrderComplete iniciado");
                Console.WriteLine($"[OrderController] Request body: OrderId={dto?.OrderId}, Items count={dto?.Items?.Count}");
                
                _logger.LogInformation($"UpdateOrderComplete llamado con OrderId: {dto?.OrderId}, Items count: {dto?.Items?.Count}");

                if (dto == null)
                {
                    Console.WriteLine($"[OrderController] ERROR: dto es null");
                    _logger.LogWarning("UpdateOrderComplete: dto es null");
                    return BadRequest(new { error = "Datos de entrada inválidos" });
                }

                if (dto.OrderId == Guid.Empty)
                {
                    Console.WriteLine($"[OrderController] ERROR: OrderId vacío");
                    _logger.LogWarning("UpdateOrderComplete: OrderId vacío");
                    return BadRequest(new { error = "OrderId es requerido" });
                }

                if (dto.Items == null || !dto.Items.Any())
                {
                    Console.WriteLine($"[OrderController] ERROR: No hay items para actualizar");
                    _logger.LogWarning("UpdateOrderComplete: No hay items para actualizar");
                    return BadRequest(new { error = "Debe proporcionar al menos un item" });
                }

                Console.WriteLine($"[OrderController] Llamando a UpdateOrderCompleteAsync...");
                _logger.LogInformation($"UpdateOrderComplete: Llamando a UpdateOrderCompleteAsync");
                
                var updatedOrder = await _orderService.UpdateOrderCompleteAsync(dto.OrderId, dto.Items);
                
                Console.WriteLine($"[OrderController] UpdateOrderCompleteAsync completado exitosamente");
                _logger.LogInformation($"UpdateOrderComplete: Orden actualizada exitosamente");
                
                return Ok(new
                {
                    success = true,
                    message = "Orden actualizada exitosamente",
                    orderId = updatedOrder.Id,
                    totalAmount = updatedOrder.TotalAmount,
                    itemsCount = updatedOrder.OrderItems.Count
                });
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"[OrderController] KeyNotFoundException: {ex.Message}");
                _logger.LogError(ex, "UpdateOrderComplete: Orden no encontrada");
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"[OrderController] InvalidOperationException: {ex.Message}");
                _logger.LogError(ex, "UpdateOrderComplete: Operación inválida");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderController] Exception: {ex.Message}");
                Console.WriteLine($"[OrderController] Stack trace: {ex.StackTrace}");
                _logger.LogError(ex, "UpdateOrderComplete: Error inesperado");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }
    }
} 