using Microsoft.EntityFrameworkCore;

using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services
{
    public class AreaService : IAreaService
    {
        private readonly RestBarContext _context;

        public AreaService(RestBarContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Area>> GetAllAsync()
        {
            return await _context.Areas
                .Include(a => a.Branch)
                .Include(a => a.Company)
                .Include(a => a.Tables)
                .ToListAsync();
        }

        public async Task<Area?> GetByIdAsync(Guid id)
        {
            return await _context.Areas
                .Include(a => a.Branch)
                .Include(a => a.Company)
                .Include(a => a.Tables)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Area> CreateAsync(Area area)
        {
            _context.Areas.Add(area);
            await _context.SaveChangesAsync();
            return area;
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
                .Include(a => a.Branch)
                .Include(a => a.Company)
                .Include(a => a.Tables)
                .ToListAsync();
        }

        public async Task<IEnumerable<Area>> GetByCompanyIdAsync(Guid companyId)
        {
            return await _context.Areas
                .Where(a => a.CompanyId == companyId)
                .Include(a => a.Branch)
                .Include(a => a.Company)
                .Include(a => a.Tables)
                .ToListAsync();
        }

        public async Task<User?> GetCurrentUserWithAssignmentsAsync(Guid userId)
        {
            try
            {
                Console.WriteLine($"🔍 [AreaService] GetCurrentUserWithAssignmentsAsync() - Buscando usuario con ID: {userId}");
                
                var user = await _context.Users
                    .Include(u => u.Branch)
                    .Include(u => u.Company) // ✅ Agregado
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    Console.WriteLine($"⚠️ [AreaService] GetCurrentUserWithAssignmentsAsync() - Usuario no encontrado: {userId}");
                    return null;
                }

                Console.WriteLine($"✅ [AreaService] GetCurrentUserWithAssignmentsAsync() - Usuario encontrado: {user.FullName ?? user.Email}");
                Console.WriteLine($"🏢 [AreaService] GetCurrentUserWithAssignmentsAsync() - CompanyId: {user.CompanyId}");
                Console.WriteLine($"🏪 [AreaService] GetCurrentUserWithAssignmentsAsync() - BranchId: {user.BranchId}");

                // Ahora tenemos acceso completo a CompanyId y BranchId del usuario
                Console.WriteLine($"🎯 [AreaService] GetCurrentUserWithAssignmentsAsync() - Datos completos disponibles");

                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AreaService] GetCurrentUserWithAssignmentsAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [AreaService] GetCurrentUserWithAssignmentsAsync() - StackTrace: {ex.StackTrace}");
                return null;
            }
        }
    }
}
