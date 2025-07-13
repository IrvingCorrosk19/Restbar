using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface INotificationSettingsService
    {
        Task<IEnumerable<NotificationSettings>> GetAllAsync(Guid? companyId = null);
        Task<NotificationSettings?> GetByIdAsync(Guid id);
        Task<NotificationSettings> CreateAsync(NotificationSettings notificationSettings);
        Task<NotificationSettings> UpdateAsync(NotificationSettings notificationSettings);
        Task<bool> DeleteAsync(Guid id);
        Task<NotificationSettings?> GetByTypeAsync(string notificationType, Guid? companyId = null);
        Task<bool> IsEnabledAsync(string notificationType, Guid? companyId = null);
        Task<bool> EnableNotificationAsync(string notificationType, Guid? companyId = null);
        Task<bool> DisableNotificationAsync(string notificationType, Guid? companyId = null);
    }
} 