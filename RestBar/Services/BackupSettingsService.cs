using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Claims;

namespace RestBar.Services
{
    public class BackupSettingsService : IBackupSettingsService
    {
        private readonly RestBarContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BackupSettingsService(RestBarContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<BackupSettings>> GetAllAsync(Guid? companyId = null)
        {
            var targetCompanyId = companyId ?? await GetCurrentUserCompanyIdAsync();
            
            return await _context.BackupSettings
                .Where(b => b.CompanyId == targetCompanyId)
                .OrderBy(b => b.BackupType)
                .ToListAsync();
        }

        public async Task<BackupSettings?> GetByIdAsync(Guid id)
        {
            return await _context.BackupSettings
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<BackupSettings> CreateAsync(BackupSettings backupSettings)
        {
            backupSettings.Id = Guid.NewGuid();
            // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService

            _context.BackupSettings.Add(backupSettings);
            await _context.SaveChangesAsync();

            return backupSettings;
        }

        public async Task<BackupSettings> UpdateAsync(BackupSettings backupSettings)
        {
            // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService
            _context.BackupSettings.Update(backupSettings);
            await _context.SaveChangesAsync();

            return backupSettings;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var backupSettings = await _context.BackupSettings.FindAsync(id);
                if (backupSettings != null)
                {
                    _context.BackupSettings.Remove(backupSettings);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<BackupSettings?> GetByTypeAsync(string backupType, Guid? companyId = null)
        {
            var targetCompanyId = companyId ?? await GetCurrentUserCompanyIdAsync();
            
            return await _context.BackupSettings
                .FirstOrDefaultAsync(b => b.BackupType == backupType && b.CompanyId == targetCompanyId);
        }

        public async Task<bool> IsEnabledAsync(string backupType, Guid? companyId = null)
        {
            var backupSettings = await GetByTypeAsync(backupType, companyId);
            return backupSettings?.IsEnabled ?? false;
        }

        public async Task<bool> EnableBackupAsync(string backupType, Guid? companyId = null)
        {
            try
            {
                var backupSettings = await GetByTypeAsync(backupType, companyId);
                if (backupSettings != null)
                {
                    backupSettings.IsEnabled = true;
                    backupSettings.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DisableBackupAsync(string backupType, Guid? companyId = null)
        {
            try
            {
                var backupSettings = await GetByTypeAsync(backupType, companyId);
                if (backupSettings != null)
                {
                    backupSettings.IsEnabled = false;
                    backupSettings.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExecuteBackupAsync(string backupType, Guid? companyId = null)
        {
            try
            {
                var backupSettings = await GetByTypeAsync(backupType, companyId);
                if (backupSettings != null && backupSettings.IsEnabled)
                {
                    // Simular ejecución de respaldo
                    await Task.Delay(2000); // Simular tiempo de respaldo
                    
                    backupSettings.LastBackupDate = DateTime.UtcNow;
                    backupSettings.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<DateTime?> GetLastBackupDateAsync(string backupType, Guid? companyId = null)
        {
            var backupSettings = await GetByTypeAsync(backupType, companyId);
            return backupSettings?.LastBackupDate;
        }

        private async Task<Guid?> GetCurrentUserCompanyIdAsync()
        {
            var userEmail = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail)) return null;

            var user = await _context.Users
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            return user?.Branch?.CompanyId;
        }
    }
} 