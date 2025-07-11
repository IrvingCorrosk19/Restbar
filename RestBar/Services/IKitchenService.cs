using RestBar.ViewModel;

namespace RestBar.Services
{
    public interface IKitchenService
    {
        Task<List<KitchenOrderViewModel>> GetPendingOrdersByStationTypeAsync(string stationType);
        Task MarkOrderAsReadyAsync(Guid orderId);
        Task MarkItemsAsReadyByStationAsync(Guid orderId, string stationType);
        Task MarkSpecificItemAsReadyAsync(Guid orderId, Guid itemId);
    }
} 