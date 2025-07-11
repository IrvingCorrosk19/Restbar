using Microsoft.EntityFrameworkCore;

using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services
{
    public class NotificationService : INotificationService
    {
        private readonly RestBarContext _context;

        public NotificationService(RestBarContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Notification>> GetAllAsync()
        {
            return await _context.Notifications
                .Include(n => n.Order)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            return await _context.Notifications
                .Include(n => n.Order)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<Notification> CreateAsync(Notification notification)
        {
            notification.CreatedAt = DateTime.UtcNow;
            
            // Validación de desarrollo para asegurar que las fechas sean UTC
            if (notification.CreatedAt.HasValue && notification.CreatedAt.Value.Kind == DateTimeKind.Unspecified)
                throw new InvalidOperationException("CreatedAt no debe ser Unspecified para columnas timestamp with time zone");
            
            notification.IsRead = false;
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task UpdateAsync(Notification notification)
        {
            _context.Entry(notification).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Notification>> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.Notifications
                .Include(n => n.Order)
                .Where(n => n.OrderId == orderId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync()
        {
            return await _context.Notifications
                .Include(n => n.Order)
                .Where(n => n.IsRead == false)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Notifications
                .Include(n => n.Order)
                .Where(n => n.CreatedAt >= startDate && n.CreatedAt <= endDate)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Notification?> GetNotificationWithOrderAsync(Guid id)
        {
            return await _context.Notifications
                .Include(n => n.Order)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task MarkAsReadAsync(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync()
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.IsRead == false)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync()
        {
            return await _context.Notifications
                .CountAsync(n => n.IsRead == false);
        }
    }
} 