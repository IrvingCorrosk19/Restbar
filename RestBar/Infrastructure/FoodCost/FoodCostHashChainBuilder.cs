using System.Security.Cryptography;
using System.Text;
using RestBar.Models;

namespace RestBar.Infrastructure.FoodCost;

public static class FoodCostHashChainBuilder
{
    public static string Compute(FoodCostAuditEvent evt, string? previous)
    {
        var payload = string.Join("|", evt.CompanyId, evt.EntityType, evt.EntityId, evt.EventType,
            evt.ActorUserId, evt.CreatedAtUtc.ToString("O"), evt.BeforeJson ?? "", evt.AfterJson ?? "", previous ?? "");
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
    }
}
