using Microsoft.AspNetCore.Http;
using RestBar.Interfaces;
using RestBar.Models;
using System.Diagnostics;
using System.Text;

namespace RestBar.Middleware
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuditMiddleware> _logger;

        public AuditMiddleware(RequestDelegate next, IServiceProvider serviceProvider, ILogger<AuditMiddleware> logger)
        {
            _next = next;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var originalBodyStream = context.Response.Body;

            try
            {
                // Capturar información de la solicitud
                var requestInfo = await CaptureRequestInfoAsync(context);
                
                // Resolver el servicio dentro del scope
                using var scope = _serviceProvider.CreateScope();
                var auditLogService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();
                
                // Log de inicio de solicitud
                await auditLogService.LogActivityAsync(
                    action: "REQUEST_START",
                    module: "SYSTEM",
                    description: $"Inicio de solicitud: {context.Request.Method} {context.Request.Path}",
                    logLevel: AuditLogLevel.INFO
                );

                // Procesar la solicitud
                using var memoryStream = new MemoryStream();
                context.Response.Body = memoryStream;

                await _next(context);

                // Restaurar el stream original
                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(originalBodyStream);

                stopwatch.Stop();

                // Log de finalización exitosa
                await auditLogService.LogActivityAsync(
                    action: "REQUEST_SUCCESS",
                    module: "SYSTEM",
                    description: $"Solicitud completada: {context.Request.Method} {context.Request.Path} - Status: {context.Response.StatusCode} - Tiempo: {stopwatch.ElapsedMilliseconds}ms",
                    logLevel: AuditLogLevel.INFO
                );
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // Log de error
                using var errorScope = _serviceProvider.CreateScope();
                var errorAuditLogService = errorScope.ServiceProvider.GetRequiredService<IAuditLogService>();
                await errorAuditLogService.LogErrorAsync(
                    module: "SYSTEM",
                    description: $"Error en solicitud: {context.Request.Method} {context.Request.Path} - Tiempo: {stopwatch.ElapsedMilliseconds}ms",
                    exception: ex
                );

                // Re-lanzar la excepción para que el middleware de manejo de errores la procese
                throw;
            }
        }

        private async Task<object> CaptureRequestInfoAsync(HttpContext context)
        {
            var requestInfo = new
            {
                Method = context.Request.Method,
                Path = context.Request.Path,
                QueryString = context.Request.QueryString.ToString(),
                Headers = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                ContentType = context.Request.ContentType,
                ContentLength = context.Request.ContentLength,
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                Timestamp = DateTime.UtcNow
            };

            // Capturar body si es necesario (solo para solicitudes pequeñas)
            if (context.Request.ContentLength.HasValue && context.Request.ContentLength < 1024 * 10) // 10KB
            {
                try
                {
                    context.Request.EnableBuffering();
                    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;

                    if (!string.IsNullOrEmpty(body))
                    {
                        // Solo loggear body para ciertos endpoints sensibles
                        if (context.Request.Path.StartsWithSegments("/api/auth") ||
                            context.Request.Path.StartsWithSegments("/api/user"))
                        {
                            // Enmascarar información sensible
                            body = MaskSensitiveData(body);
                        }
                    }
                }
                catch
                {
                    // Ignorar errores al leer el body
                }
            }

            return requestInfo;
        }

        private string MaskSensitiveData(string body)
        {
            // Enmascarar contraseñas y tokens
            var maskedBody = body;
            
            // Enmascarar contraseñas
            maskedBody = System.Text.RegularExpressions.Regex.Replace(
                maskedBody, 
                @"""password"":\s*""[^""]*""", 
                @"""password"": ""***"""
            );

            // Enmascarar tokens
            maskedBody = System.Text.RegularExpressions.Regex.Replace(
                maskedBody, 
                @"""token"":\s*""[^""]*""", 
                @"""token"": ""***"""
            );

            return maskedBody;
        }
    }

    // ✅ NUEVO: Middleware para capturar errores específicos
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, IServiceProvider serviceProvider, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log del error original inmediatamente
                _logger.LogError(ex, "Error original en la aplicación: {Message}", ex.Message);
                
                // Solo manejar la excepción si la respuesta no ha sido enviada
                if (!context.Response.HasStarted)
                {
                    await HandleExceptionAsync(context, ex);
                }
                else
                {
                    _logger.LogWarning("No se pudo manejar la excepción porque la respuesta ya fue enviada");
                }
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            try
            {
                // Verificar si la respuesta ya ha sido enviada
                if (context.Response.HasStarted)
                {
                    _logger.LogError(exception, "Error occurred but response already started");
                    return;
                }

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = exception switch
                {
                    UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                    InvalidOperationException => StatusCodes.Status400BadRequest,
                    KeyNotFoundException => StatusCodes.Status404NotFound,
                    _ => StatusCodes.Status500InternalServerError
                };

                var errorResponse = new
                {
                    Error = exception.Message,
                    StatusCode = context.Response.StatusCode,
                    Timestamp = DateTime.UtcNow,
                    Path = context.Request.Path,
                    Method = context.Request.Method
                };

                // Log del error de forma segura
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var auditLogService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();
                    await auditLogService.LogErrorAsync(
                        module: "SYSTEM",
                        description: $"Error HTTP {context.Response.StatusCode}: {context.Request.Method} {context.Request.Path}",
                        exception: exception
                    );
                }
                catch (Exception logException)
                {
                    _logger.LogError(logException, "Error logging to audit log");
                }

                // Escribir respuesta de forma segura
                try
                {
                    if (!context.Response.HasStarted)
                    {
                        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(errorResponse);
                        await context.Response.WriteAsync(jsonResponse);
                    }
                }
                catch (ObjectDisposedException)
                {
                    _logger.LogWarning("Stream was disposed, cannot write error response");
                }
                catch (Exception writeEx)
                {
                    _logger.LogError(writeEx, "Error writing error response");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HandleExceptionAsync");
            }
        }
    }

    // ✅ NUEVO: Extension methods para registrar los middlewares
    public static class AuditMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuditLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuditMiddleware>();
        }

        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
} 