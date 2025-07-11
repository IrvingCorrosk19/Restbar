using RestBar.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestBar.Interfaces
{
    public interface IStationService
    {
        Task<IEnumerable<Station>> GetAllStationsAsync();
        Task<Station?> GetStationByIdAsync(Guid id);
        Task<Station> CreateStationAsync(Station station);
        Task<Station> UpdateStationAsync(Guid id, Station station);
        Task<bool> DeleteStationAsync(Guid id);
        Task<IEnumerable<string>> GetDistinctStationTypesAsync();
        
        // Métodos adicionales para validaciones
        Task<bool> StationHasProductsAsync(Guid id);
        Task<int> GetProductCountAsync(Guid id);
        Task<bool> StationExistsAsync(Guid id);
        Task<bool> StationNameExistsAsync(string name, Guid? id = null);
    }
}