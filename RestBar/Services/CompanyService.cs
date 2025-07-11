using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly RestBarContext _context;

        public CompanyService(RestBarContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Company>> GetAllAsync()
        {
            return await _context.Companies
                .Include(c => c.Branches)
                .ToListAsync();
        }

        public async Task<Company?> GetByIdAsync(Guid id)
        {
            return await _context.Companies
                .Include(c => c.Branches)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Company> CreateAsync(Company company)
        {
            company.CreatedAt = DateTime.UtcNow;
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
            return company;
        }

        public async Task UpdateAsync(Company company)
        {
            _context.Entry(company).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Company?> GetCompanyWithBranchesAsync(Guid id)
        {
            return await _context.Companies
                .Include(c => c.Branches)
                    .ThenInclude(b => b.Areas)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Company>> GetCompaniesWithActiveBranchesAsync()
        {
            return await _context.Companies
                .Include(c => c.Branches.Where(b => b.IsActive == true))
                .ToListAsync();
        }

        public async Task<Company?> GetByLegalIdAsync(string legalId)
        {
            return await _context.Companies
                .Include(c => c.Branches)
                .FirstOrDefaultAsync(c => c.LegalId == legalId);
        }
    }
}  