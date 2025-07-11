using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface IAuditLogService
    {
        Task<IEnumerable<AuditLog>> GetAllAsync();
        Task<AuditLog?> GetByIdAsync(Guid id);
        Task<AuditLog> CreateAsync(AuditLog auditLog);
        Task UpdateAsync(AuditLog auditLog);
        Task DeleteAsync(Guid id);

        // Métodos adicionales específicos para AuditLog
        Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<AuditLog>> GetByActionAsync(string action);
        Task<IEnumerable<AuditLog>> GetByTableNameAsync(string tableName);
        Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<AuditLog?> GetAuditLogWithUserAsync(Guid id);
        Task<IEnumerable<AuditLog>> GetByRecordIdAsync(Guid recordId);
    }
} 