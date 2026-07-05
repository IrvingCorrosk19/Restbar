using Microsoft.EntityFrameworkCore;
using RestBar.Models;

namespace RestBar.Helpers;

/// <summary>
/// Validaciones operativas compartidas para proteger integridad de órdenes activas.
/// </summary>
public static class OperationalGuard
{
    public static Task<bool> TableHasActiveOrderAsync(RestBarContext ctx, Guid tableId) =>
        ctx.Orders.AnyAsync(o =>
            o.TableId == tableId &&
            o.Status != OrderStatus.Cancelled &&
            o.Status != OrderStatus.Completed);

    public static Task<bool> ProductInActiveOrdersAsync(RestBarContext ctx, Guid productId) =>
        ctx.OrderItems.AnyAsync(oi =>
            oi.ProductId == productId &&
            oi.Status != OrderItemStatus.Cancelled &&
            oi.Order != null &&
            oi.Order.Status != OrderStatus.Cancelled &&
            oi.Order.Status != OrderStatus.Completed);

    public static Task<bool> UserHasActiveOrdersAsync(RestBarContext ctx, Guid userId) =>
        ctx.Orders.AnyAsync(o =>
            o.UserId == userId &&
            o.Status != OrderStatus.Cancelled &&
            o.Status != OrderStatus.Completed);
}
