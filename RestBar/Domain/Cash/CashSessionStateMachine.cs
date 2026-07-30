using RestBar.Models;

namespace RestBar.Domain.Cash;

/// <summary>Validates CashSession state transitions per RB-010 design.</summary>
public static class CashSessionStateMachine
{
    private static readonly Dictionary<CashSessionStatus, HashSet<CashSessionStatus>> Allowed =
        new()
        {
            [CashSessionStatus.Prepared] = new() { CashSessionStatus.Open },
            [CashSessionStatus.Open] = new() { CashSessionStatus.Operating, CashSessionStatus.Suspended },
            [CashSessionStatus.Operating] = new() { CashSessionStatus.Suspended, CashSessionStatus.Counting, CashSessionStatus.Blocked },
            [CashSessionStatus.Suspended] = new() { CashSessionStatus.Operating, CashSessionStatus.Counting },
            [CashSessionStatus.Counting] = new() { CashSessionStatus.Reconciling },
            [CashSessionStatus.Reconciling] = new() { CashSessionStatus.Closed, CashSessionStatus.Operating },
            [CashSessionStatus.Closed] = new() { CashSessionStatus.Historical, CashSessionStatus.Open },
            [CashSessionStatus.Blocked] = new() { CashSessionStatus.Closed },
            [CashSessionStatus.Audited] = new(),
            [CashSessionStatus.Historical] = new()
        };

    public static bool CanTransition(CashSessionStatus from, CashSessionStatus to) =>
        Allowed.TryGetValue(from, out var targets) && targets.Contains(to);

    public static void EnsureTransition(CashSessionStatus from, CashSessionStatus to)
    {
        if (!CanTransition(from, to))
            throw new InvalidOperationException($"Invalid cash session transition: {from} → {to}");
    }

    public static bool AllowsPayments(CashSessionStatus status) =>
        status is CashSessionStatus.Open or CashSessionStatus.Operating;

    public static bool IsTerminal(CashSessionStatus status) =>
        status is CashSessionStatus.Audited or CashSessionStatus.Historical;
}
