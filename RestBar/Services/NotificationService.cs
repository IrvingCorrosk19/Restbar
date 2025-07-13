using Microsoft.EntityFrameworkCore;

using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services
{
    public class NotificationService : BaseTrackingService, INotificationService
    {
        public NotificationService(RestBarContext context, IHttpContextAccessor httpContextAccessor) 
            : base(context, httpContextAccessor)
        {
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
            // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService
            notification.IsRead = false;
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task UpdateAsync(Notification notification)
        {
            try
            {
                // Buscar si hay una entidad con el mismo ID siendo rastreada
                var existingEntity = _context.ChangeTracker.Entries<Notification>()
                    .FirstOrDefault(e => e.Entity.Id == notification.Id);

                if (existingEntity != null)
                {
                    // Detach la entidad existente para evitar conflictos
                    existingEntity.State = EntityState.Detached;
                }

                // Usar Update para manejar automáticamente el tracking
                _context.Notifications.Update(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al actualizar la notificación en la base de datos.", ex);
            }
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