using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface IAreaService
    {
        Task<IEnumerable<Area>> GetAllAsync();
        Task<Area?> GetByIdAsync(Guid id);
        Task<Area> CreateAsync(Area area);
        Task UpdateAsync(Area area);
        Task DeleteAsync(Guid id);

        // Extra: Obtener áreas por sucursal
        Task<IEnumerable<Area>> GetByBranchIdAsync(Guid branchId);
    }
}
