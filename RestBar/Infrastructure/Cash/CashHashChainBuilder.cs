using System.Security.Cryptography;
using System.Text;
using RestBar.Models;

namespace RestBar.Infrastructure.Cash;

public static class CashHashChainBuilder
{
    public static string ComputeMovementHash(CashMovement movement, string? previousHash)
    {
        var payload = string.Join("|",
            movement.CashSessionId,
            movement.SequenceNumber,
            movement.MovementType,
            movement.Direction,
            movement.Amount.ToString("F2"),
            movement.PaymentId,
            movement.IdempotencyKey ?? "",
            movement.CreatedAtUtc.ToString("O"),
            previousHash ?? "");
        return Sha256Hex(payload);
    }

    public static string ComputeAuditEventHash(CashAuditEvent evt, string? previousHash)
    {
        var payload = string.Join("|",
            evt.CashSessionId,
            evt.EventType,
            evt.ActorUserId,
            evt.CreatedAtUtc.ToString("O"),
            evt.BeforeJson ?? "",
            evt.AfterJson ?? "",
            previousHash ?? "");
        return Sha256Hex(payload);
    }

    public static string ComputeZReportHash(string reportJson, Guid sessionId, DateTime generatedAtUtc)
    {
        var payload = $"{sessionId}|{generatedAtUtc:O}|{reportJson}";
        return Sha256Hex(payload);
    }

    private static string Sha256Hex(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
