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
            // Solo asignar fecha si no viene del cliente y no tiene valor por defecto de BD
            if (company.CreatedAt == null)
            {
                company.CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            }
            
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
            return company;
        }

        public async Task UpdateAsync(Company company)
        {
            try
            {
                // Buscar si hay una entidad con el mismo ID siendo rastreada
                var existingEntity = _context.ChangeTracker.Entries<Company>()
                    .FirstOrDefault(e => e.Entity.Id == company.Id);

                if (existingEntity != null)
                {
                    // Detach la entidad existente para evitar conflictos
                    existingEntity.State = EntityState.Detached;
                }

                // Usar Update para manejar automáticamente el tracking
                _context.Companies.Update(company);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al actualizar la empresa en la base de datos.", ex);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var company = await _context.Companies
                .Include(c => c.Branches)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company != null)
            {
                // Verificar si la compañía tiene sucursales asociadas
                if (company.Branches.Any())
                {
                    throw new InvalidOperationException($"No se puede eliminar la compañía '{company.Name}' porque tiene {company.Branches.Count} sucursal(es) asociada(s). Debe eliminar o reasignar todas las sucursales antes de continuar.");
                }

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