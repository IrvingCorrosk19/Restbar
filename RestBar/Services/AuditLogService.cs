using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;

namespace RestBar.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly RestBarContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(RestBarContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // ✅ NUEVO: Método para logging automático multi-tenant
        public async Task LogActivityAsync(
            string action,
            string module,
            string description,
            Guid? recordId = null,
            string? tableName = null,
            object? oldValues = null,
            object? newValues = null,
            AuditLogLevel logLevel = AuditLogLevel.INFO,
            bool isError = false,
            Exception? exception = null)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = user?.Id,
                    CompanyId = user?.Branch?.CompanyId,
                    BranchId = user?.BranchId,
                    Action = action,
                    Module = module,
                    Description = description,
                    RecordId = recordId,
                    TableName = tableName,
                    Timestamp = DateTime.UtcNow, // This is already UTC, which is correct
                    LogLevel = logLevel.ToString(),
                    IsError = isError,
                    IpAddress = GetClientIpAddress(),
                    UserAgent = GetUserAgent(),
                    SessionId = GetSessionId()
                };

                // Serializar valores antiguos y nuevos
                if (oldValues != null)
                {
                    auditLog.OldValues = JsonSerializer.Serialize(oldValues, new JsonSerializerOptions { WriteIndented = true });
                }

                if (newValues != null)
                {
                    auditLog.NewValues = JsonSerializer.Serialize(newValues, new JsonSerializerOptions { WriteIndented = true });
                }

                // Manejar errores
                if (exception != null)
                {
                    auditLog.IsError = true;
                    auditLog.LogLevel = AuditLogLevel.ERROR.ToString();
                    auditLog.ExceptionType = exception.GetType().Name;
                    auditLog.ErrorDetails = JsonSerializer.Serialize(new
                    {
                        Message = exception.Message,
                        Source = exception.Source,
                        InnerException = exception.InnerException?.Message
                    }, new JsonSerializerOptions { WriteIndented = true });
                    auditLog.StackTrace = exception.StackTrace;
                }

                await CreateAsync(auditLog);
            }
            catch (Exception ex)
            {
                // Log del error de logging (fallback a console)
                Console.WriteLine($"[AuditLogService] Error al registrar actividad: {ex.Message}");
                // No lanzar la excepción para evitar que afecte la funcionalidad principal
            }
        }

        // ✅ NUEVO: Método para logging de errores específicos
        public async Task LogErrorAsync(
            string module,
            string description,
            Exception exception,
            Guid? recordId = null,
            string? tableName = null)
        {
            await LogActivityAsync(
                action: AuditAction.ERROR.ToString(),
                module: module,
                description: description,
                recordId: recordId,
                tableName: tableName,
                logLevel: AuditLogLevel.ERROR,
                isError: true,
                exception: exception
            );
        }

        // ✅ NUEVO: Método para logging de cambios de datos
        public async Task LogDataChangeAsync(
            string action,
            string module,
            string description,
            Guid recordId,
            string tableName,
            object? oldValues = null,
            object? newValues = null)
        {
            await LogActivityAsync(
                action: action,
                module: module,
                description: description,
                recordId: recordId,
                tableName: tableName,
                oldValues: oldValues,
                newValues: newValues,
                logLevel: AuditLogLevel.INFO
            );
        }

        // ✅ NUEVO: Método para logging de seguridad
        public async Task LogSecurityEventAsync(
            string action,
            string description,
            bool isError = false,
            Exception? exception = null)
        {
            await LogActivityAsync(
                action: action,
                module: AuditModule.SECURITY.ToString(),
                description: description,
                logLevel: isError ? AuditLogLevel.ERROR : AuditLogLevel.WARNING,
                isError: isError,
                exception: exception
            );
        }

        // ✅ NUEVO: Método para logging de sistema
        public async Task LogSystemEventAsync(
            string action,
            string description,
            bool isError = false,
            Exception? exception = null)
        {
            await LogActivityAsync(
                action: action,
                module: AuditModule.SYSTEM.ToString(),
                description: description,
                logLevel: isError ? AuditLogLevel.ERROR : AuditLogLevel.INFO,
                isError: isError,
                exception: exception
            );
        }

        // ✅ NUEVO: Métodos de consulta multi-tenant
        public async Task<IEnumerable<AuditLog>> GetByCompanyAsync(Guid companyId)
        {
            return await _context.AuditLogs
                .Include(al => al.User)
                .Include(al => al.Company)
                .Include(al => al.Branch)
                .Where(al => al.CompanyId == companyId)
                .OrderByDescending(al => al.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByBranchAsync(Guid branchId)
        {
            return await _context.AuditLogs
                .Include(al => al.User)
                .Include(al => al.Company)
                .Include(al => al.Branch)
                .Where(al => al.BranchId == branchId)
                .OrderByDescending(al => al.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetErrorsAsync(Guid? companyId = null, Guid? branchId = null)
        {
            var query = _context.AuditLogs
                .Include(al => al.User)
                .Include(al => al.Company)
                .Include(al => al.Branch)
                .Where(al => al.IsError);

            if (companyId.HasValue)
                query = query.Where(al => al.CompanyId == companyId);

            if (branchId.HasValue)
                query = query.Where(al => al.BranchId == branchId);

            return await query
                .OrderByDescending(al => al.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByModuleAsync(string module, Guid? companyId = null)
        {
            var query = _context.AuditLogs
                .Include(al => al.User)
                .Include(al => al.Company)
                .Include(al => al.Branch)
                .Where(al => al.Module == module);

            if (companyId.HasValue)
                query = query.Where(al => al.CompanyId == companyId);

            return await query
                .OrderByDescending(al => al.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByLogLevelAsync(string logLevel, Guid? companyId = null)
        {
            var query = _context.AuditLogs
                .Include(al => al.User)
                .Include(al => al.Company)
                .Include(al => al.Branch)
                .Where(al => al.LogLevel == logLevel);

            if (companyId.HasValue)
                query = query.Where(al => al.CompanyId == companyId);

            return await query
                .OrderByDescending(al => al.Timestamp)
                .ToListAsync();
        }

        // Métodos existentes mejorados
        public async Task<IEnumerable<AuditLog>> GetAllAsync()
        {
            return await _context.AuditLogs
                .Include(al => al.User)
                .Include(al => al.Company)
                .Include(al => al.Branch)
                .OrderByDescending(al => al.Timestamp)
                .ToListAsync();
        }

        public async Task<AuditLog?> GetByIdAsync(Guid id)
        {
            return await _context.AuditLogs
                .Include(al => al.User)
                .Include(al => al.Company)
                .Include(al => al.Branch)
                .FirstOrDefaultAsync(al => al.Id == id);
        }

        public async Task<AuditLog> CreateAsync(AuditLog auditLog)
        {
            // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
            return auditLog;
        }

        public async Task UpdateAsync(AuditLog auditLog)
        {
            try
            {
                var existingEntity = _context.ChangeTracker.Entries<AuditLog>()
                    .FirstOrDefault(e => e.Entity.Id == auditLog.Id);

                if (existingEntity != null)
                {
                    existingEntity.State = EntityState.Detached;
                }

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
                .Include(al => al.Company)
                .Include(al => al.Branch)
                .Where(al => al.UserId == userId)
                .OrderByDescending(al => al.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action)
        {
            return await _context.AuditLogs
                .Include(al => al.User)
                .Include(al => al.Company)
                .Include(al => al.Branch)
                .Where(al => al.Action == action)
                .OrderByDescending(al => al.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByTableNameAsync(string tableName)
        {
            return await _context.AuditLogs
                .Include(al => al.User)
                .Include(al => al.Company)
                .Include(al => al.Branch)
                .Where(al => al.TableName == tableName)
                .OrderByDescending(al => al.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.AuditLogs
                .Include(al => al.User)
                .Include(al => al.Company)
                .Include(al => al.Branch)
                .Where(al => al.Timestamp >= startDate && al.Timestamp <= endDate)
                .OrderByDescending(al => al.Timestamp)
                .ToListAsync();
        }

        public async Task<AuditLog?> GetAuditLogWithUserAsync(Guid id)
        {
            return await _context.AuditLogs
                .Include(al => al.User)
                .Include(al => al.Company)
                .Include(al => al.Branch)
                .FirstOrDefaultAsync(al => al.Id == id);
        }

        public async Task<IEnumerable<AuditLog>> GetByRecordIdAsync(Guid recordId)
        {
            return await _context.AuditLogs
                .Include(al => al.User)
                .Include(al => al.Company)
                .Include(al => al.Branch)
                .Where(al => al.RecordId == recordId)
                .OrderByDescending(al => al.Timestamp)
                .ToListAsync();
        }

        // ✅ NUEVOS: Métodos auxiliares
        private async Task<User?> GetCurrentUserAsync()
        {
            var userEmail = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail)) return null;

            return await _context.Users
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Email == userEmail);
        }

        private string? GetClientIpAddress()
        {
            return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }

        private string? GetUserAgent()
        {
            return _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();
        }

        private string? GetSessionId()
        {
            try
            {
                return _httpContextAccessor.HttpContext?.Session?.Id;
            }
            catch (InvalidOperationException)
            {
                // Session no está configurada, devolver null
                return null;
            }
        }
    }
} 