using RestBar.Models;
using Microsoft.Extensions.Logging;

namespace RestBar.Interfaces
{
    public interface IAuditLogService
    {
        // Métodos básicos
        Task<IEnumerable<AuditLog>> GetAllAsync();
        Task<AuditLog?> GetByIdAsync(Guid id);
        Task<AuditLog> CreateAsync(AuditLog auditLog);
        Task UpdateAsync(AuditLog auditLog);
        Task DeleteAsync(Guid id);

        // Métodos específicos para AuditLog
        Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<AuditLog>> GetByActionAsync(string action);
        Task<IEnumerable<AuditLog>> GetByTableNameAsync(string tableName);
        Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<AuditLog?> GetAuditLogWithUserAsync(Guid id);
        Task<IEnumerable<AuditLog>> GetByRecordIdAsync(Guid recordId);

        // ✅ NUEVOS: Métodos de logging multi-tenant
        Task LogActivityAsync(
            string action,
            string module,
            string description,
            Guid? recordId = null,
            string? tableName = null,
            object? oldValues = null,
            object? newValues = null,
            AuditLogLevel logLevel = AuditLogLevel.INFO,
            bool isError = false,
            Exception? exception = null);

        Task LogErrorAsync(
            string module,
            string description,
            Exception exception,
            Guid? recordId = null,
            string? tableName = null);

        Task LogDataChangeAsync(
            string action,
            string module,
            string description,
            Guid recordId,
            string tableName,
            object? oldValues = null,
            object? newValues = null);

        Task LogSecurityEventAsync(
            string action,
            string description,
            bool isError = false,
            Exception? exception = null);

        Task LogSystemEventAsync(
            string action,
            string description,
            bool isError = false,
            Exception? exception = null);

        // ✅ NUEVOS: Métodos de consulta multi-tenant
        Task<IEnumerable<AuditLog>> GetByCompanyAsync(Guid companyId);
        Task<IEnumerable<AuditLog>> GetByBranchAsync(Guid branchId);
        Task<IEnumerable<AuditLog>> GetErrorsAsync(Guid? companyId = null, Guid? branchId = null);
        Task<IEnumerable<AuditLog>> GetByModuleAsync(string module, Guid? companyId = null);
        Task<IEnumerable<AuditLog>> GetByLogLevelAsync(string logLevel, Guid? companyId = null);
    }
} 