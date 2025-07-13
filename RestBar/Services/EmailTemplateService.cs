using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Claims;

namespace RestBar.Services
{
    /// <summary>
    /// Servicio para gestionar templates de email
    /// </summary>
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly RestBarContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EmailTemplateService(RestBarContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<EmailTemplate>> GetAllAsync(Guid? companyId = null)
        {
            var query = _context.EmailTemplates.AsQueryable();

            if (companyId.HasValue)
            {
                query = query.Where(t => t.CompanyId == companyId || t.CompanyId == null);
            }

            return await query.OrderBy(t => t.Category).ThenBy(t => t.Name).ToListAsync();
        }

        public async Task<EmailTemplate?> GetByIdAsync(Guid id)
        {
            return await _context.EmailTemplates.FindAsync(id);
        }

        public async Task<EmailTemplate?> GetByNameAsync(string name, Guid? companyId = null)
        {
            var query = _context.EmailTemplates.Where(t => t.Name == name && t.IsActive);

            if (companyId.HasValue)
            {
                query = query.Where(t => t.CompanyId == companyId || t.CompanyId == null);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<EmailTemplate> CreateAsync(EmailTemplate template)
        {
            try
            {
                Console.WriteLine($"🔍 [EmailTemplateService] CreateAsync() - Creando template: {template.Name}");

                template.Id = Guid.NewGuid();
                template.CreatedAt = DateTime.UtcNow;
                template.UpdatedAt = DateTime.UtcNow;

                _context.EmailTemplates.Add(template);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ [EmailTemplateService] CreateAsync() - Template creado exitosamente: {template.Name}");
                return template;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [EmailTemplateService] CreateAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [EmailTemplateService] CreateAsync() - StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<EmailTemplate> UpdateAsync(EmailTemplate template)
        {
            try
            {
                Console.WriteLine($"🔍 [EmailTemplateService] UpdateAsync() - Actualizando template: {template.Name}");

                template.UpdatedAt = DateTime.UtcNow;

                _context.EmailTemplates.Update(template);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ [EmailTemplateService] UpdateAsync() - Template actualizado exitosamente: {template.Name}");
                return template;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [EmailTemplateService] UpdateAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [EmailTemplateService] UpdateAsync() - StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                Console.WriteLine($"🔍 [EmailTemplateService] DeleteAsync() - Eliminando template: {id}");

                var template = await _context.EmailTemplates.FindAsync(id);
                if (template == null)
                {
                    Console.WriteLine($"⚠️ [EmailTemplateService] DeleteAsync() - Template no encontrado: {id}");
                    return false;
                }

                _context.EmailTemplates.Remove(template);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ [EmailTemplateService] DeleteAsync() - Template eliminado exitosamente: {id}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [EmailTemplateService] DeleteAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [EmailTemplateService] DeleteAsync() - StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Inicializa templates por defecto en el sistema
        /// </summary>
        public async Task<bool> InitializeDefaultTemplatesAsync()
        {
            try
            {
                Console.WriteLine($"🔍 [EmailTemplateService] InitializeDefaultTemplatesAsync() - Iniciando inicialización de templates...");

                var templates = new List<EmailTemplate>
                {
                    new EmailTemplate
                    {
                        Name = "OrderConfirmation",
                        Subject = "Confirmación de Orden #{{OrderNumber}}",
                        Body = GetOrderConfirmationTemplate(),
                        Description = "Email de confirmación de orden para clientes",
                        Category = "Order",
                        IsActive = true,
                        Placeholders = "[\"OrderNumber\", \"OrderDate\", \"TotalAmount\", \"Items\"]"
                    },
                    new EmailTemplate
                    {
                        Name = "PasswordRecovery",
                        Subject = "Recuperación de Contraseña - RestBar",
                        Body = GetPasswordRecoveryTemplate(),
                        Description = "Email de recuperación de contraseña",
                        Category = "User",
                        IsActive = true,
                        Placeholders = "[\"UserName\", \"ResetLink\", \"ResetToken\", \"ExpirationMinutes\"]"
                    },
                    new EmailTemplate
                    {
                        Name = "Welcome",
                        Subject = "Bienvenido a RestBar - {{UserName}}",
                        Body = GetWelcomeTemplate(),
                        Description = "Email de bienvenida para nuevos usuarios",
                        Category = "User",
                        IsActive = true,
                        Placeholders = "[\"UserName\", \"Email\", \"TemporaryPassword\", \"LoginUrl\", \"CompanyName\"]"
                    },
                    new EmailTemplate
                    {
                        Name = "LowStockAlert",
                        Subject = "⚠️ Alerta de Inventario Bajo - {{ProductName}}",
                        Body = GetLowStockAlertTemplate(),
                        Description = "Email de alerta cuando el inventario está bajo",
                        Category = "Inventory",
                        IsActive = true,
                        Placeholders = "[\"ProductName\", \"CurrentStock\", \"MinimumStock\", \"ProductId\"]"
                    },
                    new EmailTemplate
                    {
                        Name = "DailyReport",
                        Subject = "📊 Reporte Diario - {{ReportDate}}",
                        Body = GetDailyReportTemplate(),
                        Description = "Email de reporte diario",
                        Category = "Report",
                        IsActive = true,
                        Placeholders = "[\"ReportDate\", \"ReportDateLong\", \"TotalSales\", \"TotalOrders\", \"AverageOrderValue\"]"
                    }
                };

                foreach (var template in templates)
                {
                    var existing = await _context.EmailTemplates
                        .FirstOrDefaultAsync(t => t.Name == template.Name && t.CompanyId == null);

                    if (existing == null)
                    {
                        Console.WriteLine($"📝 [EmailTemplateService] InitializeDefaultTemplatesAsync() - Creando template: {template.Name}");
                        await CreateAsync(template);
                    }
                    else
                    {
                        Console.WriteLine($"ℹ️ [EmailTemplateService] InitializeDefaultTemplatesAsync() - Template ya existe: {template.Name}");
                    }
                }

                Console.WriteLine($"✅ [EmailTemplateService] InitializeDefaultTemplatesAsync() - Templates inicializados exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [EmailTemplateService] InitializeDefaultTemplatesAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [EmailTemplateService] InitializeDefaultTemplatesAsync() - StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        #region Template Bodies

        private string GetOrderConfirmationTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Confirmación de Orden</title>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
    <div style='max-width: 600px; margin: 20px auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
        <!-- Header -->
        <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center;'>
            <h1 style='color: #ffffff; margin: 0; font-size: 28px;'>✅ Confirmación de Orden</h1>
        </div>
        
        <!-- Content -->
        <div style='padding: 30px;'>
            <p style='color: #333333; font-size: 16px; line-height: 1.6;'>Hola,</p>
            <p style='color: #333333; font-size: 16px; line-height: 1.6;'>Gracias por tu orden. Aquí están los detalles:</p>
            
            <div style='background-color: #f8f9fa; padding: 20px; border-radius: 6px; margin: 20px 0;'>
                <p style='margin: 5px 0; color: #666666;'><strong style='color: #333333;'>Número de Orden:</strong> #{{OrderNumber}}</p>
                <p style='margin: 5px 0; color: #666666;'><strong style='color: #333333;'>Fecha:</strong> {{OrderDate}}</p>
                <p style='margin: 5px 0; color: #666666;'><strong style='color: #333333;'>Total:</strong> {{TotalAmount}}</p>
            </div>
            
            <div style='margin: 20px 0;'>
                <h3 style='color: #333333; border-bottom: 2px solid #667eea; padding-bottom: 10px;'>Items:</h3>
                <div style='margin-top: 15px;'>
                    {{Items}}
                </div>
            </div>
            
            <p style='color: #666666; font-size: 14px; margin-top: 30px;'>Te mantendremos informado sobre el estado de tu orden.</p>
        </div>
        
        <!-- Footer -->
        <div style='background-color: #f8f9fa; padding: 20px; text-align: center; border-top: 1px solid #e0e0e0;'>
            <p style='color: #999999; font-size: 12px; margin: 0;'>RestBar Sistema - © " + DateTime.Now.Year + @"</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetPasswordRecoveryTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Recuperación de Contraseña</title>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
    <div style='max-width: 600px; margin: 20px auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
        <!-- Header -->
        <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center;'>
            <h1 style='color: #ffffff; margin: 0; font-size: 28px;'>🔐 Recuperación de Contraseña</h1>
        </div>
        
        <!-- Content -->
        <div style='padding: 30px;'>
            <p style='color: #333333; font-size: 16px; line-height: 1.6;'>Hola {{UserName}},</p>
            <p style='color: #333333; font-size: 16px; line-height: 1.6;'>Recibimos una solicitud para recuperar tu contraseña.</p>
            
            <div style='background-color: #f8f9fa; padding: 20px; border-radius: 6px; margin: 20px 0; text-align: center;'>
                <p style='color: #666666; margin-bottom: 20px;'>Haz clic en el botón siguiente para restablecer tu contraseña:</p>
                <a href='{{ResetLink}}' style='display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #ffffff; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Restablecer Contraseña</a>
            </div>
            
            <p style='color: #999999; font-size: 14px; margin-top: 20px;'>Este enlace expirará en {{ExpirationMinutes}} minutos.</p>
            <p style='color: #999999; font-size: 14px;'>Si no solicitaste este cambio, puedes ignorar este email.</p>
            
            <div style='margin-top: 30px; padding: 15px; background-color: #fff3cd; border-left: 4px solid #ffc107; border-radius: 4px;'>
                <p style='color: #856404; font-size: 14px; margin: 0;'><strong>Nota:</strong> Si el botón no funciona, copia y pega este enlace en tu navegador:</p>
                <p style='color: #856404; font-size: 12px; margin: 5px 0 0 0; word-break: break-all;'>{{ResetLink}}</p>
            </div>
        </div>
        
        <!-- Footer -->
        <div style='background-color: #f8f9fa; padding: 20px; text-align: center; border-top: 1px solid #e0e0e0;'>
            <p style='color: #999999; font-size: 12px; margin: 0;'>RestBar Sistema - © " + DateTime.Now.Year + @"</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetWelcomeTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Bienvenido a RestBar</title>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
    <div style='max-width: 600px; margin: 20px auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
        <!-- Header -->
        <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center;'>
            <h1 style='color: #ffffff; margin: 0; font-size: 28px;'>🎉 Bienvenido a RestBar</h1>
        </div>
        
        <!-- Content -->
        <div style='padding: 30px;'>
            <p style='color: #333333; font-size: 16px; line-height: 1.6;'>Hola {{UserName}},</p>
            <p style='color: #333333; font-size: 16px; line-height: 1.6;'>¡Bienvenido a {{CompanyName}}! Tu cuenta ha sido creada exitosamente.</p>
            
            <div style='background-color: #f8f9fa; padding: 20px; border-radius: 6px; margin: 20px 0;'>
                <p style='margin: 5px 0; color: #666666;'><strong style='color: #333333;'>Email:</strong> {{Email}}</p>
                <p style='margin: 5px 0; color: #666666;'><strong style='color: #333333;'>Contraseña Temporal:</strong> <code style='background-color: #fff; padding: 2px 6px; border-radius: 3px;'>{{TemporaryPassword}}</code></p>
            </div>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{{LoginUrl}}' style='display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #ffffff; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Iniciar Sesión</a>
            </div>
            
            <div style='margin-top: 30px; padding: 15px; background-color: #d1ecf1; border-left: 4px solid #0c5460; border-radius: 4px;'>
                <p style='color: #0c5460; font-size: 14px; margin: 0;'><strong>Importante:</strong> Por favor, cambia tu contraseña temporal después de iniciar sesión por primera vez.</p>
            </div>
        </div>
        
        <!-- Footer -->
        <div style='background-color: #f8f9fa; padding: 20px; text-align: center; border-top: 1px solid #e0e0e0;'>
            <p style='color: #999999; font-size: 12px; margin: 0;'>RestBar Sistema - © " + DateTime.Now.Year + @"</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetLowStockAlertTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Alerta de Inventario Bajo</title>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
    <div style='max-width: 600px; margin: 20px auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
        <!-- Header -->
        <div style='background: linear-gradient(135deg, #ff9800 0%, #f57c00 100%); padding: 30px; text-align: center;'>
            <h1 style='color: #ffffff; margin: 0; font-size: 28px;'>⚠️ Alerta de Inventario Bajo</h1>
        </div>
        
        <!-- Content -->
        <div style='padding: 30px;'>
            <p style='color: #333333; font-size: 16px; line-height: 1.6;'>El siguiente producto tiene inventario bajo:</p>
            
            <div style='background-color: #fff3cd; padding: 20px; border-radius: 6px; margin: 20px 0; border-left: 4px solid #ffc107;'>
                <p style='margin: 5px 0; color: #856404;'><strong style='color: #333333; font-size: 18px;'>Producto:</strong> {{ProductName}}</p>
                <p style='margin: 5px 0; color: #856404;'><strong style='color: #333333;'>Stock Actual:</strong> <span style='color: #dc3545; font-weight: bold;'>{{CurrentStock}}</span></p>
                <p style='margin: 5px 0; color: #856404;'><strong style='color: #333333;'>Stock Mínimo:</strong> {{MinimumStock}}</p>
            </div>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='#' style='display: inline-block; background: linear-gradient(135deg, #ff9800 0%, #f57c00 100%); color: #ffffff; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Ver Detalles del Producto</a>
            </div>
            
            <p style='color: #666666; font-size: 14px; margin-top: 20px;'>Por favor, revisa el inventario y realiza el reorden necesario.</p>
        </div>
        
        <!-- Footer -->
        <div style='background-color: #f8f9fa; padding: 20px; text-align: center; border-top: 1px solid #e0e0e0;'>
            <p style='color: #999999; font-size: 12px; margin: 0;'>RestBar Sistema - © " + DateTime.Now.Year + @"</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetDailyReportTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Reporte Diario</title>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
    <div style='max-width: 600px; margin: 20px auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
        <!-- Header -->
        <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center;'>
            <h1 style='color: #ffffff; margin: 0; font-size: 28px;'>📊 Reporte Diario</h1>
            <p style='color: #ffffff; margin: 10px 0 0 0; font-size: 16px;'>{{ReportDateLong}}</p>
        </div>
        
        <!-- Content -->
        <div style='padding: 30px;'>
            <div style='background-color: #f8f9fa; padding: 20px; border-radius: 6px; margin: 20px 0;'>
                <h3 style='color: #333333; margin-top: 0; border-bottom: 2px solid #667eea; padding-bottom: 10px;'>Resumen del Día</h3>
                <p style='margin: 10px 0; color: #666666;'><strong style='color: #333333;'>Total de Ventas:</strong> {{TotalSales}}</p>
                <p style='margin: 10px 0; color: #666666;'><strong style='color: #333333;'>Total de Órdenes:</strong> {{TotalOrders}}</p>
                <p style='margin: 10px 0; color: #666666;'><strong style='color: #333333;'>Valor Promedio por Orden:</strong> {{AverageOrderValue}}</p>
            </div>
            
            <p style='color: #666666; font-size: 14px; margin-top: 20px;'>Este es un resumen automático del día {{ReportDate}}.</p>
        </div>
        
        <!-- Footer -->
        <div style='background-color: #f8f9fa; padding: 20px; text-align: center; border-top: 1px solid #e0e0e0;'>
            <p style='color: #999999; font-size: 12px; margin: 0;'>RestBar Sistema - © " + DateTime.Now.Year + @"</p>
        </div>
    </div>
</body>
</html>";
        }

        #endregion
    }
}
