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
            try
            {
                Console.WriteLine($"🔍 [OrderHub] JoinKitchenGroup() - ConnectionId: {Context.ConnectionId}");
                await Groups.AddToGroupAsync(Context.ConnectionId, "kitchen");
                Console.WriteLine($"✅ [OrderHub] JoinKitchenGroup() - Usuario unido al grupo 'kitchen' exitosamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [OrderHub] JoinKitchenGroup() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [OrderHub] JoinKitchenGroup() - StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task LeaveKitchenGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "kitchen");
        }

        // ✅ NUEVO: Método para unirse al grupo de órdenes
        public async Task JoinOrdersGroup()
        {
            try
            {
                Console.WriteLine($"🔍 [OrderHub] JoinOrdersGroup() - INICIANDO - ConnectionId: {Context.ConnectionId}");
                Console.WriteLine($"📋 [OrderHub] JoinOrdersGroup() - Agregando conexión al grupo 'orders'");
                
                await Groups.AddToGroupAsync(Context.ConnectionId, "orders");
                
                Console.WriteLine($"✅ [OrderHub] JoinOrdersGroup() - COMPLETADO - Usuario unido al grupo 'orders' exitosamente");
                Console.WriteLine($"📊 [OrderHub] JoinOrdersGroup() - ConnectionId: {Context.ConnectionId} ahora en grupo 'orders'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [OrderHub] JoinOrdersGroup() - ERROR: {ex.Message}");
                Console.WriteLine($"🔍 [OrderHub] JoinOrdersGroup() - StackTrace: {ex.StackTrace}");
                throw;
            }
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