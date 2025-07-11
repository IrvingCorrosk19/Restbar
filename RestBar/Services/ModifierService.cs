using Microsoft.EntityFrameworkCore;

using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services
{
    public class ModifierService : IModifierService
    {
        private readonly RestBarContext _context;

        public ModifierService(RestBarContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Modifier>> GetAllAsync()
        {
            return await _context.Modifiers
                .Include(m => m.Products)
                .ToListAsync();
        }

        public async Task<Modifier?> GetByIdAsync(Guid id)
        {
            return await _context.Modifiers
                .Include(m => m.Products)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Modifier> CreateAsync(Modifier modifier)
        {
            _context.Modifiers.Add(modifier);
            await _context.SaveChangesAsync();
            return modifier;
        }

        public async Task UpdateAsync(Modifier modifier)
        {
            _context.Entry(modifier).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var modifier = await _context.Modifiers.FindAsync(id);
            if (modifier != null)
            {
                _context.Modifiers.Remove(modifier);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Modifier?> GetByNameAsync(string name)
        {
            return await _context.Modifiers
                .Include(m => m.Products)
                .FirstOrDefaultAsync(m => m.Name == name);
        }

        public async Task<IEnumerable<Modifier>> GetByCostRangeAsync(decimal minCost, decimal maxCost)
        {
            return await _context.Modifiers
                .Include(m => m.Products)
                .Where(m => m.ExtraCost >= minCost && m.ExtraCost <= maxCost)
                .ToListAsync();
        }

        public async Task<Modifier?> GetModifierWithProductsAsync(Guid id)
        {
            return await _context.Modifiers
                .Include(m => m.Products)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Modifier>> GetModifiersWithProductsAsync()
        {
            return await _context.Modifiers
                .Include(m => m.Products)
                .ToListAsync();
        }

        public async Task<IEnumerable<Modifier>> GetFreeModifiersAsync()
        {
            return await _context.Modifiers
                .Include(m => m.Products)
                .Where(m => m.ExtraCost == 0 || m.ExtraCost == null)
                .ToListAsync();
        }

        public async Task<IEnumerable<Modifier>> GetPaidModifiersAsync()
        {
            return await _context.Modifiers
                .Include(m => m.Products)
                .Where(m => m.ExtraCost > 0)
                .ToListAsync();
        }
    }
} 