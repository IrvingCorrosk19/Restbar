using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace RestBar.Services
{
    public class SupplierService : BaseTrackingService, ISupplierService
    {
        public SupplierService(RestBarContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var userEmail = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
                return null;

            return await _context.Users
                .Include(u => u.Branch)
                .ThenInclude(b => b.Company)
                .FirstOrDefaultAsync(u => u.Email == userEmail);
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var companyId = user?.Branch?.CompanyId;

                // Si no hay usuario autenticado o no tiene company, devolver todos los proveedores
                if (companyId == null)
                {
                    return await _context.Suppliers
                        .Include(s => s.Products)
                        .Include(s => s.Company)
                        .OrderBy(s => s.Name)
                        .ToListAsync();
                }

                return await _context.Suppliers
                    .Where(s => s.CompanyId == companyId)
                    .Include(s => s.Products)
                    .Include(s => s.Company)
                    .OrderBy(s => s.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierService] Error en GetAllAsync: {ex.Message}");
                // En caso de error, devolver lista vacía
                return new List<Supplier>();
            }
        }

        public async Task<Supplier?> GetByIdAsync(Guid id)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var companyId = user?.Branch?.CompanyId;

                // Si no hay usuario autenticado o no tiene company, buscar sin filtro de company
                if (companyId == null)
                {
                    return await _context.Suppliers
                        .Where(s => s.Id == id)
                        .Include(s => s.Products)
                        .Include(s => s.Company)
                        .FirstOrDefaultAsync();
                }

                return await _context.Suppliers
                    .Where(s => s.Id == id && s.CompanyId == companyId)
                    .Include(s => s.Products)
                    .Include(s => s.Company)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierService] Error en GetByIdAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<Supplier> CreateAsync(Supplier supplier)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                supplier.Id = Guid.NewGuid();
                supplier.CompanyId = user?.Branch?.CompanyId;
                supplier.CreatedAt = DateTime.UtcNow;
                supplier.IsActive = true;
                
                _context.Suppliers.Add(supplier);
                await _context.SaveChangesAsync();
                
                return supplier;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SupplierService] Error en CreateAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<Supplier> UpdateAsync(Supplier supplier)
        {
            var user = await GetCurrentUserAsync();
            var companyId = user?.Branch?.CompanyId;

            var existingSupplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == supplier.Id && s.CompanyId == companyId);
            
            if (existingSupplier == null)
                throw new ArgumentException("Proveedor no encontrado");

            existingSupplier.Name = supplier.Name;
            existingSupplier.Description = supplier.Description;
            existingSupplier.ContactPerson = supplier.ContactPerson;
            existingSupplier.Email = supplier.Email;
            existingSupplier.Phone = supplier.Phone;
            existingSupplier.Fax = supplier.Fax;
            existingSupplier.Address = supplier.Address;
            existingSupplier.City = supplier.City;
            existingSupplier.State = supplier.State;
            existingSupplier.PostalCode = supplier.PostalCode;
            existingSupplier.Country = supplier.Country;
            existingSupplier.TaxId = supplier.TaxId;
            existingSupplier.AccountNumber = supplier.AccountNumber;
            existingSupplier.Website = supplier.Website;
            existingSupplier.Notes = supplier.Notes;
            existingSupplier.PaymentTerms = supplier.PaymentTerms;
            existingSupplier.LeadTimeDays = supplier.LeadTimeDays;
            existingSupplier.IsActive = supplier.IsActive;
            existingSupplier.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingSupplier;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await GetCurrentUserAsync();
            var companyId = user?.Branch?.CompanyId;

            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == id && s.CompanyId == companyId);
            
            if (supplier == null)
                return false;

            // Verificar si tiene productos asociados
            var hasProducts = await _context.Products.AnyAsync(p => p.SupplierId == id);
            if (hasProducts)
                throw new InvalidOperationException("No se puede eliminar el proveedor porque tiene productos asociados");

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Supplier>> GetActiveSuppliersAsync()
        {
            var user = await GetCurrentUserAsync();
            var companyId = user?.Branch?.CompanyId;

            return await _context.Suppliers
                .Where(s => s.IsActive && s.CompanyId == companyId)
                .Include(s => s.Products)
                .Include(s => s.Company)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Supplier>> SearchSuppliersAsync(string searchTerm)
        {
            var user = await GetCurrentUserAsync();
            var companyId = user?.Branch?.CompanyId;

            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetActiveSuppliersAsync();

            return await _context.Suppliers
                .Where(s => s.IsActive && s.CompanyId == companyId && (
                    s.Name.Contains(searchTerm) ||
                    s.ContactPerson!.Contains(searchTerm) ||
                    s.Email!.Contains(searchTerm) ||
                    s.Phone!.Contains(searchTerm) ||
                    s.City!.Contains(searchTerm)
                ))
                .Include(s => s.Products)
                .Include(s => s.Company)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsBySupplierAsync(Guid supplierId)
        {
            return await _context.Products
                .Where(p => p.SupplierId == supplierId && p.IsActive == true)
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            var user = await GetCurrentUserAsync();
            var companyId = user?.Branch?.CompanyId;

            return await _context.Suppliers.AnyAsync(s => s.Id == id && s.CompanyId == companyId);
        }

        public async Task<int> GetCountAsync()
        {
            var user = await GetCurrentUserAsync();
            var companyId = user?.Branch?.CompanyId;

            return await _context.Suppliers.CountAsync(s => s.CompanyId == companyId);
        }
    }
} 