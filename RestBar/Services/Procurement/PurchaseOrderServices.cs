using Microsoft.EntityFrameworkCore;
using RestBar.Domain.Procurement;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services.Procurement;

public class PurchaseRequestService : IPurchaseRequestService
{
    private readonly RestBarContext _db;
    private readonly IProcurementIntegrityService _audit;
    private readonly IPurchaseOrderService _orders;

    public PurchaseRequestService(RestBarContext db, IProcurementIntegrityService audit, IPurchaseOrderService orders)
    {
        _db = db;
        _audit = audit;
        _orders = orders;
    }

    public async Task<PurchaseRequest> CreateDraftAsync(PurchaseRequest request, CancellationToken ct = default)
    {
        request.Id = Guid.NewGuid();
        request.Status = PurchaseRequestStatus.Draft;
        request.CreatedAt = DateTime.UtcNow;
        if (string.IsNullOrEmpty(request.RequestNumber))
            request.RequestNumber = await NextNumberAsync("PR", request.CompanyId, ct);
        foreach (var line in request.Lines)
            line.Id = line.Id == Guid.Empty ? Guid.NewGuid() : line.Id;
        _db.PurchaseRequests.Add(request);
        await _db.SaveChangesAsync(ct);
        return request;
    }

    public async Task<PurchaseRequest> SubmitAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var pr = await LoadAsync(id, ct);
        PurchaseRequestStateMachine.Ensure(pr.Status, PurchaseRequestStatus.Pending);
        pr.Status = PurchaseRequestStatus.Pending;
        pr.SubmittedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        await _audit.AppendAuditAsync(new ProcurementAuditInput(pr.CompanyId, pr.BranchId, userId, "PurchaseRequestSubmitted", "PurchaseRequest", id), ct);
        return pr;
    }

    public async Task<PurchaseRequest> ApproveAsync(Guid id, Guid approverId, CancellationToken ct = default)
    {
        var pr = await LoadAsync(id, ct);
        PurchaseRequestStateMachine.Ensure(pr.Status, PurchaseRequestStatus.Approved);
        pr.Status = PurchaseRequestStatus.Approved;
        pr.ApprovedByUserId = approverId;
        pr.ResolvedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        await _audit.AppendAuditAsync(new ProcurementAuditInput(pr.CompanyId, pr.BranchId, approverId, "PurchaseRequestApproved", "PurchaseRequest", id), ct);
        return pr;
    }

    public async Task<PurchaseRequest> RejectAsync(Guid id, Guid approverId, string reason, CancellationToken ct = default)
    {
        var pr = await LoadAsync(id, ct);
        PurchaseRequestStateMachine.Ensure(pr.Status, PurchaseRequestStatus.Rejected);
        pr.Status = PurchaseRequestStatus.Rejected;
        pr.ApprovedByUserId = approverId;
        pr.Notes = $"{pr.Notes}\nREJECT: {reason}".Trim();
        pr.ResolvedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return pr;
    }

    public async Task<PurchaseOrder> ConvertToOrderAsync(Guid requestId, Guid supplierId, Guid userId, CancellationToken ct = default)
    {
        var pr = await _db.PurchaseRequests.Include(r => r.Lines).FirstOrDefaultAsync(r => r.Id == requestId, ct)
            ?? throw new InvalidOperationException("PR not found.");
        PurchaseRequestStateMachine.Ensure(pr.Status, PurchaseRequestStatus.Converted);

        var order = new PurchaseOrder
        {
            CompanyId = pr.CompanyId,
            BranchId = pr.BranchId,
            SupplierId = supplierId,
            PurchaseRequestId = pr.Id,
            RequestedByUserId = userId,
            ExpectedDelivery = DateTime.UtcNow.AddDays(2),
            Lines = pr.Lines.Select(l => new PurchaseOrderLine
            {
                Id = Guid.NewGuid(),
                ProductId = l.ProductId,
                QuantityOrdered = l.Quantity,
                UnitPrice = l.EstimatedUnitCost ?? 0,
                LineTotal = Math.Round(l.Quantity * (l.EstimatedUnitCost ?? 0), 2),
                UnitOfMeasure = l.UnitOfMeasure,
                StationId = l.StationId
            }).ToList()
        };
        order.Subtotal = order.Lines.Sum(l => l.LineTotal);
        order.Total = order.Subtotal;

        var created = await _orders.CreateDraftAsync(order, ct);
        pr.Status = PurchaseRequestStatus.Converted;
        await _db.SaveChangesAsync(ct);
        return created;
    }

    private async Task<PurchaseRequest> LoadAsync(Guid id, CancellationToken ct) =>
        await _db.PurchaseRequests.Include(r => r.Lines).FirstOrDefaultAsync(r => r.Id == id, ct)
        ?? throw new InvalidOperationException("PR not found.");

    private async Task<string> NextNumberAsync(string prefix, Guid companyId, CancellationToken ct)
    {
        var count = await _db.PurchaseRequests.CountAsync(r => r.CompanyId == companyId, ct);
        return $"{prefix}-{DateTime.UtcNow:yyyy}-{count + 1:D5}";
    }
}

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly RestBarContext _db;
    private readonly IProcurementIntegrityService _audit;
    public const decimal DualApprovalThreshold = 500m;

    public PurchaseOrderService(RestBarContext db, IProcurementIntegrityService audit)
    {
        _db = db;
        _audit = audit;
    }

    public bool RequiresDualApproval(decimal total) => total >= DualApprovalThreshold;

    public async Task<PurchaseOrder> CreateDraftAsync(PurchaseOrder order, CancellationToken ct = default)
    {
        var supplier = await _db.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == order.SupplierId, ct)
            ?? throw new InvalidOperationException("Supplier not found.");
        if (supplier.Status is SupplierStatus.Blacklisted or SupplierStatus.Inactive)
            throw new InvalidOperationException("Cannot create PO for blacklisted/inactive supplier.");

        order.Id = Guid.NewGuid();
        order.Status = PurchaseOrderStatus.Draft;
        order.OrderDate = DateTime.UtcNow;
        if (string.IsNullOrEmpty(order.PoNumber))
        {
            var count = await _db.PurchaseOrders.CountAsync(o => o.CompanyId == order.CompanyId, ct);
            order.PoNumber = $"PO-{DateTime.UtcNow:yyyy}-{count + 1:D5}";
        }
        foreach (var line in order.Lines)
        {
            line.Id = line.Id == Guid.Empty ? Guid.NewGuid() : line.Id;
            line.LineTotal = Math.Round(line.QuantityOrdered * line.UnitPrice, 2);
        }
        order.Subtotal = order.Lines.Sum(l => l.LineTotal);
        order.Total = order.Subtotal + order.Tax;
        _db.PurchaseOrders.Add(order);
        await _db.SaveChangesAsync(ct);
        return order;
    }

    public async Task<PurchaseOrder> SubmitForApprovalAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var po = await LoadAsync(id, ct);
        PurchaseOrderStateMachine.Ensure(po.Status, PurchaseOrderStatus.PendingApproval);
        if (po.Lines.Count == 0) throw new InvalidOperationException("PO requires lines.");
        po.Status = PurchaseOrderStatus.PendingApproval;
        await _db.SaveChangesAsync(ct);

        if (RequiresDualApproval(po.Total))
        {
            _db.PurchaseApprovals.Add(new PurchaseApproval
            {
                Id = Guid.NewGuid(),
                CompanyId = po.CompanyId,
                BranchId = po.BranchId,
                EntityType = "PurchaseOrder",
                EntityId = po.Id,
                ApprovalType = PurchaseApprovalType.Order,
                Status = PurchaseApprovalStatus.Pending,
                RequestedByUserId = userId,
                ActualAmount = po.Total,
                ThresholdAmount = DualApprovalThreshold
            });
            await _db.SaveChangesAsync(ct);
        }

        await _audit.AppendAuditAsync(new ProcurementAuditInput(po.CompanyId, po.BranchId, userId, "PurchaseOrderSubmitted", "PurchaseOrder", id), ct);
        return po;
    }

    public async Task<PurchaseOrder> ApproveAsync(Guid id, Guid approverId, CancellationToken ct = default)
    {
        var po = await LoadAsync(id, ct);
        if (RequiresDualApproval(po.Total) && po.RequestedByUserId == approverId)
            throw new InvalidOperationException("Dual approval: requester cannot approve.");

        PurchaseOrderStateMachine.Ensure(po.Status, PurchaseOrderStatus.Approved);
        po.Status = PurchaseOrderStatus.Approved;
        po.ApprovedByUserId = approverId;

        var pending = await _db.PurchaseApprovals
            .Where(a => a.EntityId == id && a.Status == PurchaseApprovalStatus.Pending)
            .ToListAsync(ct);
        foreach (var a in pending)
        {
            a.Status = PurchaseApprovalStatus.Approved;
            a.ApprovedByUserId = approverId;
            a.ResolvedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        await _audit.AppendAuditAsync(new ProcurementAuditInput(po.CompanyId, po.BranchId, approverId, "PurchaseOrderApproved", "PurchaseOrder", id), ct);
        return po;
    }

    public async Task<PurchaseOrder> SendAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var po = await LoadAsync(id, ct);
        PurchaseOrderStateMachine.Ensure(po.Status, PurchaseOrderStatus.Sent);
        po.Status = PurchaseOrderStatus.Sent;
        po.SentAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        await _audit.AppendAuditAsync(new ProcurementAuditInput(po.CompanyId, po.BranchId, userId, "PurchaseOrderSent", "PurchaseOrder", id), ct);
        return po;
    }

    public async Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.PurchaseOrders.AsNoTracking()
            .Include(o => o.Lines).Include(o => o.Supplier)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<IReadOnlyList<PurchaseOrder>> ListOpenByBranchAsync(Guid branchId, CancellationToken ct = default) =>
        await _db.PurchaseOrders.AsNoTracking()
            .Include(o => o.Supplier)
            .Where(o => o.BranchId == branchId &&
                        o.Status != PurchaseOrderStatus.Closed &&
                        o.Status != PurchaseOrderStatus.Cancelled &&
                        o.Status != PurchaseOrderStatus.Audited)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(ct);

    public async Task<PurchaseOrder> CloseAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var po = await LoadAsync(id, ct);
        PurchaseOrderStateMachine.Ensure(po.Status, PurchaseOrderStatus.Closed);
        po.Status = PurchaseOrderStatus.Closed;
        po.ClosedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return po;
    }

    private async Task<PurchaseOrder> LoadAsync(Guid id, CancellationToken ct) =>
        await _db.PurchaseOrders.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == id, ct)
        ?? throw new InvalidOperationException("PO not found.");
}
