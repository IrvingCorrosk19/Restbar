using System.Security.Cryptography;
using System.Text;
using RestBar.Models;

namespace RestBar.Infrastructure.Procurement;

public static class ProcurementHashChainBuilder
{
    public static string ComputeEventHash(ProcurementAuditEvent evt, string? previousHash)
    {
        var payload = string.Join("|",
            evt.CompanyId, evt.EntityType, evt.EntityId, evt.EventType,
            evt.ActorUserId, evt.CreatedAtUtc.ToString("O"),
            evt.BeforeJson ?? "", evt.AfterJson ?? "", previousHash ?? "");
        return Sha256Hex(payload);
    }

    private static string Sha256Hex(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
