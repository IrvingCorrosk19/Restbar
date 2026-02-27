using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
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
        private readonly IEmailService _emailService;

        public PaymentController(
            IPaymentService paymentService,
            ISplitPaymentService splitPaymentService,
            IOrderService orderService,
            RestBarContext context,
            IOrderHubService orderHubService,
            IProductService productService,
            IEmailService emailService)
        {
            _paymentService = paymentService;
            _splitPaymentService = splitPaymentService;
            _orderService = orderService;
            _context = context;
            _orderHubService = orderHubService;
            _productService = productService;
            _emailService = emailService;
        }

        [HttpPost("partial")]
        public async Task<IActionResult> CreatePartialPayment([FromBody] PaymentRequestDto request)
        {
            if (request == null)
                return BadRequest(new { success = false, message = "Datos de pago inválidos" });

            if (request.OrderId == Guid.Empty)
                return BadRequest(new { success = false, message = "OrderId es requerido" });

            if (request.Amount <= 0)
                return BadRequest(new { success = false, message = "El monto debe ser mayor a 0" });

            try
            {
                var order = await _orderService.GetOrderWithDetailsAsync(request.OrderId);
                if (order == null)
                    return NotFound(new { success = false, message = "Orden no encontrada" });

                // Regla de negocio: no pagar órdenes canceladas o ya completadas
                if (order.Status == OrderStatus.Cancelled)
                    return BadRequest(new { success = false, message = "No se puede pagar una orden cancelada" });

                if (order.Status == OrderStatus.Completed)
                    return BadRequest(new { success = false, message = "La orden ya está completada y pagada" });

                if (request.IsShared)
                {
                    if (request.Method != "Compartido")
                        return BadRequest(new { success = false, message = "Para pagos compartidos, el método debe ser 'Compartido'" });
                    if (request.SplitPayments == null || request.SplitPayments.Count == 0)
                        return BadRequest(new { success = false, message = "Para pagos compartidos, debe especificar al menos una persona" });
                }
                else
                {
                    if (request.Method == "Compartido")
                        return BadRequest(new { success = false, message = "Para pagos individuales, no use el método 'Compartido'" });
                }

                // Total de la orden solo con ítems no cancelados (regla de negocio POS)
                var payableItems = order.OrderItems?.Where(oi => oi.Status != OrderItemStatus.Cancelled) ?? Enumerable.Empty<OrderItem>();
                var orderTotal = payableItems.Sum(i => i.Quantity * i.UnitPrice - i.Discount);

                // Pre-validación de split payments (no requiere DB)
                if (request.SplitPayments != null && request.SplitPayments.Any())
                {
                    var splitTotal = request.SplitPayments.Sum(sp => sp.Amount);
                    if (Math.Abs(splitTotal - request.Amount) > 0.01m)
                        return BadRequest(new { success = false, message = "La suma de los pagos divididos debe ser igual al monto total" });
                }

                Payment createdPayment;
                bool isFullyPaid;
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // FIX: totalPaid re-leído DENTRO de la transacción para evitar race condition.
                        // Aunque el ConcurrencyToken (Version) protege la consistencia en DB,
                        // validar aquí evita el error confuso "modificado por otro usuario".
                        var totalPaid = await _paymentService.GetTotalPaymentsByOrderAsync(request.OrderId);
                        var remainingAmount = orderTotal - totalPaid;

                        if (remainingAmount <= 0)
                        {
                            await transaction.RollbackAsync();
                            return BadRequest(new { success = false, message = "La orden ya está pagada por completo" });
                        }

                        if (request.Amount > remainingAmount + 0.01m)
                        {
                            await transaction.RollbackAsync();
                            return BadRequest(new { success = false, message = $"El monto excede el saldo pendiente. Saldo: ${remainingAmount:F2}" });
                        }

                        var payment = new Payment
                        {
                            Id = Guid.NewGuid(),
                            OrderId = request.OrderId,
                            Amount = request.Amount,
                            Method = request.Method ?? "Efectivo",
                            IsShared = request.IsShared,
                            PayerName = request.PayerName,
                            PaidAt = DateTime.UtcNow,
                            IsVoided = false
                        };
                        _context.Payments.Add(payment);

                        if (request.IsShared && request.SplitPayments != null && request.SplitPayments.Any())
                        {
                            foreach (var splitRequest in request.SplitPayments)
                            {
                                _context.SplitPayments.Add(new SplitPayment
                                {
                                    Id = Guid.NewGuid(),
                                    PaymentId = payment.Id,
                                    PersonName = splitRequest.PersonName,
                                    Amount = splitRequest.Amount,
                                    Method = splitRequest.Method ?? "Efectivo"
                                });
                            }
                        }

                        await _context.SaveChangesAsync();

                        var totalPaidAfterPayment = totalPaid + request.Amount;
                        var orderTotalForComparison = payableItems.Sum(i => i.Quantity * i.UnitPrice - i.Discount);
                        isFullyPaid = totalPaidAfterPayment >= orderTotalForComparison - 0.01m;

                        var hasPendingItems = order.OrderItems?.Any(oi => oi.Status != OrderItemStatus.Cancelled && (oi.Status == OrderItemStatus.Pending || oi.Status == OrderItemStatus.Preparing)) ?? false;
                        var hasReadyItems = order.OrderItems?.Any(oi => oi.Status != OrderItemStatus.Cancelled && oi.Status == OrderItemStatus.Ready) ?? false;
                        var allItemsReadyOrServed = order.OrderItems?.Where(oi => oi.Status != OrderItemStatus.Cancelled).All(oi => oi.Status == OrderItemStatus.Ready || oi.Status == OrderItemStatus.Served) ?? false;

                        if (isFullyPaid)
                        {
                            if (allItemsReadyOrServed)
                            {
                                order.Status = OrderStatus.Completed;
                                order.ClosedAt = DateTime.UtcNow;
                                foreach (var item in order.OrderItems.Where(oi => oi.Status == OrderItemStatus.Ready))
                                    item.Status = OrderItemStatus.Served;
                                if (order.TableId.HasValue)
                                {
                                    var table = await _context.Tables.FindAsync(order.TableId.Value);
                                    if (table != null) table.Status = TableStatus.Disponible;
                                }
                            }
                            else if (hasPendingItems || hasReadyItems)
                            {
                                order.Status = OrderStatus.ReadyToPay;
                                foreach (var item in order.OrderItems.Where(oi => oi.Status == OrderItemStatus.Ready))
                                    item.Status = OrderItemStatus.Served;
                                if (order.TableId.HasValue)
                                {
                                    var table = await _context.Tables.FindAsync(order.TableId.Value);
                                    if (table != null) table.Status = TableStatus.ParaPago;
                                }
                            }
                            else
                            {
                                order.Status = OrderStatus.Completed;
                                order.ClosedAt = DateTime.UtcNow;
                                if (order.TableId.HasValue)
                                {
                                    var table = await _context.Tables.FindAsync(order.TableId.Value);
                                    if (table != null) table.Status = TableStatus.Disponible;
                                }
                            }
                        }
                        else
                        {
                            if (hasPendingItems || hasReadyItems)
                            {
                                if (order.Status != OrderStatus.ReadyToPay && order.Status != OrderStatus.Completed)
                                {
                                    order.Status = OrderStatus.ReadyToPay;
                                    if (order.TableId.HasValue)
                                    {
                                        var table = await _context.Tables.FindAsync(order.TableId.Value);
                                        if (table != null && table.Status != TableStatus.EnPreparacion) table.Status = TableStatus.ParaPago;
                                    }
                                }
                            }
                            else if (allItemsReadyOrServed && order.Status != OrderStatus.Completed && order.Status != OrderStatus.Served)
                            {
                                order.Status = OrderStatus.Served;
                                foreach (var item in order.OrderItems.Where(oi => oi.Status == OrderItemStatus.Ready))
                                    item.Status = OrderItemStatus.Served;
                                if (order.TableId.HasValue)
                                {
                                    var table = await _context.Tables.FindAsync(order.TableId.Value);
                                    if (table != null) table.Status = TableStatus.ParaPago;
                                }
                            }
                        }

                        order.Version++;
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        createdPayment = payment;
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }

                // Notificaciones fuera de la transacción (no críticas para consistencia)
                if (isFullyPaid)
                {
                    // Notificar que la orden está completada
                    await _orderHubService.NotifyOrderStatusChanged(order.Id, order.Status);
                    foreach (var item in order.OrderItems.Where(oi => oi.Status == OrderItemStatus.Served))
                        await _orderHubService.NotifyOrderItemStatusChanged(order.Id, item.Id, item.Status);
                    if (order.TableId.HasValue)
                        await _orderHubService.NotifyTableStatusChanged(order.TableId.Value, "Disponible");
                    await _orderHubService.NotifyKitchenUpdate();
                }
                else
                {
                    await _orderHubService.NotifyOrderStatusChanged(order.Id, order.Status);
                    await _orderHubService.NotifyKitchenUpdate();
                }
                await _orderHubService.NotifyPaymentProcessed(order.Id, request.Amount, request.Method ?? "Efectivo", isFullyPaid);

                if (isFullyPaid)
                {
                    try
                    {
                        var customerEmail = order.Customer?.Email ?? (await _context.Users.FindAsync(order.UserId))?.Email;
                        if (!string.IsNullOrEmpty(customerEmail))
                            await _emailService.SendOrderConfirmationAsync(order, customerEmail);
                    }
                    catch { /* no fallar el flujo por email */ }
                }

                var paymentWithSplits = await _paymentService.GetPaymentWithSplitsAsync(createdPayment.Id);
                var responseDto = new PaymentResponseDto
                {
                    Id = paymentWithSplits!.Id,
                    OrderId = paymentWithSplits.OrderId!.Value,
                    Amount = paymentWithSplits.Amount,
                    Method = paymentWithSplits.Method!,
                    PaidAt = paymentWithSplits.PaidAt,
                    IsVoided = paymentWithSplits.IsVoided,
                    IsShared = paymentWithSplits.IsShared,
                    PayerName = paymentWithSplits.PayerName,
                    SplitPayments = paymentWithSplits.SplitPayments?.Select(sp => new SplitPaymentResponseDto
                    {
                        Id = sp.Id,
                        PersonName = sp.PersonName!,
                        Amount = sp.Amount ?? 0,
                        Method = sp.Method!
                    }).ToList() ?? new List<SplitPaymentResponseDto>()
                };

                return Ok(new { success = true, isFullyPaid, message = isFullyPaid ? "Pago completado" : "Pago parcial registrado", payment = responseDto });
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(409, new { success = false, message = "La orden fue modificada por otro usuario. Actualice e intente de nuevo." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("order/{orderId}/summary")]
        public async Task<IActionResult> GetOrderPaymentSummary(Guid orderId)
        {
            try
            {
                var order = await _orderService.GetOrderWithDetailsAsync(orderId);
                if (order == null)
                    return NotFound(new { success = false, message = "Orden no encontrada" });

                var payableItems = order.OrderItems?.Where(oi => oi.Status != OrderItemStatus.Cancelled) ?? Enumerable.Empty<OrderItem>();
                var orderTotal = payableItems.Sum(i => i.Quantity * i.UnitPrice - i.Discount);
                var totalPaid = await _paymentService.GetTotalPaymentsByOrderAsync(orderId);
                var remainingAmount = Math.Max(0m, orderTotal - totalPaid); // P0-FIX-01 defensive: never negative
                var isOverpaid = totalPaid > orderTotal;
                var overpaidAmount = isOverpaid ? Math.Max(0m, totalPaid - orderTotal) : (decimal?)null;

                var payments = await _paymentService.GetByOrderIdAsync(orderId);
                var summary = new OrderPaymentSummaryDto
                {
                    OrderId = orderId,
                    TotalOrderAmount = orderTotal,
                    TotalPaidAmount = totalPaid,
                    RemainingAmount = remainingAmount,
                    IsOverpaid = isOverpaid ? true : null,
                    OverpaidAmount = overpaidAmount,
                    WarningCode = isOverpaid ? "OVERPAID" : null,
                    Payments = payments.Select(p => new PaymentResponseDto
                    {
                        Id = p.Id,
                        OrderId = p.OrderId!.Value,
                        Amount = p.Amount,
                        Method = p.Method!,
                        PaidAt = p.PaidAt,
                        IsVoided = p.IsVoided,
                        IsShared = p.IsShared,       // P2-FIX-05
                        PayerName = p.PayerName,     // P2-FIX-05
                        SplitPayments = p.SplitPayments?.Select(sp => new SplitPaymentResponseDto
                        {
                            Id = sp.Id,
                            PersonName = sp.PersonName!,
                            Amount = sp.Amount ?? 0,
                            Method = sp.Method ?? string.Empty  // P2-FIX-05
                        }).ToList() ?? new List<SplitPaymentResponseDto>()
                    }).ToList()
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
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
                    PaidAt = p.PaidAt,
                    IsVoided = p.IsVoided,
                    SplitPayments = p.SplitPayments?.Select(sp => new SplitPaymentResponseDto
                    {
                        Id = sp.Id,
                        PersonName = sp.PersonName!,
                        Amount = sp.Amount ?? 0
                    }).ToList() ?? new List<SplitPaymentResponseDto>()
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
                
                // ✅ NUEVO: Obtener pago antes de anular para notificaciones
                var payment = await _paymentService.GetPaymentWithSplitsAsync(paymentId);
                if (payment == null)
                {
                    return NotFound(new { error = "Pago no encontrado" });
                }
                
                var orderId = payment.OrderId;
                
                await _paymentService.VoidPaymentAsync(paymentId);
                
                Console.WriteLine($"[PaymentController] Pago anulado exitosamente");
                
                // ✅ NUEVO: Obtener orden actualizada para notificaciones
                if (orderId.HasValue)
                {
                    var order = await _orderService.GetByIdAsync(orderId.Value);
                    if (order != null)
                    {
                        // Notificar cambios de estado de orden
                        await _orderHubService.NotifyOrderStatusChanged(order.Id, order.Status);
                        Console.WriteLine($"[PaymentController] Notificación enviada: Estado de orden actualizado a {order.Status}");
                        
                        // Notificar cambio de mesa si existe
                        if (order.TableId.HasValue)
                        {
                            var table = await _context.Tables.FindAsync(order.TableId.Value);
                            if (table != null)
                            {
                                await _orderHubService.NotifyTableStatusChanged(table.Id, table.Status.ToString());
                                Console.WriteLine($"[PaymentController] Notificación enviada: Mesa {table.TableNumber} actualizada");
                            }
                        }
                        
                        // Notificar actualización general
                        await _orderHubService.NotifyKitchenUpdate();
                        await _orderHubService.NotifyPaymentProcessed(order.Id, -payment.Amount, payment.Method ?? "Anulado", false);
                        Console.WriteLine($"[PaymentController] Notificaciones SignalR enviadas");
                    }
                }
                
                return Ok(new
                {
                    success = true,
                    message = "Pago anulado correctamente"
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
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(409, new { success = false, message = "La orden fue modificada simultáneamente. Intente de nuevo." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }
    }
} 