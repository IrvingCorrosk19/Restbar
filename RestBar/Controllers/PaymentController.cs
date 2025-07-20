using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestBar.Interfaces;
using RestBar.Models;
using RestBar.ViewModel;
using RestBar.Services;
using System.ComponentModel.DataAnnotations;

namespace RestBar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "PaymentAccess")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ISplitPaymentService _splitPaymentService;
        private readonly IOrderService _orderService;
        private readonly RestBarContext _context;
        private readonly IOrderHubService _orderHubService;
        private readonly IProductService _productService;
        private readonly IInventoryService _inventoryService;

        public PaymentController(
            IPaymentService paymentService,
            ISplitPaymentService splitPaymentService,
            IOrderService orderService,
            RestBarContext context,
            IOrderHubService orderHubService,
            IProductService productService,
            IInventoryService inventoryService)
        {
            Console.WriteLine($"[PaymentController] 🔥🔥🔥 CONSTRUCTOR PaymentController LLAMADO 🔥🔥🔥");
            _paymentService = paymentService;
            _splitPaymentService = splitPaymentService;
            _orderService = orderService;
            _context = context;
            _orderHubService = orderHubService;
            _productService = productService;
            _inventoryService = inventoryService;
            Console.WriteLine($"[PaymentController] ✅ Constructor completado - ProductService: {_productService != null}, InventoryService: {_inventoryService != null}");
        }

        [HttpPost("partial")]
        public async Task<IActionResult> CreatePartialPayment([FromBody] PaymentRequestDto request)
        {
            try
            {
                Console.WriteLine($"[PaymentController] 🔥🔥🔥 MÉTODO CreatePartialPayment LLAMADO 🔥🔥🔥");
                Console.WriteLine($"[PaymentController] === INICIANDO PROCESAMIENTO DE PAGO ===");
                Console.WriteLine($"[PaymentController] OrderId: {request.OrderId}");
                Console.WriteLine($"[PaymentController] Amount: ${request.Amount}");
                Console.WriteLine($"[PaymentController] Method: {request.Method}");
                Console.WriteLine($"[PaymentController] IsShared: {request.IsShared}");
                Console.WriteLine($"[PaymentController] PayerName: {request.PayerName}");
                Console.WriteLine($"[PaymentController] Split Payments Count: {request.SplitPayments?.Count ?? 0}");
                
                // ✅ NUEVO: Verificar stock antes de procesar el pago
                Console.WriteLine($"[PaymentController] Verificando stock disponible...");
                var order = await _orderService.GetOrderWithDetailsAsync(request.OrderId);
                if (order == null)
                {
                    Console.WriteLine($"[PaymentController] ERROR: Orden no encontrada");
                    return NotFound("Orden no encontrada");
                }
                Console.WriteLine($"[PaymentController] ✅ Orden encontrada - Items: {order.OrderItems?.Count ?? 0}");

                // Verificar stock para cada item de la orden
                foreach (var item in order.OrderItems)
                {
                    if (item.ProductId.HasValue)
                    {
                        var product = await _productService.GetByIdAsync(item.ProductId.Value);
                        if (product != null && product.Stock.HasValue)
                        {
                            Console.WriteLine($"[PaymentController] Verificando stock para {product.Name}: Disponible {product.Stock}, Requerido {item.Quantity}");
                            
                            if (product.Stock < item.Quantity)
                            {
                                Console.WriteLine($"[PaymentController] ❌ ERROR: Stock insuficiente para {product.Name}");
                                Console.WriteLine($"[PaymentController] Stock disponible: {product.Stock}, Cantidad requerida: {item.Quantity}");
                                return BadRequest($"Stock insuficiente para {product.Name}. Disponible: {product.Stock}, Requerido: {item.Quantity}");
                            }
                            
                            if (product.Stock <= 0)
                            {
                                Console.WriteLine($"[PaymentController] ❌ ERROR: Producto {product.Name} está agotado");
                                return BadRequest($"El producto '{product.Name}' está agotado.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[PaymentController] ⚠️ WARNING: Producto {item.Product?.Name} no tiene stock configurado");
                        }
                    }
                }
                Console.WriteLine($"[PaymentController] ✅ Verificación de stock completada - Stock suficiente para todos los productos");
                
                // Validar lógica de pagos compartidos
                if (request.IsShared)
                {
                    Console.WriteLine($"[PaymentController] Validando pago compartido...");
                    
                    if (request.Method != "Compartido")
                    {
                        Console.WriteLine($"[PaymentController] ERROR: Pago compartido debe tener método 'Compartido', recibido: {request.Method}");
                        return BadRequest("Para pagos compartidos, el método debe ser 'Compartido'");
                    }
                    
                    if (request.SplitPayments == null || request.SplitPayments.Count == 0)
                    {
                        Console.WriteLine($"[PaymentController] ERROR: Pago compartido debe tener al menos un split payment");
                        return BadRequest("Para pagos compartidos, debe especificar al menos una persona");
                    }
                    
                    Console.WriteLine($"[PaymentController] ✅ Validación de pago compartido exitosa");
                }
                else
                {
                    Console.WriteLine($"[PaymentController] Validando pago individual...");
                    
                    if (request.Method == "Compartido")
                    {
                        Console.WriteLine($"[PaymentController] ERROR: Pago individual no puede tener método 'Compartido'");
                        return BadRequest("Para pagos individuales, no se puede usar el método 'Compartido'");
                    }
                    
                    Console.WriteLine($"[PaymentController] ✅ Validación de pago individual exitosa");
                }

                // Validar que el monto no exceda el total de la orden
                Console.WriteLine($"[PaymentController] Calculando montos de la orden...");
                var totalPaid = await _paymentService.GetTotalPaymentsByOrderAsync(request.OrderId);
                var orderTotal = order.OrderItems?.Sum(i => i.Quantity * i.UnitPrice) ?? 0;
                var remainingAmount = orderTotal - totalPaid;
                
                Console.WriteLine($"[PaymentController] Total orden: ${orderTotal:F2}");
                Console.WriteLine($"[PaymentController] Total pagado: ${totalPaid:F2}");
                Console.WriteLine($"[PaymentController] Monto restante: ${remainingAmount:F2}");

                if (request.Amount > remainingAmount)
                {
                    Console.WriteLine($"[PaymentController] ERROR: El monto ${request.Amount:F2} excede el saldo pendiente ${remainingAmount:F2}");
                    return BadRequest($"El monto excede el saldo pendiente. Saldo: ${remainingAmount:F2}");
                }

                // Validar split payments antes de crear el pago principal
                if (request.SplitPayments != null && request.SplitPayments.Any())
                {
                    Console.WriteLine($"[PaymentController] Validando pagos divididos...");
                    var splitTotal = request.SplitPayments.Sum(sp => sp.Amount);
                    Console.WriteLine($"[PaymentController] Suma de split payments: ${splitTotal:F2}");
                    Console.WriteLine($"[PaymentController] Monto total del pago: ${request.Amount:F2}");
                    
                    for (int i = 0; i < request.SplitPayments.Count; i++)
                    {
                        var split = request.SplitPayments[i];
                        Console.WriteLine($"[PaymentController] Split {i + 1}: {split.PersonName} - ${split.Amount:F2}");
                    }
                    
                    if (Math.Abs(splitTotal - request.Amount) > 0.01m)
                    {
                        Console.WriteLine($"[PaymentController] ERROR: La suma de split payments ${splitTotal:F2} no coincide con el monto total ${request.Amount:F2}");
                        return BadRequest("La suma de los pagos divididos debe ser igual al monto total");
                    }
                    Console.WriteLine($"[PaymentController] ✅ Validación de split payments exitosa");
                }

                // ✅ NUEVO: Verificar stock ANTES de crear el pago (sin reducir aún)
                Console.WriteLine($"[PaymentController] Verificando stock disponible para todos los items...");
                
                try
                {
                    foreach (var item in order.OrderItems)
                    {
                        if (item.ProductId.HasValue)
                        {
                            var product = await _productService.GetByIdAsync(item.ProductId.Value);
                            if (product != null && product.Stock.HasValue)
                            {
                                // Verificar stock disponible
                                if (product.Stock < item.Quantity)
                                {
                                    Console.WriteLine($"[PaymentController] ❌ ERROR: Stock insuficiente para {product.Name}");
                                    throw new InvalidOperationException($"Stock insuficiente para {product.Name}. Disponible: {product.Stock}, Requerido: {item.Quantity}");
                                }
                                
                                Console.WriteLine($"[PaymentController] ✅ Stock suficiente para {product.Name}: {product.Stock} >= {item.Quantity}");
                            }
                        }
                    }
                    
                    Console.WriteLine($"[PaymentController] ✅ Verificación de stock completada - Stock suficiente para todos los productos");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PaymentController] ❌ ERROR al verificar stock: {ex.Message}");
                    return BadRequest($"Error al verificar stock: {ex.Message}");
                }

                // Crear el pago principal
                Console.WriteLine($"[PaymentController] Creando pago principal...");
                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    OrderId = request.OrderId,
                    Amount = request.Amount,
                    Method = request.Method,
                    IsShared = request.IsShared,
                    PayerName = request.PayerName,
                    PaidAt = DateTime.UtcNow,
                    IsVoided = false
                };
                Console.WriteLine($"[PaymentController] Pago principal creado con ID: {payment.Id}");

                var createdPayment = await _paymentService.CreateAsync(payment);
                Console.WriteLine($"[PaymentController] ✅ Pago principal guardado exitosamente");

                // Crear pagos divididos si es pago compartido
                if (request.IsShared && request.SplitPayments != null && request.SplitPayments.Any())
                {
                    Console.WriteLine($"[PaymentController] Creando {request.SplitPayments.Count} pagos divididos...");
                    
                    for (int i = 0; i < request.SplitPayments.Count; i++)
                    {
                        var splitRequest = request.SplitPayments[i];
                        Console.WriteLine($"[PaymentController] Creando split payment {i + 1}: {splitRequest.PersonName} - ${splitRequest.Amount:F2} - {splitRequest.Method}");
                        
                        var splitPayment = new SplitPayment
                        {
                            Id = Guid.NewGuid(),
                            PaymentId = createdPayment.Id,
                            PersonName = splitRequest.PersonName,
                            Amount = splitRequest.Amount,
                            Method = splitRequest.Method
                        };
                        Console.WriteLine($"[PaymentController] Split payment creado con ID: {splitPayment.Id}");

                        await _splitPaymentService.CreateAsync(splitPayment);
                        Console.WriteLine($"[PaymentController] ✅ Split payment {i + 1} guardado exitosamente");
                    }
                    
                    Console.WriteLine($"[PaymentController] ✅ Todos los split payments creados exitosamente");
                }

                // Verificar si la orden está completamente pagada
                var totalPaidAfterPayment = await _paymentService.GetTotalPaymentsByOrderAsync(request.OrderId);
                var orderTotalAfterPayment = order.OrderItems?.Sum(i => i.Quantity * i.UnitPrice) ?? 0;
                var isFullyPaid = totalPaidAfterPayment >= orderTotalAfterPayment;

                Console.WriteLine($"[PaymentController] Verificando pago completo - Total pagado: ${totalPaidAfterPayment}, Total orden: ${orderTotalAfterPayment}, Pagado completo: {isFullyPaid}");

                // Actualizar estado de la orden según el pago
                if (isFullyPaid)
                {
                    // Pago completo: cambiar orden a Completed
                    Console.WriteLine($"[PaymentController] Pago completo - Cambiando orden de {order.Status} a Completed");
                    order.Status = OrderStatus.Completed;
                    order.ClosedAt = DateTime.UtcNow;
                    
                    // ✅ NUEVO: Descontar stock cuando la orden se completa
                    Console.WriteLine($"[PaymentController] 🔥🔥🔥 DESCONTANDO STOCK AL COMPLETAR ORDEN 🔥🔥🔥");
                    var completedOrderReducedItems = new List<(Guid productId, decimal quantity)>();
                    
                    try
                    {
                        foreach (var item in order.OrderItems)
                        {
                            if (item.ProductId.HasValue)
                            {
                                // ✅ NUEVO: Usar el método DecrementStockAsync del InventoryService
                                var success = await _inventoryService.DecrementStockAsync(item.ProductId.Value, item.Quantity, _orderHubService);
                                
                                if (success)
                                {
                                    completedOrderReducedItems.Add((item.ProductId.Value, item.Quantity));
                                    Console.WriteLine($"[PaymentController] ✅ Stock descontado exitosamente para item: {item.Product?.Name}");
                                }
                                else
                                {
                                    Console.WriteLine($"[PaymentController] ❌ ERROR: No se pudo descontar stock para {item.Product?.Name}");
                                    throw new InvalidOperationException($"No se pudo descontar stock para {item.Product?.Name}");
                                }
                            }
                        }
                        
                        Console.WriteLine($"[PaymentController] ✅ Stock actualizado en base de datos al completar orden");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[PaymentController] ❌ ERROR al descontar stock al completar orden: {ex.Message}");
                        
                        // Rollback: restaurar stock de los items que ya se procesaron
                        Console.WriteLine($"[PaymentController] Realizando rollback del stock...");
                        foreach (var (productId, quantity) in completedOrderReducedItems)
                        {
                            try
                            {
                                var product = await _productService.GetByIdAsync(productId);
                                if (product != null && product.Stock.HasValue)
                                {
                                    product.Stock += quantity;
                                    _context.Products.Update(product);
                                    Console.WriteLine($"[PaymentController] ✅ Stock restaurado para producto {product.Name}");
                                }
                            }
                            catch (Exception restoreEx)
                            {
                                Console.WriteLine($"[PaymentController] ERROR al restaurar stock para producto {productId}: {restoreEx.Message}");
                            }
                        }
                        
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"[PaymentController] ✅ Rollback completado");
                        
                        return BadRequest($"Error al procesar stock: {ex.Message}");
                    }
                    
                    // Cambiar todos los items a Served
                    foreach (var item in order.OrderItems)
                    {
                        if (item.Status == OrderItemStatus.Ready)
                        {
                            item.Status = OrderItemStatus.Served;
                            Console.WriteLine($"[PaymentController] Item {item.Product?.Name} cambiado a Served");
                        }
                    }
                    
                    // Actualizar el estado de la mesa
                    if (order.TableId.HasValue)
                    {
                        var table = await _context.Tables.FindAsync(order.TableId.Value);
                        if (table != null)
                        {
                            table.Status = "Disponible";
                            Console.WriteLine($"[PaymentController] Mesa {table.TableNumber} cambiada a Disponible");
                        }
                    }
                }
                else
                {
                    // Pago parcial: cambiar orden a Served si estaba en ReadyToPay
                    if (order.Status == OrderStatus.ReadyToPay)
                    {
                        Console.WriteLine($"[PaymentController] Pago parcial - Cambiando orden de {order.Status} a Served");
                        order.Status = OrderStatus.Served;
                    }
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"[PaymentController] Estados actualizados - Orden: {order.Status}");

                // Enviar notificaciones SignalR sobre cambios de estado
                if (isFullyPaid)
                {
                    // Notificar que la orden está completada
                    await _orderHubService.NotifyOrderStatusChanged(order.Id, order.Status);
                    Console.WriteLine($"[PaymentController] Notificación enviada: Orden completada");
                    
                    // Notificar cambio de estado de cada item individual
                    foreach (var item in order.OrderItems)
                    {
                        if (item.Status == OrderItemStatus.Served)
                        {
                            await _orderHubService.NotifyOrderItemStatusChanged(order.Id, item.Id, item.Status);
                            Console.WriteLine($"[PaymentController] Notificación enviada: Item {item.Product?.Name} cambiado a Served");
                        }
                    }
                    
                    // Notificar cambio de estado de mesa
                    if (order.TableId.HasValue)
                    {
                        await _orderHubService.NotifyTableStatusChanged(order.TableId.Value, "Disponible");
                        Console.WriteLine($"[PaymentController] Notificación enviada: Mesa disponible");
                    }
                    
                    // Notificar actualización general de cocina para refrescar vistas
                    await _orderHubService.NotifyKitchenUpdate();
                    Console.WriteLine($"[PaymentController] Notificación enviada: Actualización general de cocina");
                }
                else
                {
                    // Notificar cambio de estado de orden (pago parcial)
                    await _orderHubService.NotifyOrderStatusChanged(order.Id, order.Status);
                    Console.WriteLine($"[PaymentController] Notificación enviada: Orden en estado {order.Status}");
                    
                    // Notificar actualización general para refrescar resumen
                    await _orderHubService.NotifyKitchenUpdate();
                    Console.WriteLine($"[PaymentController] Notificación enviada: Actualización general para pago parcial");
                }
                
                // Notificar específicamente sobre el pago procesado
                await _orderHubService.NotifyPaymentProcessed(order.Id, request.Amount, request.Method, isFullyPaid);
                Console.WriteLine($"[PaymentController] Notificación enviada: Pago procesado - ${request.Amount} ({request.Method}) - Completo: {isFullyPaid}");

                // Obtener el pago con sus splits para la respuesta
                var paymentWithSplits = await _paymentService.GetPaymentWithSplitsAsync(createdPayment.Id);
                
                var response = new PaymentResponseDto
                {
                    Id = paymentWithSplits!.Id,
                    OrderId = paymentWithSplits.OrderId!.Value,
                    Amount = paymentWithSplits.Amount,
                    Method = paymentWithSplits.Method!,
                    PaidAt = paymentWithSplits.PaidAt!.Value,
                    IsVoided = paymentWithSplits.IsVoided ?? false,
                    IsShared = paymentWithSplits.IsShared,
                    PayerName = paymentWithSplits.PayerName,
                    SplitPayments = paymentWithSplits.SplitPayments.Select(sp => new SplitPaymentResponseDto
                    {
                        Id = sp.Id,
                        PersonName = sp.PersonName!,
                        Amount = sp.Amount!.Value,
                        Method = sp.Method!
                    }).ToList()
                };

                Console.WriteLine($"[PaymentController] ✅ Pago procesado exitosamente");
                Console.WriteLine($"[PaymentController] === FIN PROCESAMIENTO DE PAGO EXITOSO ===");
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PaymentController] ❌ ERROR CRÍTICO en CreatePartialPayment:");
                Console.WriteLine($"[PaymentController] Error Type: {ex.GetType().Name}");
                Console.WriteLine($"[PaymentController] Error Message: {ex.Message}");
                Console.WriteLine($"[PaymentController] Stack Trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[PaymentController] Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"[PaymentController] Inner Stack Trace: {ex.InnerException.StackTrace}");
                }
                
                Console.WriteLine($"[PaymentController] === FIN ERROR CRÍTICO ===");
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("order/{orderId}/summary")]
        public async Task<IActionResult> GetOrderPaymentSummary(Guid orderId)
        {
            try
            {
                var order = await _orderService.GetOrderWithDetailsAsync(orderId);
                if (order == null)
                {
                    return NotFound("Orden no encontrada");
                }

                var payments = await _paymentService.GetByOrderIdAsync(orderId);
                var totalPaid = await _paymentService.GetTotalPaymentsByOrderAsync(orderId);
                var orderTotal = order.OrderItems?.Sum(i => i.Quantity * i.UnitPrice) ?? 0;
                var remainingAmount = orderTotal - totalPaid;

                var summary = new OrderPaymentSummaryDto
                {
                    OrderId = orderId,
                    TotalOrderAmount = orderTotal,
                    TotalPaidAmount = totalPaid,
                    RemainingAmount = remainingAmount,
                    Payments = payments.Select(p => new PaymentResponseDto
                    {
                        Id = p.Id,
                        OrderId = p.OrderId!.Value,
                        Amount = p.Amount,
                        Method = p.Method!,
                        PaidAt = p.PaidAt!.Value,
                        IsVoided = p.IsVoided ?? false,
                        SplitPayments = p.SplitPayments.Select(sp => new SplitPaymentResponseDto
                        {
                            Id = sp.Id,
                            PersonName = sp.PersonName!,
                            Amount = sp.Amount!.Value
                        }).ToList()
                    }).ToList()
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetOrderPayments(Guid orderId)
        {
            try
            {
                var payments = await _paymentService.GetByOrderIdAsync(orderId);
                var response = payments.Select(p => new PaymentResponseDto
                {
                    Id = p.Id,
                    OrderId = p.OrderId!.Value,
                    Amount = p.Amount,
                    Method = p.Method!,
                    PaidAt = p.PaidAt!.Value,
                    IsVoided = p.IsVoided ?? false,
                    SplitPayments = p.SplitPayments.Select(sp => new SplitPaymentResponseDto
                    {
                        Id = sp.Id,
                        PersonName = sp.PersonName!,
                        Amount = sp.Amount!.Value
                    }).ToList()
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpDelete("{paymentId}")]
        public async Task<IActionResult> VoidPayment(Guid paymentId)
        {
            try
            {
                Console.WriteLine($"[PaymentController] VoidPayment endpoint llamado");
                Console.WriteLine($"[PaymentController] paymentId: {paymentId}");
                
                await _paymentService.VoidPaymentAsync(paymentId);
                
                Console.WriteLine($"[PaymentController] Pago anulado exitosamente");
                
                return Ok(new
                {
                    success = true,
                    message = "Pago anulado correctamente"
                });
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"[PaymentController] ERROR: Pago no encontrado - {ex.Message}");
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"[PaymentController] ERROR: Operación inválida - {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PaymentController] ERROR interno del servidor: {ex.Message}");
                Console.WriteLine($"[PaymentController] Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }
    }
} 