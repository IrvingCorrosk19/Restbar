using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Claims;

namespace RestBar.Services
{
    public class CustomerService : BaseTrackingService, ICustomerService
    {
        public CustomerService(RestBarContext context, IHttpContextAccessor httpContextAccessor) 
            : base(context, httpContextAccessor)
        {
        }

        private bool IsSuperAdmin()
        {
            var role = _httpContextAccessor?.HttpContext?.User?.FindFirst("UserRole")?.Value
                       ?? _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
            return string.Equals(role, "superadmin", StringComparison.OrdinalIgnoreCase);
        }

        private Guid? CurrentCompanyId()
        {
            var claim = _httpContextAccessor?.HttpContext?.User?.FindFirst("CompanyId")?.Value;
            return Guid.TryParse(claim, out var id) ? id : null;
        }

        private IQueryable<Customer> ScopedCustomers()
        {
            var q = _context.Customers.AsQueryable();
            if (IsSuperAdmin()) return q;
            var companyId = CurrentCompanyId();
            if (companyId is null) return q.Where(_ => false);
            return q.Where(c => c.CompanyId == companyId);
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
            => await ScopedCustomers().ToListAsync();

        public async Task<Customer?> GetByIdAsync(Guid id)
            => await ScopedCustomers().FirstOrDefaultAsync(c => c.Id == id);

        public async Task<Customer> CreateAsync(Customer customer)
        {
            if (customer.Id == Guid.Empty)
                customer.Id = Guid.NewGuid();

            SetCreatedTracking(customer);
            customer.LoyaltyPoints = 0;

            if (!customer.CompanyId.HasValue || !customer.BranchId.HasValue)
            {
                var userIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)
                    ?? _httpContextAccessor?.HttpContext?.User?.FindFirst("UserId");
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    var user = await _context.Users
                        .Include(u => u.Branch)
                        .FirstOrDefaultAsync(u => u.Id == userId);

                    if (user?.Branch != null)
                    {
                        customer.CompanyId ??= user.Branch.CompanyId;
                        customer.BranchId ??= user.BranchId;
                    }
                }

                customer.CompanyId ??= CurrentCompanyId();
            }

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task UpdateAsync(Customer customer)
        {
            var existing = await GetByIdAsync(customer.Id)
                ?? throw new UnauthorizedAccessException("Cliente no accesible en este tenant.");

            var tracked = _context.ChangeTracker.Entries<Customer>()
                .FirstOrDefault(e => e.Entity.Id == customer.Id);
            if (tracked != null) tracked.State = EntityState.Detached;

            customer.CompanyId ??= existing.CompanyId;
            customer.BranchId ??= existing.BranchId;
            SetUpdatedTracking(customer);
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var customer = await GetByIdAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Customer?> GetByEmailAsync(string email)
            => await ScopedCustomers().FirstOrDefaultAsync(c => c.Email == email);

        public async Task<Customer?> GetByPhoneAsync(string phone)
            => await ScopedCustomers().FirstOrDefaultAsync(c => c.Phone == phone);

        public async Task<IEnumerable<Customer>> GetByLoyaltyPointsRangeAsync(int minPoints, int maxPoints)
            => await ScopedCustomers()
                .Where(c => c.LoyaltyPoints >= minPoints && c.LoyaltyPoints <= maxPoints)
                .ToListAsync();

        public async Task<Customer?> GetCustomerWithOrdersAsync(Guid id)
            => await ScopedCustomers()
                .Include(c => c.Orders).ThenInclude(o => o.OrderItems)
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<Customer?> GetCustomerWithInvoicesAsync(Guid id)
            => await ScopedCustomers()
                .Include(c => c.Invoices)
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task UpdateLoyaltyPointsAsync(Guid id, int points)
        {
            var customer = await GetByIdAsync(id);
            if (customer != null)
            {
                customer.LoyaltyPoints = points;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
            => await ScopedCustomers()
                .Where(c => (c.FullName != null && c.FullName.Contains(searchTerm)) ||
                           (c.Email != null && c.Email.Contains(searchTerm)) ||
                           (c.Phone != null && c.Phone.Contains(searchTerm)))
                .ToListAsync();
    }
}
