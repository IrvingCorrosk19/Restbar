using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
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
                _logger.LogInformation($"[AuthController] Intento de login para: {email}");

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    ModelState.AddModelError("", "Email y contraseña son requeridos");
                    ViewData["ReturnUrl"] = returnUrl;
                    return View();
                }

                var user = await _authService.LoginAsync(email, password);
                if (user == null)
                {
                    ModelState.AddModelError("", "Email o contraseña incorrectos");
                    ViewData["ReturnUrl"] = returnUrl;
                    return View();
                }

                // Crear claims y autenticar
                var claimsPrincipal = await _authService.GetClaimsPrincipalAsync(user);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                _logger.LogInformation($"[AuthController] Login exitoso para: {email}, Rol: {user.Role}");

                // Redirigir según el rol
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", GetControllerByRole(user.Role));
            }
            catch (Exception ex)
            {
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

        private string GetControllerByRole(UserRole role)
        {
            return role switch
            {
                UserRole.admin => "Home",
                UserRole.manager => "Home", 
                UserRole.supervisor => "Order",
                UserRole.waiter => "Order",
                UserRole.cashier => "Order",
                UserRole.chef => "StationOrders",
                UserRole.bartender => "StationOrders",
                UserRole.inventory => "Product",
                UserRole.accountant => "Home",
                UserRole.support => "Home",
                _ => "Home"
            };
        }
    }
} 