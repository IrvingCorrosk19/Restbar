using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Claims;

namespace RestBar.Services
{
    public class NotificationSettingsService : INotificationSettingsService
    {
        private readonly RestBarContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationSettingsService(RestBarContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<NotificationSettings>> GetAllAsync(Guid? companyId = null)
        {
            var targetCompanyId = companyId ?? await GetCurrentUserCompanyIdAsync();
            
            return await _context.NotificationSettings
                .Where(n => n.CompanyId == targetCompanyId)
                .OrderBy(n => n.NotificationType)
                .ToListAsync();
        }

        public async Task<NotificationSettings?> GetByIdAsync(Guid id)
        {
            return await _context.NotificationSettings
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<NotificationSettings> CreateAsync(NotificationSettings notificationSettings)
        {
            notificationSettings.Id = Guid.NewGuid();
            // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService

            _context.NotificationSettings.Add(notificationSettings);
            await _context.SaveChangesAsync();

            return notificationSettings;
        }

        public async Task<NotificationSettings> UpdateAsync(NotificationSettings notificationSettings)
        {
            // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService
            _context.NotificationSettings.Update(notificationSettings);
            await _context.SaveChangesAsync();

            return notificationSettings;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var notificationSettings = await _context.NotificationSettings.FindAsync(id);
                if (notificationSettings != null)
                {
                    _context.NotificationSettings.Remove(notificationSettings);
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

        public async Task<NotificationSettings?> GetByTypeAsync(string notificationType, Guid? companyId = null)
        {
            var targetCompanyId = companyId ?? await GetCurrentUserCompanyIdAsync();
            
            return await _context.NotificationSettings
                .FirstOrDefaultAsync(n => n.NotificationType == notificationType && n.CompanyId == targetCompanyId);
        }

        public async Task<bool> IsEnabledAsync(string notificationType, Guid? companyId = null)
        {
            var notificationSettings = await GetByTypeAsync(notificationType, companyId);
            return notificationSettings?.IsEnabled ?? false;
        }

        public async Task<bool> EnableNotificationAsync(string notificationType, Guid? companyId = null)
        {
            try
            {
                var notificationSettings = await GetByTypeAsync(notificationType, companyId);
                if (notificationSettings != null)
                {
                    notificationSettings.IsEnabled = true;
                    notificationSettings.UpdatedAt = DateTime.UtcNow;
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

        public async Task<bool> DisableNotificationAsync(string notificationType, Guid? companyId = null)
        {
            try
            {
                var notificationSettings = await GetByTypeAsync(notificationType, companyId);
                if (notificationSettings != null)
                {
                    notificationSettings.IsEnabled = false;
                    notificationSettings.UpdatedAt = DateTime.UtcNow;
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