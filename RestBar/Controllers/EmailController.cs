using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Controllers
{
    /// <summary>
    /// Controlador para gestión de emails y pruebas
    /// </summary>
    [Authorize(Policy = "SystemConfig")]
    public class EmailController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ILogger<EmailController> _logger;

        public EmailController(
            IEmailService emailService,
            IEmailTemplateService emailTemplateService,
            ILogger<EmailController> logger)
        {
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
            _logger = logger;
        }

        /// <summary>
        /// Vista principal de gestión de emails
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                Console.WriteLine("🔍 [EmailController] Index() - Cargando vista principal...");
                
                var templates = await _emailTemplateService.GetAllAsync();
                
                ViewBag.TemplatesCount = templates.Count();
                ViewBag.EmailEnabled = bool.Parse(HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()["Email:Enabled"] ?? "false");
                
                return View(templates);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [EmailController] Index() - Error: {ex.Message}");
                _logger.LogError(ex, "[EmailController] Error al cargar índice");
                TempData["Error"] = $"Error al cargar la vista: {ex.Message}";
                return View(new List<EmailTemplate>());
            }
        }

        /// <summary>
        /// Prueba la conexión SMTP
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                Console.WriteLine("🔍 [EmailController] TestConnection() - Probando conexión SMTP...");
                
                var result = await _emailService.TestSmtpConnectionAsync();
                
                if (result)
                {
                    Console.WriteLine("✅ [EmailController] TestConnection() - Conexión SMTP exitosa");
                    return Json(new { success = true, message = "Conexión SMTP exitosa" });
                }
                else
                {
                    Console.WriteLine("❌ [EmailController] TestConnection() - Error en conexión SMTP");
                    return Json(new { success = false, message = "Error al conectar con el servidor SMTP" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [EmailController] TestConnection() - Error: {ex.Message}");
                _logger.LogError(ex, "[EmailController] Error al probar conexión SMTP");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Envía un email de prueba
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SendTestEmail([FromBody] SendTestEmailRequest request)
        {
            try
            {
                Console.WriteLine($"🔍 [EmailController] SendTestEmail() - Enviando email de prueba a: {request.Email}...");
                
                if (string.IsNullOrEmpty(request.Email))
                {
                    return Json(new { success = false, message = "Email es requerido" });
                }

                var subject = "🧪 Email de Prueba - RestBar Sistema";
                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Email de Prueba</h2>
                        <p>Este es un email de prueba enviado desde el sistema RestBar.</p>
                        <p>Si recibiste este email, significa que la configuración SMTP está funcionando correctamente.</p>
                        <p><strong>Fecha y Hora:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
                    </body>
                    </html>
                ";

                var result = await _emailService.SendEmailAsync(request.Email, subject, body, true);
                
                if (result)
                {
                    Console.WriteLine($"✅ [EmailController] SendTestEmail() - Email de prueba enviado exitosamente");
                    return Json(new { success = true, message = "Email de prueba enviado exitosamente" });
                }
                else
                {
                    Console.WriteLine("❌ [EmailController] SendTestEmail() - Error al enviar email de prueba");
                    return Json(new { success = false, message = "Error al enviar el email de prueba" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [EmailController] SendTestEmail() - Error: {ex.Message}");
                _logger.LogError(ex, "[EmailController] Error al enviar email de prueba");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Inicializa los templates por defecto
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> InitializeTemplates()
        {
            try
            {
                Console.WriteLine("🔍 [EmailController] InitializeTemplates() - Inicializando templates...");
                
                var result = await _emailTemplateService.InitializeDefaultTemplatesAsync();
                
                if (result)
                {
                    Console.WriteLine("✅ [EmailController] InitializeTemplates() - Templates inicializados exitosamente");
                    TempData["Success"] = "Templates inicializados exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    Console.WriteLine("❌ [EmailController] InitializeTemplates() - Error al inicializar templates");
                    TempData["Error"] = "Error al inicializar los templates";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [EmailController] InitializeTemplates() - Error: {ex.Message}");
                _logger.LogError(ex, "[EmailController] Error al inicializar templates");
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Obtiene un template por ID
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTemplate(Guid id)
        {
            try
            {
                Console.WriteLine($"🔍 [EmailController] GetTemplate() - Obteniendo template: {id}");
                
                var template = await _emailTemplateService.GetByIdAsync(id);
                
                if (template == null)
                {
                    return Json(new { success = false, message = "Template no encontrado" });
                }

                return Json(new { success = true, data = template });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [EmailController] GetTemplate() - Error: {ex.Message}");
                _logger.LogError(ex, "[EmailController] Error al obtener template");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Crea o actualiza un template
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveTemplate([FromBody] EmailTemplate template)
        {
            try
            {
                Console.WriteLine($"🔍 [EmailController] SaveTemplate() - Guardando template: {template.Name}");
                
                if (template.Id == Guid.Empty)
                {
                    var created = await _emailTemplateService.CreateAsync(template);
                    Console.WriteLine($"✅ [EmailController] SaveTemplate() - Template creado: {created.Name}");
                    return Json(new { success = true, message = "Template creado exitosamente", data = created });
                }
                else
                {
                    var updated = await _emailTemplateService.UpdateAsync(template);
                    Console.WriteLine($"✅ [EmailController] SaveTemplate() - Template actualizado: {updated.Name}");
                    return Json(new { success = true, message = "Template actualizado exitosamente", data = updated });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [EmailController] SaveTemplate() - Error: {ex.Message}");
                _logger.LogError(ex, "[EmailController] Error al guardar template");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }

    /// <summary>
    /// Request model para enviar email de prueba
    /// </summary>
    public class SendTestEmailRequest
    {
        public string Email { get; set; } = null!;
    }
}
