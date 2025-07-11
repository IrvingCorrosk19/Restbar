using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface ITableService
    {
        Task<IEnumerable<Table>> GetAllAsync();
        Task<Table?> GetByIdAsync(Guid id);
        Task<Table> CreateAsync(Table table);
        Task UpdateAsync(Table table);
        Task DeleteAsync(Guid id);

        // Métodos adicionales específicos para Table
        Task<IEnumerable<Table>> GetByAreaIdAsync(Guid areaId);
        Task<IEnumerable<Table>> GetActiveTablesAsync();
        Task<IEnumerable<Table>> GetTablesByStatusAsync(string status);
        Task<Table?> GetTableWithOrdersAsync(Guid id);
        Task<IEnumerable<Table>> GetAvailableTablesAsync();
        
        // Método para obtener mesas con formato específico para ViewBag
        Task<IEnumerable<object>> GetTablesForViewBagAsync();
    }
} 