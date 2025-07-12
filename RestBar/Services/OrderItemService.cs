using Microsoft.EntityFrameworkCore;

using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services
{
    public class OrderItemService : IOrderItemService
    {
        private readonly RestBarContext _context;

        public OrderItemService(RestBarContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderItem>> GetAllAsync()
        {
            return await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .ToListAsync();
        }

        public async Task<OrderItem?> GetByIdAsync(Guid id)
        {
            return await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .FirstOrDefaultAsync(oi => oi.Id == id);
        }

        public async Task<OrderItem> CreateAsync(OrderItem orderItem)
        {
            _context.OrderItems.Add(orderItem);
            await _context.SaveChangesAsync();
            return orderItem;
        }

        public async Task UpdateAsync(OrderItem orderItem)
        {
            try
            {
                // Buscar si hay una entidad con el mismo ID siendo rastreada
                var existingEntity = _context.ChangeTracker.Entries<OrderItem>()
                    .FirstOrDefault(e => e.Entity.Id == orderItem.Id);

                if (existingEntity != null)
                {
                    // Detach la entidad existente para evitar conflictos
                    existingEntity.State = EntityState.Detached;
                }

                // Usar Update para manejar automáticamente el tracking
                _context.OrderItems.Update(orderItem);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al actualizar el item de la orden en la base de datos.", ex);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem != null)
            {
                _context.OrderItems.Remove(orderItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderItem>> GetByProductIdAsync(Guid productId)
        {
            return await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .Where(oi => oi.ProductId == productId)
                .ToListAsync();
        }

        public async Task<decimal> GetOrderTotalAsync(Guid orderId)
        {
            return await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .SumAsync(oi => (oi.UnitPrice * oi.Quantity) - (oi.Discount ?? 0));
        }

        public async Task<OrderItem?> GetOrderItemWithDetailsAsync(Guid id)
        {
            return await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .FirstOrDefaultAsync(oi => oi.Id == id);
        }

        public async Task<IEnumerable<OrderItem>> GetByQuantityRangeAsync(decimal minQuantity, decimal maxQuantity)
        {
            return await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .Where(oi => oi.Quantity >= minQuantity && oi.Quantity <= maxQuantity)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderItem>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .Where(oi => oi.UnitPrice >= minPrice && oi.UnitPrice <= maxPrice)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderItem>> GetDiscountedItemsAsync()
        {
            return await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .Where(oi => oi.Discount > 0)
                .ToListAsync();
        }

        public async Task<OrderItem?> GetOrderItemWithOrderAndProductAsync(Guid id)
        {
            return await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .FirstOrDefaultAsync(oi => oi.Id == id);
        }

        public async Task<Guid?> DeleteOrderItemAndGetOrderIdAsync(Guid orderItemId)
        {
            var orderItem = await _context.OrderItems.FindAsync(orderItemId);
            if (orderItem != null)
            {
                var orderId = orderItem.OrderId;
                _context.OrderItems.Remove(orderItem);
                await _context.SaveChangesAsync();
                return orderId;
            }
            return null;
        }
    }
} 