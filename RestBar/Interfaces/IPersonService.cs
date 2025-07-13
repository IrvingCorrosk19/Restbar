using RestBar.Models;

namespace RestBar.Interfaces;

public interface IPersonService
{
    Task<Person> CreatePersonAsync(string name, Guid orderId, Guid? companyId = null, Guid? branchId = null);
    Task<IEnumerable<Person>> GetPersonsByOrderAsync(Guid orderId);
    Task<bool> AssignItemToPersonAsync(Guid itemId, Guid personId, string personName);
    Task<bool> MarkItemAsSharedAsync(Guid itemId);
    Task<decimal> CalculateTotalByPersonAsync(Guid personId);
    Task<IEnumerable<OrderItem>> GetSharedItemsAsync(Guid orderId);
    Task<bool> DeletePersonAsync(Guid personId);
}
