using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface ISupplierService
    {
        Task<IEnumerable<Supplier>> GetAllAsync();
        Task<Supplier?> GetByIdAsync(Guid id);
        Task<Supplier> CreateAsync(Supplier supplier);
        Task<Supplier> UpdateAsync(Supplier supplier);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<Supplier>> GetActiveSuppliersAsync();
        Task<IEnumerable<Supplier>> GetAllActiveSuppliersAsync();
        Task<IEnumerable<Supplier>> SearchSuppliersAsync(string searchTerm);
        Task<IEnumerable<Product>> GetProductsBySupplierAsync(Guid supplierId);
        Task<bool> ExistsAsync(Guid id);
        Task<int> GetCountAsync();
    }
} 