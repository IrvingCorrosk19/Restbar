using RestBar.Interfaces;
using RestBar.Models;
using System.Text.Json;

namespace RestBar.Services
{
    public class GlobalLoggingService : IGlobalLoggingService
    {
        private readonly IAuditLogService _auditLogService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GlobalLoggingService(IAuditLogService auditLogService, IHttpContextAccessor httpContextAccessor)
        {
            _auditLogService = auditLogService;
            _httpContextAccessor = httpContextAccessor;
        }

        // ✅ NUEVO: Logging para módulo de usuarios
        public async Task LogUserActivityAsync(string action, string description, Guid? userId = null, object? oldValues = null, object? newValues = null)
        {
            await _auditLogService.LogActivityAsync(
                action: action,
                module: AuditModule.USER.ToString(),
                description: description,
                recordId: userId,
                tableName: "users",
                oldValues: oldValues,
                newValues: newValues
            );
        }

        // ✅ NUEVO: Logging para módulo de órdenes
        public async Task LogOrderActivityAsync(string action, string description, Guid orderId, object? oldValues = null, object? newValues = null)
        {
            await _auditLogService.LogActivityAsync(
                action: action,
                module: AuditModule.ORDER.ToString(),
                description: description,
                recordId: orderId,
                tableName: "orders",
                oldValues: oldValues,
                newValues: newValues
            );
        }



        // ✅ NUEVO: Logging para módulo de productos
        public async Task LogProductActivityAsync(string action, string description, Guid productId, object? oldValues = null, object? newValues = null)
        {
            await _auditLogService.LogActivityAsync(
                action: action,
                module: AuditModule.PRODUCT.ToString(),
                description: description,
                recordId: productId,
                tableName: "products",
                oldValues: oldValues,
                newValues: newValues
            );
        }

        // ✅ NUEVO: Logging para módulo de pagos
        public async Task LogPaymentActivityAsync(string action, string description, Guid paymentId, object? oldValues = null, object? newValues = null)
        {
            await _auditLogService.LogActivityAsync(
                action: action,
                module: AuditModule.PAYMENT.ToString(),
                description: description,
                recordId: paymentId,
                tableName: "payments",
                oldValues: oldValues,
                newValues: newValues
            );
        }

        // ✅ NUEVO: Logging para módulo de contabilidad
        public async Task LogAccountingActivityAsync(string action, string description, Guid? recordId = null, object? oldValues = null, object? newValues = null)
        {
            await _auditLogService.LogActivityAsync(
                action: action,
                module: AuditModule.ACCOUNTING.ToString(),
                description: description,
                recordId: recordId,
                tableName: "journal_entries",
                oldValues: oldValues,
                newValues: newValues
            );
        }

        // ✅ NUEVO: Logging para módulo de reportes
        public async Task LogReportActivityAsync(string action, string description, string reportType, object? parameters = null)
        {
            await _auditLogService.LogActivityAsync(
                action: action,
                module: AuditModule.REPORT.ToString(),
                description: $"{description} - Reporte: {reportType}",
                oldValues: parameters
            );
        }

        // ✅ NUEVO: Logging para módulo de configuración
        public async Task LogConfigurationActivityAsync(string action, string description, string settingKey, object? oldValue = null, object? newValue = null)
        {
            await _auditLogService.LogActivityAsync(
                action: action,
                module: AuditModule.CONFIGURATION.ToString(),
                description: $"{description} - Configuración: {settingKey}",
                oldValues: oldValue != null ? new { SettingKey = settingKey, Value = oldValue } : null,
                newValues: newValue != null ? new { SettingKey = settingKey, Value = newValue } : null
            );
        }

        // ✅ NUEVO: Logging para módulo de backup
        public async Task LogBackupActivityAsync(string action, string description, bool isError = false, Exception? exception = null)
        {
            await _auditLogService.LogActivityAsync(
                action: action,
                module: AuditModule.BACKUP.ToString(),
                description: description,
                logLevel: isError ? AuditLogLevel.ERROR : AuditLogLevel.INFO,
                isError: isError,
                exception: exception
            );
        }

        // ✅ NUEVO: Logging de errores específicos por módulo
        public async Task LogModuleErrorAsync(string module, string description, Exception exception, Guid? recordId = null, string? tableName = null)
        {
            await _auditLogService.LogErrorAsync(
                module: module,
                description: description,
                exception: exception,
                recordId: recordId,
                tableName: tableName
            );
        }

        // ✅ NUEVO: Logging de eventos de seguridad
        public async Task LogSecurityEventAsync(string action, string description, bool isError = false, Exception? exception = null)
        {
            await _auditLogService.LogSecurityEventAsync(action, description, isError, exception);
        }

        // ✅ NUEVO: Logging de eventos del sistema
        public async Task LogSystemEventAsync(string action, string description, bool isError = false, Exception? exception = null)
        {
            await _auditLogService.LogSystemEventAsync(action, description, isError, exception);
        }

        // ✅ NUEVO: Logging de cambios de datos con comparación
        public async Task LogDataChangeAsync<T>(string action, string module, string description, Guid recordId, string tableName, T? oldEntity, T? newEntity) where T : class
        {
            object? oldValues = null;
            object? newValues = null;

            if (oldEntity != null)
            {
                oldValues = ExtractEntityValues(oldEntity);
            }

            if (newEntity != null)
            {
                newValues = ExtractEntityValues(newEntity);
            }

            await _auditLogService.LogDataChangeAsync(
                action: action,
                module: module,
                description: description,
                recordId: recordId,
                tableName: tableName,
                oldValues: oldValues,
                newValues: newValues
            );
        }

        // ✅ NUEVO: Método auxiliar para extraer valores de entidades
        private object ExtractEntityValues<T>(T entity) where T : class
        {
            try
            {
                // Usar reflexión para obtener propiedades públicas
                var properties = typeof(T).GetProperties()
                    .Where(p => p.CanRead && p.GetMethod?.IsPublic == true)
                    .ToDictionary(
                        p => p.Name,
                        p => p.GetValue(entity)
                    );

                return properties;
            }
            catch
            {
                // Fallback a serialización JSON
                return JsonSerializer.Serialize(entity, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        // ✅ NUEVO: Logging de inicio de sesión
        public async Task LogLoginAsync(string email, bool isSuccess, string? errorMessage = null)
        {
            var description = isSuccess 
                ? $"Inicio de sesión exitoso para: {email}"
                : $"Intento de inicio de sesión fallido para: {email} - Error: {errorMessage}";

            await _auditLogService.LogSecurityEventAsync(
                action: isSuccess ? AuditAction.LOGIN.ToString() : "LOGIN_FAILED",
                description: description,
                isError: !isSuccess
            );
        }

        // ✅ NUEVO: Logging de cierre de sesión
        public async Task LogLogoutAsync(string email)
        {
            await _auditLogService.LogSecurityEventAsync(
                action: AuditAction.LOGOUT.ToString(),
                description: $"Cierre de sesión para: {email}"
            );
        }

        // ✅ NUEVO: Logging de exportación de datos
        public async Task LogDataExportAsync(string module, string description, string exportType, object? parameters = null)
        {
            await _auditLogService.LogActivityAsync(
                action: AuditAction.DATA_EXPORT.ToString(),
                module: module,
                description: $"{description} - Tipo: {exportType}",
                oldValues: parameters
            );
        }

        // ✅ NUEVO: Logging de importación de datos
        public async Task LogDataImportAsync(string module, string description, string importType, int recordCount, object? parameters = null)
        {
            await _auditLogService.LogActivityAsync(
                action: AuditAction.DATA_IMPORT.ToString(),
                module: module,
                description: $"{description} - Tipo: {importType} - Registros: {recordCount}",
                oldValues: parameters
            );
        }
    }
} 