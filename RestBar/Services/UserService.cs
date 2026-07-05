using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using RestBar.Helpers;
using System.Security.Cryptography;
using System.Text;

namespace RestBar.Services
{
    public class UserService : BaseTrackingService, IUserService
    {
        private readonly IGlobalLoggingService _loggingService;

        public UserService(RestBarContext context, IGlobalLoggingService loggingService, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
            _loggingService = loggingService;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.Branch)
                .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> CreateAsync(User user)
        {
            try
            {
                // Validar que el password no esté vacío
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    throw new ArgumentException("La contraseña no puede estar vacía", nameof(user.PasswordHash));
                }

                user.IsActive = true;
                user.PasswordHash = HashPassword(user.PasswordHash);
                
                // ✅ Usar SetCreatedTracking para establecer todos los campos de auditoría
                SetCreatedTracking(user);
                
                // Si el controlador ya estableció CreatedBy, mantenerlo
                if (!string.IsNullOrWhiteSpace(user.CreatedBy))
                {
                    user.UpdatedBy = user.CreatedBy;
                }
                
                Console.WriteLine($"✅ [UserService] CreateAsync() - Campos establecidos: CreatedBy={user.CreatedBy}, CreatedAt={user.CreatedAt}, UpdatedAt={user.UpdatedAt}");
                
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // ✅ NUEVO: Logging de creación de usuario
                await _loggingService.LogUserActivityAsync(
                    action: AuditAction.CREATE.ToString(),
                    description: $"Usuario creado: {user.FullName} ({user.Email}) - Rol: {user.Role}",
                    userId: user.Id,
                    newValues: new { user.FullName, user.Email, user.Role, user.BranchId }
                );

                return user;
            }
            catch (Exception ex)
            {
                // ✅ NUEVO: Logging de error
                await _loggingService.LogModuleErrorAsync(
                    module: AuditModule.USER.ToString(),
                    description: $"Error al crear usuario: {user.Email}",
                    exception: ex
                );

                throw new ApplicationException("Error al crear el usuario en la base de datos.", ex);
            }
        }

        public async Task UpdateAsync(User user)
        {
            try
            {
                // Obtener valores anteriores para logging
                var oldUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.Id);

                // Buscar si hay una entidad con el mismo ID siendo rastreada
                var existingEntity = _context.ChangeTracker.Entries<User>()
                    .FirstOrDefault(e => e.Entity.Id == user.Id);

                if (existingEntity != null)
                {
                    // Detach la entidad existente para evitar conflictos
                    existingEntity.State = EntityState.Detached;
                }

                if (oldUser != null && oldUser.IsActive && !user.IsActive &&
                    await OperationalGuard.UserHasActiveOrdersAsync(_context, user.Id))
                {
                    throw new InvalidOperationException(
                        "No se puede desactivar un usuario con órdenes activas. Reasigne las mesas u órdenes primero.");
                }

                // Solo hashear el password si no está vacío y no parece estar ya hasheado
                if (!string.IsNullOrEmpty(user.PasswordHash) && 
                    !user.PasswordHash.Contains("=") && // Los hashes Base64 contienen "="
                    user.PasswordHash.Length < 50) // Los hashes son más largos
                {
                    user.PasswordHash = HashPassword(user.PasswordHash);
                }

                // ✅ Usar SetUpdatedTracking para establecer campos de auditoría de actualización
                SetUpdatedTracking(user);
                
                Console.WriteLine($"✅ [UserService] UpdateAsync() - Campos actualizados: UpdatedBy={user.UpdatedBy}, UpdatedAt={user.UpdatedAt}");

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                // ✅ NUEVO: Logging de actualización de usuario
                await _loggingService.LogUserActivityAsync(
                    action: AuditAction.UPDATE.ToString(),
                    description: $"Usuario actualizado: {user.FullName} ({user.Email})",
                    userId: user.Id,
                    oldValues: oldUser != null ? new { oldUser.FullName, oldUser.Email, oldUser.Role, oldUser.BranchId, oldUser.IsActive } : null,
                    newValues: new { user.FullName, user.Email, user.Role, user.BranchId, user.IsActive }
                );
            }
            catch (Exception ex)
            {
                // ✅ NUEVO: Logging de error
                await _loggingService.LogModuleErrorAsync(
                    module: AuditModule.USER.ToString(),
                    description: $"Error al actualizar usuario: {user.Email}",
                    exception: ex,
                    recordId: user.Id
                );

                throw new ApplicationException("Error al actualizar el usuario en la base de datos.", ex);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            if (await OperationalGuard.UserHasActiveOrdersAsync(_context, id))
                throw new InvalidOperationException(
                    "No se puede eliminar un usuario con órdenes activas. Reasigne las órdenes primero.");

            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                // ✅ NUEVO: Logging de eliminación de usuario
                await _loggingService.LogUserActivityAsync(
                    action: AuditAction.DELETE.ToString(),
                    description: $"Usuario eliminado: {user.FullName} ({user.Email})",
                    userId: user.Id,
                    oldValues: new { user.FullName, user.Email, user.Role, user.BranchId, user.IsActive }
                );

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetByBranchIdAsync(Guid branchId)
        {
            return await _context.Users
                .Where(u => u.BranchId == branchId)
                .Include(u => u.Branch)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
        {
            return await _context.Users
                .Where(u => u.Role == role)
                .Include(u => u.Branch)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _context.Users
                .Where(u => u.IsActive == true)
                .Include(u => u.Branch)
                .ToListAsync();
        }

        public async Task<User?> GetUserWithOrdersAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.Branch)
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetUserWithAuditLogsAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.Branch)
                .Include(u => u.AuditLogs)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool> ValidateUserAsync(string email, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !user.IsActive == true)
                return false;

            var hashedPassword = HashPassword(password);
            return user.PasswordHash == hashedPassword;
        }

        // ✅ NUEVO: Método para obtener usuario actual con asignaciones
        public async Task<User?> GetCurrentUserWithAssignmentsAsync(Guid userId)
        {
            try
            {
                Console.WriteLine($"🔍 [UserService] GetCurrentUserWithAssignmentsAsync() - Iniciando para userId: {userId}");

                var user = await _context.Users
                    .Include(u => u.Branch)
                    .ThenInclude(b => b.Company)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    Console.WriteLine($"⚠️ [UserService] GetCurrentUserWithAssignmentsAsync() - Usuario con ID {userId} no encontrado");
                    return null;
                }

                Console.WriteLine($"✅ [UserService] GetCurrentUserWithAssignmentsAsync() - Usuario encontrado: {user.Email}, CompanyId: {user.Branch?.CompanyId}, BranchId: {user.BranchId}");
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [UserService] GetCurrentUserWithAssignmentsAsync() - Error: {ex.Message}");
                return null;
            }
        }

        private string HashPassword(string password)
        {
            // Validar que el password no sea nulo o vacío
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));
            }

            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
} 