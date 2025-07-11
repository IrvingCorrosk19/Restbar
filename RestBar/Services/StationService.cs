using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestBar.Services
{
    public class StationService : IStationService
    {
        private readonly RestBarContext _context;

        public StationService(RestBarContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Station>> GetAllStationsAsync()
        {
            return await _context.Stations
                .Include(s => s.Products)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Station?> GetStationByIdAsync(Guid id)
        {
            return await _context.Stations
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Station> CreateStationAsync(Station station)
        {
            if (station == null)
                throw new ArgumentNullException(nameof(station), "La estación no puede ser null");

            if (string.IsNullOrWhiteSpace(station.Name))
                throw new ArgumentException("El nombre de la estación es requerido");

            if (string.IsNullOrWhiteSpace(station.Type))
                throw new ArgumentException("El tipo de estación es requerido");

            // Verificar si ya existe una estación con el mismo nombre
            var existingStation = await _context.Stations
                .FirstOrDefaultAsync(s => s.Name.ToLower() == station.Name.ToLower());
            
            if (existingStation != null)
                throw new InvalidOperationException($"Ya existe una estación con el nombre '{station.Name}'");

            station.Id = Guid.NewGuid();
            _context.Stations.Add(station);
            await _context.SaveChangesAsync();
            return station;
        }

        public async Task<Station> UpdateStationAsync(Guid id, Station station)
        {
            if (station == null)
                throw new ArgumentNullException(nameof(station), "La estación no puede ser null");

            if (string.IsNullOrWhiteSpace(station.Name))
                throw new ArgumentException("El nombre de la estación es requerido");

            if (string.IsNullOrWhiteSpace(station.Type))
                throw new ArgumentException("El tipo de estación es requerido");

            var existing = await _context.Stations.FindAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Estación con ID {id} no encontrada");

            // Verificar si ya existe otra estación con el mismo nombre (excluyendo la actual)
            var duplicateStation = await _context.Stations
                .FirstOrDefaultAsync(s => s.Name.ToLower() == station.Name.ToLower() && s.Id != id);
            
            if (duplicateStation != null)
                throw new InvalidOperationException($"Ya existe otra estación con el nombre '{station.Name}'");

            existing.Name = station.Name;
            existing.Type = station.Type;
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteStationAsync(Guid id)
        {
            var station = await _context.Stations
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Id == id);
                
            if (station == null)
                return false;

            // Verificar si la estación tiene productos asociados
            if (station.Products.Any())
            {
                var productCount = station.Products.Count;
                throw new InvalidOperationException(
                    $"No se puede eliminar la estación '{station.Name}' porque tiene {productCount} producto(s) asociado(s). " +
                    "Primero debe reasignar o eliminar los productos de esta estación.");
            }

            _context.Stations.Remove(station);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<string>> GetDistinctStationTypesAsync()
        {
            return await _context.Stations
                .Where(s => !string.IsNullOrEmpty(s.Type))
                .Select(s => s.Type)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }

        public async Task<bool> StationHasProductsAsync(Guid id)
        {
            return await _context.Products
                .AnyAsync(p => p.StationId == id);
        }

        public async Task<int> GetProductCountAsync(Guid id)
        {
            return await _context.Products
                .CountAsync(p => p.StationId == id);
        }

        public async Task<bool> StationExistsAsync(Guid id)
        {
            return await _context.Stations.AnyAsync(e => e.Id == id);
        }

        public async Task<bool> StationNameExistsAsync(string name, Guid? id = null)
        {
            var query = _context.Stations.AsQueryable();

            if (id.HasValue)
            {
                query = query.Where(s => s.Id != id.Value);
            }

            return await query.AnyAsync(s => s.Name.ToLower() == name.ToLower());
        }
    }
} 