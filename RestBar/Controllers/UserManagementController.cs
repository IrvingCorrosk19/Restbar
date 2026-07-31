using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestBar.Models;
using System.Security.Cryptography;
using System.Text;

namespace RestBar.Controllers
{
    [Authorize(Roles = "admin,superadmin")]
    public class UserManagementController : Controller
    {
        private readonly RestBarContext _context;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(RestBarContext context, ILogger<UserManagementController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: UserManagement
        public async Task<IActionResult> Index()
        {
            try
            {
                Console.WriteLine("🔍 [UserManagementController] Index() - Cargando lista de usuarios...");

                var currentUserRole = User.FindFirst("UserRole")?.Value;
                var currentUserBranchId = User.FindFirst("BranchId")?.Value;

                Console.WriteLine($"🔍 [UserManagementController] Index() - Usuario actual: Rol={currentUserRole}, BranchId={currentUserBranchId}");

                var usersQuery = _context.Users
                    .Include(u => u.Branch)
                    .ThenInclude(b => b.Company)
                    .AsQueryable();

                // Si es admin (no superadmin), solo mostrar usuarios de su sucursal
                if (currentUserRole == "admin" && !string.IsNullOrEmpty(currentUserBranchId))
                {
                    if (Guid.TryParse(currentUserBranchId, out var branchId))
                    {
                        usersQuery = usersQuery.Where(u => u.BranchId == branchId);
                        Console.WriteLine($"🔍 [UserManagementController] Index() - Filtrando por sucursal: {branchId}");
                    }
                }

                var users = await usersQuery
                    .Where(u => u.Role != UserRole.superadmin) // Nunca mostrar superadmins
                    .OrderBy(u => u.FullName)
                    .ToListAsync();

                Console.WriteLine($"✅ [UserManagementController] Index() - {users.Count} usuarios encontrados");

                return View(users);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [UserManagementController] Index() - Error: {ex.Message}");
                _logger.LogError(ex, "[UserManagementController] Error en Index");
                return View("Error");
            }
        }

        // GET: UserManagement/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                Console.WriteLine("🔍 [UserManagementController] Create() - Cargando vista de creación...");

                var currentUserRole = User.FindFirst("UserRole")?.Value;
                var currentUserBranchId = User.FindFirst("BranchId")?.Value;

                // Si es admin, solo puede crear usuarios para su sucursal
                if (currentUserRole == "admin" && !string.IsNullOrEmpty(currentUserBranchId))
                {
                    if (Guid.TryParse(currentUserBranchId, out var branchId))
                    {
                        var branch = await _context.Branches
                            .Include(b => b.Company)
                            .FirstOrDefaultAsync(b => b.Id == branchId);

                        if (branch != null)
                        {
                            ViewBag.BranchId = branchId;
                            ViewBag.BranchName = branch.Name;
                            ViewBag.CompanyName = branch.Company?.Name;
                        }
                    }
                }
                else if (currentUserRole == "superadmin")
                {
                    // SuperAdmin puede crear usuarios para cualquier sucursal
                    var branches = await _context.Branches
                        .Include(b => b.Company)
                        .Where(b => b.IsActive)
                        .OrderBy(b => b.Company.Name)
                        .ThenBy(b => b.Name)
                        .ToListAsync();

                    ViewBag.Branches = branches;
                }

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [UserManagementController] Create() - Error: {ex.Message}");
                _logger.LogError(ex, "[UserManagementController] Error en Create GET");
                return View("Error");
            }
        }

        // POST: UserManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, string password)
        {
            try
            {
                Console.WriteLine($"🔍 [UserManagementController] Create() - Creando usuario: {user.Email}, Rol: {user.Role}");

                var currentUserRole = User.FindFirst("UserRole")?.Value;
                var currentUserBranchId = User.FindFirst("BranchId")?.Value;

                // Validaciones de seguridad
                if (currentUserRole == "admin")
                {
                    // Admin no puede crear otros admins ni superadmins
                    if (user.Role == UserRole.admin || user.Role == UserRole.superadmin)
                    {
                        ModelState.AddModelError("Role", "No tienes permisos para crear usuarios con este rol");
                        return View(user);
                    }

                    // Admin solo puede crear usuarios para su sucursal
                    if (!string.IsNullOrEmpty(currentUserBranchId) && Guid.TryParse(currentUserBranchId, out var branchId))
                    {
                        user.BranchId = branchId;
                    }
                }

                if (ModelState.IsValid && !string.IsNullOrEmpty(password))
                {
                    // Verificar que el email no exista
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == user.Email);

                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Email", "Ya existe un usuario con este email");
                        return View(user);
                    }

                    user.IsActive = true;
                    user.PasswordHash = HashPassword(password);

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"✅ [UserManagementController] Create() - Usuario creado: {user.Email} (ID: {user.Id})");
                    TempData["SuccessMessage"] = $"Usuario '{user.Email}' creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                Console.WriteLine($"⚠️ [UserManagementController] Create() - ModelState inválido o password vacío");

                // Recargar datos para la vista
                if (currentUserRole == "superadmin")
                {
                    var branches = await _context.Branches
                        .Include(b => b.Company)
                        .Where(b => b.IsActive)
                        .OrderBy(b => b.Company.Name)
                        .ThenBy(b => b.Name)
                        .ToListAsync();

                    ViewBag.Branches = branches;
                }
                else if (currentUserRole == "admin" && !string.IsNullOrEmpty(currentUserBranchId))
                {
                    if (Guid.TryParse(currentUserBranchId, out var branchId))
                    {
                        var branch = await _context.Branches
                            .Include(b => b.Company)
                            .FirstOrDefaultAsync(b => b.Id == branchId);

                        if (branch != null)
                        {
                            ViewBag.BranchId = branchId;
                            ViewBag.BranchName = branch.Name;
                            ViewBag.CompanyName = branch.Company?.Name;
                        }
                    }
                }

                return View(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [UserManagementController] Create() - Error: {ex.Message}");
                _logger.LogError(ex, "[UserManagementController] Error en Create POST");
                ModelState.AddModelError("", "Error al crear el usuario");

                // Recargar datos para la vista
                var currentUserRole = User.FindFirst("UserRole")?.Value;
                if (currentUserRole == "superadmin")
                {
                    var branches = await _context.Branches
                        .Include(b => b.Company)
                        .Where(b => b.IsActive)
                        .OrderBy(b => b.Company.Name)
                        .ThenBy(b => b.Name)
                        .ToListAsync();

                    ViewBag.Branches = branches;
                }

                return View(user);
            }
        }

        // GET: UserManagement/GetBranchesByCompany
        [HttpGet]
        public async Task<IActionResult> GetBranchesByCompany(Guid companyId)
        {
            try
            {
                Console.WriteLine($"🔍 [UserManagementController] GetBranchesByCompany() - Obteniendo sucursales para compañía: {companyId}");

                var branches = await _context.Branches
                    .Where(b => b.CompanyId == companyId && b.IsActive)
                    .Select(b => new { b.Id, b.Name })
                    .ToListAsync();

                Console.WriteLine($"✅ [UserManagementController] GetBranchesByCompany() - {branches.Count} sucursales encontradas");

                return Json(branches);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [UserManagementController] GetBranchesByCompany() - Error: {ex.Message}");
                _logger.LogError(ex, "[UserManagementController] Error en GetBranchesByCompany");
                return Json(new List<object>());
            }
        }

        public class ToggleUserDto
        {
            public Guid Id { get; set; }
            public bool IsActive { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUser([FromBody] ToggleUserDto dto)
        {
            try
            {
                var user = await _context.Users.FindAsync(dto.Id);
                if (user == null || user.Role == UserRole.superadmin)
                    return Json(new { success = false, message = "Usuario no encontrado" });

                var currentUserRole = User.FindFirst("UserRole")?.Value;
                var currentUserBranchId = User.FindFirst("BranchId")?.Value;
                if (currentUserRole == "admin" && Guid.TryParse(currentUserBranchId, out var branchId)
                    && user.BranchId != branchId)
                    return Json(new { success = false, message = "No autorizado" });

                user.IsActive = dto.IsActive;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = $"Usuario {(dto.IsActive ? "activado" : "desactivado")}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserManagementController] ToggleUser");
                return Json(new { success = false, message = "Error al cambiar estado" });
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
