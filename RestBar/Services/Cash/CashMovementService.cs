using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestBar.Infrastructure.Cash;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services.Cash;

public class CashMovementService : ICashMovementService
{
    private readonly RestBarContext _context;
    private readonly ILogger<CashMovementService> _logger;

    private static readonly HashSet<CashMovementType> NonCashDrawerTypes = new()
    {
        CashMovementType.SaleCard, CashMovementType.SaleYappy, CashMovementType.SaleACH, CashMovementType.SaleOther,
        CashMovementType.TipNonCash
    };

    public CashMovementService(RestBarContext context, ILogger<CashMovementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CashMovement> RecordMovementAsync(CashMovementRequest request, CancellationToken ct = default)
    {
        if (!string.IsNullOrEmpty(request.IdempotencyKey))
        {
            var existing = await _context.CashMovements.AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdempotencyKey == request.IdempotencyKey, ct);
            if (existing != null)
                return existing;
        }

        var session = await _context.CashSessions.FirstOrDefaultAsync(s => s.Id == request.CashSessionId, ct)
            ?? throw new InvalidOperationException("Cash session not found.");

        var affectsDrawer = request.AffectsCashDrawer ?? !NonCashDrawerTypes.Contains(request.MovementType);
        var lastSeq = await _context.CashMovements
            .Where(m => m.CashSessionId == request.CashSessionId)
            .Select(m => (int?)m.SequenceNumber)
            .MaxAsync(ct) ?? 0;

        var previousHash = await _context.CashMovements.AsNoTracking()
            .Where(m => m.CashSessionId == request.CashSessionId)
            .OrderByDescending(m => m.SequenceNumber)
            .Select(m => m.RecordHash)
            .FirstOrDefaultAsync(ct);

        var movement = new CashMovement
        {
            Id = Guid.NewGuid(),
            CompanyId = session.CompanyId,
            BranchId = session.BranchId,
            CashSessionId = request.CashSessionId,
            MovementType = request.MovementType,
            Direction = request.Direction,
            Amount = request.Amount,
            PaymentId = request.PaymentId,
            OrderId = request.OrderId,
            PaymentRefundId = request.PaymentRefundId,
            ReasonCode = request.ReasonCode,
            Comments = request.Comments,
            PerformedByUserId = request.PerformedByUserId,
            AuthorizedByUserId = request.AuthorizedByUserId,
            SequenceNumber = lastSeq + 1,
            PreviousHash = previousHash,
            IdempotencyKey = request.IdempotencyKey,
            Source = request.Source,
            AffectsCashDrawer = affectsDrawer
        };

        movement.RecordHash = CashHashChainBuilder.ComputeMovementHash(movement, previousHash);
        _context.CashMovements.Add(movement);

        UpdateSessionTotals(session, movement, affectsDrawer);
        await _context.SaveChangesAsync(ct);

        _logger.LogDebug("[Cash] Movement {Type} {Amount} on session {SessionId}", request.MovementType, request.Amount, request.CashSessionId);
        return movement;
    }

    public async Task<IReadOnlyList<CashMovement>> GetSessionMovementsAsync(Guid sessionId, CancellationToken ct = default)
    {
        return await _context.CashMovements.AsNoTracking()
            .Where(m => m.CashSessionId == sessionId)
            .OrderBy(m => m.SequenceNumber)
            .ToListAsync(ct);
    }

    internal static void UpdateSessionTotals(CashSession session, CashMovement movement, bool affectsDrawer)
    {
        if (movement.Direction == CashMovementDirection.In)
        {
            if (movement.MovementType is CashMovementType.SaleCash or CashMovementType.SaleCard or CashMovementType.SaleYappy
                or CashMovementType.SaleACH or CashMovementType.SaleOther)
                session.TotalSales += movement.Amount;

            if (movement.MovementType == CashMovementType.TipCash || movement.MovementType == CashMovementType.TipNonCash)
                session.TotalTips += movement.Amount;

            if (movement.MovementType == CashMovementType.PaidIn)
                session.TotalPaidIn += movement.Amount;

            if (movement.MovementType == CashMovementType.SaleCard)
                session.ExpectedCard += movement.Amount;
            else if (movement.MovementType is CashMovementType.SaleYappy or CashMovementType.SaleACH)
                session.ExpectedDigital += movement.Amount;
        }
        else
        {
            if (movement.MovementType == CashMovementType.RefundCash)
                session.TotalRefunds += movement.Amount;
            if (movement.MovementType == CashMovementType.PaidOut)
                session.TotalPaidOut += movement.Amount;
        }

        if (affectsDrawer)
        {
            session.ExpectedCash += movement.Direction == CashMovementDirection.In
                ? movement.Amount
                : -movement.Amount;
        }
    }
}
