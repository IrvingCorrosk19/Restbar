using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services
{
    public class BranchService : BaseTrackingService, IBranchService
    {
        public BranchService(RestBarContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<IEnumerable<Branch>> GetAllAsync()
        {
            var query = _context.Branches
                .Include(b => b.Company)
                .Include(b => b.Areas)
                .Include(b => b.Users)
                .AsQueryable();

            var user = _httpContextAccessor?.HttpContext?.User;
            var role = user?.FindFirst("UserRole")?.Value;
            if (!string.Equals(role, "superadmin", StringComparison.OrdinalIgnoreCase)
                && Guid.TryParse(user?.FindFirst("CompanyId")?.Value, out var companyId))
            {
                query = query.Where(b => b.CompanyId == companyId);
            }

            return await query.ToListAsync();
        }

        public async Task<Branch?> GetByIdAsync(Guid id)
        {
            var branch = await _context.Branches
                .Include(b => b.Company)
                .Include(b => b.Areas)
                .Include(b => b.Users)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (branch == null) return null;

            var user = _httpContextAccessor?.HttpContext?.User;
            var role = user?.FindFirst("UserRole")?.Value;
            if (!string.Equals(role, "superadmin", StringComparison.OrdinalIgnoreCase)
                && Guid.TryParse(user?.FindFirst("CompanyId")?.Value, out var companyId)
                && branch.CompanyId != companyId)
            {
                return null;
            }

            return branch;
        }

        public async Task<Branch> CreateAsync(Branch branch)
        {
            // Configurar tracking automático antes de crear
            SetCreatedTracking(branch);
            
            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();
            return branch;
        }

        public async Task UpdateAsync(Branch branch)
        {
            try
            {
                // Buscar si hay una entidad con el mismo ID siendo rastreada
                var existingEntity = _context.ChangeTracker.Entries<Branch>()
                    .FirstOrDefault(e => e.Entity.Id == branch.Id);

                if (existingEntity != null)
                {
                    // Detach la entidad existente para evitar conflictos
                    existingEntity.State = EntityState.Detached;
                }

                // Configurar tracking automático antes de actualizar
                SetUpdatedTracking(branch);
                
                _context.Branches.Update(branch);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al actualizar la sucursal en la base de datos.", ex);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var branch = await _context.Branches
                .Include(b => b.Areas)
                .Include(b => b.Users)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (branch != null)
            {
                // Verificar si la sucursal tiene áreas asociadas
                if (branch.Areas.Any())
                {
                    throw new InvalidOperationException($"No se puede eliminar la sucursal '{branch.Name}' porque tiene {branch.Areas.Count} área(s) asociada(s). Debe eliminar o reasignar todas las áreas antes de continuar.");
                }

                // Verificar si la sucursal tiene usuarios asociados
                if (branch.Users.Any())
                {
                    throw new InvalidOperationException($"No se puede eliminar la sucursal '{branch.Name}' porque tiene {branch.Users.Count} usuario(s) asociado(s). Debe eliminar o reasignar todos los usuarios antes de continuar.");
                }

                _context.Branches.Remove(branch);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Branch>> GetByCompanyIdAsync(Guid companyId)
        {
            return await _context.Branches
                .Where(b => b.CompanyId == companyId)
                .Include(b => b.Areas)
                .ToListAsync();
        }

        public async Task<IEnumerable<Branch>> GetActiveBranchesAsync()
        {
            return await _context.Branches
                .Where(b => b.IsActive == true)
                .Include(b => b.Company)
                .ToListAsync();
        }

        public async Task<IEnumerable<Branch>> GetBranchesWithAreasAsync()
        {
            return await _context.Branches
                .Include(b => b.Areas)
                .ThenInclude(a => a.Tables)
                .ToListAsync();
        }
    }
} 