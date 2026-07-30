using System.Security.Cryptography;
using System.Text;
using RestBar.Models;

namespace RestBar.Infrastructure.Intelligence;

public static class BiHashChainBuilder
{
    public static string Compute(BiAuditEvent evt)
    {
        var payload = $"{evt.CompanyId}|{evt.BranchId}|{evt.ActorUserId}|{evt.QueryName}|{evt.DurationMs}|{evt.CreatedAtUtc:O}|{evt.FiltersJson}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
    }
}
