using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface IProductCategoryService
    {
        Task<IEnumerable<ProductCategory>> GetAllAsync();
        Task<ProductCategory?> GetByIdAsync(Guid id);
        Task<ProductCategory> CreateAsync(ProductCategory category);
        Task UpdateAsync(ProductCategory category);
        Task DeleteAsync(Guid id);

        // Métodos adicionales específicos para ProductCategory
        Task<ProductCategory?> GetByNameAsync(string name);
        Task<ProductCategory?> GetCategoryWithProductsAsync(Guid id);
        Task<IEnumerable<ProductCategory>> GetCategoriesWithProductsAsync();
        Task<IEnumerable<ProductCategory>> SearchCategoriesAsync(string searchTerm);
        Task<int> GetProductCountAsync(Guid categoryId);
    }
} 