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
                .Include(a => a.Tables)
                .ToListAsync();
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
                .Include(a => a.Tables)
                .ToListAsync();
        }
    }
}
