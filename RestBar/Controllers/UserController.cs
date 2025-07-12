using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IBranchService _branchService;
        private readonly ICompanyService _companyService;
        private readonly IAuthService _authService;

        public UserController(IUserService userService, IBranchService branchService, ICompanyService companyService, IAuthService authService)
        {
            _userService = userService;
            _branchService = branchService;
            _companyService = companyService;
            _authService = authService;
        }

        private async Task<bool> HasUserManagementPermissionAsync()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync(User);
                if (currentUser == null) return false;
                
                return currentUser.Role == UserRole.admin || 
                       currentUser.Role == UserRole.manager || 
                       currentUser.Role == UserRole.support;
            }
            catch
            {
                return false;
            }
        }

        private async Task<IActionResult> CheckUserManagementPermissionAsync()
        {
            if (!await HasUserManagementPermissionAsync())
            {
                return Json(new { success = false, message = "No tienes permisos para gestionar usuarios" });
            }
            return null;
        }

        [Authorize(Policy = "UserManagement")]
        public IActionResult Index()
        {
            return View();
        }

        // Nueva vista para gestión completa de usuarios y roles
        [Authorize(Policy = "UserManagement")]
        public IActionResult UserManagement()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(string searchTerm, string role, Guid? branchId, bool? isActive)
        {
            var permissionCheck = await CheckUserManagementPermissionAsync();
            if (permissionCheck != null) return permissionCheck;

            try
            {
                var users = await _userService.GetAllAsync();

                // Aplicar filtros
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    users = users.Where(u =>
                        (u.FullName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                        (u.Email?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true));
                }

                if (!string.IsNullOrEmpty(role) && Enum.TryParse<UserRole>(role, true, out var userRole))
                {
                    users = users.Where(u => u.Role == userRole);
                }

                if (branchId.HasValue)
                {
                    users = users.Where(u => u.BranchId == branchId);
                }

                if (isActive.HasValue)
                {
                    users = users.Where(u => u.IsActive == isActive);
                }

                // Obtener todas las sucursales para el mapeo
                var branches = await _branchService.GetAllAsync();

                // Mapear los datos para evitar problemas de serialización
                var userData = users.Select(u => {
                    var branch = branches.FirstOrDefault(b => b.Id == u.BranchId);
                    return new {
                        id = u.Id,
                        fullName = u.FullName,
                        email = u.Email,
                        role = u.Role.ToString().ToLower(),
                        branchId = u.BranchId,
                        branchName = branch?.Name ?? "Sin sucursal",
                        companyId = branch?.CompanyId,
                        companyName = branch?.Company?.Name ?? "Sin compañía",
                        isActive = u.IsActive,
                        createdAt = u.CreatedAt
                    };
                }).ToList();

                return Json(new { success = true, data = userData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var permissionCheck = await CheckUserManagementPermissionAsync();
            if (permissionCheck != null) return permissionCheck;

            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                // Obtener la información de la sucursal
                var branch = user.BranchId.HasValue ? await _branchService.GetByIdAsync(user.BranchId.Value) : null;

                // Mapear los datos para evitar problemas de serialización
                var userData = new {
                    id = user.Id,
                    fullName = user.FullName,
                    email = user.Email,
                    role = user.Role.ToString().ToLower(),
                    branchId = user.BranchId,
                    branchName = branch?.Name ?? "Sin sucursal",
                    companyId = branch?.CompanyId,
                    companyName = branch?.Company?.Name ?? "Sin compañía",
                    isActive = user.IsActive,
                    createdAt = user.CreatedAt
                };

                return Json(new { success = true, data = userData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] User user, [FromForm] string password)
        {
            var permissionCheck = await CheckUserManagementPermissionAsync();
            if (permissionCheck != null) return permissionCheck;

            try
            {
                if (user == null)
                {
                    return Json(new { success = false, message = "Los datos del usuario no pueden estar vacíos" });
                }

                // Validar que el password no esté vacío
                if (string.IsNullOrEmpty(password))
                {
                    return Json(new { success = false, message = "La contraseña es requerida" });
                }

                // Validar longitud mínima del password
                if (password.Length < 6)
                {
                    return Json(new { success = false, message = "La contraseña debe tener al menos 6 caracteres" });
                }

                // Asignar el password al user
                user.PasswordHash = password;

                // Validar que el email no esté vacío
                if (string.IsNullOrEmpty(user.Email))
                {
                    return Json(new { success = false, message = "El email es requerido" });
                }

                // Validar que el email no esté duplicado
                var existingUser = await _userService.GetByEmailAsync(user.Email);
                if (existingUser != null)
                {
                    return Json(new { success = false, message = "El correo electrónico ya está registrado" });
                }

                // Validar que la sucursal exista
                if (user.BranchId.HasValue)
                {
                    var branch = await _branchService.GetByIdAsync(user.BranchId.Value);
                    if (branch == null)
                    {
                        return Json(new { success = false, message = "La sucursal especificada no existe" });
                    }
                }

                // Validar el rol
                if (!Enum.IsDefined(typeof(UserRole), user.Role))
                {
                    return Json(new { success = false, message = "El rol especificado no es válido" });
                }

                // Limpiar campos que no deben ser establecidos por el cliente
                user.Id = Guid.NewGuid();
                user.CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                user.IsActive = true;

                var createdUser = await _userService.CreateAsync(user);
                
                // Devolver solo los datos necesarios para evitar problemas de serialización
                return Json(new { 
                    success = true, 
                    data = new {
                        id = createdUser.Id,
                        fullName = createdUser.FullName,
                        email = createdUser.Email,
                        role = createdUser.Role,
                        branchId = createdUser.BranchId,
                        isActive = createdUser.IsActive,
                        createdAt = createdUser.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al crear el usuario: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromForm] User user, [FromForm] string password = "")
        {
            var permissionCheck = await CheckUserManagementPermissionAsync();
            if (permissionCheck != null) return permissionCheck;

            try
            {
                if (user == null)
                {
                    return Json(new { success = false, message = "Los datos del usuario no pueden estar vacíos" });
                }

                // Validar que el usuario exista
                var existingUser = await _userService.GetByIdAsync(user.Id);
                if (existingUser == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                // Si se proporciona password, validarlo y asignarlo
                if (!string.IsNullOrEmpty(password))
                {
                    if (password.Length < 6)
                    {
                        return Json(new { success = false, message = "La contraseña debe tener al menos 6 caracteres" });
                    }
                    user.PasswordHash = password;
                }
                else
                {
                    // Mantener el password existente si no se proporciona uno nuevo
                    user.PasswordHash = existingUser.PasswordHash;
                }

                // Validar que el email no esté duplicado (si se cambió)
                if (existingUser.Email != user.Email)
                {
                    var userWithEmail = await _userService.GetByEmailAsync(user.Email);
                    if (userWithEmail != null)
                    {
                        return Json(new { success = false, message = "El correo electrónico ya está registrado" });
                    }
                }

                // Validar que la sucursal exista
                if (user.BranchId.HasValue)
                {
                    var branch = await _branchService.GetByIdAsync(user.BranchId.Value);
                    if (branch == null)
                    {
                        return Json(new { success = false, message = "La sucursal especificada no existe" });
                    }
                }

                // Validar el rol
                if (!Enum.IsDefined(typeof(UserRole), user.Role))
                {
                    return Json(new { success = false, message = "El rol especificado no es válido" });
                }

                // Mantener campos que no deben ser modificados
                user.CreatedAt = existingUser.CreatedAt;

                await _userService.UpdateAsync(user);

                // Devolver los datos actualizados
                return Json(new { 
                    success = true, 
                    data = new {
                        id = user.Id,
                        fullName = user.FullName,
                        email = user.Email,
                        role = user.Role,
                        branchId = user.BranchId,
                        isActive = user.IsActive,
                        createdAt = user.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al actualizar el usuario: {ex.Message}" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            var permissionCheck = await CheckUserManagementPermissionAsync();
            if (permissionCheck != null) return permissionCheck;

            try
            {
                await _userService.DeleteAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            var permissionCheck = await CheckUserManagementPermissionAsync();
            if (permissionCheck != null) return permissionCheck;

            try
            {
                var companies = await _companyService.GetAllAsync();
                var data = companies.Select(c => new {
                    id = c.Id,
                    name = c.Name
                }).ToList();
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBranches()
        {
            var permissionCheck = await CheckUserManagementPermissionAsync();
            if (permissionCheck != null) return permissionCheck;

            try
            {
                var branches = await _branchService.GetAllAsync();
                var data = branches.Select(b => new {
                    id = b.Id,
                    name = b.Name,
                    companyId = b.CompanyId,
                    companyName = b.Company?.Name ?? "Sin compañía"
                }).ToList();
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSupervisors()
        {
            var permissionCheck = await CheckUserManagementPermissionAsync();
            if (permissionCheck != null) return permissionCheck;

            try
            {
                var supervisors = await _userService.GetByRoleAsync(UserRole.supervisor);
                var data = supervisors
                    .Where(s => s.IsActive == true)
                    .Select(s => new {
                        id = s.Id,
                        fullName = s.FullName,
                        email = s.Email
                    }).ToList();
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
} 