using Microsoft.AspNetCore.Authorization;
using RestBar.Interfaces;
using System.Security.Claims;

namespace RestBar.Middleware
{
    /// <summary>
    /// Middleware para validar permisos específicos basado en roles y acciones
    /// </summary>
    public class PermissionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PermissionMiddleware> _logger;

        public PermissionMiddleware(RequestDelegate next, ILogger<PermissionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IAuthService authService)
        {
            // Permitir acceso a rutas públicas esenciales
            if (context.Request.Path.StartsWithSegments("/Auth") || 
                context.Request.Path.StartsWithSegments("/Home/Error") ||
                context.Request.Path.Value == "/" ||
                context.Request.Path.StartsWithSegments("/css") ||
                context.Request.Path.StartsWithSegments("/js") ||
                context.Request.Path.StartsWithSegments("/images") ||
                context.Request.Path.StartsWithSegments("/lib") ||
                context.Request.Path.StartsWithSegments("/favicon.ico"))
            {
                await _next(context);
                return;
            }

            // Verificar si el usuario está autenticado
            if (!context.User.Identity.IsAuthenticated)
            {
                _logger.LogWarning($"[PermissionMiddleware] Acceso no autorizado a: {context.Request.Path}");
                context.Response.Redirect("/Auth/Login");
                return;
            }

            // Obtener información del usuario
            var userIdClaim = context.User.FindFirst("UserId")?.Value;
            var roleClaim = context.User.FindFirst("UserRole")?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning($"[PermissionMiddleware] UserId inválido para path: {context.Request.Path}");
                context.Response.Redirect("/Auth/Login");
                return;
            }

            // SuperAdmin tiene acceso a todo, no necesita validación adicional
            if (roleClaim?.ToLower() == "superadmin")
            {
                _logger.LogInformation($"[PermissionMiddleware] SuperAdmin {userId} accedió a {context.Request.Path}");
                await _next(context);
                return;
            }

            // Determinar la acción requerida basada en la ruta
            var requiredAction = GetRequiredActionFromPath(context.Request.Path);
            
            if (!string.IsNullOrEmpty(requiredAction))
            {
                // Verificar permisos usando el AuthService
                var hasPermission = await authService.HasPermissionAsync(userId, requiredAction);
                
                if (!hasPermission)
                {
                    _logger.LogWarning($"[PermissionMiddleware] Usuario {userId} ({roleClaim}) sin permisos para {requiredAction} en {context.Request.Path}");
                    context.Response.Redirect("/Auth/AccessDenied");
                    return;
                }
                
                _logger.LogInformation($"[PermissionMiddleware] Usuario {userId} ({roleClaim}) accedió a {requiredAction} en {context.Request.Path}");
            }

            await _next(context);
        }

        /// <summary>
        /// Determina qué acción/permiso se requiere basado en la ruta
        /// </summary>
        private string GetRequiredActionFromPath(PathString path)
        {
            var pathValue = path.Value?.ToLower();
            
            if (string.IsNullOrEmpty(pathValue))
                return "";

            // Mapear rutas a acciones/permisos
            return pathValue switch
            {
                // Rutas de órdenes
                _ when pathValue.StartsWith("/order") => "orders",
                
                // Rutas de cocina
                _ when pathValue.StartsWith("/stationorders") => "kitchen",
                
                // Rutas de pagos
                _ when pathValue.StartsWith("/payment") => "payments",
                
                // Rutas de mesas
                _ when pathValue.StartsWith("/table") => "tables",
                
                // Rutas de productos
                _ when pathValue.StartsWith("/product") => "products",
                
                // Rutas de inventario
    
                
                // Rutas de usuarios
                _ when pathValue.StartsWith("/user") => "users",
                
                // Rutas de reportes
                _ when pathValue.StartsWith("/report") => "reports",
                
                // Rutas de configuración (company, branch, category, etc.)
                _ when pathValue.StartsWith("/company") => "admin_only",
                _ when pathValue.StartsWith("/branch") => "admin_only",
                _ when pathValue.StartsWith("/category") => "admin_only",
                _ when pathValue.StartsWith("/area") => "admin_only",
                _ when pathValue.StartsWith("/station") => "admin_only",
                
                // Rutas del SuperAdmin (solo para superadmin)
                _ when pathValue.StartsWith("/superadmin") => "superadmin_only",
                
                // Rutas públicas o sin restricciones específicas
                _ => ""
            };
        }
    }

    /// <summary>
    /// Extensión para registrar el middleware
    /// </summary>
    public static class PermissionMiddlewareExtensions
    {
        public static IApplicationBuilder UsePermissionValidation(this IApplicationBuilder app)
        {
            return app.UseMiddleware<PermissionMiddleware>();
        }
    }
} 