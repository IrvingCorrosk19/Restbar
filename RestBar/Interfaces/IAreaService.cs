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
        
        // ✅ NUEVO: Obtener usuario actual con asignaciones
        Task<User?> GetCurrentUserWithAssignmentsAsync();
        Task<User?> GetCurrentUserWithAssignmentsAsync(Guid userId);
        
        // ✅ NUEVO: Obtener áreas por CompanyId y BranchId específicos
        Task<IEnumerable<Area>> GetAreasByCompanyAndBranchAsync(Guid companyId, Guid branchId);
    }
}
