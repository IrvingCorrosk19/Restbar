using RestBar.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestBar.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<IEnumerable<Category>> GetCategoriesByCompanyAndBranchAsync(Guid companyId, Guid branchId);
        Task<Category> GetCategoryByIdAsync(Guid id);
        Task<Category> CreateCategoryAsync(Category category);
        Task<Category> UpdateCategoryAsync(Guid id, Category category);
        Task<bool> DeleteCategoryAsync(Guid id);
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<IEnumerable<Category>> GetActiveCategoriesByCompanyAndBranchAsync(Guid companyId, Guid branchId);
    }
}