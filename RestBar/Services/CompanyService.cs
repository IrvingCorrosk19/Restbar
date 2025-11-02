using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services
{
    public class CompanyService : BaseTrackingService, ICompanyService
    {
        public CompanyService(RestBarContext context, IHttpContextAccessor httpContextAccessor) 
            : base(context, httpContextAccessor)
        {
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
            try
            {
                Console.WriteLine($"🔍 [CompanyService] CreateAsync() - Iniciando creación de compañía: {company.Name}");
                
                if (string.IsNullOrWhiteSpace(company.Name))
                    throw new ArgumentException("El nombre es requerido", nameof(company.Name));

                var newCompany = new Company
                {
                    Id = Guid.NewGuid(),
                    Name = company.Name,
                    LegalId = company.LegalId,
                    TaxId = company.TaxId,
                    Address = company.Address,
                    Phone = company.Phone,
                    Email = company.Email,
                    IsActive = true // Activo por defecto
                };

                // ✅ Usar SetCreatedTracking para establecer todos los campos de auditoría
                SetCreatedTracking(newCompany);
                
                // Si el controlador ya estableció CreatedBy, mantenerlo
                if (!string.IsNullOrWhiteSpace(company.CreatedBy))
                {
                    newCompany.CreatedBy = company.CreatedBy;
                    newCompany.UpdatedBy = company.CreatedBy;
                }
                
                Console.WriteLine($"✅ [CompanyService] CreateAsync() - Campos establecidos: CreatedBy={newCompany.CreatedBy}, CreatedAt={newCompany.CreatedAt}, UpdatedAt={newCompany.UpdatedAt}");

                _context.Companies.Add(newCompany);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ [CompanyService] CreateAsync() - Compañía creada exitosamente con ID: {newCompany.Id}");
                return newCompany;
            }
            catch (ArgumentException ex)
            {
                // Error de validación
                throw;
            }
            catch (DbUpdateException ex)
            {
                // Error de base de datos
                throw new ApplicationException("Ocurrió un error al guardar la compañía en la base de datos.", ex);
            }
            catch (Exception ex)
            {
                // Error general
                throw new ApplicationException("Ocurrió un error inesperado al crear la compañía.", ex);
            }
        }


        public async Task UpdateAsync(Company company)
        {
            try
            {
                Console.WriteLine($"🔍 [CompanyService] UpdateAsync() - Actualizando compañía: {company.Name} (ID: {company.Id})");
                
                // Buscar si hay una entidad con el mismo ID siendo rastreada
                var existingEntity = _context.ChangeTracker.Entries<Company>()
                    .FirstOrDefault(e => e.Entity.Id == company.Id);

                if (existingEntity != null)
                {
                    // Detach la entidad existente para evitar conflictos
                    existingEntity.State = EntityState.Detached;
                }

                // ✅ Usar SetUpdatedTracking para establecer campos de auditoría de actualización
                SetUpdatedTracking(company);
                
                Console.WriteLine($"✅ [CompanyService] UpdateAsync() - Campos actualizados: UpdatedBy={company.UpdatedBy}, UpdatedAt={company.UpdatedAt}");

                _context.Companies.Update(company);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"✅ [CompanyService] UpdateAsync() - Compañía actualizada exitosamente");
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