using RestBar.Models;
using RestBar.Services;

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
        Task<Inventory> AdjustStockAsync(Guid productId, Guid branchId, decimal adjustment);
        Task<Inventory> AdjustStockWithMovementAsync(Guid productId, Guid branchId, decimal adjustment, MovementType movementType, string reason, Guid? userId = null);
        Task<bool> DecrementStockAsync(Guid productId, decimal quantity, IOrderHubService? orderHubService = null);
        Task<IEnumerable<object>> GetStockHistoryAsync(Guid productId, Guid branchId);
        
        // ✅ NUEVOS MÉTODOS para manejo de stock dual
        Task SyncGlobalStockAsync(Guid productId);
        Task<IEnumerable<object>> GetStockReportByBranchAsync();
    }
} 