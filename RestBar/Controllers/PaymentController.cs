using Microsoft.AspNetCore.Mvc;
using RestBar.Interfaces;
using RestBar.Models;
using RestBar.ViewModel;
using RestBar.Services;
using System.ComponentModel.DataAnnotations;

namespace RestBar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ISplitPaymentService _splitPaymentService;
        private readonly IOrderService _orderService;
        private readonly RestBarContext _context;
        private readonly IOrderHubService _orderHubService;

        public PaymentController(
            IPaymentService paymentService,
            ISplitPaymentService splitPaymentService,
            IOrderService orderService,
            RestBarContext context,
            IOrderHubService orderHubService)
        {
            _paymentService = paymentService;
            _splitPaymentService = splitPaymentService;
            _orderService = orderService;
            _context = context;
            _orderHubService = orderHubService;
        }

        [HttpPost("partial")]
        public async Task<IActionResult> CreatePartialPayment([FromBody] PaymentRequestDto request)
        {
            try
            {
                // Validar que la orden existe
                var order = await _orderService.GetOrderWithDetailsAsync(request.OrderId);
                if (order == null)
                {
                    return NotFound("Orden no encontrada");
                }

                // Validar que el monto no exceda el total de la orden
                var totalPaid = await _paymentService.GetTotalPaymentsByOrderAsync(request.OrderId);
                var orderTotal = order.OrderItems?.Sum(i => i.Quantity * i.UnitPrice) ?? 0;
                var remainingAmount = orderTotal - totalPaid;

                if (request.Amount > remainingAmount)
                {
                    return BadRequest($"El monto excede el saldo pendiente. Saldo: ${remainingAmount:F2}");
                }

                // Crear el pago
                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    OrderId = request.OrderId,
                    Amount = request.Amount,
                    Method = request.Method,
                    PaidAt = DateTime.UtcNow,
                    IsVoided = false
                };

                var createdPayment = await _paymentService.CreateAsync(payment);

                // Crear pagos divididos si se proporcionan
                if (request.SplitPayments != null && request.SplitPayments.Any())
                {
                    var splitTotal = request.SplitPayments.Sum(sp => sp.Amount);
                    if (Math.Abs(splitTotal - request.Amount) > 0.01m)
                    {
                        return BadRequest("La suma de los pagos divididos debe ser igual al monto total");
                    }

                    foreach (var splitRequest in request.SplitPayments)
                    {
                        var splitPayment = new SplitPayment
                        {
                            Id = Guid.NewGuid(),
                            PaymentId = createdPayment.Id,
                            PersonName = splitRequest.PersonName,
                            Amount = splitRequest.Amount
                        };

                        await _splitPaymentService.CreateAsync(splitPayment);
                    }
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
                    SplitPayments = paymentWithSplits.SplitPayments.Select(sp => new SplitPaymentResponseDto
                    {
                        Id = sp.Id,
                        PersonName = sp.PersonName!,
                        Amount = sp.Amount!.Value
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
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