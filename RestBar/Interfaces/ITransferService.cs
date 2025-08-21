using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface ITransferService
    {
        Task<IEnumerable<Transfer>> GetAllAsync();
        Task<Transfer?> GetByIdAsync(Guid id);
        Task<Transfer> CreateAsync(Transfer transfer);
        Task<Transfer> UpdateAsync(Transfer transfer);
        Task<bool> DeleteAsync(Guid id);
        
        // Status-based queries
        Task<IEnumerable<Transfer>> GetByStatusAsync(TransferStatus status);
        Task<IEnumerable<Transfer>> GetPendingTransfersAsync();
        Task<IEnumerable<Transfer>> GetApprovedTransfersAsync();
        Task<IEnumerable<Transfer>> GetInTransitTransfersAsync();
        
        // Branch-based queries
        Task<IEnumerable<Transfer>> GetBySourceBranchAsync(Guid branchId);
        Task<IEnumerable<Transfer>> GetByDestinationBranchAsync(Guid branchId);
        
        // Date range queries
        Task<IEnumerable<Transfer>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        
        // Approval and status management
        Task<Transfer> ApproveAsync(Guid transferId, Guid approvedById);
        Task<Transfer> RejectAsync(Guid transferId, Guid rejectedById, string reason);
        Task<Transfer> CancelAsync(Guid transferId, Guid cancelledById, string reason);
        Task<Transfer> MarkInTransitAsync(Guid transferId);
        Task<Transfer> ReceiveAsync(Guid transferId, Guid receivedById);
        
        // Utility methods
        Task<string> GenerateTransferNumberAsync();
        Task<bool> ExistsAsync(Guid id);
        Task<int> GetCountAsync();
        Task<decimal> GetTotalAmountAsync();
        
        // Statistics
        Task<object> GetTransferStatisticsAsync();
    }
} 