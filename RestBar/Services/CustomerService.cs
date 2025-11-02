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
            try
            {
                Console.WriteLine($"🔍 [CustomerService] CreateAsync() - Iniciando creación de cliente: {customer.FullName}");
                
                // ✅ Generar ID si no existe
                if (customer.Id == Guid.Empty)
                {
                    customer.Id = Guid.NewGuid();
                }
                
                // ✅ Usar SetCreatedTracking para establecer todos los campos de auditoría
                SetCreatedTracking(customer);
                
                customer.LoyaltyPoints = 0;
                
                // ✅ Obtener usuario actual para CompanyId y BranchId si no están establecidos
                if (!customer.CompanyId.HasValue || !customer.BranchId.HasValue)
                {
                    var userIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                    if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                    {
                        var user = await _context.Users
                            .Include(u => u.Branch)
                            .FirstOrDefaultAsync(u => u.Id == userId);
                        
                        if (user != null && user.Branch != null)
                        {
                            if (!customer.CompanyId.HasValue)
                                customer.CompanyId = user.Branch.CompanyId;
                            if (!customer.BranchId.HasValue)
                                customer.BranchId = user.BranchId;
                            Console.WriteLine($"✅ [CustomerService] CreateAsync() - Asignando CompanyId: {customer.CompanyId}, BranchId: {customer.BranchId}");
                        }
                    }
                }
                
                Console.WriteLine($"✅ [CustomerService] CreateAsync() - Campos establecidos: CreatedBy={customer.CreatedBy}, CreatedAt={customer.CreatedAt}, UpdatedAt={customer.UpdatedAt}");
                
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"✅ [CustomerService] CreateAsync() - Cliente creado exitosamente: {customer.FullName} (ID: {customer.Id})");
                return customer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [CustomerService] CreateAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [CustomerService] CreateAsync() - StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task UpdateAsync(Customer customer)
        {
            try
            {
                Console.WriteLine($"🔍 [CustomerService] UpdateAsync() - Actualizando cliente: {customer.FullName} (ID: {customer.Id})");
                
                // Buscar si hay una entidad con el mismo ID siendo rastreada
                var existingEntity = _context.ChangeTracker.Entries<Customer>()
                    .FirstOrDefault(e => e.Entity.Id == customer.Id);

                if (existingEntity != null)
                {
                    // Detach la entidad existente para evitar conflictos
                    existingEntity.State = EntityState.Detached;
                }

                // ✅ Usar SetUpdatedTracking para establecer campos de auditoría de actualización
                SetUpdatedTracking(customer);
                
                Console.WriteLine($"✅ [CustomerService] UpdateAsync() - Campos actualizados: UpdatedBy={customer.UpdatedBy}, UpdatedAt={customer.UpdatedAt}");

                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"✅ [CustomerService] UpdateAsync() - Cliente actualizado exitosamente");
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