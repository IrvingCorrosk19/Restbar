using RestBar.Models;

namespace RestBar.Domain.Procurement;

public static class PurchaseOrderStateMachine
{
    private static readonly Dictionary<PurchaseOrderStatus, HashSet<PurchaseOrderStatus>> Allowed = new()
    {
        [PurchaseOrderStatus.Draft] = new() { PurchaseOrderStatus.PendingApproval, PurchaseOrderStatus.Cancelled },
        [PurchaseOrderStatus.PendingApproval] = new() { PurchaseOrderStatus.Approved, PurchaseOrderStatus.Draft, PurchaseOrderStatus.Cancelled },
        [PurchaseOrderStatus.Approved] = new() { PurchaseOrderStatus.Sent, PurchaseOrderStatus.Cancelled },
        [PurchaseOrderStatus.Sent] = new() { PurchaseOrderStatus.PartiallyReceived, PurchaseOrderStatus.FullyReceived, PurchaseOrderStatus.Cancelled },
        [PurchaseOrderStatus.PartiallyReceived] = new() { PurchaseOrderStatus.FullyReceived, PurchaseOrderStatus.Closed, PurchaseOrderStatus.Returned },
        [PurchaseOrderStatus.FullyReceived] = new() { PurchaseOrderStatus.Closed },
        [PurchaseOrderStatus.Closed] = new() { PurchaseOrderStatus.Audited },
        [PurchaseOrderStatus.Cancelled] = new(),
        [PurchaseOrderStatus.Returned] = new() { PurchaseOrderStatus.Closed },
        [PurchaseOrderStatus.Audited] = new()
    };

    public static bool CanTransition(PurchaseOrderStatus from, PurchaseOrderStatus to) =>
        Allowed.TryGetValue(from, out var t) && t.Contains(to);

    public static void Ensure(PurchaseOrderStatus from, PurchaseOrderStatus to)
    {
        if (!CanTransition(from, to))
            throw new InvalidOperationException($"Invalid PO transition: {from} → {to}");
    }

    public static bool CanReceive(PurchaseOrderStatus status) =>
        status is PurchaseOrderStatus.Sent or PurchaseOrderStatus.PartiallyReceived or PurchaseOrderStatus.Approved;
}

public static class PurchaseRequestStateMachine
{
    private static readonly Dictionary<PurchaseRequestStatus, HashSet<PurchaseRequestStatus>> Allowed = new()
    {
        [PurchaseRequestStatus.Draft] = new() { PurchaseRequestStatus.Pending, PurchaseRequestStatus.Cancelled },
        [PurchaseRequestStatus.Pending] = new() { PurchaseRequestStatus.Approved, PurchaseRequestStatus.Rejected },
        [PurchaseRequestStatus.Approved] = new() { PurchaseRequestStatus.Converted, PurchaseRequestStatus.Cancelled },
        [PurchaseRequestStatus.Rejected] = new(),
        [PurchaseRequestStatus.Cancelled] = new(),
        [PurchaseRequestStatus.Converted] = new() { PurchaseRequestStatus.Completed },
        [PurchaseRequestStatus.Completed] = new() { PurchaseRequestStatus.Audited },
        [PurchaseRequestStatus.Audited] = new()
    };

    public static bool CanTransition(PurchaseRequestStatus from, PurchaseRequestStatus to) =>
        Allowed.TryGetValue(from, out var t) && t.Contains(to);

    public static void Ensure(PurchaseRequestStatus from, PurchaseRequestStatus to)
    {
        if (!CanTransition(from, to))
            throw new InvalidOperationException($"Invalid PR transition: {from} → {to}");
    }
}

/// <summary>Weighted average cost math — pure function for unit tests.</summary>
public static class CostEngineMath
{
    public static decimal ComputeMovingAverage(decimal stockBefore, decimal avgCostBefore, decimal qtyAccepted, decimal unitCost)
    {
        var totalQty = stockBefore + qtyAccepted;
        if (totalQty <= 0) return unitCost;
        if (stockBefore <= 0) return unitCost;
        return Math.Round((stockBefore * avgCostBefore + qtyAccepted * unitCost) / totalQty, 4);
    }

    public static decimal ComputeOverallScore(decimal price, decimal otif, decimal quality, decimal reliability) =>
        Math.Round(0.25m * price + 0.30m * otif + 0.25m * quality + 0.20m * reliability, 2);
}
