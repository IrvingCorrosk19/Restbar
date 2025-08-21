using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestBar.Interfaces;
using RestBar.Models;
using RestBar.ViewModel;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic; // Added for List

namespace RestBar.Controllers
{
    [Authorize(Policy = "PaymentAccess")]
    public class PaymentViewController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly RestBarContext _context;

        public PaymentViewController(
            IPaymentService paymentService,
            IOrderService orderService,
            RestBarContext context)
        {
            _paymentService = paymentService;
            _orderService = orderService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> DashboardStats()
        {
            try
            {
                var today = DateTime.Today;
                var monthStart = new DateTime(today.Year, today.Month, 1);

                // Get total revenue for current month
                var monthlyPayments = await _paymentService.GetPaymentsByDateRangeAsync(monthStart, today.AddDays(1));
                var totalRevenue = monthlyPayments.Sum(p => p.Amount);

                // Get total orders paid
                var totalOrders = monthlyPayments.Select(p => p.OrderId).Distinct().Count();

                // Get pending payments count
                var pendingOrders = await _orderService.GetPendingPaymentOrdersAsync();
                var pendingPayments = pendingOrders.Count();

                return Json(new
                {
                    success = true,
                    totalRevenue = totalRevenue,
                    totalOrders = totalOrders,
                    pendingPayments = pendingPayments
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> RecentPayments(string dateFilter = "month", string statusFilter = "all", string methodFilter = "all")
        {
            try
            {
                var startDate = GetStartDate(dateFilter);
                var endDate = DateTime.Now;

                var payments = (await _paymentService.GetPaymentsByDateRangeAsync(startDate, endDate)).ToList();

                // Apply filters
                if (statusFilter != "all")
                {
                    payments = payments.Where(p => p.Status.ToUpper() == statusFilter.ToUpper()).ToList();
                }

                if (methodFilter != "all")
                {
                    payments = payments.Where(p => p.Method == methodFilter).ToList();
                }

                var paymentData = payments.Select(p => new
                {
                    id = p.Id,
                    orderNumber = p.Order?.OrderNumber ?? "N/A",
                    tableNumber = p.Order?.Table?.TableNumber ?? "N/A",
                    amount = p.Amount,
                    method = p.Method,
                    status = p.Status,
                    createdAt = p.CreatedAt,
                    payerName = p.PayerName
                }).OrderByDescending(p => p.createdAt).Take(50).ToList();

                return Json(new
                {
                    success = true,
                    data = paymentData
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PendingPayments()
        {
            try
            {
                var pendingOrders = await _orderService.GetPendingPaymentOrdersAsync();

                var pendingData = pendingOrders.Select(o => new
                {
                    id = o.Id,
                    orderNumber = o.OrderNumber,
                    tableNumber = o.Table?.TableNumber ?? "N/A",
                    total = o.OrderItems?.Sum(i => i.Quantity * i.UnitPrice) ?? 0,
                    paidAmount = _paymentService.GetTotalPaymentsByOrderAsync(o.Id).Result,
                    pendingAmount = (o.OrderItems?.Sum(i => i.Quantity * i.UnitPrice) ?? 0) - _paymentService.GetTotalPaymentsByOrderAsync(o.Id).Result,
                    itemsCount = o.OrderItems?.Count ?? 0,
                    status = o.Status.ToString()
                }).ToList();

                return Json(new
                {
                    success = true,
                    data = pendingData
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Analytics()
        {
            try
            {
                var today = DateTime.Today;
                var monthStart = new DateTime(today.Year, today.Month, 1);

                // Payment methods analytics
                var monthlyPayments = (await _paymentService.GetPaymentsByDateRangeAsync(monthStart, today.AddDays(1))).ToList();
                var paymentMethods = monthlyPayments
                    .GroupBy(p => p.Method)
                    .Select(g => new
                    {
                        method = g.Key,
                        amount = g.Sum(p => p.Amount),
                        count = g.Count()
                    })
                    .OrderByDescending(x => x.amount)
                    .ToList();

                // Daily sales for current week
                var weekStart = today.AddDays(-(int)today.DayOfWeek);
                var dailySales = new List<object>();
                for (int i = 0; i < 7; i++)
                {
                    var date = weekStart.AddDays(i);
                    var dayPayments = (await _paymentService.GetPaymentsByDateRangeAsync(date, date.AddDays(1))).ToList();
                    var dayTotal = dayPayments.Sum(p => p.Amount);
                    
                    dailySales.Add(new
                    {
                        date = date.ToString("ddd"),
                        amount = dayTotal
                    });
                }

                // Monthly performance (last 6 months)
                var monthlyPerformance = new List<object>();
                for (int i = 5; i >= 0; i--)
                {
                    var month = today.AddMonths(-i);
                    var monthStartDate = new DateTime(month.Year, month.Month, 1);
                    var monthEndDate = monthStartDate.AddMonths(1);
                    var monthPayments = (await _paymentService.GetPaymentsByDateRangeAsync(monthStartDate, monthEndDate)).ToList();
                    var monthTotal = monthPayments.Sum(p => p.Amount);
                    
                    monthlyPerformance.Add(new
                    {
                        month = month.ToString("MMM"),
                        amount = monthTotal
                    });
                }

                return Json(new
                {
                    success = true,
                    paymentMethods = paymentMethods,
                    dailySales = dailySales,
                    monthlyPerformance = monthlyPerformance
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentDetails(Guid paymentId)
        {
            try
            {
                var payment = await _paymentService.GetByIdAsync(paymentId);
                if (payment == null)
                {
                    return Json(new { success = false, message = "Pago no encontrado" });
                }

                var order = payment.OrderId.HasValue ? await _orderService.GetOrderWithDetailsAsync(payment.OrderId.Value) : null;
                var orderItems = order?.OrderItems?.Select(i => new
                {
                    productName = i.Product?.Name ?? "Producto no encontrado",
                    quantity = i.Quantity,
                    unitPrice = i.UnitPrice,
                    total = i.Quantity * i.UnitPrice
                }).ToList();
                
                var orderItemsList = orderItems?.Cast<object>().ToList() ?? new List<object>();

                var paymentData = new
                {
                    id = payment.Id,
                    orderNumber = order?.OrderNumber ?? "N/A",
                    tableNumber = order?.Table?.TableNumber ?? "N/A",
                    amount = payment.Amount,
                    method = payment.Method,
                    status = payment.Status,
                    createdAt = payment.CreatedAt,
                    payerName = payment.PayerName,
                    orderItems = orderItemsList
                };

                return Json(new
                {
                    success = true,
                    data = paymentData
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePayment(Guid paymentId, [FromBody] UpdatePaymentDto dto)
        {
            try
            {
                var payment = await _paymentService.GetByIdAsync(paymentId);
                if (payment == null)
                {
                    return Json(new { success = false, message = "Pago no encontrado" });
                }

                // Update payment properties
                payment.Amount = dto.Amount;
                payment.Method = dto.Method;
                payment.PayerName = dto.PayerName;
                // Note: Payment model doesn't have UpdatedAt property

                await _paymentService.UpdateAsync(payment);

                return Json(new
                {
                    success = true,
                    message = "Pago actualizado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PrintReceipt(Guid paymentId)
        {
            try
            {
                var payment = await _paymentService.GetByIdAsync(paymentId);
                if (payment == null)
                {
                    return Json(new { success = false, message = "Pago no encontrado" });
                }

                var order = payment.OrderId.HasValue ? await _orderService.GetOrderWithDetailsAsync(payment.OrderId.Value) : null;

                // Generate receipt HTML
                var receiptHtml = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <title>Recibo - Orden #{order?.OrderNumber}</title>
                        <style>
                            body {{ font-family: Arial, sans-serif; margin: 20px; }}
                            .receipt {{ max-width: 400px; margin: 0 auto; border: 1px solid #ccc; padding: 20px; }}
                            .header {{ text-align: center; border-bottom: 2px solid #000; padding-bottom: 10px; margin-bottom: 20px; }}
                            .item {{ display: flex; justify-content: space-between; margin: 5px 0; }}
                            .total {{ border-top: 1px solid #000; padding-top: 10px; margin-top: 20px; font-weight: bold; }}
                            .footer {{ text-align: center; margin-top: 20px; font-size: 12px; }}
                        </style>
                    </head>
                    <body>
                        <div class='receipt'>
                            <div class='header'>
                                <h2>RestBar</h2>
                                <p>Recibo de Pago</p>
                                <p>Fecha: {payment.CreatedAt:dd/MM/yyyy HH:mm}</p>
                            </div>
                            <div>
                                <p><strong>Orden:</strong> #{order?.OrderNumber}</p>
                                <p><strong>Mesa:</strong> {order?.Table?.TableNumber}</p>
                                <p><strong>Método de Pago:</strong> {payment.Method}</p>
                                <p><strong>Pagador:</strong> {payment.PayerName ?? "N/A"}</p>
                            </div>
                            <div class='total'>
                                <div class='item'>
                                    <span>Total Pagado:</span>
                                    <span>${payment.Amount:F2}</span>
                                </div>
                            </div>
                            <div class='footer'>
                                <p>¡Gracias por su visita!</p>
                                <p>Pago ID: {payment.Id}</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                // Return HTML content
                return Content(receiptHtml, "text/html");
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetails(Guid orderId)
        {
            try
            {
                var order = await _orderService.GetOrderWithDetailsAsync(orderId);
                if (order == null)
                {
                    return Json(new { success = false, message = "Orden no encontrada" });
                }

                var paidAmount = await _paymentService.GetTotalPaymentsByOrderAsync(orderId);
                var pendingAmount = (order.OrderItems?.Sum(i => i.Quantity * i.UnitPrice) ?? 0) - paidAmount;

                var orderData = new
                {
                    id = order.Id,
                    orderNumber = order.OrderNumber,
                    tableNumber = order.Table?.TableNumber ?? "N/A",
                    total = order.OrderItems?.Sum(i => i.Quantity * i.UnitPrice) ?? 0,
                    paidAmount = paidAmount,
                    pendingAmount = pendingAmount,
                    status = order.Status.ToString(),
                    createdAt = order.OpenedAt,
                    orderItems = order.OrderItems?.Select(i => new
                    {
                        productName = i.Product?.Name ?? "Producto no encontrado",
                        quantity = i.Quantity,
                        unitPrice = i.UnitPrice,
                        total = i.Quantity * i.UnitPrice
                    }).Cast<object>().ToList() ?? new List<object>()
                };

                return Json(new
                {
                    success = true,
                    data = orderData
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PrintOrder(Guid orderId)
        {
            try
            {
                var order = await _orderService.GetOrderWithDetailsAsync(orderId);
                if (order == null)
                {
                    return Json(new { success = false, message = "Orden no encontrada" });
                }

                var paidAmount = await _paymentService.GetTotalPaymentsByOrderAsync(orderId);
                var pendingAmount = (order.OrderItems?.Sum(i => i.Quantity * i.UnitPrice) ?? 0) - paidAmount;

                // Generate order HTML
                var orderHtml = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <title>Orden #{order.OrderNumber}</title>
                        <style>
                            body {{ font-family: Arial, sans-serif; margin: 20px; }}
                            .order {{ max-width: 500px; margin: 0 auto; border: 1px solid #ccc; padding: 20px; }}
                            .header {{ text-align: center; border-bottom: 2px solid #000; padding-bottom: 10px; margin-bottom: 20px; }}
                            .item {{ display: flex; justify-content: space-between; margin: 5px 0; }}
                            .total {{ border-top: 1px solid #000; padding-top: 10px; margin-top: 20px; font-weight: bold; }}
                            .footer {{ text-align: center; margin-top: 20px; font-size: 12px; }}
                            table {{ width: 100%; border-collapse: collapse; }}
                            th, td {{ padding: 8px; text-align: left; border-bottom: 1px solid #ddd; }}
                            th {{ background-color: #f2f2f2; }}
                        </style>
                    </head>
                    <body>
                        <div class='order'>
                            <div class='header'>
                                <h2>RestBar</h2>
                                <p>Orden #{order.OrderNumber}</p>
                                <p>Fecha: {order.OpenedAt:dd/MM/yyyy HH:mm}</p>
                            </div>
                            <div>
                                <p><strong>Mesa:</strong> {order.Table?.TableNumber}</p>
                                <p><strong>Estado:</strong> {order.Status}</p>
                            </div>
                            <div>
                                <h4>Items de la Orden</h4>
                                <table>
                                    <thead>
                                        <tr>
                                            <th>Producto</th>
                                            <th>Cantidad</th>
                                            <th>Precio Unit.</th>
                                            <th>Total</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {string.Join("", order.OrderItems?.Select((OrderItem i) => $@"
                                            <tr>
                                                <td>{i.Product?.Name ?? "N/A"}</td>
                                                <td>{i.Quantity}</td>
                                                <td>${i.UnitPrice:F2}</td>
                                                <td>${(i.Quantity * i.UnitPrice):F2}</td>
                                            </tr>") ?? new List<string>())}
                                    </tbody>
                                </table>
                            </div>
                            <div class='total'>
                                <div class='item'>
                                    <span>Total Orden:</span>
                                    <span>${(order.OrderItems?.Sum(i => i.Quantity * i.UnitPrice) ?? 0):F2}</span>
                                </div>
                                <div class='item'>
                                    <span>Pagado:</span>
                                    <span>${paidAmount:F2}</span>
                                </div>
                                <div class='item'>
                                    <span>Pendiente:</span>
                                    <span>${pendingAmount:F2}</span>
                                </div>
                            </div>
                            <div class='footer'>
                                <p>Orden ID: {order.Id}</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                // Return HTML content
                return Content(orderHtml, "text/html");
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportPayments(string dateFilter = "month", string statusFilter = "all", string methodFilter = "all")
        {
            try
            {
                var startDate = GetStartDate(dateFilter);
                var endDate = DateTime.Now;

                var payments = (await _paymentService.GetPaymentsByDateRangeAsync(startDate, endDate)).ToList();

                // Apply filters
                if (statusFilter != "all")
                {
                    payments = payments.Where(p => p.Status.ToUpper() == statusFilter.ToUpper()).ToList();
                }

                if (methodFilter != "all")
                {
                    payments = payments.Where(p => p.Method == methodFilter).ToList();
                }

                var exportData = payments.Select(p => new
                {
                    ID = p.Id.ToString(),
                    Orden = p.Order?.OrderNumber ?? "N/A",
                    Mesa = p.Order?.Table?.TableNumber ?? "N/A",
                    Monto = p.Amount,
                    Método = p.Method,
                    Estado = p.Status,
                    Fecha = p.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    Pagador = p.PayerName ?? "N/A"
                }).ToList();

                // For now, return JSON. In a real implementation, you'd use a library like EPPlus or ClosedXML
                // to create Excel files, or iTextSharp for PDFs
                return Json(new
                {
                    success = true,
                    message = "Exportación completada",
                    data = exportData
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GenerateReport(string type)
        {
            try
            {
                var today = DateTime.Today;
                DateTime startDate, endDate;
                string reportTitle;

                switch (type.ToLower())
                {
                    case "daily":
                        startDate = today;
                        endDate = today.AddDays(1);
                        reportTitle = $"Reporte Diario - {today:dd/MM/yyyy}";
                        break;
                    case "weekly":
                        startDate = today.AddDays(-(int)today.DayOfWeek);
                        endDate = startDate.AddDays(7);
                        reportTitle = $"Reporte Semanal - {startDate:dd/MM/yyyy} a {endDate.AddDays(-1):dd/MM/yyyy}";
                        break;
                    case "monthly":
                        startDate = new DateTime(today.Year, today.Month, 1);
                        endDate = startDate.AddMonths(1);
                        reportTitle = $"Reporte Mensual - {startDate:MMMM yyyy}";
                        break;
                    default:
                        return Json(new { success = false, message = "Tipo de reporte no válido" });
                }

                var payments = (await _paymentService.GetPaymentsByDateRangeAsync(startDate, endDate)).ToList();
                var totalRevenue = payments.Sum(p => p.Amount);
                var totalPayments = payments.Count;
                var paymentMethods = payments.GroupBy(p => p.Method)
                    .Select(g => new { Method = g.Key, Count = g.Count(), Amount = g.Sum(p => p.Amount) })
                    .ToList();

                var reportData = new
                {
                    title = reportTitle,
                    period = $"{startDate:dd/MM/yyyy} - {endDate.AddDays(-1):dd/MM/yyyy}",
                    totalRevenue = totalRevenue,
                    totalPayments = totalPayments,
                    paymentMethods = paymentMethods,
                    payments = payments.Select(p => new
                    {
                        orderNumber = p.Order?.OrderNumber ?? "N/A",
                        tableNumber = p.Order?.Table?.TableNumber ?? "N/A",
                        amount = p.Amount,
                        method = p.Method,
                        status = p.Status,
                        createdAt = p.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                        payerName = p.PayerName ?? "N/A"
                    }).ToList()
                };

                // For now, return JSON. In a real implementation, you'd generate PDF using iTextSharp
                return Json(new
                {
                    success = true,
                    message = "Reporte generado exitosamente",
                    data = reportData
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        private DateTime GetStartDate(string filter)
        {
            var today = DateTime.Today;
            switch (filter)
            {
                case "today":
                    return today;
                case "week":
                    return today.AddDays(-(int)today.DayOfWeek);
                case "month":
                    return new DateTime(today.Year, today.Month, 1);
                case "year":
                    return new DateTime(today.Year, 1, 1);
                default:
                    return today.AddDays(-30); // Default to last 30 days
            }
        }
    }

    // DTOs para operaciones de pagos
    public class UpdatePaymentDto
    {
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string? PayerName { get; set; }
    }
}