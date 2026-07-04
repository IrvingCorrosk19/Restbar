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
    public class AuthService : BaseTrackingService, IAuthService
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthService> _logger;

        // Identificador de hash BCrypt ($2a$, $2b$, $2y$)
        private const string BcryptPrefix = "$2";

        public AuthService(
            RestBarContext context,
            IUserService userService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthService> logger)
            : base(context, httpContextAccessor)
        {
            _userService = userService;
            _logger = logger;
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            try
            {
                _logger.LogInformation("[AuthService] Intento de login para email: {Email}", email);

                var user = await _context.Users
                    .Include(u => u.Branch)
                    .ThenInclude(b => b.Company)
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    _logger.LogWarning("[AuthService] Usuario no encontrado: {Email}", email);
                    return null;
                }

                if (user.IsActive != true)
                {
                    _logger.LogWarning("[AuthService] Usuario inactivo: {Email}", email);
                    return null;
                }

                if (user.Role != UserRole.superadmin)
                {
                    if (user.Branch?.Company != null && !user.Branch.Company.IsActive)
                    {
                        _logger.LogWarning("[AuthService] Empresa suspendida: {Email}", email);
                        return null;
                    }
                    if (user.Branch != null && !user.Branch.IsActive)
                    {
                        _logger.LogWarning("[AuthService] Sucursal suspendida: {Email}", email);
                        return null;
                    }
                }

                // Token de reset activo — bloquear login normal
                if (user.PasswordHash?.StartsWith("RESET_TOKEN:") == true)
                {
                    _logger.LogWarning("[AuthService] Login bloqueado: token de reset activo para {Email}", email);
                    return null;
                }

                bool passwordValid = VerifyPassword(password, user.PasswordHash ?? string.Empty);

                if (!passwordValid)
                {
                    _logger.LogWarning("[AuthService] Contraseña incorrecta para: {Email}", email);
                    return null;
                }

                // MIGRACIÓN PROGRESIVA: re-hashear a BCrypt si aún es SHA256
                if (!user.PasswordHash!.StartsWith(BcryptPrefix))
                {
                    _logger.LogInformation("[AuthService] Migrando hash a BCrypt para usuario {Email}", email);
                    user.PasswordHash = HashPasswordBcrypt(password);
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("[AuthService] Login exitoso. Email: {Email}, Rol: {Role}", email, user.Role);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthService] Error en LoginAsync para email: {Email}", email);
                return null;
            }
        }

        /// <summary>
        /// Verifica contraseña soportando BCrypt (nuevo) y SHA256 Base64 (legacy).
        /// Nunca lanza excepción al exterior.
        /// </summary>
        private bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash)) return false;

            if (storedHash.StartsWith(BcryptPrefix))
            {
                try
                {
                    return BCrypt.Net.BCrypt.Verify(password, storedHash);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[AuthService] Error al verificar BCrypt hash");
                    return false;
                }
            }

            // Fallback SHA256 legacy — solo durante migración progresiva
            return storedHash == HashPasswordSha256(password);
        }

        /// <summary>Hash BCrypt con work factor 12.</summary>
        public static string HashPasswordBcrypt(string password)
            => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

        /// <summary>Hash SHA256 Base64 legacy — solo para comparación durante migración.</summary>
        private static string HashPasswordSha256(string password)
        {
            using var sha256 = SHA256.Create();
            return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        public async Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal)
        {
            try
            {
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return null;

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
                _logger.LogError(ex, "[AuthService] Error en GetUserByIdAsync. UserId: {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> IsUserActiveAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                return user?.IsActive == true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthService] Error en IsUserActiveAsync. UserId: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> HasPermissionAsync(Guid userId, string action)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null || user.IsActive != true) return false;

                return user.Role switch
                {
                    UserRole.superadmin => true,
                    UserRole.admin => action != "superadmin_only",
                    UserRole.manager => action != "admin_only" && action != "superadmin_only",
                    UserRole.supervisor => action is "orders" or "kitchen" or "payments" or "tables",
                    UserRole.waiter => action is "orders" or "tables" or "customers",
                    UserRole.cashier => action is "orders" or "payments" or "customers",
                    UserRole.chef => action is "kitchen" or "orders",
                    UserRole.bartender => action is "orders" or "kitchen",
                    UserRole.accountant => action is "payments" or "reports",
                    UserRole.support => action is "orders" or "users",
                    UserRole.inventarista => action is "inventory" or "reports",
                    _ => false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthService] Error en HasPermissionAsync. UserId: {UserId}, Action: {Action}", userId, action);
                return false;
            }
        }

        public async Task<string> GenerateJwtTokenAsync(User user)
        {
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
                if (user.Branch != null)
                    claims.Add(new Claim("BranchName", user.Branch.Name));
            }

            if (user.Branch?.CompanyId != null)
            {
                claims.Add(new Claim("CompanyId", user.Branch.CompanyId.ToString()));
                if (user.Branch.Company != null)
                    claims.Add(new Claim("CompanyName", user.Branch.Company.Name));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(claimsIdentity);
        }

        public async Task LogoutAsync(string userId)
        {
            try
            {
                _logger.LogInformation("[AuthService] Logout para userId: {UserId}", userId);
                if (_httpContextAccessor.HttpContext != null)
                    await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthService] Error en LogoutAsync. UserId: {UserId}", userId);
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
                    PasswordHash = HashPasswordBcrypt("Admin123!"),
                    Role = UserRole.admin,
                    IsActive = true,
                    BranchId = null
                };

                _context.Users.Add(admin);
                await _context.SaveChangesAsync();

                _logger.LogInformation("[AuthService] Admin creado. Id: {AdminId}", admin.Id);
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
    }
}
