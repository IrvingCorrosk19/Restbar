using RestBar.Models;

namespace RestBar.Interfaces
{
    /// <summary>
    /// Servicio para envío de emails
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Envía un email simple
        /// </summary>
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);

        /// <summary>
        /// Envía un email con múltiples destinatarios
        /// </summary>
        Task<bool> SendEmailAsync(List<string> to, string subject, string body, bool isHtml = true);

        /// <summary>
        /// Envía un email con copia (CC) y copia oculta (BCC)
        /// </summary>
        Task<bool> SendEmailAsync(string to, string subject, string body, List<string>? cc = null, List<string>? bcc = null, bool isHtml = true);

        /// <summary>
        /// Envía un email usando un template
        /// </summary>
        Task<bool> SendTemplatedEmailAsync(string templateName, string to, Dictionary<string, string> templateData, List<string>? cc = null, List<string>? bcc = null);

        /// <summary>
        /// Verifica la configuración del servidor SMTP
        /// </summary>
        Task<bool> TestSmtpConnectionAsync();

        /// <summary>
        /// Envía email de confirmación de orden
        /// </summary>
        Task<bool> SendOrderConfirmationAsync(Order order, string customerEmail);

        /// <summary>
        /// Envía email de recuperación de contraseña
        /// </summary>
        Task<bool> SendPasswordRecoveryAsync(User user, string resetToken);

        /// <summary>
        /// Envía email de bienvenida
        /// </summary>
        Task<bool> SendWelcomeEmailAsync(User user, string temporaryPassword);

        /// <summary>
        /// Envía notificación de inventario bajo
        /// </summary>
        Task<bool> SendLowStockAlertAsync(Product product, decimal currentStock, string recipientEmail);

        /// <summary>
        /// Envía reporte diario por email
        /// </summary>
        Task<bool> SendDailyReportAsync(string recipientEmail, DateTime reportDate, Dictionary<string, object> reportData);

        /// <summary>
        /// Envía notificación de nueva orden a cocina
        /// </summary>
        Task<bool> SendNewOrderNotificationAsync(Order order, List<string> kitchenEmails);
    }
}
