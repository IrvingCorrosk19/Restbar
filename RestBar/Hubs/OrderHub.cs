using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RestBar.Infrastructure.Foundation;
using System.Security.Claims;

namespace RestBar.Hubs;

[Authorize]
public class OrderHub : Hub
{
    private Guid? CompanyId =>
        Guid.TryParse(Context.User?.FindFirst("CompanyId")?.Value, out var id) ? id : null;

    private Guid? BranchId =>
        Guid.TryParse(Context.User?.FindFirst("BranchId")?.Value, out var id) ? id : null;

    private bool IsSuperAdmin =>
        string.Equals(Context.User?.FindFirst("UserRole")?.Value, "superadmin", StringComparison.OrdinalIgnoreCase)
        || Context.User?.IsInRole("superadmin") == true;

    public async Task JoinStationTypeGroup(string stationType)
    {
        if (string.IsNullOrWhiteSpace(stationType) || CompanyId is null) return;
        await Groups.AddToGroupAsync(Context.ConnectionId, SignalRTenantGroups.Station(CompanyId.Value, stationType));
    }

    public async Task LeaveStationTypeGroup(string stationType)
    {
        if (string.IsNullOrWhiteSpace(stationType) || CompanyId is null) return;
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, SignalRTenantGroups.Station(CompanyId.Value, stationType));
    }

    public async Task JoinOrderGroup(string orderId)
    {
        if (!string.IsNullOrWhiteSpace(orderId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
    }

    public async Task LeaveOrderGroup(string orderId)
    {
        if (!string.IsNullOrWhiteSpace(orderId))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order_{orderId}");
    }

    public async Task JoinTableGroup(string tableId)
    {
        if (!string.IsNullOrWhiteSpace(tableId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"table_{tableId}");
    }

    public async Task LeaveTableGroup(string tableId)
    {
        if (!string.IsNullOrWhiteSpace(tableId))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"table_{tableId}");
    }

    public async Task JoinAllTablesGroup()
    {
        if (CompanyId is null) return;
        await Groups.AddToGroupAsync(Context.ConnectionId, SignalRTenantGroups.TableAll(CompanyId.Value));
    }

    public async Task LeaveAllTablesGroup()
    {
        if (CompanyId is null) return;
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, SignalRTenantGroups.TableAll(CompanyId.Value));
    }

    public async Task JoinKitchenGroup()
    {
        if (CompanyId is null) return;
        await Groups.AddToGroupAsync(Context.ConnectionId, SignalRTenantGroups.Kitchen(CompanyId.Value));
    }

    public async Task LeaveKitchenGroup()
    {
        if (CompanyId is null) return;
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, SignalRTenantGroups.Kitchen(CompanyId.Value));
    }

    public async Task JoinOrdersGroup()
    {
        if (CompanyId is null) return;
        await Groups.AddToGroupAsync(Context.ConnectionId, SignalRTenantGroups.Orders(CompanyId.Value));
    }

    public async Task LeaveOrdersGroup()
    {
        if (CompanyId is null) return;
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, SignalRTenantGroups.Orders(CompanyId.Value));
    }

    public async Task JoinStockUpdatesGroup()
    {
        if (CompanyId is null) return;
        await Groups.AddToGroupAsync(Context.ConnectionId, SignalRTenantGroups.Stock(CompanyId.Value));
    }

    public async Task LeaveStockUpdatesGroup()
    {
        if (CompanyId is null) return;
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, SignalRTenantGroups.Stock(CompanyId.Value));
    }

    public async Task JoinCashRegisterGroup(string registerId)
    {
        if (!string.IsNullOrWhiteSpace(registerId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"cash_register_{registerId}");
    }

    public async Task JoinCashDashboardGroup()
    {
        if (CompanyId is null) return;
        await Groups.AddToGroupAsync(Context.ConnectionId, SignalRTenantGroups.CashDashboard(CompanyId.Value));
    }
}
