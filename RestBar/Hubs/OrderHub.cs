using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace RestBar.Hubs
{
    public class OrderHub : Hub
    {
        // ─── GRUPOS POR TIPO DE ESTACIÓN ────────────────────────────────────────
        // Cada vista de estación se une a "station_{stationType}" (ej. "station_kitchen", "station_bar").
        // Esto permite notificaciones dirigidas sin que la cocina reciba eventos del bar y viceversa.
        // Las vistas también siguen unidas a "kitchen" para recibir eventos de difusión general.

        /// <summary>
        /// Une la conexión al grupo específico de la estación indicada.
        /// El nombre del grupo sigue el patrón: station_{stationType} (minúsculas).
        /// </summary>
        public async Task JoinStationTypeGroup(string stationType)
        {
            if (string.IsNullOrWhiteSpace(stationType))
                return;

            var groupName = $"station_{stationType.ToLower().Trim()}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        /// <summary>
        /// Saca la conexión del grupo específico de la estación indicada.
        /// </summary>
        public async Task LeaveStationTypeGroup(string stationType)
        {
            if (string.IsNullOrWhiteSpace(stationType))
                return;

            var groupName = $"station_{stationType.ToLower().Trim()}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }


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

        public async Task JoinOrdersGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "orders");
        }

        public async Task LeaveOrdersGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "orders");
        }

        // ✅ NUEVO: Métodos para notificar cambios de stock
        public async Task JoinStockUpdatesGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "stock_updates");
        }

        public async Task LeaveStockUpdatesGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "stock_updates");
        }

    }
} 