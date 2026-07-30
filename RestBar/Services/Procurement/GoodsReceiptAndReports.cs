using Microsoft.EntityFrameworkCore;
using RestBar.Domain.Procurement;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services.Procurement;

public class GoodsReceiptService : IGoodsReceiptService
{
    private readonly RestBarContext _db;
    private readonly IProductService _products;
    private readonly IInventoryOperationsService _inventory;
    private readonly IProcurementCostEngine _costEngine;
    private readonly ISupplierScoreService _scores;
    private readonly IProcurementIntegrityService _audit;

    public GoodsReceiptService(
        RestBarContext db,
        IProductService products,
        IInventoryOperationsService inventory,
        IProcurementCostEngine costEngine,
        ISupplierScoreService scores,
        IProcurementIntegrityService audit)
    {
        _db = db;
        _products = products;
        _inventory = inventory;
        _costEngine = costEngine;
        _scores = scores;
        _audit = audit;
    }

    public async Task<GoodsReceipt> CreateDraftAsync(Guid purchaseOrderId, Guid userId, CancellationToken ct = default)
    {
        var po = await _db.PurchaseOrders.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == purchaseOrderId, ct)
            ?? throw new InvalidOperationException("PO not found.");
        if (!PurchaseOrderStateMachine.CanReceive(po.Status))
            throw new InvalidOperationException($"PO status {po.Status} cannot receive.");

        var count = await _db.GoodsReceipts.CountAsync(r => r.CompanyId == po.CompanyId, ct);
        var receipt = new GoodsReceipt
        {
            Id = Guid.NewGuid(),
            CompanyId = po.CompanyId,
            BranchId = po.BranchId,
            PurchaseOrderId = po.Id,
            ReceiptNumber = $"GRN-{DateTime.UtcNow:yyyy}-{count + 1:D5}",
            Status = GoodsReceiptStatus.Draft,
            ReceivedByUserId = userId,
            ReceivedAt = DateTime.UtcNow
        };
        _db.GoodsReceipts.Add(receipt);
        await _db.SaveChangesAsync(ct);
        return receipt;
    }

    public async Task<GoodsReceipt> CompleteAsync(Guid receiptId, IEnumerable<GoodsReceiptLineInput> lines, Guid userId, CancellationToken ct = default)
    {
        var receipt = await _db.GoodsReceipts
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == receiptId, ct)
            ?? throw new InvalidOperationException("Receipt not found.");

        if (receipt.Status == GoodsReceiptStatus.Completed)
            return receipt; // idempotent

        var po = await _db.PurchaseOrders.Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == receipt.PurchaseOrderId, ct)
            ?? throw new InvalidOperationException("PO not found.");

        if (!PurchaseOrderStateMachine.CanReceive(po.Status))
            throw new InvalidOperationException($"PO status {po.Status} cannot receive.");

        receipt.Status = GoodsReceiptStatus.InProgress;
        await _db.SaveChangesAsync(ct);

        foreach (var input in lines)
        {
            var poLine = po.Lines.FirstOrDefault(l => l.Id == input.PurchaseOrderLineId)
                ?? throw new InvalidOperationException("PO line not found.");

            var grLine = new GoodsReceiptLine
            {
                Id = Guid.NewGuid(),
                GoodsReceiptId = receipt.Id,
                PurchaseOrderLineId = input.PurchaseOrderLineId,
                ProductId = poLine.ProductId,
                QtyOrdered = poLine.QuantityOrdered,
                QtyReceived = input.QtyReceived,
                QtyAccepted = input.QtyAccepted,
                QtyRejected = input.QtyRejected,
                Disposition = input.Disposition,
                UnitCost = input.UnitCost,
                LotNumber = input.LotNumber,
                ExpiryDate = input.ExpiryDate,
                Notes = input.Notes
            };
            _db.GoodsReceiptLines.Add(grLine);

            if (input.QtyAccepted > 0 &&
                input.Disposition is ReceiptLineDisposition.Accepted or ReceiptLineDisposition.Partial or ReceiptLineDisposition.Over)
            {
                var stationId = poLine.StationId;
                var stockBefore = await _products.GetAvailableStockAsync(poLine.ProductId, po.BranchId);
                await _products.RestoreStockAsync(poLine.ProductId, input.QtyAccepted, stationId, po.BranchId);
                var stockAfter = await _products.GetAvailableStockAsync(poLine.ProductId, po.BranchId);

                var movement = await _inventory.LogMovementAsync(
                    poLine.ProductId, InventoryMovementType.Purchase, input.QtyAccepted,
                    stockBefore, stockAfter, stationId, po.BranchId, po.CompanyId, userId,
                    null, "GoodsReceipt", receipt.ReceiptNumber);

                movement.GoodsReceiptId = receipt.Id;
                movement.PurchaseOrderId = po.Id;
                movement.SupplierId = po.SupplierId;
                movement.UnitCost = input.UnitCost;

                await _costEngine.ApplyReceiptLineAsync(
                    poLine.ProductId, po.CompanyId, po.SupplierId, receipt.Id,
                    input.QtyAccepted, input.UnitCost, stockBefore, ct);
            }

            poLine.QuantityReceived += input.QtyAccepted;
        }

        receipt.Status = GoodsReceiptStatus.Completed;
        receipt.ReceivedByUserId = userId;

        var allFull = po.Lines.All(l => l.QuantityReceived >= l.QuantityOrdered - 0.0001m);
        var anyReceived = po.Lines.Any(l => l.QuantityReceived > 0);

        if (po.Status == PurchaseOrderStatus.Approved)
        {
            po.Status = PurchaseOrderStatus.Sent;
            po.SentAt ??= DateTime.UtcNow;
        }

        if (allFull)
        {
            if (po.Status == PurchaseOrderStatus.PartiallyReceived)
                PurchaseOrderStateMachine.Ensure(PurchaseOrderStatus.PartiallyReceived, PurchaseOrderStatus.FullyReceived);
            else
                PurchaseOrderStateMachine.Ensure(PurchaseOrderStatus.Sent, PurchaseOrderStatus.FullyReceived);
            po.Status = PurchaseOrderStatus.FullyReceived;
        }
        else if (anyReceived)
        {
            if (po.Status == PurchaseOrderStatus.Sent)
                PurchaseOrderStateMachine.Ensure(PurchaseOrderStatus.Sent, PurchaseOrderStatus.PartiallyReceived);
            po.Status = PurchaseOrderStatus.PartiallyReceived;
        }

        await _db.SaveChangesAsync(ct);
        await _scores.RecomputeAsync(po.SupplierId, ct);
        await _audit.AppendAuditAsync(new ProcurementAuditInput(
            po.CompanyId, po.BranchId, userId, "GoodsReceiptCompleted", "GoodsReceipt", receipt.Id), ct);

        return receipt;
    }
}

public class SupplierScoreService : ISupplierScoreService
{
    private readonly RestBarContext _db;

    public SupplierScoreService(RestBarContext db) => _db = db;

    public async Task RecomputeAsync(Guid supplierId, CancellationToken ct = default)
    {
        var supplier = await _db.Suppliers.FirstOrDefaultAsync(s => s.Id == supplierId, ct);
        if (supplier == null) return;

        var periodEnd = DateTime.UtcNow;
        var periodStart = periodEnd.AddDays(-90);

        var receipts = await _db.GoodsReceipts.AsNoTracking()
            .Include(r => r.Lines)
            .Include(r => r.PurchaseOrder)
            .Where(r => r.PurchaseOrder!.SupplierId == supplierId &&
                        r.Status == GoodsReceiptStatus.Completed &&
                        r.ReceivedAt >= periodStart)
            .ToListAsync(ct);

        decimal otif = 70, quality = 70, price = 70, reliability = 70;

        if (receipts.Count > 0)
        {
            var onTimeFull = 0;
            var rejected = 0m;
            var received = 0m;
            foreach (var r in receipts)
            {
                var expected = r.PurchaseOrder?.ExpectedDelivery;
                var onTime = expected == null || r.ReceivedAt <= expected.Value.AddDays(1);
                var ordered = r.Lines.Sum(l => l.QtyOrdered);
                var accepted = r.Lines.Sum(l => l.QtyAccepted);
                var inFull = ordered <= 0 || accepted >= ordered * 0.98m;
                if (onTime && inFull) onTimeFull++;
                rejected += r.Lines.Sum(l => l.QtyRejected);
                received += r.Lines.Sum(l => l.QtyReceived);
            }
            otif = Math.Round(100m * onTimeFull / receipts.Count, 2);
            quality = received <= 0 ? 100 : Math.Round(100m * (1 - rejected / received), 2);
            quality = Math.Clamp(quality, 0, 100);
            reliability = otif;
        }

        var overall = CostEngineMath.ComputeOverallScore(price, otif, quality, reliability);

        _db.SupplierScores.Add(new SupplierScore
        {
            Id = Guid.NewGuid(),
            SupplierId = supplierId,
            CompanyId = supplier.CompanyId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            PriceScore = price,
            OtifScore = otif,
            QualityScore = quality,
            ReliabilityScore = reliability,
            OverallScore = overall,
            ComputedAt = DateTime.UtcNow
        });

        supplier.ScoreOverall = overall;
        supplier.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }
}

public class ProcurementDashboardService : IProcurementDashboardService
{
    private readonly RestBarContext _db;

    public ProcurementDashboardService(RestBarContext db) => _db = db;

    public async Task<object> GetCommandCenterAsync(Guid companyId, Guid branchId, CancellationToken ct = default)
    {
        var openPos = await _db.PurchaseOrders.AsNoTracking()
            .Where(o => o.BranchId == branchId &&
                        o.Status != PurchaseOrderStatus.Closed &&
                        o.Status != PurchaseOrderStatus.Cancelled &&
                        o.Status != PurchaseOrderStatus.Audited)
            .Select(o => new { o.Id, o.PoNumber, o.Status, o.Total, o.ExpectedDelivery, Supplier = o.Supplier!.Name })
            .ToListAsync(ct);

        var overdue = openPos.Count(o => o.ExpectedDelivery != null && o.ExpectedDelivery < DateTime.UtcNow &&
                                         o.Status is PurchaseOrderStatus.Sent or PurchaseOrderStatus.PartiallyReceived);

        var lowStock = await _db.Products.AsNoTracking()
            .Where(p => p.CompanyId == companyId && p.TrackInventory &&
                        p.Stock != null && p.MinStock != null && p.Stock <= p.MinStock)
            .OrderBy(p => p.Stock)
            .Take(10)
            .Select(p => new { p.Id, p.Name, p.Stock, p.MinStock, p.Cost })
            .ToListAsync(ct);

        var criticalSuppliers = await _db.Suppliers.AsNoTracking()
            .Where(s => s.CompanyId == companyId && (s.ScoreOverall < 50 || s.Status == SupplierStatus.Blacklisted))
            .OrderBy(s => s.ScoreOverall)
            .Take(5)
            .Select(s => new { s.Id, s.Name, s.ScoreOverall, s.Status })
            .ToListAsync(ct);

        var today = DateTime.UtcNow.Date;
        var spendToday = await _db.GoodsReceipts.AsNoTracking()
            .Where(r => r.BranchId == branchId && r.Status == GoodsReceiptStatus.Completed && r.ReceivedAt >= today)
            .SelectMany(r => r.Lines)
            .SumAsync(l => (decimal?)(l.QtyAccepted * l.UnitCost), ct) ?? 0m;

        return new
        {
            OpenPurchaseOrders = openPos.Count,
            OverdueOrders = overdue,
            SpendToday = spendToday,
            LowStockItems = lowStock,
            CriticalSuppliers = criticalSuppliers,
            Orders = openPos.Take(15)
        };
    }
}

public class ProcurementReportService : IProcurementReportService
{
    private readonly RestBarContext _db;
    private readonly IProcurementCostEngine _cost;

    public ProcurementReportService(RestBarContext db, IProcurementCostEngine cost)
    {
        _db = db;
        _cost = cost;
    }

    public async Task<object> GetSupplierAnalysisAsync(Guid companyId, CancellationToken ct = default)
    {
        var suppliers = await _db.Suppliers.AsNoTracking()
            .Where(s => s.CompanyId == companyId)
            .OrderByDescending(s => s.ScoreOverall)
            .Select(s => new
            {
                s.Id, s.Code, s.Name, s.Status, s.IsPreferred, s.ScoreOverall, s.LeadTimeDays
            })
            .ToListAsync(ct);

        var spend = await _db.PurchaseOrders.AsNoTracking()
            .Where(o => o.CompanyId == companyId && o.Status != PurchaseOrderStatus.Cancelled)
            .GroupBy(o => o.SupplierId)
            .Select(g => new { SupplierId = g.Key, TotalSpend = g.Sum(x => x.Total), OrderCount = g.Count() })
            .ToListAsync(ct);

        return new
        {
            TotalSuppliers = suppliers.Count,
            ActiveSuppliers = suppliers.Count(s => s.Status == SupplierStatus.Active || s.Status == SupplierStatus.Preferred),
            Suppliers = suppliers.Select(s => new
            {
                s.Id, s.Code, s.Name, s.Status, s.IsPreferred, s.ScoreOverall, s.LeadTimeDays,
                Spend = spend.FirstOrDefault(x => x.SupplierId == s.Id)?.TotalSpend ?? 0,
                Orders = spend.FirstOrDefault(x => x.SupplierId == s.Id)?.OrderCount ?? 0
            })
        };
    }
}
