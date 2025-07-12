using Microsoft.EntityFrameworkCore;

using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly RestBarContext _context;

        public AuditLogService(RestBarContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AuditLog>> GetAllAsync()
        {
            return await _context.AuditLogs
                .Include(al => al.User)
                .OrderByDescending(al => al.Timestamp)
                .ToListAsync();
        }

        public async Task<AuditLog?> GetByIdAsync(Guid id)
        {
            return await _context.AuditLogs
                .Include(al => al.User)
                .FirstOrDefaultAsync(al => al.Id == id);
        }

        public async Task<AuditLog> CreateAsync(AuditLog auditLog)
        {
            auditLog.Timestamp = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
            return auditLog;
        }

        public async Task UpdateAsync(AuditLog auditLog)
        {
            try
            {
                // Buscar si hay una entidad con el mismo ID siendo rastreada
                var existingEntity = _context.ChangeTracker.Entries<AuditLog>()
                    .FirstOrDefault(e => e.Entity.Id == auditLog.Id);

                if (existingEntity != null)
                {
                    // Detach la entidad existente para evitar conflictos
                    existingEntity.State = EntityState.Detached;
                }

                // Usar Update para manejar automáticamente el tracking
                _context.AuditLogs.Update(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al actualizar el registro de auditoría en la base de datos.", ex);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var auditLog = await _context.AuditLogs.FindAsync(id);
            if (auditLog != null)
            {
                _context.AuditLogs.Remove(auditLog);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId)
        {
            return await _context.AuditLogs
                .Include(al => al.User)
                .Where(al => al.UserId == userId)
                .OrderByDescending(al => al.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action)
        {
            return await _context.AuditLogs
                .Include(al => al.User)
                .Where(al => al.Action == action)
                .OrderByDescending(al => al.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByTableNameAsync(string tableName)
        {
            return await _context.AuditLogs
                .Include(al => al.User)
                .Where(al => al.TableName == tableName)
                .OrderByDescending(al => al.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.AuditLogs
                .Include(al => al.User)
                .Where(al => al.Timestamp >= startDate && al.Timestamp <= endDate)
                .OrderByDescending(al => al.Timestamp)
                .ToListAsync();
        }

        public async Task<AuditLog?> GetAuditLogWithUserAsync(Guid id)
        {
            return await _context.AuditLogs
                .Include(al => al.User)
                .FirstOrDefaultAsync(al => al.Id == id);
        }

        public async Task<IEnumerable<AuditLog>> GetByRecordIdAsync(Guid recordId)
        {
            return await _context.AuditLogs
                .Include(al => al.User)
                .Where(al => al.RecordId == recordId)
                .OrderByDescending(al => al.Timestamp)
                .ToListAsync();
        }
    }
} 