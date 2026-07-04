using RestBar.Models;

namespace RestBar.Interfaces;

public interface IInventoryOperationsService
{
    Task DeductInventoryForSaleAsync(Guid productId, decimal quantity, Guid? stationId, Guid? branchId, Guid? companyId, Guid? orderId, Guid? userId);
    Task RestoreInventoryForCancelAsync(Guid productId, decimal quantity, Guid? stationId, Guid? branchId, Guid? companyId, Guid? orderId, Guid? userId);
    Task TransferStockBetweenStationsAsync(Guid productId, Guid fromStationId, Guid toStationId, decimal quantity, Guid? branchId, Guid? companyId, Guid? userId, string? reason = null);
    Task<InventoryMovement> LogMovementAsync(Guid productId, InventoryMovementType type, decimal quantity, decimal stockBefore, decimal stockAfter, Guid? stationId, Guid? branchId, Guid? companyId, Guid? userId, Guid? orderId, string? reason, string? reference);
    Task AllocateTipsAsync(Guid paymentId, Guid orderId, decimal tipAmount);
    Task<decimal> GetCommissionRateAsync(Guid? companyId, Guid? branchId, UserRole? role, Guid? stationId);
}
