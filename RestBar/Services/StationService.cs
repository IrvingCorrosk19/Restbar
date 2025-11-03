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
            try
            {
                Console.WriteLine("🔍 [StationService] GetAllStationsAsync() - Iniciando...");
                
                // Obtener usuario actual y sus asignaciones
                var currentUser = await GetCurrentUserWithAssignmentsAsync();
                
                if (currentUser == null)
                {
                    Console.WriteLine("⚠️ [StationService] GetAllStationsAsync() - Usuario no encontrado");
                    return new List<Station>();
                }

                if (currentUser.Branch == null)
                {
                    Console.WriteLine("⚠️ [StationService] GetAllStationsAsync() - Usuario no tiene sucursal asignada");
                    return new List<Station>();
                }

                Console.WriteLine($"✅ [StationService] GetAllStationsAsync() - Usuario: {currentUser.Email}, CompanyId: {currentUser.Branch?.CompanyId}, BranchId: {currentUser.BranchId}");

                // Filtrar estaciones por compañía y sucursal del usuario
                var stations = await _context.Stations
                    .Include(s => s.Products)
                    .Include(s => s.Area)
                    .Include(s => s.Company)
                    .Include(s => s.Branch)
                    .Where(s => s.CompanyId == currentUser.Branch.CompanyId && s.BranchId == currentUser.BranchId)
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                Console.WriteLine($"📊 [StationService] GetAllStationsAsync() - Estaciones encontradas: {stations.Count}");
                return stations;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [StationService] GetAllStationsAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [StationService] GetAllStationsAsync() - StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<Station?> GetStationByIdAsync(Guid id)
        {
            return await _context.Stations
                .Include(s => s.Products)
                .Include(s => s.Area)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Station> CreateStationAsync(Station station)
        {
            try
            {
                Console.WriteLine("🔍 [StationService] CreateStationAsync() - Iniciando...");
                
                if (station == null)
                    throw new ArgumentNullException(nameof(station), "La estación no puede ser null");

                if (string.IsNullOrWhiteSpace(station.Name))
                    throw new ArgumentException("El nombre de la estación es requerido");

                if (string.IsNullOrWhiteSpace(station.Type))
                    throw new ArgumentException("El tipo de estación es requerido");

                // Obtener usuario actual y sus asignaciones
                var currentUser = await GetCurrentUserWithAssignmentsAsync();
                
                if (currentUser == null)
                {
                    Console.WriteLine("❌ [StationService] CreateStationAsync() - Usuario no encontrado");
                    throw new InvalidOperationException("Usuario no autenticado");
                }

                Console.WriteLine($"✅ [StationService] CreateStationAsync() - Usuario: {currentUser.Email}, CompanyId: {currentUser.Branch?.CompanyId}, BranchId: {currentUser.BranchId}");

                // Asignar automáticamente CompanyId y BranchId del usuario actual
                station.CompanyId = currentUser.Branch.CompanyId;
                station.BranchId = currentUser.BranchId;

                Console.WriteLine($"✅ [StationService] CreateStationAsync() - Asignando CompanyId: {station.CompanyId}, BranchId: {station.BranchId}");

                // Verificar si ya existe una estación con el mismo nombre en la misma compañía/sucursal
                var existingStation = await _context.Stations
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == station.Name.ToLower() 
                                             && s.CompanyId == currentUser.Branch.CompanyId 
                                             && s.BranchId == currentUser.BranchId);
                
                if (existingStation != null)
                    throw new InvalidOperationException($"Ya existe una estación con el nombre '{station.Name}' en esta sucursal");

                // ✅ Generar ID si no existe
                if (station.Id == Guid.Empty)
                {
                    station.Id = Guid.NewGuid();
                }
                
                // ✅ Usar SetCreatedTracking para establecer todos los campos de auditoría
                SetCreatedTracking(station);
                
                Console.WriteLine($"✅ [StationService] CreateStationAsync() - Campos establecidos: CreatedBy={station.CreatedBy}, CreatedAt={station.CreatedAt}, UpdatedAt={station.UpdatedAt}");
                
                _context.Stations.Add(station);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"✅ [StationService] CreateStationAsync() - Estación creada exitosamente: {station.Name}");
                return station;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [StationService] CreateStationAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [StationService] CreateStationAsync() - StackTrace: {ex.StackTrace}");
                throw;
            }
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

            // Obtener usuario actual para auditoría
            var currentUser = await GetCurrentUserWithAssignmentsAsync();
            
            existing.Name = station.Name;
            existing.Type = station.Type;
            existing.Icon = station.Icon;
            existing.AreaId = station.AreaId;
            existing.IsActive = station.IsActive;
            
            // ✅ Usar SetUpdatedTracking para establecer campos de auditoría de actualización
            SetUpdatedTracking(existing);
            
            Console.WriteLine($"✅ [StationService] UpdateStationAsync() - Campos actualizados: UpdatedBy={existing.UpdatedBy}, UpdatedAt={existing.UpdatedAt}");
            
            await _context.SaveChangesAsync();
            
            // Recargar con las relaciones
            await _context.Entry(existing)
                .Reference(s => s.Area)
                .LoadAsync();
                
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
            try
            {
                Console.WriteLine("🔍 [StationService] GetDistinctStationTypesAsync() - Iniciando consulta de tipos de estaciones...");
                
                var stationTypes = await _context.Stations
                    .Where(s => s.IsActive && !string.IsNullOrEmpty(s.Type))
                    .Select(s => s.Type)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync();
                
                Console.WriteLine($"✅ [StationService] GetDistinctStationTypesAsync() - Tipos de estaciones encontrados: {stationTypes.Count}");
                foreach (var type in stationTypes)
                {
                    Console.WriteLine($"📋 [StationService] GetDistinctStationTypesAsync() - Tipo: {type}");
                }
                
                return stationTypes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [StationService] GetDistinctStationTypesAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [StationService] GetDistinctStationTypesAsync() - StackTrace: {ex.StackTrace}");
                return new List<string>();
            }
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

        // ✅ NUEVO: Método para obtener usuario actual con asignaciones
        private async Task<User?> GetCurrentUserWithAssignmentsAsync()
        {
            try
            {
                var userIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    Console.WriteLine("⚠️ [StationService] GetCurrentUserWithAssignmentsAsync() - No se pudo obtener userId del contexto");
                    return null;
                }

                var user = await _context.Users
                    .Include(u => u.Branch)
                    .ThenInclude(b => b.Company)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    Console.WriteLine($"⚠️ [StationService] GetCurrentUserWithAssignmentsAsync() - Usuario con ID {userId} no encontrado");
                    return null;
                }

                Console.WriteLine($"✅ [StationService] GetCurrentUserWithAssignmentsAsync() - Usuario encontrado: {user.Email}, CompanyId: {user.Branch?.CompanyId}, BranchId: {user.BranchId}");
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [StationService] GetCurrentUserWithAssignmentsAsync() - Error: {ex.Message}");
                return null;
            }
        }
    }
} 