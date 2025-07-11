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
            _context.Entry(area).State = EntityState.Modified;
            await _context.SaveChangesAsync();
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
