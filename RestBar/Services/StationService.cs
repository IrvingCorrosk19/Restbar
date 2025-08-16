using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestBar.Services
{
    public class StationService : BaseTrackingService, IStationService
    {
        public StationService(RestBarContext context, IHttpContextAccessor httpContextAccessor) 
            : base(context, httpContextAccessor)
        {
        }

        public async Task<IEnumerable<Station>> GetAllStationsAsync()
        {
            return await _context.Stations
                .Include(s => s.Products)
                .Include(s => s.Area)
                .Include(s => s.Company)
                .Include(s => s.Branch)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Station?> GetStationByIdAsync(Guid id)
        {
            return await _context.Stations
                .Include(s => s.Products)
                .Include(s => s.Area)
                .Include(s => s.Company)
                .Include(s => s.Branch)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Station> CreateStationAsync(Station station)
        {
            try
            {
                Console.WriteLine($"[StationService] CreateStationAsync iniciado");
                Console.WriteLine($"[StationService] Station recibida - Name: {station?.Name}, Type: {station?.Type}, AreaId: {station?.AreaId}, IsActive: {station?.IsActive}");

                if (station == null)
                {
                    Console.WriteLine($"[StationService] ❌ ERROR: Station es null");
                    throw new ArgumentNullException(nameof(station), "La estación no puede ser null");
                }

                if (string.IsNullOrWhiteSpace(station.Name))
                {
                    Console.WriteLine($"[StationService] ❌ ERROR: Nombre de estación vacío");
                    throw new ArgumentException("El nombre de la estación es requerido");
                }

                if (string.IsNullOrWhiteSpace(station.Type))
                {
                    Console.WriteLine($"[StationService] ❌ ERROR: Tipo de estación vacío");
                    throw new ArgumentException("El tipo de estación es requerido");
                }

                Console.WriteLine($"[StationService] Verificando duplicados de nombre: {station.Name}");
                // Verificar si ya existe una estación con el mismo nombre
                var existingStation = await _context.Stations
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == station.Name.ToLower());
                
                if (existingStation != null)
                {
                    Console.WriteLine($"[StationService] ❌ ERROR: Ya existe una estación con el nombre '{station.Name}'");
                    throw new InvalidOperationException($"Ya existe una estación con el nombre '{station.Name}'");
                }

                Console.WriteLine($"[StationService] ✅ No hay duplicados, procediendo a crear...");

                // Obtener información del usuario actual antes de crear
                var currentUser = GetCurrentUser();
                Console.WriteLine($"[StationService] Usuario actual: {currentUser}");

                station.Id = Guid.NewGuid();
                Console.WriteLine($"[StationService] ID generado: {station.Id}");

                // Configurar tracking manualmente para asegurar que se aplique
                var currentTime = DateTime.UtcNow;
                station.CreatedAt = currentTime;
                station.UpdatedAt = currentTime;
                station.CreatedBy = currentUser;
                station.UpdatedBy = currentUser;

                Console.WriteLine($"[StationService] Tracking configurado:");
                Console.WriteLine($"[StationService]   - CreatedAt: {station.CreatedAt}");
                Console.WriteLine($"[StationService]   - UpdatedAt: {station.UpdatedAt}");
                Console.WriteLine($"[StationService]   - CreatedBy: {station.CreatedBy}");
                Console.WriteLine($"[StationService]   - UpdatedBy: {station.UpdatedBy}");

                _context.Stations.Add(station);
                Console.WriteLine($"[StationService] Estación agregada al contexto");

                // Usar el método de tracking automático del BaseTrackingService
                Console.WriteLine($"[StationService] Guardando cambios con tracking automático...");
                await SaveChangesWithTrackingAsync();
                Console.WriteLine($"[StationService] ✅ Estación creada exitosamente");

                return station;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StationService] ❌ ERROR en CreateStationAsync: {ex.Message}");
                Console.WriteLine($"[StationService] Tipo de excepción: {ex.GetType().Name}");
                Console.WriteLine($"[StationService] Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[StationService] Inner Exception: {ex.InnerException.Message}");
                }
                throw; // Re-lanzar la excepción para que sea manejada por el controlador
            }
        }

        public async Task<Station> UpdateStationAsync(Guid id, Station station)
        {
            try
            {
                Console.WriteLine($"[StationService] UpdateStationAsync iniciado - ID: {id}");
                Console.WriteLine($"[StationService] Station recibida - Name: {station?.Name}, Type: {station?.Type}, AreaId: {station?.AreaId}, IsActive: {station?.IsActive}");

                if (station == null)
                {
                    Console.WriteLine($"[StationService] ❌ ERROR: Station es null");
                    throw new ArgumentNullException(nameof(station), "La estación no puede ser null");
                }

                if (string.IsNullOrWhiteSpace(station.Name))
                {
                    Console.WriteLine($"[StationService] ❌ ERROR: Nombre de estación vacío");
                    throw new ArgumentException("El nombre de la estación es requerido");
                }

                if (string.IsNullOrWhiteSpace(station.Type))
                {
                    Console.WriteLine($"[StationService] ❌ ERROR: Tipo de estación vacío");
                    throw new ArgumentException("El tipo de estación es requerido");
                }

                Console.WriteLine($"[StationService] Buscando estación existente con ID: {id}");
                var existing = await _context.Stations.FindAsync(id);
                if (existing == null)
                {
                    Console.WriteLine($"[StationService] ❌ ERROR: Estación con ID {id} no encontrada");
                    throw new KeyNotFoundException($"Estación con ID {id} no encontrada");
                }

                Console.WriteLine($"[StationService] ✅ Estación encontrada - Name: {existing.Name}, Type: {existing.Type}");

                // Verificar si ya existe otra estación con el mismo nombre (excluyendo la actual)
                Console.WriteLine($"[StationService] Verificando duplicados de nombre: {station.Name}");
                var duplicateStation = await _context.Stations
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == station.Name.ToLower() && s.Id != id);
                
                if (duplicateStation != null)
                {
                    Console.WriteLine($"[StationService] ❌ ERROR: Ya existe otra estación con el nombre '{station.Name}'");
                    throw new InvalidOperationException($"Ya existe otra estación con el nombre '{station.Name}'");
                }

                Console.WriteLine($"[StationService] ✅ No hay duplicados, procediendo a actualizar...");

                // Actualizar propiedades
                Console.WriteLine($"[StationService] Actualizando propiedades...");
                Console.WriteLine($"[StationService]   - Name: {existing.Name} -> {station.Name}");
                Console.WriteLine($"[StationService]   - Type: {existing.Type} -> {station.Type}");
                Console.WriteLine($"[StationService]   - Icon: {existing.Icon} -> {station.Icon}");
                Console.WriteLine($"[StationService]   - AreaId: {existing.AreaId} -> {station.AreaId}");
                Console.WriteLine($"[StationService]   - CompanyId: {existing.CompanyId} -> {station.CompanyId}");
                Console.WriteLine($"[StationService]   - BranchId: {existing.BranchId} -> {station.BranchId}");
                Console.WriteLine($"[StationService]   - IsActive: {existing.IsActive} -> {station.IsActive}");

                existing.Name = station.Name;
                existing.Type = station.Type;
                existing.Icon = station.Icon;
                existing.AreaId = station.AreaId;
                existing.CompanyId = station.CompanyId;
                existing.BranchId = station.BranchId;
                existing.IsActive = station.IsActive;
                
                Console.WriteLine($"[StationService] Guardando cambios en la base de datos...");
                // Usar el método de tracking automático del BaseTrackingService
                await SaveChangesWithTrackingAsync();
                Console.WriteLine($"[StationService] ✅ Cambios guardados exitosamente");
                
                // Recargar con las relaciones
                Console.WriteLine($"[StationService] Recargando relaciones...");
                await _context.Entry(existing)
                    .Reference(s => s.Area)
                    .LoadAsync();
                await _context.Entry(existing)
                    .Reference(s => s.Company)
                    .LoadAsync();
                await _context.Entry(existing)
                    .Reference(s => s.Branch)
                    .LoadAsync();
                
                Console.WriteLine($"[StationService] ✅ Relaciones recargadas");
                Console.WriteLine($"[StationService] ✅ UpdateStationAsync completado exitosamente");
                
                return existing;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StationService] ❌ ERROR en UpdateStationAsync: {ex.Message}");
                Console.WriteLine($"[StationService] Tipo de excepción: {ex.GetType().Name}");
                Console.WriteLine($"[StationService] Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[StationService] Inner Exception: {ex.InnerException.Message}");
                }
                throw; // Re-lanzar la excepción para que sea manejada por el controlador
            }
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
            // Usar el método de tracking automático del BaseTrackingService
            await SaveChangesWithTrackingAsync();
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