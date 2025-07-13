using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services
{
    public class InvoiceService : BaseTrackingService, IInvoiceService
    {
        public InvoiceService(RestBarContext context, IHttpContextAccessor httpContextAccessor) 
            : base(context, httpContextAccessor)
        {
        }

        public async Task<IEnumerable<Invoice>> GetAllAsync()
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Order)
                .ToListAsync();
        }

        public async Task<Invoice?> GetByIdAsync(Guid id)
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Order)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Invoice> CreateAsync(Invoice invoice)
        {
            // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task UpdateAsync(Invoice invoice)
        {
            try
            {
                // Buscar si hay una entidad con el mismo ID siendo rastreada
                var existingEntity = _context.ChangeTracker.Entries<Invoice>()
                    .FirstOrDefault(e => e.Entity.Id == invoice.Id);

                if (existingEntity != null)
                {
                    // Detach la entidad existente para evitar conflictos
                    existingEntity.State = EntityState.Detached;
                }

                // Usar Update para manejar automáticamente el tracking
                _context.Invoices.Update(invoice);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al actualizar la factura en la base de datos.", ex);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice != null)
            {
                _context.Invoices.Remove(invoice);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Invoice>> GetByCustomerIdAsync(Guid customerId)
        {
            return await _context.Invoices
                .Where(i => i.CustomerId == customerId)
                .Include(i => i.Order)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.Invoices
                .Where(i => i.OrderId == orderId)
                .Include(i => i.Customer)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Invoices
                .Where(i => i.CreatedAt >= startDate && i.CreatedAt <= endDate)
                .Include(i => i.Customer)
                .Include(i => i.Order)
                .ToListAsync();
        }

        public async Task<Invoice?> GetInvoiceWithDetailsAsync(Guid id)
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<decimal> CalculateInvoiceTotalAsync(Guid id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Order)
                    .ThenInclude(o => o.OrderItems)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null || invoice.Order == null)
                return 0;

            decimal subtotal = invoice.Order.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice);
            decimal tax = subtotal * invoice.Tax / 100;
            decimal total = subtotal + tax;

            invoice.Total = total;
            await _context.SaveChangesAsync();
            return total;
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByTotalRangeAsync(decimal minTotal, decimal maxTotal)
        {
            return await _context.Invoices
                .Where(i => i.Total >= minTotal && i.Total <= maxTotal)
                .Include(i => i.Customer)
                .Include(i => i.Order)
                .ToListAsync();
        }
    }
} 