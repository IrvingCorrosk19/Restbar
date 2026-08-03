using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Cryptography;
using System.Text;

namespace RestBar.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;
        private readonly RestBarContext _context;
        private readonly ILogger<AuthController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;

        public AuthController(IAuthService authService, IEmailService emailService, RestBarContext context, ILogger<AuthController> logger, IWebHostEnvironment env, IMemoryCache cache, IConfiguration config)
        {
            _authService = authService;
            _emailService = emailService;
            _context = context;
            _logger = logger;
            _env = env;
            _cache = cache;
            _config = config;
        }

        private bool RequireMfa => _config.GetValue("FeatureFlags:RequireMfa", false);

        // GET: /Auth/Login
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("auth_endpoints")]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            try
            {
                _logger.LogInformation("[AuthController] Intento de login. Email: {Email}, IP: {IP}",
                    email, HttpContext.Connection.RemoteIpAddress);

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    Console.WriteLine($"⚠️ [AuthController] Login() - Email o password vacíos");
                    ModelState.AddModelError("", "Email y contraseña son requeridos");
                    ViewData["ReturnUrl"] = returnUrl;
                    return View();
                }

                // Normalizar contraseña (eliminar espacios al inicio y final)
                password = password?.Trim() ?? string.Empty;
                var user = await _authService.LoginAsync(email, password);
                
                if (user == null)
                {
                    _logger.LogWarning("[AuthController] Login fallido. Email: {Email}, IP: {IP}",
                        email, HttpContext.Connection.RemoteIpAddress);
                    ModelState.AddModelError("", "Email o contraseña incorrectos");
                    ViewData["ReturnUrl"] = returnUrl;
                    return View();
                }

                var privileged = user.Role is UserRole.superadmin or UserRole.admin or UserRole.manager or UserRole.supervisor;
                if (RequireMfa && privileged && user.MfaEnabled && !string.IsNullOrEmpty(user.MfaSecret))
                {
                    var pendingKey = $"mfa_pending_{Guid.NewGuid():N}";
                    _cache.Set(pendingKey, user.Id, TimeSpan.FromMinutes(5));
                    TempData["MfaPendingKey"] = pendingKey;
                    TempData["ReturnUrl"] = returnUrl;
                    return RedirectToAction(nameof(MfaChallenge));
                }

                var claimsPrincipal = await _authService.GetClaimsPrincipalAsync(user);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                _logger.LogInformation("[AuthController] Login exitoso. Email: {Email}, Rol: {Role}",
                    email, user.Role);

                // Privileged roles without MFA are forced to enroll before using the console.
                if (RequireMfa && privileged && !user.MfaEnabled)
                    return RedirectToAction(nameof(MfaSetup));

                // Redirigir según el rol
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    Console.WriteLine($"🔍 [AuthController] Login() - Redirigiendo a returnUrl: {returnUrl}");
                    return Redirect(returnUrl);
                }

                if (user.Role == UserRole.chef)
                    return RedirectToAction("StationOrders", "Order", new { stationType = "kitchen" });
                if (user.Role == UserRole.bartender)
                    return RedirectToAction("StationOrders", "Order", new { stationType = "bar" });

                var controllerName = GetControllerByRole(user.Role);
                Console.WriteLine($"🔍 [AuthController] Login() - Redirigiendo a controller: {controllerName}");
                return RedirectToAction("Index", controllerName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AuthController] Login() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [AuthController] Login() - StackTrace: {ex.StackTrace}");
                _logger.LogError(ex, $"[AuthController] Error en login para: {email}");
                ModelState.AddModelError("", "Error interno del servidor");
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult MfaChallenge()
        {
            if (TempData["MfaPendingKey"] == null)
                return RedirectToAction(nameof(Login));
            TempData.Keep("MfaPendingKey");
            TempData.Keep("ReturnUrl");
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("auth_endpoints")]
        public async Task<IActionResult> MfaChallenge(string code)
        {
            var pendingKey = TempData["MfaPendingKey"] as string;
            var returnUrl = TempData["ReturnUrl"] as string;
            if (string.IsNullOrEmpty(pendingKey) || !_cache.TryGetValue(pendingKey, out Guid userId))
            {
                ModelState.AddModelError("", "Sesión MFA expirada. Inicie sesión de nuevo.");
                return RedirectToAction(nameof(Login));
            }

            var user = await _context.Users
                .Include(u => u.Branch).ThenInclude(b => b!.Company)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || !user.MfaEnabled || string.IsNullOrEmpty(user.MfaSecret)
                || !Helpers.TotpHelper.VerifyCode(user.MfaSecret, code ?? ""))
            {
                TempData["MfaPendingKey"] = pendingKey;
                TempData["ReturnUrl"] = returnUrl;
                _cache.Set(pendingKey, userId, TimeSpan.FromMinutes(5));
                ModelState.AddModelError("", "Código MFA inválido");
                return View();
            }

            _cache.Remove(pendingKey);
            var claimsPrincipal = await _authService.GetClaimsPrincipalAsync(user);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", GetControllerByRole(user.Role));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MfaSetup()
        {
            var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
            var user = await _context.Users.FirstAsync(u => u.Id == userId);
            if (user.MfaEnabled && !string.IsNullOrEmpty(user.MfaSecret))
                return RedirectToAction("Index", "Home");

            var secret = Helpers.TotpHelper.GenerateSecret();
            TempData["MfaSetupSecret"] = secret;
            ViewBag.Secret = secret;
            ViewBag.Uri = Helpers.TotpHelper.GetProvisioningUri(user.Email, secret);
            ViewBag.Email = user.Email;
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MfaSetup(string code)
        {
            var secret = TempData["MfaSetupSecret"] as string;
            if (string.IsNullOrEmpty(secret))
                return RedirectToAction(nameof(MfaSetup));

            if (!Helpers.TotpHelper.VerifyCode(secret, code ?? ""))
            {
                TempData["MfaSetupSecret"] = secret;
                ViewBag.Secret = secret;
                ViewBag.Uri = Helpers.TotpHelper.GetProvisioningUri(User.Identity?.Name ?? "user", secret);
                ModelState.AddModelError("", "Código inválido. Escanee de nuevo e intente.");
                return View();
            }

            var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
            var user = await _context.Users
                .Include(u => u.Branch).ThenInclude(b => b!.Company)
                .FirstAsync(u => u.Id == userId);
            user.MfaEnabled = true;
            user.MfaSecret = secret;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            var claimsPrincipal = await _authService.GetClaimsPrincipalAsync(user);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
            TempData["Success"] = "MFA activado.";
            return RedirectToAction("Index", "Home");
        }

        // POST: /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                _logger.LogInformation($"[AuthController] Logout para userId: {userId}");

                await _authService.LogoutAsync(userId ?? "unknown");
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthController] Error en logout");
                return RedirectToAction("Login");
            }
        }

        // GET: /Auth/AccessDenied
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: /Auth/Profile
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToAction("Login");
                }

                return View(currentUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthController] Error en Profile");
                return RedirectToAction("Login");
            }
        }

        // GET: /Auth/CurrentUser (API endpoint para JavaScript)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CurrentUser()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var userData = new
                {
                    id = currentUser.Id,
                    email = currentUser.Email,
                    fullName = currentUser.FullName,
                    role = currentUser.Role.ToString(),
                    branchId = currentUser.BranchId,
                    branchName = currentUser.Branch?.Name,
                    companyId = currentUser.Branch?.CompanyId,
                    companyName = currentUser.Branch?.Company?.Name,
                    isActive = currentUser.IsActive
                };

                return Json(new { success = true, user = userData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthController] Error en CurrentUser");
                return Json(new { success = false, message = "Error interno" });
            }
        }

        // POST: /Auth/CheckPermission (API endpoint para verificar permisos)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CheckPermission(string action)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Json(new { success = false, hasPermission = false });
                }

                var hasPermission = await _authService.HasPermissionAsync(userId, action);
                return Json(new { success = true, hasPermission });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[AuthController] Error en CheckPermission para action: {action}");
                return Json(new { success = false, hasPermission = false });
            }
        }

        // GET: /Auth/CreateAdmin (Solo para desarrollo - crear admin por defecto)
        [AllowAnonymous]
        public async Task<IActionResult> CreateAdmin()
        {
            if (!_env.IsDevelopment())
                return NotFound();

            try
            {
                // Solo permitir si no hay admins
                var hasAdmin = await _authService.HasAdminUserAsync();
                if (hasAdmin)
                {
                    TempData["Message"] = "Ya existe un usuario administrador en el sistema";
                    return RedirectToAction("Login");
                }

                var admin = await _authService.CreateDefaultAdminAsync();
                TempData["Message"] = $"Usuario administrador creado: {admin.Email} / Admin123!";
                _logger.LogInformation($"[AuthController] Usuario admin por defecto creado: {admin.Email}");
                
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthController] Error en CreateAdmin");
                TempData["Error"] = "Error al crear usuario administrador";
                return RedirectToAction("Login");
            }
        }

        // GET: /Auth/ForgotPassword
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Auth/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("auth_endpoints")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            try
            {
                Console.WriteLine($"🔍 [AuthController] ForgotPassword() - Iniciando recuperación para: {email}");

                if (string.IsNullOrEmpty(email))
                {
                    ModelState.AddModelError("", "Email es requerido");
                    return View();
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive == true);
                if (user == null)
                {
                    // Por seguridad, no revelar si el email existe o no
                    TempData["Message"] = "Si el email existe en nuestro sistema, recibirás un correo con instrucciones para recuperar tu contraseña.";
                    return View();
                }

                // Token en cache — NO sobrescribir PasswordHash (bug histórico)
                var resetToken = GenerateResetToken();
                _cache.Set(ResetCacheKey(email), resetToken, TimeSpan.FromMinutes(30));

                // Enviar email de recuperación
                var emailSent = await _emailService.SendPasswordRecoveryAsync(user, resetToken);
                if (emailSent)
                {
                    Console.WriteLine($"✅ [AuthController] ForgotPassword() - Email de recuperación enviado");
                    TempData["Message"] = "Se ha enviado un correo electrónico con instrucciones para recuperar tu contraseña.";
                }
                else
                {
                    Console.WriteLine($"⚠️ [AuthController] ForgotPassword() - No se pudo enviar email");
                    TempData["Error"] = "No se pudo enviar el correo electrónico. Por favor, intenta más tarde.";
                }

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AuthController] ForgotPassword() - Error: {ex.Message}");
                _logger.LogError(ex, "[AuthController] Error en ForgotPassword");
                TempData["Error"] = "Error interno del servidor";
                return View();
            }
        }

        // GET: /Auth/ResetPassword
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Token o email inválido";
                return RedirectToAction("ForgotPassword");
            }

            ViewData["Token"] = token;
            ViewData["Email"] = email;
            return View();
        }

        // POST: /Auth/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string token, string email, string newPassword)
        {
            try
            {
                Console.WriteLine($"🔍 [AuthController] ResetPassword() - Iniciando reset para: {email}");

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(newPassword))
                {
                    ModelState.AddModelError("", "Token, email y nueva contraseña son requeridos");
                    ViewData["Token"] = token;
                    ViewData["Email"] = email;
                    return View();
                }

                if (newPassword.Length < 6)
                {
                    ModelState.AddModelError("", "La contraseña debe tener al menos 6 caracteres");
                    ViewData["Token"] = token;
                    ViewData["Email"] = email;
                    return View();
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive == true);
                if (user == null)
                {
                    TempData["Error"] = "Usuario no encontrado";
                    return RedirectToAction("ForgotPassword");
                }

                if (!_cache.TryGetValue(ResetCacheKey(email), out string? cachedToken) ||
                    !string.Equals(cachedToken, token, StringComparison.Ordinal))
                {
                    TempData["Error"] = "Token inválido o expirado";
                    return RedirectToAction("ForgotPassword");
                }

                var passwordHash = HashPassword(newPassword);
                user.PasswordHash = passwordHash;
                await _context.SaveChangesAsync();
                _cache.Remove(ResetCacheKey(email));

                Console.WriteLine($"✅ [AuthController] ResetPassword() - Contraseña actualizada exitosamente");
                TempData["Message"] = "Tu contraseña ha sido actualizada exitosamente. Puedes iniciar sesión ahora.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AuthController] ResetPassword() - Error: {ex.Message}");
                _logger.LogError(ex, "[AuthController] Error en ResetPassword");
                TempData["Error"] = "Error interno del servidor";
                ViewData["Token"] = token;
                ViewData["Email"] = email;
                return View();
            }
        }

        private string GenerateResetToken()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        private static string ResetCacheKey(string email) => $"pwd-reset:{email.Trim().ToLowerInvariant()}";

        private string HashPassword(string password)
            => RestBar.Services.AuthService.HashPasswordBcrypt(password);

        private string GetControllerByRole(UserRole role)
        {
            return role switch
            {
                UserRole.superadmin => "SuperAdmin",
                UserRole.admin => "Home",
                UserRole.manager => "Home", 
                UserRole.supervisor => "Order",
                UserRole.waiter => "Order",
                UserRole.cashier => "Order",
                UserRole.chef => "Order",
                UserRole.bartender => "Order",

                UserRole.accountant => "Home",
                UserRole.support => "Home",
                _ => "Home"
            };
        }
    }
} 