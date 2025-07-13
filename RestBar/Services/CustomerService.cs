using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services
{
    public class CustomerService : BaseTrackingService, ICustomerService
    {
        public CustomerService(RestBarContext context, IHttpContextAccessor httpContextAccessor) 
            : base(context, httpContextAccessor)
        {
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers.ToListAsync();
        }

        public async Task<Customer?> GetByIdAsync(Guid id)
        {
            return await _context.Customers.FindAsync(id);
        }

        public async Task<Customer> CreateAsync(Customer customer)
        {
            // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService
            customer.LoyaltyPoints = 0;
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task UpdateAsync(Customer customer)
        {
            try
            {
                // Buscar si hay una entidad con el mismo ID siendo rastreada
                var existingEntity = _context.ChangeTracker.Entries<Customer>()
                    .FirstOrDefault(e => e.Entity.Id == customer.Id);

                if (existingEntity != null)
                {
                    // Detach la entidad existente para evitar conflictos
                    existingEntity.State = EntityState.Detached;
                }

                // Usar Update para manejar automáticamente el tracking
                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al actualizar el cliente en la base de datos.", ex);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<Customer?> GetByPhoneAsync(string phone)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Phone == phone);
        }

        public async Task<IEnumerable<Customer>> GetByLoyaltyPointsRangeAsync(int minPoints, int maxPoints)
        {
            return await _context.Customers
                .Where(c => c.LoyaltyPoints >= minPoints && c.LoyaltyPoints <= maxPoints)
                .ToListAsync();
        }

        public async Task<Customer?> GetCustomerWithOrdersAsync(Guid id)
        {
            return await _context.Customers
                .Include(c => c.Orders)
                    .ThenInclude(o => o.OrderItems)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Customer?> GetCustomerWithInvoicesAsync(Guid id)
        {
            return await _context.Customers
                .Include(c => c.Invoices)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task UpdateLoyaltyPointsAsync(Guid id, int points)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                customer.LoyaltyPoints = points;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
        {
            return await _context.Customers
                .Where(c => (c.FullName != null && c.FullName.Contains(searchTerm)) ||
                           (c.Email != null && c.Email.Contains(searchTerm)) ||
                           (c.Phone != null && c.Phone.Contains(searchTerm)))
                .ToListAsync();
        }
    }
} 