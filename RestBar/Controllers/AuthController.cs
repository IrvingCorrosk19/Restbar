using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public AuthController(IAuthService authService, IEmailService emailService, RestBarContext context, ILogger<AuthController> logger)
        {
            _authService = authService;
            _emailService = emailService;
            _context = context;
            _logger = logger;
        }

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
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            try
            {
                Console.WriteLine($"🔍 [AuthController] Login() - Iniciando login para: {email}");
                _logger.LogInformation($"[AuthController] Intento de login para: {email}");

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    Console.WriteLine($"⚠️ [AuthController] Login() - Email o password vacíos");
                    ModelState.AddModelError("", "Email y contraseña son requeridos");
                    ViewData["ReturnUrl"] = returnUrl;
                    return View();
                }

                // Normalizar contraseña (eliminar espacios al inicio y final)
                password = password?.Trim() ?? string.Empty;
                Console.WriteLine($"🔍 [AuthController] Login() - Password length: {password.Length}, First char: {(password.Length > 0 ? ((int)password[0]).ToString() : "empty")}");

                Console.WriteLine($"🔍 [AuthController] Login() - Llamando a _authService.LoginAsync()");
                var user = await _authService.LoginAsync(email, password);
                
                if (user == null)
                {
                    Console.WriteLine($"❌ [AuthController] Login() - Usuario no encontrado o credenciales incorrectas");
                    ModelState.AddModelError("", "Email o contraseña incorrectos");
                    ViewData["ReturnUrl"] = returnUrl;
                    return View();
                }

                Console.WriteLine($"✅ [AuthController] Login() - Usuario encontrado: {user.Email}, Rol: {user.Role}, Activo: {user.IsActive}");

                // Crear claims y autenticar
                Console.WriteLine($"🔍 [AuthController] Login() - Creando claims principal");
                var claimsPrincipal = await _authService.GetClaimsPrincipalAsync(user);
                
                Console.WriteLine($"🔍 [AuthController] Login() - Firmando cookie de autenticación");
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                Console.WriteLine($"✅ [AuthController] Login() - Login exitoso para: {email}, Rol: {user.Role}");
                _logger.LogInformation($"[AuthController] Login exitoso para: {email}, Rol: {user.Role}");

                // Redirigir según el rol
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    Console.WriteLine($"🔍 [AuthController] Login() - Redirigiendo a returnUrl: {returnUrl}");
                    return Redirect(returnUrl);
                }

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

                // Generar token de recuperación
                var resetToken = GenerateResetToken();
                
                // Guardar token temporalmente (en producción usar una tabla de tokens o cache)
                // Por ahora, guardar en una propiedad del usuario temporalmente
                user.PasswordHash = $"RESET_TOKEN:{resetToken}:{DateTime.UtcNow.AddMinutes(30):O}"; // Temporal - usar tabla dedicada
                await _context.SaveChangesAsync();

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

                // Verificar token (simplificado - en producción usar tabla dedicada)
                if (!user.PasswordHash!.StartsWith("RESET_TOKEN:") || !user.PasswordHash.Contains(token))
                {
                    TempData["Error"] = "Token inválido o expirado";
                    return RedirectToAction("ForgotPassword");
                }

                // Extraer fecha de expiración
                var parts = user.PasswordHash.Split(':');
                if (parts.Length >= 4 && DateTime.TryParse(parts[3], out var expirationDate))
                {
                    if (DateTime.UtcNow > expirationDate)
                    {
                        TempData["Error"] = "El token ha expirado. Por favor, solicita uno nuevo.";
                        return RedirectToAction("ForgotPassword");
                    }
                }

                // Actualizar contraseña usando el mismo método que AuthService
                var passwordHash = HashPassword(newPassword);
                user.PasswordHash = passwordHash;
                await _context.SaveChangesAsync();

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

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

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
                UserRole.chef => "StationOrders",
                UserRole.bartender => "StationOrders",

                UserRole.accountant => "Home",
                UserRole.support => "Home",
                _ => "Home"
            };
        }
    }
} 