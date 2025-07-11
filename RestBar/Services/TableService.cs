using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services
{
    public class TableService : ITableService
    {
        private readonly RestBarContext _context;

        public TableService(RestBarContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Table>> GetAllAsync()
        {
            return await _context.Tables
                .Include(t => t.Area)
                .ToListAsync();
        }

        public async Task<Table?> GetByIdAsync(Guid id)
        {
            return await _context.Tables
                .Include(t => t.Area)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Table> CreateAsync(Table table)
        {
            table.IsActive = true;
            _context.Tables.Add(table);
            await _context.SaveChangesAsync();
            return table;
        }

        public async Task UpdateAsync(Table table)
        {
            _context.Entry(table).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table != null)
            {
                _context.Tables.Remove(table);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Table>> GetByAreaIdAsync(Guid areaId)
        {
            return await _context.Tables
                .Where(t => t.AreaId == areaId)
                .Include(t => t.Area)
                .ToListAsync();
        }

        public async Task<IEnumerable<Table>> GetActiveTablesAsync()
        {
            return await _context.Tables
                .Where(t => t.IsActive == true)
                .Include(t => t.Area)
                .ToListAsync();
        }

        public async Task<IEnumerable<Table>> GetTablesByStatusAsync(string status)
        {
            return await _context.Tables
                .Where(t => t.Status == status && t.IsActive == true)
                .Include(t => t.Area)
                .ToListAsync();
        }

        public async Task<Table?> GetTableWithOrdersAsync(Guid id)
        {
            return await _context.Tables
                .Include(t => t.Area)
                .Include(t => t.Orders)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Table>> GetAvailableTablesAsync()
        {
            return await _context.Tables
                .Where(t => t.IsActive == true && t.Status == "Available")
                .Include(t => t.Area)
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetTablesForViewBagAsync()
        {
            return await _context.Tables
                .Where(t => t.IsActive == true)
                .Select(t => new { t.Id, TableNumber = t.TableNumber })
                .ToListAsync();
        }
    }
} 