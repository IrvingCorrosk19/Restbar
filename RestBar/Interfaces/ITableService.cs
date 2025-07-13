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
        
        // ✅ NUEVO: Métodos para filtrar por CompanyId y BranchId
        Task<IEnumerable<Table>> GetTablesByCompanyAndBranchAsync(Guid companyId, Guid branchId);
        Task<IEnumerable<Table>> GetActiveTablesByCompanyAndBranchAsync(Guid companyId, Guid branchId);
        
        // ✅ NUEVO: Método para obtener el contexto (necesario para FixTableStatus)
        RestBarContext GetContext();
    }
} 