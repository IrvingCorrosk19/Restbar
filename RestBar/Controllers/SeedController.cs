using Microsoft.AspNetCore.Mvc;
using RestBar.Models;
using RestBar.Services;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace RestBar.Controllers
{
    public class SeedController : Controller
    {
        private readonly RestBarContext _context;

        public SeedController(RestBarContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Método temporal para generar hash de contraseña
        [HttpGet]
        public IActionResult GeneratePasswordHash()
        {
            string password = "123456";
            
            // Generar hash usando SHA256 (alternativa a BCrypt)
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var hash = Convert.ToBase64String(hashedBytes);
                
                return Json(new { 
                    password = password, 
                    hash = hash,
                    bcrypt_hash = "$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi"
                });
            }
        }

        // Método para crear usuario administrador
        [HttpGet]
        public async Task<IActionResult> CreateAdminUser()
        {
            try
            {
                // Verificar si ya existe el usuario
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@restbar.com");
                if (existingUser != null)
                {
                    return Json(new { success = false, message = "El usuario administrador ya existe" });
                }

                // Crear compañía si no existe
                var company = await _context.Companies.FirstOrDefaultAsync(c => c.Name == "RestBar Principal");
                if (company == null)
                {
                    company = new Company
                    {
                        Id = Guid.Parse("770e8400-e29b-41d4-a716-446655440001"),
                        Name = "RestBar Principal",
                        LegalId = "123456789",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "Sistema"
                    };
                    _context.Companies.Add(company);
                    await _context.SaveChangesAsync();
                }

                // Crear sucursal si no existe
                var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Name == "RestBar Centro");
                if (branch == null)
                {
                    branch = new Branch
                    {
                        Id = Guid.Parse("660e8400-e29b-41d4-a716-446655440001"),
                        CompanyId = company.Id,
                        Name = "RestBar Centro",
                        Address = "Calle Principal #123",
                        Phone = "+507 123-4567",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "Sistema"
                    };
                    _context.Branches.Add(branch);
                    await _context.SaveChangesAsync();
                }

                // Crear usuario administrador
                var adminUser = new User
                {
                    Id = Guid.Parse("770e8400-e29b-41d4-a716-446655440001"),
                    BranchId = branch.Id,
                    FullName = "Administrador del Sistema",
                    Email = "admin@restbar.com",
                    PasswordHash = "$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi", // 123456 hasheada con BCrypt
                    Role = UserRole.admin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Sistema"
                };

                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Usuario administrador creado exitosamente",
                    user = new {
                        email = adminUser.Email,
                        password = "123456",
                        role = adminUser.Role.ToString()
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
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