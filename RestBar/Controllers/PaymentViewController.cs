using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestBar.Interfaces;
using RestBar.Models;
using RestBar.ViewModel;
using ClosedXML.Excel;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Net;

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

                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Pagos");
                ws.Cell(1, 1).Value = "ID";
                ws.Cell(1, 2).Value = "Orden";
                ws.Cell(1, 3).Value = "Mesa";
                ws.Cell(1, 4).Value = "Monto";
                ws.Cell(1, 5).Value = "Método";
                ws.Cell(1, 6).Value = "Estado";
                ws.Cell(1, 7).Value = "Fecha";
                ws.Cell(1, 8).Value = "Pagador";
                for (var i = 0; i < exportData.Count; i++)
                {
                    var row = exportData[i];
                    ws.Cell(i + 2, 1).Value = row.ID;
                    ws.Cell(i + 2, 2).Value = row.Orden;
                    ws.Cell(i + 2, 3).Value = row.Mesa;
                    ws.Cell(i + 2, 4).Value = row.Monto;
                    ws.Cell(i + 2, 5).Value = row.Método;
                    ws.Cell(i + 2, 6).Value = row.Estado;
                    ws.Cell(i + 2, 7).Value = row.Fecha;
                    ws.Cell(i + 2, 8).Value = row.Pagador;
                }
                ws.Columns().AdjustToContents();
                using var ms = new MemoryStream();
                wb.SaveAs(ms);
                return File(ms.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"pagos_{DateTime.Now:yyyyMMdd}.xlsx");
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

                var sb = new StringBuilder();
                sb.Append("<!doctype html><html><head><meta charset='utf-8'><title>")
                  .Append(WebUtility.HtmlEncode(reportTitle))
                  .Append("</title><style>body{font-family:Segoe UI,Arial,sans-serif;font-size:12px;margin:24px}table{border-collapse:collapse;width:100%}th,td{border:1px solid #ccc;padding:4px}th{background:#eee}@media print{.no-print{display:none}}</style></head><body>");
                sb.Append("<h1>").Append(WebUtility.HtmlEncode(reportTitle)).Append("</h1>");
                sb.Append($"<p>Periodo: {startDate:dd/MM/yyyy} — {endDate.AddDays(-1):dd/MM/yyyy}<br>Total: {totalRevenue:N2} · Pagos: {totalPayments}</p>");
                sb.Append("<p class='no-print'><button onclick='window.print()'>Imprimir / Guardar como PDF</button></p>");
                sb.Append("<h2>Por método</h2><table><tr><th>Método</th><th>Cantidad</th><th>Monto</th></tr>");
                foreach (var m in paymentMethods)
                    sb.Append($"<tr><td>{WebUtility.HtmlEncode(m.Method)}</td><td>{m.Count}</td><td>{m.Amount:N2}</td></tr>");
                sb.Append("</table><h2>Detalle</h2><table><tr><th>Orden</th><th>Mesa</th><th>Monto</th><th>Método</th><th>Estado</th><th>Fecha</th></tr>");
                foreach (var p in payments)
                    sb.Append($"<tr><td>{WebUtility.HtmlEncode(p.Order?.OrderNumber ?? "N/A")}</td><td>{WebUtility.HtmlEncode(p.Order?.Table?.TableNumber ?? "N/A")}</td><td>{p.Amount:N2}</td><td>{WebUtility.HtmlEncode(p.Method)}</td><td>{WebUtility.HtmlEncode(p.Status)}</td><td>{p.CreatedAt:dd/MM/yyyy HH:mm}</td></tr>");
                sb.Append("</table></body></html>");
                return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/html; charset=utf-8", $"reporte_pagos_{type}_{DateTime.Now:yyyyMMdd}_print.html");
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