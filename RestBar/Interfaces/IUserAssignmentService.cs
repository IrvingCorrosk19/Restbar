using RestBar.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestBar.Interfaces
{
    public interface IUserAssignmentService
    {
        Task<IEnumerable<UserAssignment>> GetAllAsync();
        Task<UserAssignment?> GetByIdAsync(Guid id);
        Task<UserAssignment?> GetActiveByUserIdAsync(Guid userId);
        Task<IEnumerable<UserAssignment>> GetByUserRoleAsync(UserRole role);
        Task<IEnumerable<UserAssignment>> GetByStationIdAsync(Guid stationId);
        Task<IEnumerable<UserAssignment>> GetByAreaIdAsync(Guid areaId);
        Task<UserAssignment> CreateAsync(UserAssignment assignment);
        Task UpdateAsync(UserAssignment assignment);
        Task DeleteAsync(Guid id);
        Task<bool> UnassignUserAsync(Guid userId);
        Task<bool> HasActiveAssignmentAsync(Guid userId);
        Task<IEnumerable<User>> GetUnassignedUsersByRoleAsync(UserRole role);
        Task<IEnumerable<Station>> GetAvailableStationsAsync();
        Task<IEnumerable<Area>> GetAvailableAreasAsync();
        Task<IEnumerable<Table>> GetAvailableTablesAsync(Guid? areaId = null);
    }
} 