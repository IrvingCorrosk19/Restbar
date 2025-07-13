using Microsoft.EntityFrameworkCore;

using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services
{
    public class AreaService : IAreaService
    {
        private readonly RestBarContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AreaService(RestBarContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<Area>> GetAllAsync()
        {
            try
            {
                Console.WriteLine("🔍 [AreaService] GetAllAsync() - Iniciando...");
                
                // Obtener usuario actual y sus asignaciones
                var currentUser = await GetCurrentUserWithAssignmentsAsync();
                
                if (currentUser == null)
                {
                    Console.WriteLine("⚠️ [AreaService] GetAllAsync() - Usuario no encontrado");
                    return new List<Area>();
                }

                Console.WriteLine($"✅ [AreaService] GetAllAsync() - Usuario: {currentUser.Email}, CompanyId: {currentUser.Branch?.CompanyId}, BranchId: {currentUser.BranchId}");

                // Filtrar áreas por compañía y sucursal del usuario
                var areas = await _context.Areas
                    .Include(a => a.Branch)
                    .Include(a => a.Tables)
                    .Include(a => a.Company)
                    .Where(a => a.CompanyId == currentUser.Branch.CompanyId && a.BranchId == currentUser.BranchId)
                    .ToListAsync();

                Console.WriteLine($"📊 [AreaService] GetAllAsync() - Áreas encontradas: {areas.Count}");
                return areas;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AreaService] GetAllAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [AreaService] GetAllAsync() - StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<Area?> GetByIdAsync(Guid id)
        {
            return await _context.Areas
                .Include(a => a.Branch)
                .Include(a => a.Tables)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Area> CreateAsync(Area area)
        {
            try
            {
                Console.WriteLine("🔍 [AreaService] CreateAsync() - Iniciando...");
                
                if (area == null)
                    throw new ArgumentNullException(nameof(area), "El área no puede ser null");

                if (string.IsNullOrWhiteSpace(area.Name))
                    throw new ArgumentException("El nombre del área es requerido");

                // Obtener usuario actual y sus asignaciones
                var currentUser = await GetCurrentUserWithAssignmentsAsync();
                
                if (currentUser == null)
                {
                    Console.WriteLine("❌ [AreaService] CreateAsync() - Usuario no encontrado");
                    throw new InvalidOperationException("Usuario no autenticado");
                }

                Console.WriteLine($"✅ [AreaService] CreateAsync() - Usuario: {currentUser.Email}, CompanyId: {currentUser.Branch?.CompanyId}, BranchId: {currentUser.BranchId}");

                // Asignar automáticamente CompanyId y BranchId del usuario actual
                area.CompanyId = currentUser.Branch.CompanyId;
                area.BranchId = currentUser.BranchId;

                Console.WriteLine($"✅ [AreaService] CreateAsync() - Asignando CompanyId: {area.CompanyId}, BranchId: {area.BranchId}");

                // Verificar si ya existe un área con el mismo nombre en la misma compañía/sucursal
                var existingArea = await _context.Areas
                    .FirstOrDefaultAsync(a => a.Name.ToLower() == area.Name.ToLower() 
                                             && a.CompanyId == currentUser.Branch.CompanyId 
                                             && a.BranchId == currentUser.BranchId);
                
                if (existingArea != null)
                    throw new InvalidOperationException($"Ya existe un área con el nombre '{area.Name}' en esta sucursal");

                area.Id = Guid.NewGuid();
                _context.Areas.Add(area);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"✅ [AreaService] CreateAsync() - Área creada exitosamente: {area.Name}");
                return area;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AreaService] CreateAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [AreaService] CreateAsync() - StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task UpdateAsync(Area area)
        {
            try
            {
                // Buscar si hay una entidad con el mismo ID siendo rastreada
                var existingEntity = _context.ChangeTracker.Entries<Area>()
                    .FirstOrDefault(e => e.Entity.Id == area.Id);

                if (existingEntity != null)
                {
                    // Detach la entidad existente para evitar conflictos
                    existingEntity.State = EntityState.Detached;
                }

                // Usar Update para manejar automáticamente el tracking
                _context.Areas.Update(area);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al actualizar el área en la base de datos.", ex);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var area = await _context.Areas.FindAsync(id);
            if (area != null)
            {
                _context.Areas.Remove(area);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Area>> GetByBranchIdAsync(Guid branchId)
        {
            return await _context.Areas
                .Where(a => a.BranchId == branchId)
                .Include(a => a.Tables)
                .ToListAsync();
        }

        // ✅ NUEVO: Método para obtener usuario actual con asignaciones
        public async Task<User?> GetCurrentUserWithAssignmentsAsync()
        {
            try
            {
                var userIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    Console.WriteLine("⚠️ [AreaService] GetCurrentUserWithAssignmentsAsync() - No se pudo obtener userId del contexto");
                    return null;
                }

                var user = await _context.Users
                    .Include(u => u.Branch)
                    .ThenInclude(b => b.Company)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    Console.WriteLine($"⚠️ [AreaService] GetCurrentUserWithAssignmentsAsync() - Usuario con ID {userId} no encontrado");
                    return null;
                }

                Console.WriteLine($"✅ [AreaService] GetCurrentUserWithAssignmentsAsync() - Usuario encontrado: {user.Email}, CompanyId: {user.Branch?.CompanyId}, BranchId: {user.BranchId}");
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AreaService] GetCurrentUserWithAssignmentsAsync() - Error: {ex.Message}");
                return null;
            }
        }

        // ✅ NUEVO: Método sobrecargado que acepta userId como parámetro
        public async Task<User?> GetCurrentUserWithAssignmentsAsync(Guid userId)
        {
            try
            {
                Console.WriteLine($"🔍 [AreaService] GetCurrentUserWithAssignmentsAsync(userId) - Iniciando para userId: {userId}");

                var user = await _context.Users
                    .Include(u => u.Branch)
                    .ThenInclude(b => b.Company)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    Console.WriteLine($"⚠️ [AreaService] GetCurrentUserWithAssignmentsAsync(userId) - Usuario con ID {userId} no encontrado");
                    return null;
                }

                Console.WriteLine($"✅ [AreaService] GetCurrentUserWithAssignmentsAsync(userId) - Usuario encontrado: {user.Email}, CompanyId: {user.Branch?.CompanyId}, BranchId: {user.BranchId}");
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AreaService] GetCurrentUserWithAssignmentsAsync(userId) - Error: {ex.Message}");
                return null;
            }
        }

        // ✅ NUEVO: Método para obtener áreas por CompanyId y BranchId específicos
        public async Task<IEnumerable<Area>> GetAreasByCompanyAndBranchAsync(Guid companyId, Guid branchId)
        {
            try
            {
                Console.WriteLine($"🔍 [AreaService] GetAreasByCompanyAndBranchAsync() - Filtrando por CompanyId: {companyId}, BranchId: {branchId}");
                
                var areas = await _context.Areas
                    .Include(a => a.Branch)
                    .Include(a => a.Tables)
                    .Include(a => a.Company)
                    .Where(a => a.CompanyId == companyId && a.BranchId == branchId)
                    .ToListAsync();
                
                Console.WriteLine($"✅ [AreaService] GetAreasByCompanyAndBranchAsync() - Áreas encontradas: {areas.Count}");
                return areas;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AreaService] GetAreasByCompanyAndBranchAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [AreaService] GetAreasByCompanyAndBranchAsync() - StackTrace: {ex.StackTrace}");
                return new List<Area>();
            }
        }
    }
}
