using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<Inventory>> GetAllAsync();
        Task<Inventory?> GetByIdAsync(Guid id);
        Task<Inventory> CreateAsync(Inventory inventory);
        Task UpdateAsync(Inventory inventory);
        Task DeleteAsync(Guid id);

        // Métodos adicionales específicos para Inventory
        Task<IEnumerable<Inventory>> GetByBranchIdAsync(Guid branchId);
        Task<IEnumerable<Inventory>> GetByProductIdAsync(Guid productId);
        Task<IEnumerable<Inventory>> GetLowStockItemsAsync();
        Task<Inventory?> GetByBranchAndProductAsync(Guid branchId, Guid productId);
        Task UpdateStockAsync(Guid id, decimal quantity);
    }
} 