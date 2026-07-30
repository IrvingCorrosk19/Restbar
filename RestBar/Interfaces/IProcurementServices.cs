using RestBar.Models;

namespace RestBar.Interfaces;

public interface ISupplierService
{
    Task<Supplier> CreateAsync(Supplier supplier, CancellationToken ct = default);
    Task<Supplier> UpdateAsync(Supplier supplier, CancellationToken ct = default);
    Task<IReadOnlyList<Supplier>> ListByCompanyAsync(Guid companyId, CancellationToken ct = default);
    Task<Supplier?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task BlacklistAsync(Guid supplierId, Guid actorUserId, string reason, CancellationToken ct = default);
    Task<SupplierProduct> UpsertProductAsync(SupplierProduct sp, CancellationToken ct = default);
    Task<IReadOnlyList<SupplierProduct>> GetSupplierProductsAsync(Guid supplierId, CancellationToken ct = default);
    Task<Supplier?> RecommendForProductAsync(Guid companyId, Guid productId, CancellationToken ct = default);
}

public interface IPurchaseRequestService
{
    Task<PurchaseRequest> CreateDraftAsync(PurchaseRequest request, CancellationToken ct = default);
    Task<PurchaseRequest> SubmitAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<PurchaseRequest> ApproveAsync(Guid id, Guid approverId, CancellationToken ct = default);
    Task<PurchaseRequest> RejectAsync(Guid id, Guid approverId, string reason, CancellationToken ct = default);
    Task<PurchaseOrder> ConvertToOrderAsync(Guid requestId, Guid supplierId, Guid userId, CancellationToken ct = default);
}

public interface IPurchaseOrderService
{
    Task<PurchaseOrder> CreateDraftAsync(PurchaseOrder order, CancellationToken ct = default);
    Task<PurchaseOrder> SubmitForApprovalAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<PurchaseOrder> ApproveAsync(Guid id, Guid approverId, CancellationToken ct = default);
    Task<PurchaseOrder> SendAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<PurchaseOrder>> ListOpenByBranchAsync(Guid branchId, CancellationToken ct = default);
    Task<PurchaseOrder> CloseAsync(Guid id, Guid userId, CancellationToken ct = default);
    bool RequiresDualApproval(decimal total);
}

public interface IGoodsReceiptService
{
    Task<GoodsReceipt> CreateDraftAsync(Guid purchaseOrderId, Guid userId, CancellationToken ct = default);
    Task<GoodsReceipt> CompleteAsync(Guid receiptId, IEnumerable<GoodsReceiptLineInput> lines, Guid userId, CancellationToken ct = default);
}

public interface IProcurementCostEngine
{
    Task ApplyReceiptLineAsync(Guid productId, Guid companyId, Guid? supplierId, Guid? receiptId, decimal qtyAccepted, decimal unitCost, decimal stockBefore, CancellationToken ct = default);
    Task<decimal> GetTheoreticalFoodCostAsync(Guid productId, CancellationToken ct = default);
}

public interface ISupplierScoreService
{
    Task RecomputeAsync(Guid supplierId, CancellationToken ct = default);
}

public interface IProcurementIntegrityService
{
    Task AppendAuditAsync(ProcurementAuditInput input, CancellationToken ct = default);
}

public interface IProcurementDashboardService
{
    Task<object> GetCommandCenterAsync(Guid companyId, Guid branchId, CancellationToken ct = default);
}

public interface IProcurementReportService
{
    Task<object> GetSupplierAnalysisAsync(Guid companyId, CancellationToken ct = default);
}

public record GoodsReceiptLineInput(
    Guid PurchaseOrderLineId,
    decimal QtyReceived,
    decimal QtyAccepted,
    decimal QtyRejected,
    ReceiptLineDisposition Disposition,
    decimal UnitCost,
    string? LotNumber = null,
    DateTime? ExpiryDate = null,
    string? Notes = null);

public record ProcurementAuditInput(
    Guid CompanyId,
    Guid BranchId,
    Guid ActorUserId,
    string EventType,
    string EntityType,
    Guid? EntityId = null,
    string? ActorRole = null,
    string? BeforeJson = null,
    string? AfterJson = null);
