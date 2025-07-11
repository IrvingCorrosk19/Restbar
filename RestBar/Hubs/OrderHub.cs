using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace RestBar.Hubs
{
    public class OrderHub : Hub
    {
        public async Task JoinOrderGroup(string orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
        }

        public async Task LeaveOrderGroup(string orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order_{orderId}");
        }

        public async Task JoinTableGroup(string tableId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"table_{tableId}");
        }

        public async Task LeaveTableGroup(string tableId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"table_{tableId}");
        }

        public async Task JoinAllTablesGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "table_all");
        }

        public async Task LeaveAllTablesGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "table_all");
        }

        public async Task JoinKitchenGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "kitchen");
        }

        public async Task LeaveKitchenGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "kitchen");
        }
    }
} 