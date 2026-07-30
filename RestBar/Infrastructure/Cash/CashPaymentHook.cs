using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RestBar.Domain.Cash;
using RestBar.Infrastructure.Foundation;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Infrastructure.Cash;

public class CashPaymentHook : ICashPaymentHook
{
    private readonly RestBarContext _context;
    private readonly ICashSessionService _sessionService;
    private readonly ICashMovementService _movementService;
    private readonly FeatureFlags _flags;

    private static readonly HashSet<string> CashMethods = new(StringComparer.OrdinalIgnoreCase)
        { "Efectivo", "Cash", "efectivo" };

    private static readonly HashSet<string> CardMethods = new(StringComparer.OrdinalIgnoreCase)
        { "Tarjeta", "Card", "tarjeta", "Crédito", "Débito" };

    private static readonly HashSet<string> DigitalMethods = new(StringComparer.OrdinalIgnoreCase)
        { "Yappy", "Transferencia", "ACH", "Nequi" };

    public CashPaymentHook(
        RestBarContext context,
        ICashSessionService sessionService,
        ICashMovementService movementService,
        IOptions<FeatureFlags> flags)
    {
        _context = context;
        _sessionService = sessionService;
        _movementService = movementService;
        _flags = flags.Value;
    }

    public async Task OnPaymentCompletedAsync(Payment payment, Order order, CancellationToken ct = default)
    {
        if (!_flags.EnableCashModule)
            return;

        if (payment.ProcessedByUserId == null || payment.BranchId == null)
            return;

        var session = await _sessionService.GetActiveSessionForUserAsync(
            payment.ProcessedByUserId.Value, payment.BranchId.Value, ct);

        if (session == null)
        {
            if (IsCashMethod(payment.Method))
                throw new InvalidOperationException("Se requiere sesión de caja abierta para pagos en efectivo.");
            return;
        }

        if (!CashSessionStateMachine.AllowsPayments(session.Status))
            throw new InvalidOperationException($"Sesión de caja en estado {session.Status}; no se aceptan pagos.");

        if (session.Status == CashSessionStatus.Open)
            await _sessionService.TransitionToOperatingAsync(session.Id, payment.ProcessedByUserId.Value, ct);

        var (movementType, affectsCash) = MapPaymentMethod(payment.Method);
        var idempotencyKey = payment.IdempotencyKey != null ? $"pay-{payment.IdempotencyKey}" : $"pay-{payment.Id}";

        await _movementService.RecordMovementAsync(new CashMovementRequest(
            session.Id,
            movementType,
            CashMovementDirection.In,
            payment.Amount,
            payment.ProcessedByUserId.Value,
            PaymentId: payment.Id,
            OrderId: order.Id,
            IdempotencyKey: idempotencyKey,
            Source: CashMovementSource.Payment,
            AffectsCashDrawer: affectsCash), ct);

        if (payment.TipAmount > 0)
        {
            await _movementService.RecordMovementAsync(new CashMovementRequest(
                session.Id,
                affectsCash ? CashMovementType.TipCash : CashMovementType.TipNonCash,
                CashMovementDirection.In,
                payment.TipAmount,
                payment.ProcessedByUserId.Value,
                PaymentId: payment.Id,
                OrderId: order.Id,
                IdempotencyKey: $"{idempotencyKey}-tip",
                Source: CashMovementSource.Payment,
                AffectsCashDrawer: affectsCash), ct);
        }

        payment.CashSessionId = session.Id;
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync(ct);
    }

    public async Task OnRefundCompletedAsync(PaymentRefund refund, Payment payment, CancellationToken ct = default)
    {
        if (!_flags.EnableCashModule)
            return;

        if (payment.CashSessionId == null || refund.ProcessedByUserId == null)
            return;

        if (!IsCashMethod(payment.Method))
            return;

        await _movementService.RecordMovementAsync(new CashMovementRequest(
            payment.CashSessionId.Value,
            CashMovementType.RefundCash,
            CashMovementDirection.Out,
            refund.Amount,
            refund.ProcessedByUserId.Value,
            PaymentId: payment.Id,
            PaymentRefundId: refund.Id,
            OrderId: refund.OrderId,
            IdempotencyKey: $"refund-{refund.Id}",
            Source: CashMovementSource.Refund,
            AffectsCashDrawer: true), ct);

        refund.CashSessionId = payment.CashSessionId;
        _context.PaymentRefunds.Update(refund);
        await _context.SaveChangesAsync(ct);
    }

    private static (CashMovementType type, bool affectsCash) MapPaymentMethod(string method)
    {
        if (IsCashMethod(method))
            return (CashMovementType.SaleCash, true);
        if (CardMethods.Contains(method))
            return (CashMovementType.SaleCard, false);
        if (DigitalMethods.Contains(method))
            return method.Contains("Yappy", StringComparison.OrdinalIgnoreCase)
                ? (CashMovementType.SaleYappy, false)
                : (CashMovementType.SaleACH, false);
        return (CashMovementType.SaleOther, false);
    }

    private static bool IsCashMethod(string method) => CashMethods.Contains(method);
}
