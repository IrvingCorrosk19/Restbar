using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface IPurchaseOrderService
    {
        Task<IEnumerable<PurchaseOrder>> GetAllAsync();
        Task<PurchaseOrder?> GetByIdAsync(Guid id);
        Task<PurchaseOrder> CreateAsync(PurchaseOrder purchaseOrder);
        Task<PurchaseOrder> UpdateAsync(PurchaseOrder purchaseOrder);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<PurchaseOrder>> GetByStatusAsync(PurchaseOrderStatus status);
        Task<IEnumerable<PurchaseOrder>> GetBySupplierAsync(Guid supplierId);
        Task<IEnumerable<PurchaseOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<PurchaseOrder> ApproveAsync(Guid id);
        Task<PurchaseOrder> OrderAsync(Guid id);
        Task<PurchaseOrder> CancelAsync(Guid id);
        Task<PurchaseOrder> ReceiveAsync(Guid id, List<PurchaseOrderItem> receivedItems);
        Task<string> GenerateOrderNumberAsync();
        Task<bool> ExistsAsync(Guid id);
        Task<int> GetCountAsync();
        Task<decimal> GetTotalAmountAsync();
    }
} 