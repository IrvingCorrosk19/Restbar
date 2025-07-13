using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Cryptography;
using System.Text;

namespace RestBar.Controllers
{
    [Authorize(Roles = "superadmin")]
    public class SuperAdminController : Controller
    {
        private readonly RestBarContext _context;
        private readonly ILogger<SuperAdminController> _logger;

        public SuperAdminController(RestBarContext context, ILogger<SuperAdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: SuperAdmin
        public async Task<IActionResult> Index()
        {
            try
            {
                Console.WriteLine("🔍 [SuperAdminController] Index() - Iniciando dashboard...");

                var companies = await _context.Companies
                    .Include(c => c.Branches)
                    .ToListAsync();

                var admins = await _context.Users
                    .Where(u => u.Role == UserRole.admin) // Solo mostrar admins, no superadmins
                    .Include(u => u.Branch)
                    .ThenInclude(b => b.Company)
                    .ToListAsync();

                var stats = new
                {
                    TotalCompanies = companies.Count,
                    TotalBranches = companies.Sum(c => c.Branches.Count),
                    TotalAdmins = admins.Count,
                    ActiveCompanies = companies.Count(c => c.IsActive),
                    ActiveBranches = companies.Sum(c => c.Branches.Count(b => b.IsActive))
                };

                Console.WriteLine($"✅ [SuperAdminController] Index() - Stats: {stats.TotalCompanies} compañías, {stats.TotalBranches} sucursales, {stats.TotalAdmins} admins");

                ViewBag.Stats = stats;
                ViewBag.Companies = companies;
                ViewBag.Admins = admins;

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [SuperAdminController] Index() - Error: {ex.Message}");
                _logger.LogError(ex, "[SuperAdminController] Error en Index");
                return View("Error");
            }
        }

        // GET: SuperAdmin/Companies
        public async Task<IActionResult> Companies()
        {
            try
            {
                Console.WriteLine("🔍 [SuperAdminController] Companies() - Cargando lista de compañías...");

                var companies = await _context.Companies
                    .Include(c => c.Branches)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                Console.WriteLine($"✅ [SuperAdminController] Companies() - {companies.Count} compañías encontradas");

                return View(companies);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [SuperAdminController] Companies() - Error: {ex.Message}");
                _logger.LogError(ex, "[SuperAdminController] Error en Companies");
                return View("Error");
            }
        }

        // GET: SuperAdmin/CreateCompany
        public IActionResult CreateCompany()
        {
            return View();
        }

        // POST: SuperAdmin/CreateCompany
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCompany(Company company)
        {
            try
            {
                Console.WriteLine($"🔍 [SuperAdminController] CreateCompany() - Creando compañía: {company.Name}");

                if (ModelState.IsValid)
                {
                    company.IsActive = true;
                    _context.Companies.Add(company);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"✅ [SuperAdminController] CreateCompany() - Compañía creada: {company.Name} (ID: {company.Id})");
                    TempData["SuccessMessage"] = $"Compañía '{company.Name}' creada exitosamente";
                    return RedirectToAction(nameof(Companies));
                }

                Console.WriteLine($"⚠️ [SuperAdminController] CreateCompany() - ModelState inválido");
                return View(company);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [SuperAdminController] CreateCompany() - Error: {ex.Message}");
                _logger.LogError(ex, "[SuperAdminController] Error en CreateCompany");
                ModelState.AddModelError("", "Error al crear la compañía");
                return View(company);
            }
        }

        // GET: SuperAdmin/Branches
        public async Task<IActionResult> Branches()
        {
            try
            {
                Console.WriteLine("🔍 [SuperAdminController] Branches() - Cargando lista de sucursales...");

                var branches = await _context.Branches
                    .Include(b => b.Company)
                    .OrderBy(b => b.Company.Name)
                    .ThenBy(b => b.Name)
                    .ToListAsync();

                Console.WriteLine($"✅ [SuperAdminController] Branches() - {branches.Count} sucursales encontradas");

                return View(branches);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [SuperAdminController] Branches() - Error: {ex.Message}");
                _logger.LogError(ex, "[SuperAdminController] Error en Branches");
                return View("Error");
            }
        }

        // GET: SuperAdmin/CreateBranch
        public async Task<IActionResult> CreateBranch()
        {
            try
            {
                Console.WriteLine("🔍 [SuperAdminController] CreateBranch() - Cargando vista de creación...");

                var companies = await _context.Companies
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                ViewBag.Companies = companies;
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [SuperAdminController] CreateBranch() - Error: {ex.Message}");
                _logger.LogError(ex, "[SuperAdminController] Error en CreateBranch GET");
                return View("Error");
            }
        }

        // POST: SuperAdmin/CreateBranch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBranch(Branch branch)
        {
            try
            {
                Console.WriteLine($"🔍 [SuperAdminController] CreateBranch() - Creando sucursal: {branch.Name} para compañía: {branch.CompanyId}");

                if (ModelState.IsValid)
                {
                    branch.IsActive = true;
                    _context.Branches.Add(branch);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"✅ [SuperAdminController] CreateBranch() - Sucursal creada: {branch.Name} (ID: {branch.Id})");
                    TempData["SuccessMessage"] = $"Sucursal '{branch.Name}' creada exitosamente";
                    return RedirectToAction(nameof(Branches));
                }

                Console.WriteLine($"⚠️ [SuperAdminController] CreateBranch() - ModelState inválido");

                var companies = await _context.Companies
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                ViewBag.Companies = companies;
                return View(branch);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [SuperAdminController] CreateBranch() - Error: {ex.Message}");
                _logger.LogError(ex, "[SuperAdminController] Error en CreateBranch POST");
                ModelState.AddModelError("", "Error al crear la sucursal");

                var companies = await _context.Companies
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                ViewBag.Companies = companies;
                return View(branch);
            }
        }

        // GET: SuperAdmin/CreateAdmin
        public async Task<IActionResult> CreateAdmin()
        {
            try
            {
                Console.WriteLine("🔍 [SuperAdminController] CreateAdmin() - Cargando vista de creación...");

                var companies = await _context.Companies
                    .Include(c => c.Branches)
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                ViewBag.Companies = companies;
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [SuperAdminController] CreateAdmin() - Error: {ex.Message}");
                _logger.LogError(ex, "[SuperAdminController] Error en CreateAdmin GET");
                return View("Error");
            }
        }

        // POST: SuperAdmin/CreateAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(User user, string password, Guid companyId)
        {
            try
            {
                Console.WriteLine($"🔍 [SuperAdminController] CreateAdmin() - Creando admin: {user.Email} para sucursal: {user.BranchId}");
                Console.WriteLine($"📋 [SuperAdminController] CreateAdmin() - Password recibido: {(string.IsNullOrEmpty(password) ? "VACÍO" : "PRESENTE")}");
                Console.WriteLine($"📋 [SuperAdminController] CreateAdmin() - CompanyId recibido: {companyId}");

                // Excluir PasswordHash de la validación del modelo
                ModelState.Remove("PasswordHash");
                
                Console.WriteLine($"🔍 [SuperAdminController] CreateAdmin() - ModelState.IsValid: {ModelState.IsValid}");
                if (!ModelState.IsValid)
                {
                    Console.WriteLine($"⚠️ [SuperAdminController] CreateAdmin() - ModelState errors:");
                    foreach (var error in ModelState)
                    {
                        Console.WriteLine($"   - {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                }
                
                if (ModelState.IsValid && !string.IsNullOrEmpty(password))
                {
                    // Verificar que el email no exista
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == user.Email);

                    if (existingUser != null)
                    {
                        Console.WriteLine($"⚠️ [SuperAdminController] CreateAdmin() - Email ya existe: {user.Email}");
                        ModelState.AddModelError("Email", "Ya existe un usuario con este email");
                        var companies = await _context.Companies
                            .Include(c => c.Branches)
                            .Where(c => c.IsActive)
                            .OrderBy(c => c.Name)
                            .ToListAsync();
                        ViewBag.Companies = companies;
                        return View(user);
                    }

                    // Verificar que la sucursal pertenezca a la compañía seleccionada
                    var branch = await _context.Branches
                        .FirstOrDefaultAsync(b => b.Id == user.BranchId && b.CompanyId == companyId);

                    if (branch == null)
                    {
                        Console.WriteLine($"⚠️ [SuperAdminController] CreateAdmin() - Sucursal no válida: {user.BranchId} para compañía: {companyId}");
                        ModelState.AddModelError("BranchId", "La sucursal seleccionada no pertenece a la compañía");
                        var companies = await _context.Companies
                            .Include(c => c.Branches)
                            .Where(c => c.IsActive)
                            .OrderBy(c => c.Name)
                            .ToListAsync();
                        ViewBag.Companies = companies;
                        return View(user);
                    }

                    user.Role = UserRole.admin;
                    user.IsActive = true;
                    user.PasswordHash = HashPassword(password);

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"✅ [SuperAdminController] CreateAdmin() - Admin creado: {user.Email} (ID: {user.Id}) para sucursal: {branch.Name}");
                    TempData["SuccessMessage"] = $"Administrador '{user.Email}' creado exitosamente para la sucursal '{branch.Name}'";
                    return RedirectToAction(nameof(Index));
                }

                Console.WriteLine($"⚠️ [SuperAdminController] CreateAdmin() - ModelState inválido o password vacío");
                Console.WriteLine($"🔍 [SuperAdminController] CreateAdmin() - ModelState errors: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");

                var companiesList = await _context.Companies
                    .Include(c => c.Branches)
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                ViewBag.Companies = companiesList;
                return View(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [SuperAdminController] CreateAdmin() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [SuperAdminController] CreateAdmin() - StackTrace: {ex.StackTrace}");
                _logger.LogError(ex, "[SuperAdminController] Error en CreateAdmin POST");
                ModelState.AddModelError("", "Error al crear el administrador");

                var companies = await _context.Companies
                    .Include(c => c.Branches)
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                ViewBag.Companies = companies;
                return View(user);
            }
        }

        // GET: SuperAdmin/GetBranchesByCompany
        [HttpGet]
        public async Task<IActionResult> GetBranchesByCompany(Guid companyId)
        {
            try
            {
                Console.WriteLine($"🔍 [SuperAdminController] GetBranchesByCompany() - Obteniendo sucursales para compañía: {companyId}");

                var branches = await _context.Branches
                    .Where(b => b.CompanyId == companyId && b.IsActive)
                    .Select(b => new { b.Id, b.Name })
                    .ToListAsync();

                Console.WriteLine($"✅ [SuperAdminController] GetBranchesByCompany() - {branches.Count} sucursales encontradas");

                return Json(branches);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [SuperAdminController] GetBranchesByCompany() - Error: {ex.Message}");
                _logger.LogError(ex, "[SuperAdminController] Error en GetBranchesByCompany");
                return Json(new List<object>());
            }
        }

        // GET: SuperAdmin/EditCompany
        public async Task<IActionResult> EditCompany(Guid id)
        {
            try
            {
                Console.WriteLine($"🔍 [SuperAdminController] EditCompany() - Editando compañía: {id}");

                var company = await _context.Companies.FindAsync(id);
                if (company == null)
                {
                    Console.WriteLine($"❌ [SuperAdminController] EditCompany() - Compañía no encontrada: {id}");
                    TempData["ErrorMessage"] = "Compañía no encontrada";
                    return RedirectToAction(nameof(Companies));
                }

                Console.WriteLine($"✅ [SuperAdminController] EditCompany() - Compañía encontrada: {company.Name}");
                return View(company);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [SuperAdminController] EditCompany() - Error: {ex.Message}");
                _logger.LogError(ex, "[SuperAdminController] Error en EditCompany GET");
                TempData["ErrorMessage"] = "Error al cargar la compañía";
                return RedirectToAction(nameof(Companies));
            }
        }

        // POST: SuperAdmin/EditCompany
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCompany(Guid id, Company company)
        {
            try
            {
                Console.WriteLine($"🔍 [SuperAdminController] EditCompany() - Actualizando compañía: {id}");

                if (id != company.Id)
                {
                    Console.WriteLine($"❌ [SuperAdminController] EditCompany() - ID no coincide");
                    TempData["ErrorMessage"] = "ID de compañía no válido";
                    return RedirectToAction(nameof(Companies));
                }

                if (ModelState.IsValid)
                {
                    _context.Update(company);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"✅ [SuperAdminController] EditCompany() - Compañía actualizada: {company.Name}");
                    TempData["SuccessMessage"] = $"Compañía '{company.Name}' actualizada exitosamente";
                    return RedirectToAction(nameof(Companies));
                }

                Console.WriteLine($"⚠️ [SuperAdminController] EditCompany() - ModelState inválido");
                return View(company);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [SuperAdminController] EditCompany() - Error: {ex.Message}");
                _logger.LogError(ex, "[SuperAdminController] Error en EditCompany POST");
                TempData["ErrorMessage"] = "Error al actualizar la compañía";
                return View(company);
            }
        }

        // GET: SuperAdmin/EditBranch
        public async Task<IActionResult> EditBranch(Guid id)
        {
            try
            {
                Console.WriteLine($"🔍 [SuperAdminController] EditBranch() - Editando sucursal: {id}");

                var branch = await _context.Branches
                    .Include(b => b.Company)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (branch == null)
                {
                    Console.WriteLine($"❌ [SuperAdminController] EditBranch() - Sucursal no encontrada: {id}");
                    TempData["ErrorMessage"] = "Sucursal no encontrada";
                    return RedirectToAction(nameof(Branches));
                }

                var companies = await _context.Companies
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                ViewBag.Companies = companies;
                Console.WriteLine($"✅ [SuperAdminController] EditBranch() - Sucursal encontrada: {branch.Name}");
                return View(branch);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [SuperAdminController] EditBranch() - Error: {ex.Message}");
                _logger.LogError(ex, "[SuperAdminController] Error en EditBranch GET");
                TempData["ErrorMessage"] = "Error al cargar la sucursal";
                return RedirectToAction(nameof(Branches));
            }
        }

        // POST: SuperAdmin/EditBranch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBranch(Guid id, Branch branch)
        {
            try
            {
                Console.WriteLine($"🔍 [SuperAdminController] EditBranch() - Actualizando sucursal: {id}");

                if (id != branch.Id)
                {
                    Console.WriteLine($"❌ [SuperAdminController] EditBranch() - ID no coincide");
                    TempData["ErrorMessage"] = "ID de sucursal no válido";
                    return RedirectToAction(nameof(Branches));
                }

                if (ModelState.IsValid)
                {
                    _context.Update(branch);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"✅ [SuperAdminController] EditBranch() - Sucursal actualizada: {branch.Name}");
                    TempData["SuccessMessage"] = $"Sucursal '{branch.Name}' actualizada exitosamente";
                    return RedirectToAction(nameof(Branches));
                }

                Console.WriteLine($"⚠️ [SuperAdminController] EditBranch() - ModelState inválido");

                var companies = await _context.Companies
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                ViewBag.Companies = companies;
                return View(branch);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [SuperAdminController] EditBranch() - Error: {ex.Message}");
                _logger.LogError(ex, "[SuperAdminController] Error en EditBranch POST");
                TempData["ErrorMessage"] = "Error al actualizar la sucursal";

                var companies = await _context.Companies
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                ViewBag.Companies = companies;
                return View(branch);
            }
        }

        // POST: SuperAdmin/ToggleCompany
        [HttpPost]
        public async Task<IActionResult> ToggleCompany(Guid id, bool isActive)
        {
            try
            {
                Console.WriteLine($"🔍 [SuperAdminController] ToggleCompany() - Cambiando estado de compañía: {id} a {(isActive ? "activa" : "inactiva")}");

                var company = await _context.Companies.FindAsync(id);
                if (company == null)
                {
                    Console.WriteLine($"❌ [SuperAdminController] ToggleCompany() - Compañía no encontrada: {id}");
                    return Json(new { success = false, message = "Compañía no encontrada" });
                }

                company.IsActive = isActive;
                _context.Update(company);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ [SuperAdminController] ToggleCompany() - Estado de compañía actualizado: {company.Name}");
                return Json(new { success = true, message = $"Compañía {(isActive ? "activada" : "desactivada")} exitosamente" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [SuperAdminController] ToggleCompany() - Error: {ex.Message}");
                _logger.LogError(ex, "[SuperAdminController] Error en ToggleCompany");
                return Json(new { success = false, message = "Error al cambiar el estado de la compañía" });
            }
        }

        // POST: SuperAdmin/ToggleBranch
        [HttpPost]
        public async Task<IActionResult> ToggleBranch(Guid id, bool isActive)
        {
            try
            {
                Console.WriteLine($"🔍 [SuperAdminController] ToggleBranch() - Cambiando estado de sucursal: {id} a {(isActive ? "activa" : "inactiva")}");

                var branch = await _context.Branches.FindAsync(id);
                if (branch == null)
                {
                    Console.WriteLine($"❌ [SuperAdminController] ToggleBranch() - Sucursal no encontrada: {id}");
                    return Json(new { success = false, message = "Sucursal no encontrada" });
                }

                branch.IsActive = isActive;
                _context.Update(branch);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ [SuperAdminController] ToggleBranch() - Estado de sucursal actualizado: {branch.Name}");
                return Json(new { success = true, message = $"Sucursal {(isActive ? "activada" : "desactivada")} exitosamente" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [SuperAdminController] ToggleBranch() - Error: {ex.Message}");
                _logger.LogError(ex, "[SuperAdminController] Error en ToggleBranch");
                return Json(new { success = false, message = "Error al cambiar el estado de la sucursal" });
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
