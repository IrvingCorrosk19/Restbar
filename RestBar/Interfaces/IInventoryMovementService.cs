using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface IInventoryMovementService
    {
        // CRUD básico
        Task<IEnumerable<InventoryMovement>> GetAllAsync();
        Task<InventoryMovement?> GetByIdAsync(Guid id);
        Task<InventoryMovement> CreateAsync(InventoryMovement movement);
        Task UpdateAsync(InventoryMovement movement);
        Task DeleteAsync(Guid id);

        // Métodos específicos para movimientos
        Task<IEnumerable<InventoryMovement>> GetMovementsByInventoryAsync(Guid inventoryId);
        Task<IEnumerable<InventoryMovement>> GetMovementsByProductAsync(Guid productId);
        Task<IEnumerable<InventoryMovement>> GetMovementsByBranchAsync(Guid branchId);
        Task<IEnumerable<InventoryMovement>> GetMovementsByUserAsync(Guid userId);
        Task<IEnumerable<InventoryMovement>> GetMovementsByTypeAsync(MovementType type);
        Task<IEnumerable<InventoryMovement>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate);
        
        // Métodos para análisis y reportes
        Task<decimal> GetTotalMovementsByTypeAsync(Guid productId, MovementType type, DateTime startDate, DateTime endDate);
        Task<IEnumerable<object>> GetMovementSummaryAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<object>> GetProductMovementHistoryAsync(Guid productId, DateTime startDate, DateTime endDate);
        
        // Métodos para crear movimientos específicos
        Task<InventoryMovement> CreatePurchaseMovementAsync(Guid inventoryId, Guid productId, Guid branchId, Guid userId, decimal quantity, decimal unitCost, string? reason = null);
        Task<InventoryMovement> CreateSaleMovementAsync(Guid inventoryId, Guid productId, Guid branchId, Guid userId, decimal quantity, string? reason = null);
        Task<InventoryMovement> CreateAdjustmentMovementAsync(Guid inventoryId, Guid productId, Guid branchId, Guid userId, decimal quantity, string reason);
        Task<InventoryMovement> CreateTransferMovementAsync(Guid fromInventoryId, Guid toInventoryId, Guid productId, Guid fromBranchId, Guid toBranchId, Guid userId, decimal quantity, string? reason = null);
        Task<InventoryMovement> CreateWasteMovementAsync(Guid inventoryId, Guid productId, Guid branchId, Guid userId, decimal quantity, string reason);
        
        // Métodos para validación
        Task<bool> ValidateMovementAsync(Guid inventoryId, MovementType type, decimal quantity);
        Task<decimal> GetCurrentStockAsync(Guid inventoryId);
    }
} 