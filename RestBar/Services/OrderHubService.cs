using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestBar.Hubs;
using RestBar.Infrastructure.Foundation;
using RestBar.Models;

namespace RestBar.Services;

public class OrderHubService : IOrderHubService
{
    private readonly IHubContext<OrderHub> _hubContext;
    private readonly RestBarContext _db;
    private readonly ITenantScopeAccessor _tenant;
    private readonly ILogger<OrderHubService> _logger;

    public OrderHubService(
        IHubContext<OrderHub> hubContext,
        RestBarContext db,
        ITenantScopeAccessor tenant,
        ILogger<OrderHubService> logger)
    {
        _hubContext = hubContext;
        _db = db;
        _tenant = tenant;
        _logger = logger;
    }

    public async Task NotifyOrderStatusChanged(Guid orderId, OrderStatus newStatus)
    {
        await _hubContext.Clients.Group($"order_{orderId}")
            .SendAsync("OrderStatusChanged", orderId, newStatus.ToString());
    }

    public async Task NotifyOrderItemStatusChanged(Guid orderId, Guid orderItemId, OrderItemStatus newStatus)
    {
        var data = new
        {
            OrderId = orderId,
            ItemId = orderItemId,
            Status = newStatus.ToString(),
            Message = newStatus == OrderItemStatus.Cancelled
                ? "🗑️ Item eliminado de la orden"
                : $"✅ Item actualizado a {newStatus}",
            Type = newStatus == OrderItemStatus.Cancelled ? "item_deleted" : "item_updated",
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"order_{orderId}").SendAsync("OrderItemStatusChanged", data);

        var companyId = await ResolveOrderCompanyAsync(orderId);
        if (companyId is Guid cid)
        {
            await _hubContext.Clients.Group(SignalRTenantGroups.Kitchen(cid)).SendAsync("OrderItemStatusChanged", data);
            if (newStatus == OrderItemStatus.Cancelled)
                await _hubContext.Clients.Group(SignalRTenantGroups.Orders(cid)).SendAsync("OrderItemStatusChanged", data);
        }
    }

    public async Task NotifyOrderItemUpdated(Guid orderId, Guid orderItemId, Guid productId, string productName, string newStatus, string timestamp)
    {
        await _hubContext.Clients.Group($"order_{orderId}")
            .SendAsync("OrderItemUpdated", new
            {
                ItemId = orderItemId,
                ProductId = productId,
                ProductName = productName,
                NewStatus = newStatus,
                Timestamp = timestamp
            });
    }

    public async Task NotifyNewOrder(Guid orderId, string tableNumber)
    {
        var data = new
        {
            OrderId = orderId,
            TableNumber = tableNumber,
            Message = $"🆕 Nueva orden recibida para Mesa {tableNumber}",
            Type = "new_order",
            Timestamp = DateTime.UtcNow
        };

        var companyId = await ResolveOrderCompanyAsync(orderId);
        if (companyId is Guid cid)
        {
            await _hubContext.Clients.Group(SignalRTenantGroups.Kitchen(cid)).SendAsync("NewOrder", data);
            await _hubContext.Clients.Group(SignalRTenantGroups.Orders(cid)).SendAsync("NewOrder", data);
        }
        else
            _logger.LogWarning("[SignalR] NotifyNewOrder sin CompanyId — no se difunde a kitchen/orders. OrderId={OrderId}", orderId);
    }

    public async Task NotifyOrderCancelled(Guid orderId)
    {
        await _hubContext.Clients.Group($"order_{orderId}").SendAsync("OrderCancelled", orderId);
    }

    public async Task NotifyOrderCompleted(Guid orderId, string tableNumber)
    {
        var data = new
        {
            OrderId = orderId,
            TableNumber = tableNumber,
            Message = $"✅ Orden completada para Mesa {tableNumber}",
            Type = "order_completed",
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"order_{orderId}").SendAsync("OrderCompleted", data);
        var companyId = await ResolveOrderCompanyAsync(orderId);
        if (companyId is Guid cid)
        {
            await _hubContext.Clients.Group(SignalRTenantGroups.Kitchen(cid)).SendAsync("OrderCompleted", data);
            await _hubContext.Clients.Group(SignalRTenantGroups.Orders(cid)).SendAsync("OrderCompleted", data);
        }
    }

    public async Task NotifyTableStatusChanged(Guid tableId, string newStatus)
    {
        var message = newStatus switch
        {
            "EnPreparacion" => "👨‍🍳 Mesa cambió a EN PREPARACIÓN - Cocina trabajando",
            "ParaPago" => "💰 Mesa cambió a PARA PAGO - Lista para cobrar",
            "Ocupada" => "👥 Mesa cambió a OCUPADA - Clientes atendidos",
            "Disponible" => "✅ Mesa cambió a DISPONIBLE - Libre para nuevos clientes",
            "Servida" => "🍽️ Mesa cambió a SERVIDA - Pedido entregado",
            _ => $"🔄 Mesa cambió de estado a {newStatus}"
        };

        var data = new
        {
            TableId = tableId,
            NewStatus = newStatus,
            Message = message,
            Type = "table_status_changed",
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"table_{tableId}").SendAsync("TableStatusChanged", data);

        var companyId = await ResolveTableCompanyAsync(tableId);
        if (companyId is Guid cid)
        {
            await _hubContext.Clients.Group(SignalRTenantGroups.TableAll(cid)).SendAsync("TableStatusChanged", data);
            await _hubContext.Clients.Group(SignalRTenantGroups.Orders(cid)).SendAsync("TableStatusChanged", data);
            await _hubContext.Clients.Group(SignalRTenantGroups.Kitchen(cid)).SendAsync("TableStatusChanged", data);
        }
    }

    public async Task NotifyKitchenUpdate()
    {
        var companyId = _tenant.Current.CompanyId;
        if (companyId is Guid cid)
            await _hubContext.Clients.Group(SignalRTenantGroups.Kitchen(cid)).SendAsync("KitchenUpdate");
        else
            _logger.LogWarning("[SignalR] NotifyKitchenUpdate sin CompanyId en scope — omitido");
    }

    public async Task NotifyStationUpdate(string stationType)
    {
        var companyId = _tenant.Current.CompanyId;
        if (companyId is null)
        {
            await NotifyKitchenUpdate();
            return;
        }

        if (string.IsNullOrWhiteSpace(stationType))
        {
            await NotifyKitchenUpdate();
            return;
        }

        await _hubContext.Clients.Group(SignalRTenantGroups.Station(companyId.Value, stationType))
            .SendAsync("KitchenUpdate");
    }

    public async Task NotifyPaymentProcessed(Guid orderId, decimal amount, string method, bool isFullyPaid)
    {
        await _hubContext.Clients.Group($"order_{orderId}")
            .SendAsync("PaymentProcessed", orderId, amount, method, isFullyPaid);
    }

    public async Task NotifyStockUpdated(Guid productId, string productName, decimal newStock)
    {
        var companyId = await ResolveProductCompanyAsync(productId) ?? _tenant.Current.CompanyId;
        if (companyId is null) return;
        await _hubContext.Clients.Group(SignalRTenantGroups.Stock(companyId.Value))
            .SendAsync("StockUpdated", new
            {
                ProductId = productId,
                ProductName = productName,
                NewStock = newStock,
                Timestamp = DateTime.UtcNow
            });
    }

    public async Task NotifyStockReduced(Guid productId, string productName, decimal oldStock, decimal newStock, decimal quantityReduced)
    {
        var companyId = await ResolveProductCompanyAsync(productId) ?? _tenant.Current.CompanyId;
        if (companyId is null) return;
        await _hubContext.Clients.Group(SignalRTenantGroups.Stock(companyId.Value))
            .SendAsync("StockReduced", new
            {
                ProductId = productId,
                ProductName = productName,
                OldStock = oldStock,
                NewStock = newStock,
                QuantityReduced = quantityReduced,
                Timestamp = DateTime.UtcNow
            });
    }

    public async Task NotifyCashSessionChanged(Guid sessionId, Guid registerId, string status)
    {
        var payload = new { SessionId = sessionId, RegisterId = registerId, Status = status, Timestamp = DateTime.UtcNow };
        await _hubContext.Clients.Group($"cash_register_{registerId}").SendAsync("CashSessionChanged", payload);
        var companyId = await ResolveRegisterCompanyAsync(registerId) ?? _tenant.Current.CompanyId;
        if (companyId is Guid cid)
            await _hubContext.Clients.Group(SignalRTenantGroups.CashDashboard(cid)).SendAsync("CashSessionChanged", payload);
    }

    public async Task NotifyCashMovement(Guid sessionId, Guid registerId, string movementType, decimal amount)
    {
        var payload = new { SessionId = sessionId, RegisterId = registerId, MovementType = movementType, Amount = amount, Timestamp = DateTime.UtcNow };
        await _hubContext.Clients.Group($"cash_register_{registerId}").SendAsync("CashMovement", payload);
        var companyId = await ResolveRegisterCompanyAsync(registerId) ?? _tenant.Current.CompanyId;
        if (companyId is Guid cid)
            await _hubContext.Clients.Group(SignalRTenantGroups.CashDashboard(cid)).SendAsync("CashMovement", payload);
    }

    private async Task<Guid?> ResolveOrderCompanyAsync(Guid orderId) =>
        await _db.Orders.AsNoTracking().Where(o => o.Id == orderId).Select(o => o.CompanyId).FirstOrDefaultAsync();

    private async Task<Guid?> ResolveTableCompanyAsync(Guid tableId) =>
        await _db.Tables.AsNoTracking().Where(t => t.Id == tableId).Select(t => t.CompanyId).FirstOrDefaultAsync();

    private async Task<Guid?> ResolveProductCompanyAsync(Guid productId) =>
        await _db.Products.AsNoTracking().Where(p => p.Id == productId).Select(p => p.CompanyId).FirstOrDefaultAsync();

    private async Task<Guid?> ResolveRegisterCompanyAsync(Guid registerId)
    {
        var row = await _db.CashRegisters.AsNoTracking()
            .Where(r => r.Id == registerId)
            .Select(r => new { r.CompanyId })
            .FirstOrDefaultAsync();
        return row == null || row.CompanyId == Guid.Empty ? null : row.CompanyId;
    }
}
