namespace RestBar.Interfaces
{
    public interface IGlobalLoggingService
    {
        // ✅ Logging específico por módulo
        Task LogUserActivityAsync(string action, string description, Guid? userId = null, object? oldValues = null, object? newValues = null);
        Task LogOrderActivityAsync(string action, string description, Guid orderId, object? oldValues = null, object? newValues = null);

        Task LogProductActivityAsync(string action, string description, Guid productId, object? oldValues = null, object? newValues = null);
        Task LogPaymentActivityAsync(string action, string description, Guid paymentId, object? oldValues = null, object? newValues = null);
        Task LogAccountingActivityAsync(string action, string description, Guid? recordId = null, object? oldValues = null, object? newValues = null);
        Task LogReportActivityAsync(string action, string description, string reportType, object? parameters = null);
        Task LogConfigurationActivityAsync(string action, string description, string settingKey, object? oldValue = null, object? newValue = null);
        Task LogBackupActivityAsync(string action, string description, bool isError = false, Exception? exception = null);

        // ✅ Logging de errores y eventos
        Task LogModuleErrorAsync(string module, string description, Exception exception, Guid? recordId = null, string? tableName = null);
        Task LogSecurityEventAsync(string action, string description, bool isError = false, Exception? exception = null);
        Task LogSystemEventAsync(string action, string description, bool isError = false, Exception? exception = null);

        // ✅ Logging de cambios de datos
        Task LogDataChangeAsync<T>(string action, string module, string description, Guid recordId, string tableName, T? oldEntity, T? newEntity) where T : class;

        // ✅ Logging de autenticación
        Task LogLoginAsync(string email, bool isSuccess, string? errorMessage = null);
        Task LogLogoutAsync(string email);

        // ✅ Logging de importación/exportación
        Task LogDataExportAsync(string module, string description, string exportType, object? parameters = null);
        Task LogDataImportAsync(string module, string description, string importType, int recordCount, object? parameters = null);
    }
} 