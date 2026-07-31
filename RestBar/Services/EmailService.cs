using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MimeKit;
using RestBar.Interfaces;
using RestBar.Models;
using System.Text.RegularExpressions;

namespace RestBar.Services
{
    /// <summary>
    /// Servicio para envío de emails usando MailKit
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly RestBarContext _context;
        private readonly INotificationSettingsService _notificationSettingsService;

        // Configuración SMTP desde appsettings.json
        private string SmtpServer => _configuration["Email:Smtp:Server"] ?? "smtp.gmail.com";
        private int SmtpPort => int.Parse(_configuration["Email:Smtp:Port"] ?? "587");
        private string SmtpUsername => _configuration["Email:Smtp:Username"] ?? "";
        private string SmtpPassword => _configuration["Email:Smtp:Password"] ?? "";
        private bool SmtpUseSsl => bool.Parse(_configuration["Email:Smtp:UseSsl"] ?? "true");
        private string FromEmail => _configuration["Email:From:Email"] ?? SmtpUsername;
        private string FromName => _configuration["Email:From:Name"] ?? "RestBar Sistema";
        private bool EmailEnabled => bool.Parse(_configuration["Email:Enabled"] ?? "false");

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger,
            RestBarContext context,
            INotificationSettingsService notificationSettingsService)
        {
            _configuration = configuration;
            _logger = logger;
            _context = context;
            _notificationSettingsService = notificationSettingsService;
        }

        /// <summary>
        /// Envía un email simple
        /// </summary>
        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            return await SendEmailAsync(to, subject, body, null, null, isHtml);
        }

        /// <summary>
        /// Envía un email con múltiples destinatarios
        /// </summary>
        public async Task<bool> SendEmailAsync(List<string> to, string subject, string body, bool isHtml = true)
        {
            try
            {
                Console.WriteLine($"🔍 [EmailService] SendEmailAsync() - Iniciando envío a {to.Count} destinatarios...");

                if (!EmailEnabled)
                {
                    Console.WriteLine("⚠️ [EmailService] SendEmailAsync() - Email deshabilitado en configuración");
                    return false;
                }

                var emailSettings = await _notificationSettingsService.GetByTypeAsync("Email");
                if (emailSettings != null && !emailSettings.IsEnabled)
                {
                    Console.WriteLine("⚠️ [EmailService] SendEmailAsync() - Email deshabilitado en configuración de notificaciones");
                    return false;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(FromName, FromEmail));

                foreach (var recipient in to)
                {
                    message.To.Add(new MailboxAddress("", recipient));
                }

                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                if (isHtml)
                {
                    bodyBuilder.HtmlBody = body;
                }
                else
                {
                    bodyBuilder.TextBody = body;
                }

                message.Body = bodyBuilder.ToMessageBody();

                return await SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [EmailService] SendEmailAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [EmailService] SendEmailAsync() - StackTrace: {ex.StackTrace}");
                _logger.LogError(ex, $"[EmailService] Error al enviar email a múltiples destinatarios");
                return false;
            }
        }

        /// <summary>
        /// Envía un email con copia (CC) y copia oculta (BCC)
        /// </summary>
        public async Task<bool> SendEmailAsync(string to, string subject, string body, List<string>? cc = null, List<string>? bcc = null, bool isHtml = true)
        {
            try
            {
                Console.WriteLine($"🔍 [EmailService] SendEmailAsync() - Iniciando envío a {to}...");

                if (!EmailEnabled)
                {
                    Console.WriteLine("⚠️ [EmailService] SendEmailAsync() - Email deshabilitado en configuración");
                    return false;
                }

                var emailSettings = await _notificationSettingsService.GetByTypeAsync("Email");
                if (emailSettings != null && !emailSettings.IsEnabled)
                {
                    Console.WriteLine("⚠️ [EmailService] SendEmailAsync() - Email deshabilitado en configuración de notificaciones");
                    return false;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(FromName, FromEmail));
                message.To.Add(new MailboxAddress("", to));

                if (cc != null && cc.Any())
                {
                    foreach (var email in cc)
                    {
                        message.Cc.Add(new MailboxAddress("", email));
                    }
                }

                if (bcc != null && bcc.Any())
                {
                    foreach (var email in bcc)
                    {
                        message.Bcc.Add(new MailboxAddress("", email));
                    }
                }

                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                if (isHtml)
                {
                    bodyBuilder.HtmlBody = body;
                }
                else
                {
                    bodyBuilder.TextBody = body;
                }

                message.Body = bodyBuilder.ToMessageBody();

                return await SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [EmailService] SendEmailAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [EmailService] SendEmailAsync() - StackTrace: {ex.StackTrace}");
                _logger.LogError(ex, $"[EmailService] Error al enviar email a {to}");
                return false;
            }
        }

        /// <summary>
        /// Envía un email usando un template
        /// </summary>
        public async Task<bool> SendTemplatedEmailAsync(string templateName, string to, Dictionary<string, string> templateData, List<string>? cc = null, List<string>? bcc = null)
        {
            try
            {
                Console.WriteLine($"🔍 [EmailService] SendTemplatedEmailAsync() - Template: {templateName}, Destinatario: {to}");

                // Buscar template en base de datos
                var template = await _context.EmailTemplates
                    .FirstOrDefaultAsync(t => t.Name == templateName && t.IsActive);

                if (template == null)
                {
                    Console.WriteLine($"❌ [EmailService] SendTemplatedEmailAsync() - Template '{templateName}' no encontrado");
                    _logger.LogWarning($"[EmailService] Template '{templateName}' no encontrado");
                    return false;
                }

                // Reemplazar placeholders en subject y body
                var subject = ReplacePlaceholders(template.Subject, templateData);
                var body = ReplacePlaceholders(template.Body, templateData);

                return await SendEmailAsync(to, subject, body, cc, bcc, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [EmailService] SendTemplatedEmailAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [EmailService] SendTemplatedEmailAsync() - StackTrace: {ex.StackTrace}");
                _logger.LogError(ex, $"[EmailService] Error al enviar email con template '{templateName}'");
                return false;
            }
        }

        /// <summary>
        /// Reemplaza placeholders en un texto (formato: {{PlaceholderName}})
        /// </summary>
        private string ReplacePlaceholders(string text, Dictionary<string, string> data)
        {
            if (string.IsNullOrEmpty(text) || data == null || !data.Any())
                return text;

            var result = text;
            foreach (var kvp in data)
            {
                result = result.Replace($"{{{{{kvp.Key}}}}}", kvp.Value ?? "");
            }

            return result;
        }

        /// <summary>
        /// Envía el mensaje usando SMTP
        /// </summary>
        private async Task<bool> SendMessageAsync(MimeMessage message)
        {
            try
            {
                Console.WriteLine($"📤 [EmailService] SendMessageAsync() - Conectando a SMTP {SmtpServer}:{SmtpPort}...");

                using var client = new SmtpClient();
                
                // Conectar al servidor SMTP
                await client.ConnectAsync(SmtpServer, SmtpPort, SmtpUseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                
                // Autenticar si hay credenciales
                if (!string.IsNullOrEmpty(SmtpUsername) && !string.IsNullOrEmpty(SmtpPassword))
                {
                    Console.WriteLine($"🔐 [EmailService] SendMessageAsync() - Autenticando...");
                    await client.AuthenticateAsync(SmtpUsername, SmtpPassword);
                }

                // Enviar mensaje
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                Console.WriteLine($"✅ [EmailService] SendMessageAsync() - Email enviado exitosamente");
                _logger.LogInformation($"[EmailService] Email enviado exitosamente a {message.To}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [EmailService] SendMessageAsync() - Error al enviar: {ex.Message}");
                Console.WriteLine($"🔍 [EmailService] SendMessageAsync() - StackTrace: {ex.StackTrace}");
                _logger.LogError(ex, $"[EmailService] Error al enviar email");
                return false;
            }
        }

        /// <summary>
        /// Verifica la configuración del servidor SMTP
        /// </summary>
        public async Task<bool> TestSmtpConnectionAsync()
        {
            try
            {
                Console.WriteLine($"🔍 [EmailService] TestSmtpConnectionAsync() - Probando conexión SMTP...");

                using var client = new SmtpClient();
                await client.ConnectAsync(SmtpServer, SmtpPort, SmtpUseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                
                if (!string.IsNullOrEmpty(SmtpUsername) && !string.IsNullOrEmpty(SmtpPassword))
                {
                    await client.AuthenticateAsync(SmtpUsername, SmtpPassword);
                }

                await client.DisconnectAsync(true);

                Console.WriteLine($"✅ [EmailService] TestSmtpConnectionAsync() - Conexión SMTP exitosa");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [EmailService] TestSmtpConnectionAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [EmailService] TestSmtpConnectionAsync() - StackTrace: {ex.StackTrace}");
                _logger.LogError(ex, $"[EmailService] Error al probar conexión SMTP");
                return false;
            }
        }

        /// <summary>
        /// Envía email de confirmación de orden
        /// </summary>
        public async Task<bool> SendOrderConfirmationAsync(Order order, string customerEmail)
        {
            try
            {
                Console.WriteLine($"🔍 [EmailService] SendOrderConfirmationAsync() - Orden: {order.OrderNumber}, Email: {customerEmail}");

                // Cargar datos relacionados
                await _context.Entry(order)
                    .Collection(o => o.OrderItems)
                    .Query()
                    .Include(oi => oi.Product)
                    .LoadAsync();

                var culture = System.Globalization.CultureInfo.CreateSpecificCulture("es-PA");
                culture.NumberFormat.CurrencySymbol = "$";
                
                var orderData = new Dictionary<string, string>
                {
                    ["OrderNumber"] = order.OrderNumber ?? order.Id.ToString(),
                    ["OrderDate"] = order.OpenedAt?.ToString("dd/MM/yyyy HH:mm") ?? DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    ["TotalAmount"] = string.Format(culture, "{0:C2}", order.TotalAmount),
                    ["Items"] = string.Join("<br/>", order.OrderItems.Select(oi => 
                        $"{oi.Quantity}x {oi.Product?.Name ?? "Producto"} - {string.Format(culture, "{0:C2}", oi.UnitPrice)}"
                    ))
                };

                return await SendTemplatedEmailAsync("OrderConfirmation", customerEmail, orderData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [EmailService] SendOrderConfirmationAsync() - Error: {ex.Message}");
                _logger.LogError(ex, $"[EmailService] Error al enviar confirmación de orden");
                return false;
            }
        }

        /// <summary>
        /// Envía email de recuperación de contraseña
        /// </summary>
        public async Task<bool> SendPasswordRecoveryAsync(User user, string resetToken)
        {
            try
            {
                Console.WriteLine($"🔍 [EmailService] SendPasswordRecoveryAsync() - Usuario: {user.Email}");

                var resetLink = $"{_configuration["Email:BaseUrl"]}/Auth/ResetPassword?token={resetToken}&email={Uri.EscapeDataString(user.Email)}";

                var templateData = new Dictionary<string, string>
                {
                    ["UserName"] = user.FullName ?? user.Email,
                    ["ResetLink"] = resetLink,
                    ["ResetToken"] = resetToken,
                    ["ExpirationMinutes"] = "30"
                };

                return await SendTemplatedEmailAsync("PasswordRecovery", user.Email, templateData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [EmailService] SendPasswordRecoveryAsync() - Error: {ex.Message}");
                _logger.LogError(ex, $"[EmailService] Error al enviar recuperación de contraseña");
                return false;
            }
        }

        /// <summary>
        /// Envía email de bienvenida
        /// </summary>
        public async Task<bool> SendWelcomeEmailAsync(User user, string temporaryPassword)
        {
            try
            {
                Console.WriteLine($"🔍 [EmailService] SendWelcomeEmailAsync() - Usuario: {user.Email}");

                var loginUrl = $"{_configuration["Email:BaseUrl"]}/Auth/Login";

                var templateData = new Dictionary<string, string>
                {
                    ["UserName"] = user.FullName ?? user.Email,
                    ["Email"] = user.Email,
                    ["TemporaryPassword"] = temporaryPassword,
                    ["LoginUrl"] = loginUrl,
                    ["CompanyName"] = user.Branch?.Company?.Name ?? "RestBar"
                };

                return await SendTemplatedEmailAsync("Welcome", user.Email, templateData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [EmailService] SendWelcomeEmailAsync() - Error: {ex.Message}");
                _logger.LogError(ex, $"[EmailService] Error al enviar email de bienvenida");
                return false;
            }
        }

        /// <summary>
        /// Envía notificación de inventario bajo
        /// </summary>
        public async Task<bool> SendLowStockAlertAsync(Product product, decimal currentStock, string recipientEmail)
        {
            try
            {
                Console.WriteLine($"🔍 [EmailService] SendLowStockAlertAsync() - Producto: {product.Name}, Stock: {currentStock}");

                var templateData = new Dictionary<string, string>
                {
                    ["ProductName"] = product.Name,
                    ["CurrentStock"] = currentStock.ToString(),
                    ["MinimumStock"] = (product.MinStock ?? 0).ToString(),
                    ["ProductId"] = product.Id.ToString()
                };

                return await SendTemplatedEmailAsync("LowStockAlert", recipientEmail, templateData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [EmailService] SendLowStockAlertAsync() - Error: {ex.Message}");
                _logger.LogError(ex, $"[EmailService] Error al enviar alerta de inventario bajo");
                return false;
            }
        }

        /// <summary>
        /// Envía reporte diario por email
        /// </summary>
        public async Task<bool> SendDailyReportAsync(string recipientEmail, DateTime reportDate, Dictionary<string, object> reportData)
        {
            try
            {
                Console.WriteLine($"🔍 [EmailService] SendDailyReportAsync() - Fecha: {reportDate:yyyy-MM-dd}, Email: {recipientEmail}");

                var templateData = reportData.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.ToString() ?? ""
                );

                templateData["ReportDate"] = reportDate.ToString("dd/MM/yyyy");
                templateData["ReportDateLong"] = reportDate.ToString("dddd, d 'de' MMMM 'de' yyyy");

                return await SendTemplatedEmailAsync("DailyReport", recipientEmail, templateData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [EmailService] SendDailyReportAsync() - Error: {ex.Message}");
                _logger.LogError(ex, $"[EmailService] Error al enviar reporte diario");
                return false;
            }
        }

        /// <summary>
        /// Envía notificación de nueva orden a cocina
        /// </summary>
        public async Task<bool> SendNewOrderNotificationAsync(Order order, List<string> kitchenEmails)
        {
            try
            {
                Console.WriteLine($"🔍 [EmailService] SendNewOrderNotificationAsync() - Orden: {order.OrderNumber}, Emails: {string.Join(", ", kitchenEmails)}");

                // Cargar datos relacionados
                await _context.Entry(order)
                    .Collection(o => o.OrderItems)
                    .Query()
                    .Include(oi => oi.Product)
                    .LoadAsync();

                var orderData = new Dictionary<string, string>
                {
                    ["OrderNumber"] = order.OrderNumber ?? order.Id.ToString(),
                    ["TableNumber"] = order.Table?.TableNumber ?? "N/A",
                    ["OrderDate"] = order.OpenedAt?.ToString("dd/MM/yyyy HH:mm") ?? DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    ["Items"] = string.Join("<br/>", order.OrderItems.Select(oi => 
                        $"{oi.Quantity}x {oi.Product?.Name ?? "Producto"} - {oi.Notes ?? ""}"
                    ))
                };

                var subject = $"🆕 Nueva Orden #{orderData["OrderNumber"]} - Mesa {orderData["TableNumber"]}";
                var body = BuildOrderNotificationBody(orderData);

                return await SendEmailAsync(kitchenEmails, subject, body, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [EmailService] SendNewOrderNotificationAsync() - Error: {ex.Message}");
                _logger.LogError(ex, $"[EmailService] Error al enviar notificación de nueva orden");
                return false;
            }
        }

        /// <summary>
        /// Construye el cuerpo HTML de la notificación de orden
        /// </summary>
        private string BuildOrderNotificationBody(Dictionary<string, string> orderData)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>🆕 Nueva Orden Recibida</h2>
                    <p><strong>Número de Orden:</strong> {orderData["OrderNumber"]}</p>
                    <p><strong>Mesa:</strong> {orderData["TableNumber"]}</p>
                    <p><strong>Fecha/Hora:</strong> {orderData["OrderDate"]}</p>
                    <h3>Items:</h3>
                    {orderData["Items"]}
                </body>
                </html>
            ";
        }
    }
}
