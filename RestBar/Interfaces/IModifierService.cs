using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface IModifierService
    {
        Task<IEnumerable<Modifier>> GetAllAsync();
        Task<Modifier?> GetByIdAsync(Guid id);
        Task<Modifier> CreateAsync(Modifier modifier);
        Task UpdateAsync(Modifier modifier);
        Task DeleteAsync(Guid id);

        // Métodos adicionales específicos para Modifier
        Task<Modifier?> GetByNameAsync(string name);
        Task<IEnumerable<Modifier>> GetByCostRangeAsync(decimal minCost, decimal maxCost);
        Task<Modifier?> GetModifierWithProductsAsync(Guid id);
        Task<IEnumerable<Modifier>> GetModifiersWithProductsAsync();
        Task<IEnumerable<Modifier>> GetFreeModifiersAsync();
        Task<IEnumerable<Modifier>> GetPaidModifiersAsync();
    }
} 