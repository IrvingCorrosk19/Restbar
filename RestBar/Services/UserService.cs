using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Cryptography;
using System.Text;

namespace RestBar.Services
{
    public class UserService : IUserService
    {
        private readonly RestBarContext _context;

        public UserService(RestBarContext context)
        {
            _context = context;
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
                //user.CreatedAt = DateTime.UtcNow;
                user.IsActive = true;
                user.PasswordHash = HashPassword(user.PasswordHash);
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                // Puedes loguear el error si tienes un logger configurado
                // _logger.LogError(ex, "Error al crear el usuario");

                // También puedes lanzar una excepción más específica
                throw new ApplicationException("Error al crear el usuario en la base de datos.", ex);
            }
        }


        public async Task UpdateAsync(User user)
        {
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                user.PasswordHash = HashPassword(user.PasswordHash);
            }
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
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

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
} 