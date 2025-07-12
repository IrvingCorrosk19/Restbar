using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RestBar.Services
{
    public class AuthService : IAuthService
    {
        private readonly RestBarContext _context;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            RestBarContext context, 
            IUserService userService, 
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthService> logger)
        {
            _context = context;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            try
            {
                _logger.LogInformation($"[AuthService] Intento de login para email: {email}");

                var user = await _context.Users
                    .Include(u => u.Branch)
                    .ThenInclude(b => b.Company)
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    _logger.LogWarning($"[AuthService] Usuario no encontrado: {email}");
                    return null;
                }

                if (user.IsActive != true)
                {
                    _logger.LogWarning($"[AuthService] Usuario inactivo: {email}");
                    return null;
                }

                var hashedPassword = HashPassword(password);
                if (user.PasswordHash != hashedPassword)
                {
                    _logger.LogWarning($"[AuthService] Contraseña incorrecta para: {email}");
                    return null;
                }

                _logger.LogInformation($"[AuthService] Login exitoso para: {email}, Rol: {user.Role}");
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[AuthService] Error en LoginAsync para email: {email}");
                return null;
            }
        }

        public async Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal)
        {
            try
            {
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return null;
                }

                return await GetUserByIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthService] Error en GetCurrentUserAsync");
                return null;
            }
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            try
            {
                return await _context.Users
                    .Include(u => u.Branch)
                    .ThenInclude(b => b.Company)
                    .FirstOrDefaultAsync(u => u.Id == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[AuthService] Error en GetUserByIdAsync para userId: {userId}");
                return null;
            }
        }

        public async Task<bool> IsUserActiveAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);
                
                return user?.IsActive == true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[AuthService] Error en IsUserActiveAsync para userId: {userId}");
                return false;
            }
        }

        public async Task<bool> HasPermissionAsync(Guid userId, string action)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null || user.IsActive != true)
                {
                    return false;
                }

                // Permisos basados en roles
                return user.Role switch
                {
                    UserRole.admin => true, // Admin tiene acceso a todo
                    UserRole.manager => action != "admin_only", // Manager tiene acceso casi completo
                    UserRole.supervisor => action is "orders" or "kitchen" or "payments" or "tables",
                    UserRole.waiter => action is "orders" or "tables" or "customers",
                    UserRole.cashier => action is "orders" or "payments" or "customers",
                    UserRole.chef => action is "kitchen" or "orders",
                    UserRole.bartender => action is "orders" or "kitchen",
                    UserRole.inventory => action is "inventory" or "products",
                    UserRole.accountant => action is "payments" or "reports",
                    UserRole.support => action is "orders" or "users",
                    _ => false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[AuthService] Error en HasPermissionAsync para userId: {userId}, action: {action}");
                return false;
            }
        }

        public async Task<string> GenerateJwtTokenAsync(User user)
        {
            // Por ahora retornamos el userId como token simple
            // Más adelante se puede implementar JWT real si es necesario
            await Task.CompletedTask;
            return user.Id.ToString();
        }

        public async Task<ClaimsPrincipal> GetClaimsPrincipalAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("UserId", user.Id.ToString()),
                new Claim("UserRole", user.Role.ToString())
            };

            if (user.BranchId.HasValue)
            {
                claims.Add(new Claim("BranchId", user.BranchId.Value.ToString()));
            }

            if (user.Branch?.CompanyId != null)
            {
                claims.Add(new Claim("CompanyId", user.Branch.CompanyId.ToString()));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(claimsIdentity);
        }

        public async Task LogoutAsync(string userId)
        {
            try
            {
                _logger.LogInformation($"[AuthService] Logout para userId: {userId}");
                
                if (_httpContextAccessor.HttpContext != null)
                {
                    await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[AuthService] Error en LogoutAsync para userId: {userId}");
            }
        }

        public async Task<User> CreateDefaultAdminAsync()
        {
            try
            {
                _logger.LogInformation("[AuthService] Creando usuario admin por defecto");

                var admin = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "admin@restbar.com",
                    FullName = "Administrador del Sistema",
                    PasswordHash = HashPassword("Admin123!"),
                    Role = UserRole.admin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    BranchId = null
                };

                _context.Users.Add(admin);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"[AuthService] Usuario admin creado exitosamente con ID: {admin.Id}");
                return admin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthService] Error en CreateDefaultAdminAsync");
                throw;
            }
        }

        public async Task<bool> HasAdminUserAsync()
        {
            try
            {
                return await _context.Users
                    .AnyAsync(u => u.Role == UserRole.admin && u.IsActive == true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthService] Error en HasAdminUserAsync");
                return false;
            }
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