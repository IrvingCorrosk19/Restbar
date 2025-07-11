using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetAllAsync();
        Task<Notification?> GetByIdAsync(Guid id);
        Task<Notification> CreateAsync(Notification notification);
        Task UpdateAsync(Notification notification);
        Task DeleteAsync(Guid id);

        // Métodos adicionales específicos para Notification
        Task<IEnumerable<Notification>> GetByOrderIdAsync(Guid orderId);
        Task<IEnumerable<Notification>> GetUnreadNotificationsAsync();
        Task<IEnumerable<Notification>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Notification?> GetNotificationWithOrderAsync(Guid id);
        Task MarkAsReadAsync(Guid id);
        Task MarkAllAsReadAsync();
        Task<int> GetUnreadCountAsync();
    }
} 