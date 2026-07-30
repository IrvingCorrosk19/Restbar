using Microsoft.EntityFrameworkCore;
using RestBar.Domain.Procurement;
using RestBar.Infrastructure.Procurement;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services.Procurement;

public class ProcurementIntegrityService : IProcurementIntegrityService
{
    private readonly RestBarContext _db;
    public ProcurementIntegrityService(RestBarContext db) => _db = db;

    public async Task AppendAuditAsync(ProcurementAuditInput input, CancellationToken ct = default)
    {
        var prev = await _db.ProcurementAuditEvents.AsNoTracking()
            .Where(e => e.CompanyId == input.CompanyId)
            .OrderByDescending(e => e.CreatedAtUtc)
            .Select(e => e.EventHash)
            .FirstOrDefaultAsync(ct);

        var evt = new ProcurementAuditEvent
        {
            Id = Guid.NewGuid(),
            CompanyId = input.CompanyId,
            BranchId = input.BranchId,
            EntityType = input.EntityType,
            EntityId = input.EntityId,
            EventType = input.EventType,
            ActorUserId = input.ActorUserId,
            ActorRole = input.ActorRole,
            BeforeJson = input.BeforeJson,
            AfterJson = input.AfterJson,
            PreviousEventHash = prev
        };
        evt.EventHash = ProcurementHashChainBuilder.ComputeEventHash(evt, prev);
        _db.ProcurementAuditEvents.Add(evt);
        await _db.SaveChangesAsync(ct);
    }
}

public class ProcurementCostEngine : IProcurementCostEngine
{
    private readonly RestBarContext _db;
    private readonly IProcurementIntegrityService _audit;

    public ProcurementCostEngine(RestBarContext db, IProcurementIntegrityService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task ApplyReceiptLineAsync(
        Guid productId, Guid companyId, Guid? supplierId, Guid? receiptId,
        decimal qtyAccepted, decimal unitCost, decimal stockBefore, CancellationToken ct = default)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId, ct)
            ?? throw new InvalidOperationException("Product not found.");

        var avgBefore = product.AverageCost ?? product.Cost ?? unitCost;
        var newAvg = CostEngineMath.ComputeMovingAverage(stockBefore, avgBefore, qtyAccepted, unitCost);

        product.LastPurchaseCost = unitCost;
        product.AverageCost = newAvg;
        product.Cost = newAvg;
        product.LastPurchaseAt = DateTime.UtcNow;

        _db.PriceHistories.Add(new PriceHistory
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            ProductId = productId,
            SupplierId = supplierId,
            UnitCost = unitCost,
            Source = PriceHistorySource.Receipt,
            GoodsReceiptId = receiptId,
            RecordedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);

        await _audit.AppendAuditAsync(new ProcurementAuditInput(
            companyId, product.BranchId ?? Guid.Empty, Guid.Empty, "CostUpdated", "Product", productId,
            AfterJson: $"{{\"last\":{unitCost},\"avg\":{newAvg}}}"), ct);
    }

    public async Task<decimal> GetTheoreticalFoodCostAsync(Guid productId, CancellationToken ct = default)
    {
        var recipe = await _db.Recipes.AsNoTracking()
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.ProductId == productId && r.IsActive, ct);

        if (recipe == null || recipe.Lines.Count == 0)
        {
            var p = await _db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == productId, ct);
            return p?.AverageCost ?? p?.Cost ?? 0m;
        }

        decimal total = 0;
        foreach (var line in recipe.Lines)
        {
            var ing = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == line.IngredientProductId, ct);
            var cost = ing?.AverageCost ?? ing?.Cost ?? 0m;
            total += line.Quantity * cost;
        }
        return Math.Round(total, 4);
    }
}

public class SupplierService : ISupplierService
{
    private readonly RestBarContext _db;
    private readonly IProcurementIntegrityService _audit;

    public SupplierService(RestBarContext db, IProcurementIntegrityService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<Supplier> CreateAsync(Supplier supplier, CancellationToken ct = default)
    {
        supplier.Id = supplier.Id == Guid.Empty ? Guid.NewGuid() : supplier.Id;
        supplier.CreatedAt = DateTime.UtcNow;
        supplier.UpdatedAt = DateTime.UtcNow;
        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync(ct);
        await _audit.AppendAuditAsync(new ProcurementAuditInput(
            supplier.CompanyId, Guid.Empty, Guid.Empty, "SupplierCreated", "Supplier", supplier.Id), ct);
        return supplier;
    }

    public async Task<Supplier> UpdateAsync(Supplier supplier, CancellationToken ct = default)
    {
        var existing = await _db.Suppliers.FirstOrDefaultAsync(s => s.Id == supplier.Id, ct)
            ?? throw new InvalidOperationException("Supplier not found.");
        existing.Name = supplier.Name;
        existing.Email = supplier.Email;
        existing.Phone = supplier.Phone;
        existing.TaxId = supplier.TaxId;
        existing.PaymentTermsDays = supplier.PaymentTermsDays;
        existing.LeadTimeDays = supplier.LeadTimeDays;
        existing.IsPreferred = supplier.IsPreferred;
        existing.Status = supplier.Status;
        existing.Notes = supplier.Notes;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<IReadOnlyList<Supplier>> ListByCompanyAsync(Guid companyId, CancellationToken ct = default) =>
        await _db.Suppliers.AsNoTracking()
            .Where(s => s.CompanyId == companyId)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

    public async Task<Supplier?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Suppliers.AsNoTracking().Include(s => s.Contacts).FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task BlacklistAsync(Guid supplierId, Guid actorUserId, string reason, CancellationToken ct = default)
    {
        var s = await _db.Suppliers.FirstOrDefaultAsync(x => x.Id == supplierId, ct)
            ?? throw new InvalidOperationException("Supplier not found.");
        s.Status = SupplierStatus.Blacklisted;
        s.Notes = $"{s.Notes}\nBLACKLIST: {reason}".Trim();
        s.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        await _audit.AppendAuditAsync(new ProcurementAuditInput(
            s.CompanyId, Guid.Empty, actorUserId, "SupplierBlacklisted", "Supplier", supplierId,
            AfterJson: reason), ct);
    }

    public async Task<SupplierProduct> UpsertProductAsync(SupplierProduct sp, CancellationToken ct = default)
    {
        var existing = await _db.SupplierProducts
            .FirstOrDefaultAsync(x => x.SupplierId == sp.SupplierId && x.ProductId == sp.ProductId, ct);
        if (existing != null)
        {
            existing.AgreedUnitPrice = sp.AgreedUnitPrice;
            existing.PackSize = sp.PackSize;
            existing.UnitOfMeasure = sp.UnitOfMeasure;
            existing.MinOrderQty = sp.MinOrderQty;
            existing.IsActive = sp.IsActive;
            existing.SupplierSku = sp.SupplierSku;
            await _db.SaveChangesAsync(ct);
            return existing;
        }
        sp.Id = Guid.NewGuid();
        _db.SupplierProducts.Add(sp);
        await _db.SaveChangesAsync(ct);
        return sp;
    }

    public async Task<IReadOnlyList<SupplierProduct>> GetSupplierProductsAsync(Guid supplierId, CancellationToken ct = default) =>
        await _db.SupplierProducts.AsNoTracking()
            .Include(sp => sp.Product)
            .Where(sp => sp.SupplierId == supplierId && sp.IsActive)
            .ToListAsync(ct);

    public async Task<Supplier?> RecommendForProductAsync(Guid companyId, Guid productId, CancellationToken ct = default)
    {
        var links = await _db.SupplierProducts.AsNoTracking()
            .Where(sp => sp.CompanyId == companyId && sp.ProductId == productId && sp.IsActive)
            .ToListAsync(ct);
        if (links.Count == 0) return null;

        var supplierIds = links.Select(l => l.SupplierId).ToList();
        var suppliers = await _db.Suppliers.AsNoTracking()
            .Where(s => supplierIds.Contains(s.Id) &&
                        s.Status != SupplierStatus.Blacklisted &&
                        s.Status != SupplierStatus.OnHold &&
                        s.Status != SupplierStatus.Inactive)
            .ToListAsync(ct);

        return suppliers
            .OrderByDescending(s => s.IsPreferred)
            .ThenByDescending(s => s.ScoreOverall)
            .ThenBy(s => links.First(l => l.SupplierId == s.Id).AgreedUnitPrice)
            .FirstOrDefault();
    }
}
