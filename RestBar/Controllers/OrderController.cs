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
using System.Security.Claims;

// ✅ NUEVO: DTO para marcar mesa como ocupada
public class SetTableOccupiedDto
{
    public Guid TableId { get; set; }
}

namespace RestBar.Controllers
{
    [Authorize(Policy = "OrderAccess")] // Roles: admin, manager, supervisor, waiter, cashier
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
        private readonly IUserAssignmentService _userAssignmentService;
        private readonly IOrderHubService _orderHubService;
        private readonly IEmailService _emailService;
        private readonly RestBarContext _context;
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
            IUserAssignmentService userAssignmentService,
            IOrderHubService orderHubService,
            IEmailService emailService,
            RestBarContext context,
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
            _userAssignmentService = userAssignmentService;
            _orderHubService = orderHubService;
            _emailService = emailService;
            _context = context;
            _logger = logger;
        }

        private Guid? GetUserBranchId()
        {
            var claim = User.FindFirst("BranchId")?.Value;
            return Guid.TryParse(claim, out var id) ? id : null;
        }

        private Guid? GetUserCompanyId()
        {
            var claim = User.FindFirst("CompanyId")?.Value;
            return Guid.TryParse(claim, out var id) ? id : null;
        }

        private bool HasGlobalTenantAccess()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            return role is "superadmin" or "admin" && GetUserBranchId() == null;
        }

        private async Task<bool> ValidateTableTenantAccessAsync(Guid tableId)
        {
            if (HasGlobalTenantAccess()) return true;

            var userBranchId = GetUserBranchId();
            var userCompanyId = GetUserCompanyId();
            if (userBranchId == null || userCompanyId == null) return false;

            var table = await _context.Tables.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tableId);
            if (table == null) return false;

            if (table.BranchId != userBranchId || table.CompanyId != userCompanyId)
                return false;

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == "waiter" && Guid.TryParse(User.FindFirst("UserId")?.Value, out var waiterId))
            {
                var assignment = await _userAssignmentService.GetActiveByUserIdAsync(waiterId);
                return _userAssignmentService.CanWaiterAccessTable(assignment, table);
            }

            return true;
        }

        private async Task<bool> OrderBelongsToUserBranchAsync(Guid orderId)
        {
            if (HasGlobalTenantAccess()) return true;

            var userBranchId = GetUserBranchId();
            if (userBranchId == null) return false;

            var order = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return false;

            return order.BranchId == userBranchId;
        }

        // GET: Order
        public async Task<IActionResult> Index()
        {
            try
            {
                Console.WriteLine("🔍 [OrderController] Index() - Iniciando carga de órdenes...");
                
                var orders = await _orderService.GetAllAsync();
                ViewBag.Tables = await _tableService.GetTablesForViewBagAsync();
                ViewBag.Customers = await _customerService.GetAllAsync();
                ViewBag.Products = await _productService.GetActiveProductsForViewBagAsync();
                
                Console.WriteLine($"✅ [OrderController] Index() - Órdenes cargadas: {orders?.Count() ?? 0}");
                return View(orders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [OrderController] Index() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [OrderController] Index() - StackTrace: {ex.StackTrace}");
                _logger.LogError(ex, "[OrderController] Error en Index()");
                return View("Error", new ErrorViewModel
                {
                    RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = ex.Message
                });
            }
        }

        // ✅ NUEVO: Método de prueba simple
        [HttpGet]
        public IActionResult Test()
        {
            return Content("OrderController funciona correctamente");
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
            try
            {
                Console.WriteLine("🔍 [OrderController] GetActiveCategories() - Iniciando...");
                
                var categories = await _categoryService.GetActiveCategoriesAsync();
                Console.WriteLine($"📊 [OrderController] GetActiveCategories() - Categorías obtenidas: {categories?.Count() ?? 0}");
                
                var data = categories.Select(c => new { id = c.Id, name = c.Name }).ToList();
                Console.WriteLine($"📋 [OrderController] GetActiveCategories() - Datos mapeados: {data.Count} categorías");
                
                var result = new { success = true, data };
                Console.WriteLine($"✅ [OrderController] GetActiveCategories() - Enviando respuesta con {data.Count} categorías");
                
                return Json(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [OrderController] GetActiveCategories() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [OrderController] GetActiveCategories() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error al obtener categorías: {ex.Message}" });
            }
        }

        // GET: Order/GetProductsByCategory/{categoryId}
        [HttpGet]
        public async Task<IActionResult> GetProductsByCategory([FromRoute(Name = "id")] Guid categoryId)
        {
            var userIdValue = User.FindFirst("UserId")?.Value
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdValue) || !Guid.TryParse(userIdValue, out var userId))
                return Json(new { success = false, message = "Usuario no autenticado" });

            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.BranchId == null)
                return Json(new { success = false, message = "Usuario sin sucursal asignada" });

            var products = await _context.Products
                .AsNoTracking()
                .Where(p => p.CategoryId == categoryId
                    && p.BranchId == user.BranchId
                    && p.IsActive
                    && (p.CompanyId == null || p.CompanyId == user.Branch!.CompanyId))
                .OrderBy(p => p.Name)
                .Select(p => new { id = p.Id, name = p.Name, price = p.Price, imageUrl = p.ImageUrl, isShareable = p.IsShareable, sharePortions = p.SharePortions })
                .ToListAsync();

            return Json(new { success = true, data = products });
        }

        // GET: Order/GetActiveTables
        [HttpGet]
        public async Task<IActionResult> GetActiveTables()
        {
            try
            {
                Console.WriteLine("🔍 [OrderController] GetActiveTables() - Iniciando obtención de mesas activas...");
                
                // Verificar autenticación
                Console.WriteLine($"👤 [OrderController] GetActiveTables() - Usuario autenticado: {User.Identity.IsAuthenticated}");
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.FindFirst("UserId")?.Value;
                    var userRole = User.FindFirst("UserRole")?.Value;
                    Console.WriteLine($"👤 [OrderController] GetActiveTables() - UserId: {userId}, Role: {userRole}");
                }
                else
                {
                    Console.WriteLine("❌ [OrderController] GetActiveTables() - Usuario no autenticado");
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var companyId = GetUserCompanyId();
                var branchId = GetUserBranchId();
                IEnumerable<Table> tables;

                if (companyId.HasValue && branchId.HasValue)
                {
                    Console.WriteLine($"🔍 [OrderController] GetActiveTables() - Filtrando por CompanyId={companyId}, BranchId={branchId}");
                    tables = await _tableService.GetActiveTablesByCompanyAndBranchAsync(companyId.Value, branchId.Value);
                }
                else if (HasGlobalTenantAccess())
                {
                    Console.WriteLine("🔍 [OrderController] GetActiveTables() - Acceso global, todas las mesas activas");
                    tables = await _tableService.GetActiveTablesAsync();
                }
                else
                {
                    Console.WriteLine("❌ [OrderController] GetActiveTables() - Usuario sin sucursal asignada");
                    return Json(new { success = false, message = "Usuario sin sucursal asignada" });
                }

                // Meseros: solo mesas de su asignación (área o mesas específicas)
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                if (role == "waiter" && Guid.TryParse(User.FindFirst("UserId")?.Value, out var waiterUserId))
                {
                    var assignment = await _userAssignmentService.GetActiveByUserIdAsync(waiterUserId);
                    tables = _userAssignmentService.FilterTablesForWaiter(assignment, tables);
                    Console.WriteLine($"🔒 [OrderController] GetActiveTables() - Mesero filtrado: {tables?.Count() ?? 0} mesas asignadas");
                }

                Console.WriteLine($"📊 [OrderController] GetActiveTables() - Mesas obtenidas del servicio: {tables?.Count() ?? 0}");
                
                // ✅ NUEVO: Log detallado de cada mesa
                if (tables != null && tables.Any())
                {
                    Console.WriteLine("🔍 [OrderController] GetActiveTables() - Detalle de mesas obtenidas:");
                    foreach (var table in tables)
                    {
                        Console.WriteLine($"  📋 Mesa: ID={table.Id}, Number={table.TableNumber}, Status={table.Status}, Area={table.Area?.Name}, Capacity={table.Capacity}, IsActive={table.IsActive}");
                    }
                }
                else
                {
                    Console.WriteLine("⚠️ [OrderController] GetActiveTables() - No se encontraron mesas activas");
                }
                
                Console.WriteLine("🔄 [OrderController] GetActiveTables() - Mapeando datos para respuesta...");
                var tableData = tables.Select(t => new
                {
                    id = t.Id,
                    tableNumber = t.TableNumber,
                    status = t.Status.ToString(),
                    areaName = t.Area?.Name,
                    capacity = t.Capacity,
                    isActive = t.IsActive,
                    areaId = t.AreaId
                }).ToList();
                
                Console.WriteLine($"📋 [OrderController] GetActiveTables() - Datos mapeados: {tableData.Count} mesas");
                
                // ✅ NUEVO: Log de datos finales
                Console.WriteLine("🔍 [OrderController] GetActiveTables() - Datos finales a enviar:");
                foreach (var table in tableData)
                {
                    Console.WriteLine($"  📤 Mesa: ID={table.id}, Number={table.tableNumber}, Status={table.status}, Area={table.areaName}, Capacity={table.capacity}");
                }
                
                var result = new { success = true, data = tableData };
                Console.WriteLine($"✅ [OrderController] GetActiveTables() - Enviando respuesta con {tableData.Count} mesas");
                
                return Json(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [OrderController] GetActiveTables() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [OrderController] GetActiveTables() - StackTrace: {ex.StackTrace}");
                _logger.LogError(ex, "Error al obtener mesas activas");
                return Json(new { success = false, message = "Error al obtener mesas activas" });
            }
        }

        // ✅ NUEVO: POST: Order/SetTableOccupied
        [HttpPost]
        public async Task<IActionResult> SetTableOccupied([FromBody] SetTableOccupiedDto dto)
        {
            try
            {
                Console.WriteLine($"🔍 [OrderController] SetTableOccupied() - Iniciando...");
                Console.WriteLine($"📋 [OrderController] SetTableOccupied() - TableId: {dto.TableId}");

                if (!await ValidateTableTenantAccessAsync(dto.TableId))
                    return StatusCode(403, new { success = false, message = "No autorizado para esta mesa" });
                
                var result = await _orderService.SetTableOccupiedAsync(dto.TableId);
                
                if (result)
                {
                    Console.WriteLine($"✅ [OrderController] SetTableOccupied() - Mesa marcada como ocupada exitosamente");
                    
                    // ✅ NUEVO: Notificar cambio de estado de mesa
                    await _orderHubService.NotifyTableStatusChanged(dto.TableId, "Ocupada");
                    Console.WriteLine($"📤 [OrderController] SetTableOccupied() - Notificación SignalR enviada");
                    
                    return Json(new { success = true, message = "Mesa marcada como ocupada" });
                }
                else
                {
                    Console.WriteLine($"⚠️ [OrderController] SetTableOccupied() - No se pudo marcar la mesa como ocupada");
                    return Json(new { success = false, message = "No se pudo marcar la mesa como ocupada" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [OrderController] SetTableOccupied() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [OrderController] SetTableOccupied() - StackTrace: {ex.StackTrace}");
                _logger.LogError(ex, "Error al marcar mesa como ocupada");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
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
                // Validar que TableId no sea Guid.Empty
                if (dto.TableId == Guid.Empty)
                {
                    _logger.LogWarning("SendToKitchen: TableId es Guid.Empty");
                    return BadRequest(new { error = "Debe seleccionar una mesa antes de enviar la orden." });
                }

                if (!await ValidateTableTenantAccessAsync(dto.TableId))
                    return StatusCode(403, new { error = "No autorizado para esta mesa" });

                if (dto.Items == null || !dto.Items.Any())
                    return BadRequest(new { error = "Debe agregar al menos un producto antes de enviar la orden." });

                // ✅ Obtener userId del usuario autenticado (usando claim estándar)
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    Console.WriteLine("⚠️ [OrderController] SendToKitchen() - Usuario no autenticado o UserId inválido");
                    _logger.LogWarning("SendToKitchen: Usuario no autenticado o UserId inválido");
                    return BadRequest(new { error = "Usuario no autenticado." });
                }
                
                Console.WriteLine($"✅ [OrderController] SendToKitchen() - Usuario autenticado: {userId}");
                
                var order = await _orderService.SendToKitchenAsync(dto, userId);
                
                // ✅ NUEVO: Enviar notificación por email a cocina
                try
                {
                    Console.WriteLine("📧 [OrderController] SendToKitchen() - Enviando notificación por email a cocina...");
                    
                    // Obtener emails de usuarios con roles de cocina
                    var kitchenUsers = await _context.Users
                        .Where(u => u.IsActive == true && 
                            (u.Role == UserRole.chef || u.Role == UserRole.bartender || 
                             u.Role == UserRole.admin || u.Role == UserRole.manager || 
                             u.Role == UserRole.supervisor) &&
                            !string.IsNullOrEmpty(u.Email))
                        .Select(u => u.Email!)
                        .Distinct()
                        .ToListAsync();

                    if (kitchenUsers.Any())
                    {
                        var emailSent = await _emailService.SendNewOrderNotificationAsync(order, kitchenUsers);
                        if (emailSent)
                        {
                            Console.WriteLine($"✅ [OrderController] SendToKitchen() - Notificación por email enviada a {kitchenUsers.Count} usuarios de cocina");
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ [OrderController] SendToKitchen() - No se pudo enviar notificación por email");
                        }
                    }
                    else
                    {
                        Console.WriteLine("⚠️ [OrderController] SendToKitchen() - No hay usuarios de cocina con email configurado");
                    }
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"❌ [OrderController] SendToKitchen() - Error al enviar email: {emailEx.Message}");
                    // No lanzar excepción para no afectar el flujo de envío a cocina
                }
                
                var response = new { 
                    orderId = order.Id, 
                    status = order.Status.ToString(),
                    message = "Orden enviada a cocina exitosamente"
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
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
                Guid? deliveredBy = Guid.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : null;
                await _orderService.MarkItemAsReadyAsync(dto.OrderId, dto.ItemId, deliveredBy);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET: Order/GetActiveOrder?tableId=
        [HttpGet]
        public async Task<IActionResult> GetActiveOrder([FromQuery] Guid tableId)
        {
            try
            {
                if (tableId == Guid.Empty)
                    return BadRequest(new { hasActiveOrder = false, message = "TableId requerido" });

                if (!await ValidateTableTenantAccessAsync(tableId))
                    return StatusCode(403, new { hasActiveOrder = false, message = "No autorizado para esta mesa" });

                _logger.LogInformation($"GetActiveOrder llamado con tableId: {tableId}");
                
                var order = await _orderService.GetActiveOrderByTableAsync(tableId);
                
                // Solo considerar activa la orden si tiene items no cancelados
                var activeItems = order?.OrderItems?
                    .Where(oi => oi.Status != OrderItemStatus.Cancelled)
                    .ToList() ?? new List<OrderItem>();
                if (order == null || activeItems.Count == 0)
                {
                    _logger.LogInformation($"No se encontró orden activa para tableId: {tableId}");
                    return Json(new { 
                        hasActiveOrder = false,
                        message = "No hay orden activa para esta mesa"
                    });
                }

                _logger.LogInformation($"Orden encontrada - ID: {order.Id}, Items activos: {activeItems.Count}");
                
                // 🔍 LOG DETALLADO DE ITEMS ANTES DEL MAPEO
                _logger.LogInformation("=== ITEMS ANTES DEL MAPEO ===");
                foreach (var item in activeItems)
                {
                    _logger.LogInformation($"Item: ID={item.Id}, ProductId={item.ProductId}, ProductName={item.Product?.Name}, Quantity={item.Quantity}, Status={item.Status}, KitchenStatus={item.KitchenStatus}");
                }
                _logger.LogInformation("=== FIN ITEMS ANTES DEL MAPEO ===");

                var orderItems = activeItems.Select(oi => new
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
                    notes = oi.Notes,
                    taxRate = oi.Product?.TaxRate ?? 0
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
                
                // Si es un error de "orden no encontrada", devolver respuesta específica
                if (ex.Message.Contains("No se encontró") || ex.Message.Contains("not found"))
                {
                    return Json(new { 
                        hasActiveOrder = false,
                        message = "No hay orden activa para esta mesa",
                        error = ex.Message
                    });
                }
                
                return StatusCode(500, new { 
                    error = "Error interno del servidor al obtener orden activa",
                    details = ex.Message
                });
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

        // POST: Order/ApplyDiscount — solo manager o superior
        [HttpPost]
        public async Task<IActionResult> ApplyDiscount([FromBody] ApplyDiscountDto dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value?.ToLowerInvariant();
            if (role is not ("admin" or "manager" or "supervisor"))
                return StatusCode(403, new { success = false, message = "Solo supervisor o superior puede aplicar descuentos" });

            if (dto.OrderId == Guid.Empty)
                return BadRequest(new { success = false, message = "OrderId requerido" });

            if (!await OrderBelongsToUserBranchAsync(dto.OrderId))
                return StatusCode(403, new { success = false, message = "No autorizado" });

            Guid? userId = Guid.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : null;
            try
            {
                var order = await _orderService.ApplyOrderDiscountAsync(dto.OrderId, dto.DiscountType, dto.DiscountValue, dto.Reason, userId);
                return Json(new { success = true, discountAmount = order.DiscountAmount, totalAmount = order.TotalAmount });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        public class ApplyDiscountDto
        {
            public Guid OrderId { get; set; }
            public string DiscountType { get; set; } = "fixed";
            public decimal DiscountValue { get; set; }
            public string? Reason { get; set; }
        }

        // POST: Order/SetOrderPriority — manager marca VIP/urgente
        [HttpPost]
        public async Task<IActionResult> SetOrderPriority([FromBody] SetOrderPriorityDto dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value?.ToLowerInvariant();
            if (role is not ("admin" or "manager" or "supervisor"))
                return StatusCode(403, new { success = false, message = "Solo supervisor o superior puede cambiar prioridad" });

            if (dto.OrderId == Guid.Empty)
                return BadRequest(new { success = false, message = "OrderId requerido" });

            if (!await OrderBelongsToUserBranchAsync(dto.OrderId))
                return StatusCode(403, new { success = false, message = "No autorizado" });

            Guid? userId = Guid.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : null;
            try
            {
                var order = await _orderService.SetOrderPriorityAsync(dto.OrderId, dto.Priority, dto.IsVip, userId);
                return Json(new { success = true, priority = order.Priority, isVip = order.IsVip });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        public class SetOrderPriorityDto
        {
            public Guid OrderId { get; set; }
            public int Priority { get; set; }
            public bool IsVip { get; set; }
        }

        // POST: Order/SetOrderType — cambiar DineIn/TakeOut/Delivery en orden abierta
        [HttpPost]
        public async Task<IActionResult> SetOrderType([FromBody] SetOrderTypeDto dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value?.ToLowerInvariant();
            if (role is not ("admin" or "manager" or "supervisor" or "waiter"))
                return StatusCode(403, new { success = false, message = "No autorizado" });

            if (dto.OrderId == Guid.Empty || string.IsNullOrWhiteSpace(dto.OrderType))
                return BadRequest(new { success = false, message = "OrderId y OrderType requeridos" });

            if (!await OrderBelongsToUserBranchAsync(dto.OrderId))
                return StatusCode(403, new { success = false, message = "No autorizado" });

            if (!Enum.TryParse<OrderType>(dto.OrderType, true, out var orderType))
                return BadRequest(new { success = false, message = "OrderType inválido" });

            Guid? userId = Guid.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : null;
            try
            {
                var order = await _orderService.SetOrderTypeAsync(dto.OrderId, orderType, userId);
                return Json(new { success = true, orderType = order.OrderType.ToString() });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        public class SetOrderTypeDto
        {
            public Guid OrderId { get; set; }
            public string OrderType { get; set; } = "DineIn";
        }

        // POST: Order/RegisterOutstandingDebt — supervisor registra saldo pendiente
        [HttpPost]
        public async Task<IActionResult> RegisterOutstandingDebt([FromBody] RegisterDebtDto dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value?.ToLowerInvariant();
            if (role is not ("admin" or "manager" or "supervisor"))
                return StatusCode(403, new { success = false, message = "Solo supervisor o superior puede registrar deuda" });

            if (dto.OrderId == Guid.Empty)
                return BadRequest(new { success = false, message = "OrderId requerido" });

            if (!await OrderBelongsToUserBranchAsync(dto.OrderId))
                return StatusCode(403, new { success = false, message = "No autorizado" });

            Guid? userId = Guid.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : null;
            try
            {
                var order = await _orderService.RegisterOutstandingDebtAsync(dto.OrderId, dto.Reason ?? "Saldo pendiente", userId);
                return Json(new { success = true, notes = order.Notes, status = order.Status.ToString() });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        public class RegisterDebtDto
        {
            public Guid OrderId { get; set; }
            public string? Reason { get; set; }
        }

        // POST: Order/Cancel
        [HttpPost]
        public async Task<IActionResult> Cancel([FromBody] CancelOrderDto dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "Datos de entrada inválidos" });
            if (dto.OrderId == Guid.Empty)
                return BadRequest(new { success = false, message = "OrderId es requerido" });

            var orderExists = await _context.Orders.AsNoTracking().AnyAsync(o => o.Id == dto.OrderId);
            if (!orderExists)
                return NotFound(new { success = false, message = "Orden no encontrada" });

            if (!await OrderBelongsToUserBranchAsync(dto.OrderId))
                return StatusCode(403, new { success = false, message = "No autorizado para esta orden" });

            Guid? userId = null;
            if (!string.IsNullOrEmpty(User.FindFirst("UserId")?.Value) && Guid.TryParse(User.FindFirst("UserId")?.Value, out var parsedUserId))
                userId = parsedUserId;

            try
            {
                await _orderService.CancelOrderAsync(dto.OrderId, userId, dto.Reason, dto.SupervisorId);
                return Ok(new { success = true, message = "Orden cancelada exitosamente" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(409, new { success = false, message = "La orden fue modificada por otro usuario. Actualice e intente de nuevo." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar orden");
                return StatusCode(500, new { success = false, message = ex.Message });
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
            Console.WriteLine($"🔍 ENTRADA: RemoveItemFromOrder() - OrderId: {dto?.OrderId}, ProductId: {dto?.ProductId}");
            try
            {
                _logger.LogInformation($"RemoveItemFromOrder llamado con OrderId: {dto?.OrderId}, ProductId: {dto?.ProductId}, Status: {dto?.Status}");

                if (dto == null)
                {
                    _logger.LogWarning("RemoveItemFromOrder: dto es null");
                    return BadRequest(new { error = "Datos de entrada inválidos" });
                }

                if (dto.OrderId == Guid.Empty || dto.ProductId == Guid.Empty)
                {
                    _logger.LogWarning($"RemoveItemFromOrder: OrderId o ProductId vacío - OrderId: {dto.OrderId}, ProductId: {dto.ProductId}");
                    return BadRequest(new { error = "OrderId y ProductId son requeridos" });
                }

                _logger.LogInformation($"RemoveItemFromOrder: Llamando a RemoveItemFromOrderAsync");
                var order = await _orderService.RemoveItemFromOrderAsync(dto.OrderId, dto.ProductId, dto.Status, dto.ItemId);
                
                // Si la orden fue eliminada completamente (quedó vacía)
                if (order == null)
                {
                    _logger.LogInformation($"RemoveItemFromOrder: Orden eliminada completamente");
                    return Ok(new
                    {
                        success = true,
                        message = "Item eliminado y orden cerrada (quedó vacía)",
                        orderDeleted = true,
                        orderId = dto.OrderId
                    });
                }
                
                _logger.LogInformation($"RemoveItemFromOrder: Item eliminado, recalculando total");
                // Recalcular total
                await _orderService.CalculateOrderTotalAsync(order.Id);

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
                _logger.LogWarning(ex, "Item no encontrado para eliminar");
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operación inválida al eliminar item");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al eliminar item de la orden");
                return StatusCode(500, new { error = "Error interno del servidor al eliminar el item" });
            }
        }

        // POST: Order/UpdateItemQuantityInOrder
        [HttpPost]
        [Route("Order/UpdateItemQuantity")]  // ✅ Ruta alternativa para compatibilidad con JavaScript
        [Route("Order/UpdateItemQuantityInOrder")]  // ✅ Ruta original
        public async Task<IActionResult> UpdateItemQuantityInOrder([FromBody] UpdateItemQuantityDto dto)
        {
            try
            {
                _logger.LogInformation($"UpdateItemQuantityInOrder llamado con OrderId: {dto?.OrderId}, ProductId: {dto?.ProductId}, Quantity: {dto?.Quantity}, Status: {dto?.Status}");

                if (dto == null)
                {
                    _logger.LogWarning("UpdateItemQuantityInOrder: dto es null");
                    return BadRequest(new { error = "Datos de entrada inválidos" });
                }

                if (dto.OrderId == Guid.Empty || dto.ProductId == Guid.Empty)
                {
                    _logger.LogWarning($"UpdateItemQuantityInOrder: OrderId o ProductId vacío - OrderId: {dto.OrderId}, ProductId: {dto.ProductId}");
                    return BadRequest(new { error = "OrderId y ProductId son requeridos" });
                }

                if (dto.Quantity < 0)
                {
                    _logger.LogWarning($"UpdateItemQuantityInOrder: Cantidad negativa - Quantity: {dto.Quantity}");
                    return BadRequest(new { error = "La cantidad no puede ser negativa" });
                }

                // ✅ NUEVO: Usar el método que busca por ItemId específico
                _logger.LogInformation($"UpdateItemQuantityInOrder: Llamando a UpdateItemQuantityByIdAsync");
                
                // ✅ Si se proporciona ItemId, usarlo directamente
                if (dto.ItemId.HasValue && dto.ItemId != Guid.Empty)
                {
                    var updatedOrder = await _orderService.UpdateItemQuantityByIdAsync(dto.OrderId, dto.ItemId.Value, dto.Quantity);
                    
                    // Si la cantidad es 0, la orden fue eliminada completamente
                    if (dto.Quantity == 0)
                    {
                        _logger.LogInformation($"UpdateItemQuantityInOrder: Orden eliminada completamente");
                        return Ok(new
                        {
                            success = true,
                            message = "Item eliminado y orden cerrada (quedó vacía)",
                            orderDeleted = true,
                            orderId = dto.OrderId
                        });
                    }
                    
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
                    return NotFound(new { error = "Orden no encontrada" });
                }

                var item = order.OrderItems.FirstOrDefault(oi => 
                    oi.ProductId == dto.ProductId && 
                    (string.IsNullOrEmpty(dto.Status) || oi.Status.ToString() == dto.Status)
                );

                if (item == null)
                {
                    return NotFound(new { error = "Item no encontrado" });
                }
                
                // ✅ Usar el nuevo método con ItemId específico
                var updatedOrderFallback = await _orderService.UpdateItemQuantityByIdAsync(dto.OrderId, item.Id, dto.Quantity);
                
                // Si la cantidad es 0, la orden fue eliminada completamente
                if (dto.Quantity == 0)
                {
                    _logger.LogInformation($"UpdateItemQuantityInOrder: Orden eliminada completamente");
                    return Ok(new
                    {
                        success = true,
                        message = "Item eliminado y orden cerrada (quedó vacía)",
                        orderDeleted = true,
                        orderId = dto.OrderId
                    });
                }
                
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
                _logger.LogError(ex, "UpdateItemQuantityInOrder: Orden o producto no encontrado");
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "UpdateItemQuantityInOrder: Operación inválida");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
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
                _logger.LogInformation($"UpdateItemInOrder llamado con OrderId: {dto?.OrderId}, ProductId: {dto?.ProductId}, Quantity: {dto?.Quantity}, Notes: {dto?.Notes}, Status: {dto?.Status}");

                if (dto == null)
                {
                    _logger.LogWarning("UpdateItemInOrder: dto es null");
                    return BadRequest(new { error = "Datos de entrada inválidos" });
                }

                if (dto.OrderId == Guid.Empty || dto.ProductId == Guid.Empty)
                {
                    _logger.LogWarning($"UpdateItemInOrder: OrderId o ProductId vacío - OrderId: {dto.OrderId}, ProductId: {dto.ProductId}");
                    return BadRequest(new { error = "OrderId y ProductId son requeridos" });
                }

                if (dto.Quantity <= 0)
                {
                    _logger.LogWarning($"UpdateItemInOrder: Cantidad inválida - Quantity: {dto.Quantity}");
                    return BadRequest(new { error = "La cantidad debe ser mayor a 0" });
                }

                _logger.LogInformation($"UpdateItemInOrder: Llamando a UpdateItemAsync");
                var order = await _orderService.UpdateItemAsync(dto.OrderId, dto.ProductId, dto.Quantity, dto.Notes, dto.Status);

                if (order == null)
                {
                    _logger.LogInformation($"UpdateItemInOrder: Orden eliminada completamente");
                    return Ok(new
                    {
                        success = true,
                        message = "Item eliminado y orden cerrada (quedó vacía)",
                        orderDeleted = true,
                        orderId = dto.OrderId
                    });
                }

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
                _logger.LogError(ex, "UpdateItemInOrder: Orden o producto no encontrado");
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "UpdateItemInOrder: Operación inválida");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateItemInOrder: Error inesperado");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        // POST: Order/CheckAndUpdateTableStatus
        [HttpPost]
        public async Task<IActionResult> CheckAndUpdateTableStatus([FromBody] CheckTableStatusDto dto)
        {
            Console.WriteLine($"🔍 ENTRADA: CheckAndUpdateTableStatus() - OrderId: {dto?.OrderId}");
            try
            {
                if (dto == null)
                {
                    return BadRequest(new { error = "Datos de entrada inválidos" });
                }

                if (dto.OrderId == Guid.Empty)
                {
                    return BadRequest(new { error = "OrderId es requerido" });
                }

                await _orderService.CheckAndUpdateTableStatusAsync(dto.OrderId);
                
                return Ok(new
                {
                    success = true,
                    message = "Estado de mesa verificado y actualizado"
                });
            }
            catch (Exception ex)
            {
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
                if (!Guid.TryParse(orderId, out var guidOrderId))
                {
                    return Json(new { success = false, error = "ID de orden inválido" });
                }

                // Primero verificar si la orden existe de forma simple
                var orderExists = await _orderService.OrderExistsAsync(guidOrderId);
                
                if (!orderExists)
                {
                    return NotFound(new { success = false, error = "Orden no encontrada" }); // P2-FIX-06
                }
                
                var order = await _orderService.GetOrderWithDetailsAsync(guidOrderId);
                
                if (order == null)
                {
                    // Intentar obtener la orden de forma más simple
                    var simpleOrder = await _orderService.GetByIdAsync(guidOrderId);
                    if (simpleOrder != null)
                    {
                        // Intentar obtener los items de forma separada
                        var orderItemsFromService = await _orderService.GetOrderItemsByOrderIdAsync(guidOrderId);
                        
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
                                preparedByStationId = oi.PreparedByStationId,
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
                            totalAmount = simpleOrder.TotalAmount, // items no disponibles en este path, stale aceptable
                            items = itemsData
                        });
                    }
                    
                    return NotFound(new { success = false, error = "Orden no encontrada" }); // P2-FIX-06
                }

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
                    preparedByStationId = oi.PreparedByStationId,
                    preparedByStation = oi.PreparedByStation?.Name,
                    notes = oi.Notes
                }).ToList();

                var computedTotal = Math.Round(
                    order.OrderItems
                        .Where(oi => oi.Status != OrderItemStatus.Cancelled)
                        .Sum(oi => oi.Quantity * oi.UnitPrice - oi.Discount),
                    2);

                return Json(new
                {
                    success = true,
                    orderId = order.Id,
                    status = order.Status.ToString(),
                    totalAmount = computedTotal,
                    items = orderItems
                });
            }
            catch (Exception ex)
            {
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
                _logger.LogInformation($"UpdateOrderComplete llamado con OrderId: {dto?.OrderId}, Items count: {dto?.Items?.Count}");

                if (dto == null)
                {
                    _logger.LogWarning("UpdateOrderComplete: dto es null");
                    return BadRequest(new { error = "Datos de entrada inválidos" });
                }

                if (dto.OrderId == Guid.Empty)
                {
                    _logger.LogWarning("UpdateOrderComplete: OrderId vacío");
                    return BadRequest(new { error = "OrderId es requerido" });
                }

                if (dto.Items == null || !dto.Items.Any())
                {
                    _logger.LogWarning("UpdateOrderComplete: No hay items para actualizar");
                    return BadRequest(new { error = "Debe proporcionar al menos un item" });
                }

                _logger.LogInformation($"UpdateOrderComplete: Llamando a UpdateOrderCompleteAsync");
                
                var updatedOrder = await _orderService.UpdateOrderCompleteAsync(dto.OrderId, dto.Items);
                
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
                _logger.LogError(ex, "UpdateOrderComplete: Orden no encontrada");
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "UpdateOrderComplete: Operación inválida");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateOrderComplete: Error inesperado");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        // GET: Order/StationOrders?stationType=kitchen&stationId=&areaId=
        [Authorize(Policy = "KitchenAccess")]
        public async Task<IActionResult> StationOrders(string stationType = "kitchen", Guid? stationId = null, Guid? areaId = null)
        {
            try
            {
                _logger.LogInformation("[KDS] StationOrders - type={Type}, stationId={StationId}, areaId={AreaId}", stationType, stationId, areaId);

                var userBranchId = GetUserBranchId();
                var userCompanyId = GetUserCompanyId();

                List<Guid> matchingStationIds;

                if (stationId.HasValue)
                {
                    var single = await _context.Stations.AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Id == stationId.Value && s.IsActive);
                    if (single == null)
                    {
                        TempData["ErrorMessage"] = "Estación no encontrada.";
                        return View("StationOrders", new List<KitchenOrderViewModel>());
                    }
                    if (!HasGlobalTenantAccess() && userBranchId.HasValue && single.BranchId != userBranchId)
                        return Forbid();
                    matchingStationIds = new List<Guid> { single.Id };
                    stationType = single.Type;
                }
                else
                {
                    var stationQuery = _context.Stations.AsNoTracking()
                        .Where(s => s.IsActive && s.Type.ToLower() == stationType.ToLower());

                    if (!HasGlobalTenantAccess())
                    {
                        if (userBranchId.HasValue)
                            stationQuery = stationQuery.Where(s => s.BranchId == userBranchId);
                        if (userCompanyId.HasValue)
                            stationQuery = stationQuery.Where(s => s.CompanyId == userCompanyId);
                    }
                    if (areaId.HasValue)
                        stationQuery = stationQuery.Where(s => s.AreaId == areaId);

                    matchingStationIds = await stationQuery.Select(s => s.Id).ToListAsync();
                }

                var stationDisplayName = stationId.HasValue
                    ? (await _context.Stations.AsNoTracking().Where(s => s.Id == stationId).Select(s => s.Name).FirstOrDefaultAsync() ?? stationType)
                    : stationType;

                if (matchingStationIds.Count == 0)
                    _logger.LogWarning("[KDS] StationOrders - sin estaciones para type={Type}, area={AreaId}", stationType, areaId);

                var allOrders = await _orderService.GetKitchenOrdersAsync(
                    HasGlobalTenantAccess() ? null : userBranchId,
                    HasGlobalTenantAccess() ? null : userCompanyId);

                // Solo ítems con estación asignada que coincida — sin fugas por StationId null
                var filteredOrders = allOrders
                    .Select(order => new KitchenOrderViewModel
                    {
                        OrderId        = order.OrderId,
                        TableNumber    = order.TableNumber,
                        TableAreaId    = order.TableAreaId,
                        TableAreaName  = order.TableAreaName,
                        BranchId       = order.BranchId,
                        OpenedAt       = order.OpenedAt,
                        OrderStatus    = order.OrderStatus,
                        Items          = order.Items
                            .Where(item => item.StationId.HasValue && matchingStationIds.Contains(item.StationId.Value))
                            .ToList(),
                        TotalItems     = order.TotalItems,
                        PendingItems   = order.PendingItems,
                        ReadyItems     = order.ReadyItems,
                        PreparingItems = order.PreparingItems,
                        Notes          = order.Notes
                    })
                    .Where(order => order.Items.Any())
                    .ToList();

                _logger.LogInformation("[KDS] StationOrders - {Count} órdenes para {Stations} estación(es)", filteredOrders.Count, matchingStationIds.Count);

                ViewBag.StationType  = stationType;
                ViewBag.StationTitle = $"Órdenes — {stationDisplayName}";
                ViewBag.StationId    = stationId;
                ViewBag.AreaId       = areaId;

                return View("StationOrders", filteredOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[KDS] StationOrders - error");
                TempData["ErrorMessage"] = "Error al cargar las órdenes. Intente nuevamente.";
                return View("StationOrders", new List<KitchenOrderViewModel>());
            }
        }

        // ✅ MANTENER: GET: Order/KitchenOrders - Redirige a la vista unificada
        [Authorize(Policy = "KitchenAccess")]
        public async Task<IActionResult> KitchenOrders()
        {
            return RedirectToAction("StationOrders", new { stationType = "kitchen" });
        }

        // ✅ MANTENER: GET: Order/BarOrders - Redirige a la vista unificada
        [Authorize(Policy = "KitchenAccess")]
        public async Task<IActionResult> BarOrders()
        {
            return RedirectToAction("StationOrders", new { stationType = "bar" });
        }

        // GetStationTypesFromUrl eliminado: el filtrado por tipo ahora se hace directamente
        // en StationOrders() mediante una consulta a la tabla Stations de la base de datos,
        // sin depender de strings hard-coded ni mappings manuales.

        [HttpPost]
        public async Task<IActionResult> UpdateItemStation([FromBody] UpdateItemStationDto dto)
        {
            if (dto == null || dto.OrderId == Guid.Empty || dto.ItemId == Guid.Empty || dto.NewStationId == Guid.Empty)
                return BadRequest(new { success = false, message = "OrderId, ItemId y NewStationId son requeridos" });

            if (!await OrderBelongsToUserBranchAsync(dto.OrderId))
                return StatusCode(403, new { success = false, message = "No autorizado para esta orden" });

            Guid? userId = null;
            if (Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsed))
                userId = parsed;

            try
            {
                var item = await _orderService.UpdateItemStationAsync(dto.OrderId, dto.ItemId, dto.NewStationId, userId);
                return Ok(new
                {
                    success = true,
                    message = "Estación actualizada",
                    itemId = item.Id,
                    stationId = item.PreparedByStationId
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estación de ítem");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        public class UpdateItemStationDto
        {
            public Guid OrderId { get; set; }
            public Guid ItemId { get; set; }
            public Guid NewStationId { get; set; }
        }

        // ✅ NUEVO: POST: Order/UpdateItemStatus - Para actualizar estado de items
        [HttpPost]
        public async Task<IActionResult> UpdateItemStatus([FromBody] UpdateItemStatusDto dto)
        {
            Console.WriteLine($"🔍 ENTRADA: UpdateItemStatus() - ItemId: {dto?.ItemId}, OrderId: {dto?.OrderId}, Status: {dto?.Status}");
            try
            {
                Console.WriteLine($"🔍 [OrderController] UpdateItemStatus() - Iniciando...");
                Console.WriteLine($"📋 [OrderController] UpdateItemStatus() - ItemId: {dto.ItemId}, OrderId: {dto.OrderId}, Status: {dto.Status}");
                
                // ✅ CORREGIDO: Manejar diferentes estados
                switch (dto.Status.ToLower())
                {
                    case "ready":
                        await _orderService.MarkItemAsReadyAsync(dto.OrderId, dto.ItemId);
                        Console.WriteLine($"✅ [OrderController] UpdateItemStatus() - Item marcado como Ready");
                        break;
                        
                    case "cancelled":
                        Guid? cancelUserId = Guid.TryParse(User.FindFirst("UserId")?.Value, out var cuid) ? cuid : null;
                        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                        await _orderService.CancelOrderItemAsync(dto.OrderId, dto.ItemId, cancelUserId, userRole, dto.SupervisorId);
                        Console.WriteLine($"✅ [OrderController] UpdateItemStatus() - Item cancelado");
                        break;
                        
                    case "preparing":
                        await _orderService.MarkItemAsPreparingAsync(dto.OrderId, dto.ItemId);
                        Console.WriteLine($"✅ [OrderController] UpdateItemStatus() - Item marcado como Preparing");
                        break;
                        
                    default:
                        Console.WriteLine($"⚠️ [OrderController] UpdateItemStatus() - Estado no reconocido: {dto.Status}");
                        return Json(new { success = false, message = $"Estado no válido: {dto.Status}" });
                }
                
                // ✅ NUEVO: Notificar cambio de estado via SignalR
                await _orderHubService.NotifyOrderItemStatusChanged(dto.OrderId, dto.ItemId, 
                    dto.Status == "Cancelled" ? OrderItemStatus.Cancelled : 
                    dto.Status == "Ready" ? OrderItemStatus.Ready : OrderItemStatus.Preparing);
                
                Console.WriteLine($"✅ [OrderController] UpdateItemStatus() - Notificación SignalR enviada");
                
                return Json(new { success = true, message = "Item actualizado exitosamente" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"❌ [OrderController] UpdateItemStatus() - Error: {ex.Message}");
                _logger.LogWarning(ex, "Error de validación al actualizar estado del item");
                return StatusCode(400, new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"❌ [OrderController] UpdateItemStatus() - No encontrado: {ex.Message}");
                _logger.LogWarning(ex, "Item no encontrado al actualizar estado");
                return StatusCode(404, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [OrderController] UpdateItemStatus() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [OrderController] UpdateItemStatus() - StackTrace: {ex.StackTrace}");
                _logger.LogError(ex, "Error al actualizar estado del item");
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // ✅ NUEVO: POST: Order/CompleteOrder - Para completar órdenes
        [HttpPost]
        public async Task<IActionResult> CompleteOrder([FromBody] CompleteOrderDto dto)
        {
            try
            {
                Console.WriteLine($"🔍 [OrderController] CompleteOrder() - Iniciando...");
                Console.WriteLine($"📋 [OrderController] CompleteOrder() - OrderId: {dto.OrderId}");
                
                // Marcar todos los items como listos
                var order = await _orderService.GetOrderWithDetailsAsync(dto.OrderId);
                if (order != null)
                {
                    Console.WriteLine($"📊 [OrderController] CompleteOrder() - Items a marcar como listos: {order.OrderItems.Count(oi => oi.KitchenStatus != KitchenStatus.Ready)}");
                    
                    foreach (var item in order.OrderItems.Where(oi => oi.KitchenStatus != KitchenStatus.Ready))
                    {
                        await _orderService.MarkItemAsReadyAsync(dto.OrderId, item.Id);
                    }
                    
                    // ✅ NUEVO: Verificar y actualizar estado de mesa después de completar orden
                    await _orderService.CheckAndUpdateTableStatusAsync(dto.OrderId);
                    Console.WriteLine($"✅ [OrderController] CompleteOrder() - Estado de mesa verificado y actualizado");
                    
                    // ✅ NUEVO: Obtener la orden actualizada para enviar notificaciones correctas
                    var updatedOrder = await _orderService.GetOrderWithDetailsAsync(dto.OrderId);
                    if (updatedOrder != null && updatedOrder.Table != null)
                    {
                        // ✅ NUEVO: Notificar cambio de estado de mesa via SignalR
                        await _orderHubService.NotifyTableStatusChanged(updatedOrder.Table.Id, updatedOrder.Table.Status.ToString());
                        Console.WriteLine($"📤 [OrderController] CompleteOrder() - Notificación de mesa enviada: {updatedOrder.Table.Status}");
                    }
                    
                    // ✅ NUEVO: Notificar que la orden fue completada
                    await _orderHubService.NotifyOrderCompleted(dto.OrderId, order.Table?.TableNumber ?? "N/A");
                    Console.WriteLine($"✅ [OrderController] CompleteOrder() - Notificación de orden completada enviada");
                }
                
                Console.WriteLine($"✅ [OrderController] CompleteOrder() - Orden completada exitosamente");
                return Json(new { success = true, message = "Orden completada exitosamente" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [OrderController] CompleteOrder() - Error: {ex.Message}");
                _logger.LogError(ex, "Error al completar orden");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // DTOs para las nuevas acciones
        public class UpdateItemStatusDto
        {
            public Guid ItemId { get; set; }
            public Guid OrderId { get; set; }
            public string Status { get; set; } = string.Empty;
            public Guid? SupervisorId { get; set; }
        }

        public class CompleteOrderDto
        {
            public Guid OrderId { get; set; }
        }

        // GET: Order/GetPaymentHistory/{orderId}
        [HttpGet]
        public async Task<IActionResult> GetPaymentHistory(Guid orderId)
        {
            try
            {
                // 🎯 LOG ESTRATÉGICO: OBTENIENDO HISTORIAL DE PAGOS
                Console.WriteLine($"🚀 [OrderController] GetPaymentHistory() - OBTENIENDO HISTORIAL - OrderId: {orderId}");
                
                Console.WriteLine($"🔍 [OrderController] GetPaymentHistory() - Iniciando obtención de historial de pagos...");
                Console.WriteLine($"📋 [OrderController] GetPaymentHistory() - OrderId: {orderId}");

                if (orderId == Guid.Empty)
                {
                    Console.WriteLine("⚠️ [OrderController] GetPaymentHistory() - OrderId inválido");
                    return BadRequest(new { success = false, message = "OrderId inválido" });
                }

                // Obtener la orden con sus pagos usando el servicio
                var order = await _orderService.GetOrderWithPaymentsAsync(orderId);

                if (order == null)
                {
                    Console.WriteLine($"⚠️ [OrderController] GetPaymentHistory() - Orden no encontrada: {orderId}");
                    return NotFound(new { success = false, message = "Orden no encontrada" });
                }

                // Mapear pagos a DTO
                var payments = order.Payments.Select(p => new
                {
                    id = p.Id,
                    amount = p.Amount,
                    method = p.Method,
                    createdAt = p.CreatedAt,
                    isVoided = p.IsVoided,
                    paidAt = p.PaidAt,
                    payerName = p.PayerName,
                    status = p.Status
                }).OrderByDescending(p => p.createdAt).ToList();

                Console.WriteLine($"📊 [OrderController] GetPaymentHistory() - Total pagos encontrados: {payments.Count}");
                Console.WriteLine("✅ [OrderController] GetPaymentHistory() - Historial obtenido exitosamente");

                return Ok(new
                {
                    success = true,
                    payments = payments
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [OrderController] GetPaymentHistory() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [OrderController] GetPaymentHistory() - StackTrace: {ex.StackTrace}");
                
                _logger.LogError(ex, "Error al obtener historial de pagos para orden {OrderId}", orderId);
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        // 🎯 MÉTODO ESTRATÉGICO: OBTENER ITEMS DE UNA ORDEN
        // ✅ NUEVO: ENDPOINT PARA VERIFICAR DISPONIBILIDAD DE STOCK ANTES DE AGREGAR ITEMS
        /// <summary>
        /// Verifica la disponibilidad de stock para un producto antes de agregarlo a una orden
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckItemStockAvailability(Guid productId, decimal quantity, Guid? orderId = null)
        {
            try
            {
                Console.WriteLine($"🔍 [OrderController] CheckItemStockAvailability() - ProductId: {productId}, Quantity: {quantity}, OrderId: {orderId}");
                
                // Obtener usuario actual para BranchId
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(Guid.Parse(userIdClaim.Value));
                if (currentUser?.Branch == null)
                {
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }

                var product = await _productService.GetByIdAsync(productId);
                if (product == null)
                {
                    return Json(new { success = false, message = "Producto no encontrado" });
                }

                // Si el producto no controla inventario, siempre está disponible
                if (!product.TrackInventory)
                {
                    return Json(new { 
                        success = true, 
                        hasStock = true, 
                        availableStock = -1, // Stock ilimitado
                        isUnlimited = true,
                        message = "El producto no controla inventario"
                    });
                }

                // Verificar stock disponible
                var hasStock = await _productService.HasStockAvailableAsync(productId, quantity, currentUser.BranchId);
                var availableStock = await _productService.GetAvailableStockAsync(productId, currentUser.BranchId);

                // Encontrar la mejor estación si hay stock
                Guid? bestStationId = null;
                string? bestStationName = null;
                decimal stockInStation = 0;

                if (hasStock)
                {
                    bestStationId = await _productService.FindBestStationForProductAsync(productId, quantity, currentUser.BranchId);
                    if (bestStationId.HasValue)
                    {
                        var station = await _context.Stations.FindAsync(bestStationId.Value);
                        bestStationName = station?.Name;
                        stockInStation = await _productService.GetStockInStationAsync(productId, bestStationId.Value, currentUser.BranchId);
                    }
                }

                Console.WriteLine($"✅ [OrderController] CheckItemStockAvailability() - HasStock: {hasStock}, AvailableStock: {availableStock}, BestStationId: {bestStationId}");
                return Json(new { 
                    success = true, 
                    hasStock = hasStock, 
                    availableStock = availableStock,
                    isUnlimited = availableStock == -1,
                    bestStationId = bestStationId,
                    bestStationName = bestStationName,
                    stockInStation = stockInStation,
                    message = hasStock 
                        ? "Stock disponible" 
                        : $"Stock insuficiente. Disponible: {availableStock}, Requerido: {quantity}"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [OrderController] CheckItemStockAvailability() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [OrderController] CheckItemStockAvailability() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderItems(Guid orderId)
        {
            try
            {
                Console.WriteLine($"🔍 [OrderController] GetOrderItems() - Obteniendo items de orden...");
                Console.WriteLine($"📋 [OrderController] GetOrderItems() - OrderId: {orderId}");

                if (!await OrderBelongsToUserBranchAsync(orderId))
                    return StatusCode(403, new { success = false, message = "No autorizado para esta orden" });

                var order = await _orderService.GetOrderWithDetailsAsync(orderId);
                if (order == null)
                {
                    Console.WriteLine($"⚠️ [OrderController] GetOrderItems() - Orden no encontrada: {orderId}");
                    return Json(new { success = false, message = "Orden no encontrada" });
                }

                var items = order.OrderItems?.Select(item => new
                {
                    id = item.Id,
                    productName = item.Product?.Name ?? "Producto desconocido",
                    quantity = item.Quantity,
                    unitPrice = item.UnitPrice,
                    discount = item.Discount,
                    total = (item.Quantity * item.UnitPrice) - item.Discount,
                    notes = item.Notes,
                    status = item.Status.ToString(),
                    assignedToPersonId = item.AssignedToPersonId,
                    assignedToPersonName = item.AssignedToPersonName,
                    isShared = item.IsShared
                }).ToList();

                Console.WriteLine($"📊 [OrderController] GetOrderItems() - {items?.Count ?? 0} items obtenidos");
                
                return Json(new { 
                    success = true, 
                    data = items,
                    message = "Items de orden obtenidos exitosamente"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [OrderController] GetOrderItems() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [OrderController] GetOrderItems() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error al obtener items de orden: {ex.Message}" });
            }
        }

        public class MoveOrderToTableDto
        {
            public Guid OrderId { get; set; }
            public Guid TargetTableId { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> MoveToTable([FromBody] MoveOrderToTableDto dto)
        {
            if (dto == null || dto.OrderId == Guid.Empty || dto.TargetTableId == Guid.Empty)
                return BadRequest(new { success = false, message = "OrderId y TargetTableId son requeridos" });

            if (!await OrderBelongsToUserBranchAsync(dto.OrderId))
                return StatusCode(403, new { success = false, message = "No autorizado para esta orden" });

            if (!await ValidateTableTenantAccessAsync(dto.TargetTableId))
                return StatusCode(403, new { success = false, message = "No autorizado para la mesa destino" });

            Guid? userId = null;
            if (Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsedUserId))
                userId = parsedUserId;

            try
            {
                var order = await _orderService.MoveOrderToTableAsync(dto.OrderId, dto.TargetTableId, userId);
                return Ok(new
                {
                    success = true,
                    message = "Orden movida exitosamente",
                    orderId = order.Id,
                    tableId = order.TableId
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al mover orden de mesa");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}