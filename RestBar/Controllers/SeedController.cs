using Microsoft.AspNetCore.Mvc;
using RestBar.Models;
using RestBar.Services;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

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

        // ✅ Semilla completa de datos de prueba (compañía, sucursal, áreas, mesas, estaciones, categorías, productos, usuario, orden demo)
        [HttpGet]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SeedDemoData()
        {
            try
            {
                Console.WriteLine("🔍 [SeedController] SeedDemoData() - Iniciando siembra de datos de prueba...");

                // Compañía
                var company = await _context.Companies.FirstOrDefaultAsync(c => c.Name == "RestBar Principal");
                if (company == null)
                {
                    company = new Company
                    {
                        Id = Guid.Parse("770e8400-e29b-41d4-a716-446655440001"),
                        Name = "RestBar Principal",
                        LegalId = "123456789",
                        IsActive = true,
                        CreatedBy = "Seeder"
                    };
                    _context.Companies.Add(company);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("✅ [SeedController] Compañía creada");
                }

                // Sucursal
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
                        CreatedBy = "Seeder"
                    };
                    _context.Branches.Add(branch);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("✅ [SeedController] Sucursal creada");
                }

                // Áreas
                var areaSalon = await _context.Areas.FirstOrDefaultAsync(a => a.Name == "Salón");
                if (areaSalon == null)
                {
                    areaSalon = new Area
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = company.Id,
                        BranchId = branch.Id,
                        Name = "Salón",
                        Description = "Área principal",
                        IsActive = true,
                        CreatedBy = "Seeder"
                    };
                    _context.Areas.Add(areaSalon);
                }

                var areaTerraza = await _context.Areas.FirstOrDefaultAsync(a => a.Name == "Terraza");
                if (areaTerraza == null)
                {
                    areaTerraza = new Area
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = company.Id,
                        BranchId = branch.Id,
                        Name = "Terraza",
                        Description = "Área exterior",
                        IsActive = true,
                        CreatedBy = "Seeder"
                    };
                    _context.Areas.Add(areaTerraza);
                }
                await _context.SaveChangesAsync();

                // Mesas básicas
                async Task<Table> EnsureTableAsync(string number, Area area)
                {
                    var table = await _context.Tables.FirstOrDefaultAsync(t => t.TableNumber == number);
                    if (table == null)
                    {
                        table = new Table
                        {
                            Id = Guid.NewGuid(),
                            CompanyId = company.Id,
                            BranchId = branch.Id,
                            AreaId = area.Id,
                            TableNumber = number,
                            Capacity = 4,
                            IsActive = true,
                            CreatedBy = "Seeder"
                        };
                        _context.Tables.Add(table);
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"✅ [SeedController] Mesa {number} creada");
                    }
                    return table;
                }

                var mesa1 = await EnsureTableAsync("T-01", areaSalon);
                var mesa2 = await EnsureTableAsync("T-02", areaSalon);
                var mesa3 = await EnsureTableAsync("T-03", areaTerraza);

                // Estación
                var station = await _context.Stations.FirstOrDefaultAsync(s => s.Name == "Cocina Principal");
                if (station == null)
                {
                    station = new Station
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = company.Id,
                        BranchId = branch.Id,
                        AreaId = areaSalon.Id,
                        Name = "Cocina Principal",
                        Type = "kitchen",
                        Icon = "🍽️",
                        IsActive = true,
                        CreatedBy = "Seeder"
                    };
                    _context.Stations.Add(station);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("✅ [SeedController] Estación creada");
                }

                // Categorías
                async Task<Category> EnsureCategoryAsync(string name)
                {
                    var cat = await _context.Categories.FirstOrDefaultAsync(c => c.Name == name);
                    if (cat == null)
                    {
                        cat = new Category
                        {
                            Id = Guid.NewGuid(),
                            CompanyId = company.Id,
                            BranchId = branch.Id,
                            Name = name,
                            Description = name,
                            IsActive = true,
                            CreatedBy = "Seeder"
                        };
                        _context.Categories.Add(cat);
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"✅ [SeedController] Categoría {name} creada");
                    }
                    return cat;
                }

                var catBebidas = await EnsureCategoryAsync("Bebidas");
                var catPlatos = await EnsureCategoryAsync("Platos");

                // Productos
                async Task EnsureProductAsync(string name, decimal price, Category category)
                {
                    var prod = await _context.Products.FirstOrDefaultAsync(p => p.Name == name);
                    if (prod == null)
                    {
                        prod = new Product
                        {
                            Id = Guid.NewGuid(),
                            CompanyId = company.Id,
                            BranchId = branch.Id,
                            CategoryId = category.Id,
                            StationId = station.Id,
                            Name = name,
                            Description = name,
                            Price = price,
                            TaxRate = 0.07m,
                            Unit = "unit",
                            IsActive = true,
                            CreatedBy = "Seeder"
                        };
                        _context.Products.Add(prod);
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"✅ [SeedController] Producto {name} creado");
                    }
                }

                await EnsureProductAsync("Café Americano", 2.50m, catBebidas);
                await EnsureProductAsync("Jugo de Naranja", 3.00m, catBebidas);
                await EnsureProductAsync("Hamburguesa Clásica", 8.99m, catPlatos);
                await EnsureProductAsync("Pasta Alfredo", 9.99m, catPlatos);

                // Estación de Bar
                var stationBar = await _context.Stations.FirstOrDefaultAsync(s => s.Name == "Bar Principal");
                if (stationBar == null)
                {
                    stationBar = new Station
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = company.Id,
                        BranchId = branch.Id,
                        AreaId = areaSalon.Id,
                        Name = "Bar Principal",
                        Type = "bar",
                        Icon = "🍹",
                        IsActive = true,
                        CreatedBy = "Seeder"
                    };
                    _context.Stations.Add(stationBar);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("✅ [SeedController] Estación de bar creada");
                }

                // Más categorías y productos
                var catPostres = await EnsureCategoryAsync("Postres");
                var catBebidasAlcoholicas = await EnsureCategoryAsync("Bebidas Alcohólicas");
                await EnsureProductAsync("Tiramisú", 6.50m, catPostres);
                await EnsureProductAsync("Brownie con Helado", 7.00m, catPostres);
                await EnsureProductAsync("Cerveza Nacional", 4.50m, catBebidasAlcoholicas);
                await EnsureProductAsync("Cóctel Mojito", 8.00m, catBebidasAlcoholicas);

                // Usuarios con diferentes roles
                async Task<User> EnsureUserAsync(string email, string fullName, UserRole role, string password = "123456")
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                    if (user == null)
                    {
                        var passwordHash = HashPassword(password);
                        user = new User
                        {
                            Id = Guid.NewGuid(),
                            BranchId = branch.Id,
                            FullName = fullName,
                            Email = email,
                            PasswordHash = passwordHash,
                            Role = role,
                            IsActive = true,
                            CreatedBy = "Seeder"
                        };
                        _context.Users.Add(user);
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"✅ [SeedController] Usuario {fullName} ({role}) creado");
                    }
                    return user;
                }

                var admin = await EnsureUserAsync("admin@restbar.com", "Administrador del Sistema", UserRole.admin);
                await EnsureUserAsync("gerente@restbar.com", "Gerente General", UserRole.manager);
                await EnsureUserAsync("supervisor@restbar.com", "Supervisor de Turno", UserRole.supervisor);
                await EnsureUserAsync("mesero@restbar.com", "Mesero Principal", UserRole.waiter);
                await EnsureUserAsync("cajero@restbar.com", "Cajero Principal", UserRole.cashier);
                await EnsureUserAsync("chef@restbar.com", "Chef Principal", UserRole.chef);
                await EnsureUserAsync("bartender@restbar.com", "Bartender Principal", UserRole.bartender);
                await EnsureUserAsync("contador@restbar.com", "Contador", UserRole.accountant);
                await EnsureUserAsync("soporte@restbar.com", "Soporte Técnico", UserRole.support);

                // Plantillas de Email
                async Task EnsureEmailTemplateAsync(string name, string subject, string body, string category, string placeholders)
                {
                    var template = await _context.EmailTemplates.FirstOrDefaultAsync(t => t.Name == name);
                    if (template == null)
                    {
                        template = new EmailTemplate
                        {
                            Id = Guid.NewGuid(),
                            CompanyId = company.Id,
                            Name = name,
                            Subject = subject,
                            Body = body,
                            Category = category,
                            Placeholders = placeholders,
                            IsActive = true,
                            Description = $"Template para {name}"
                        };
                        _context.EmailTemplates.Add(template);
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"✅ [SeedController] Template de email {name} creado");
                    }
                }

                var orderConfirmationBody = @"<html><body style='font-family: Arial, sans-serif;'>
                    <h2>Confirmación de Orden</h2>
                    <p>Estimado/a cliente,</p>
                    <p>Su orden ha sido confirmada exitosamente.</p>
                    <p><strong>Número de Orden:</strong> {{OrderNumber}}</p>
                    <p><strong>Fecha:</strong> {{OrderDate}}</p>
                    <p><strong>Total:</strong> {{TotalAmount}}</p>
                    <h3>Items:</h3>
                    <div>{{Items}}</div>
                    <p>Gracias por su preferencia.</p>
                </body></html>";

                var passwordRecoveryBody = @"<html><body style='font-family: Arial, sans-serif;'>
                    <h2>Recuperación de Contraseña</h2>
                    <p>Hola {{UserName}},</p>
                    <p>Has solicitado recuperar tu contraseña. Haz clic en el siguiente enlace:</p>
                    <p><a href='{{ResetLink}}'>Recuperar Contraseña</a></p>
                    <p>O copia y pega este token: {{ResetToken}}</p>
                    <p>Este enlace expirará en {{ExpirationMinutes}} minutos.</p>
                    <p>Si no solicitaste esto, ignora este email.</p>
                </body></html>";

                var welcomeBody = @"<html><body style='font-family: Arial, sans-serif;'>
                    <h2>Bienvenido a RestBar</h2>
                    <p>Hola {{UserName}},</p>
                    <p>Tu cuenta ha sido creada exitosamente.</p>
                    <p><strong>Email:</strong> {{Email}}</p>
                    <p><strong>Contraseña Temporal:</strong> {{TemporaryPassword}}</p>
                    <p>Por favor cambia tu contraseña después del primer inicio de sesión.</p>
                    <p><a href='{{LoginUrl}}'>Iniciar Sesión</a></p>
                    <p>Bienvenido a {{CompanyName}}!</p>
                </body></html>";

                await EnsureEmailTemplateAsync("OrderConfirmation", "Confirmación de Orden #{{OrderNumber}}", orderConfirmationBody, "Orders", "OrderNumber,OrderDate,TotalAmount,Items");
                await EnsureEmailTemplateAsync("PasswordRecovery", "Recuperación de Contraseña", passwordRecoveryBody, "Auth", "UserName,ResetLink,ResetToken,ExpirationMinutes");
                await EnsureEmailTemplateAsync("Welcome", "Bienvenido a RestBar", welcomeBody, "Auth", "UserName,Email,TemporaryPassword,LoginUrl,CompanyName");

                // Configuraciones del Sistema
                async Task EnsureSystemSettingAsync(string key, string value, string category, string description)
                {
                    var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == key && s.CompanyId == company.Id);
                    if (setting == null)
                    {
                        setting = new SystemSettings
                        {
                            Id = Guid.NewGuid(),
                            CompanyId = company.Id,
                            SettingKey = key,
                            SettingValue = value,
                            Category = category,
                            Description = description,
                            IsActive = true
                        };
                        _context.SystemSettings.Add(setting);
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"✅ [SeedController] Configuración {key} creada");
                    }
                }

                await EnsureSystemSettingAsync("AppName", "RestBar Sistema", "General", "Nombre de la aplicación");
                await EnsureSystemSettingAsync("Currency", "USD", "General", "Moneda principal");
                await EnsureSystemSettingAsync("TaxRate", "0.07", "General", "Tasa de impuesto por defecto");
                await EnsureSystemSettingAsync("AllowSplitPayments", "true", "Payments", "Permitir pagos divididos");
                await EnsureSystemSettingAsync("EmailNotifications", "true", "Notifications", "Activar notificaciones por email");

                // Monedas
                var currency = await _context.Currencies.FirstOrDefaultAsync(c => c.Code == "USD" && c.CompanyId == company.Id);
                if (currency == null)
                {
                    currency = new Currency
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = company.Id,
                        Code = "USD",
                        Name = "Dólar Estadounidense",
                        Symbol = "$",
                        ExchangeRate = 1.0m,
                        IsDefault = true,
                        IsActive = true
                    };
                    _context.Currencies.Add(currency);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("✅ [SeedController] Moneda USD creada");
                }

                // Horarios de Operación
                var daysOfWeek = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
                foreach (var day in daysOfWeek)
                {
                    var hours = await _context.OperatingHours.FirstOrDefaultAsync(h => h.DayOfWeek == day && h.CompanyId == company.Id);
                    if (hours == null)
                    {
                        hours = new OperatingHours
                        {
                            Id = Guid.NewGuid(),
                            CompanyId = company.Id,
                            DayOfWeek = day,
                            OpenTime = TimeSpan.FromHours(9),
                            CloseTime = TimeSpan.FromHours(22),
                            IsOpen = true,
                            Notes = $"Horario estándar para {day}"
                        };
                        _context.OperatingHours.Add(hours);
                    }
                }
                await _context.SaveChangesAsync();
                Console.WriteLine("✅ [SeedController] Horarios de operación creados");

                // Asignaciones de Usuarios
                var mesero = await _context.Users.FirstOrDefaultAsync(u => u.Email == "mesero@restbar.com");
                if (mesero != null)
                {
                    var assignment = await _context.UserAssignments.FirstOrDefaultAsync(a => a.UserId == mesero.Id);
                    if (assignment == null)
                    {
                        assignment = new UserAssignment
                        {
                            Id = Guid.NewGuid(),
                            UserId = mesero.Id,
                            AreaId = areaSalon.Id,
                            AssignedTableIds = new List<Guid> { mesa1.Id, mesa2.Id },
                            CompanyId = company.Id,
                            BranchId = branch.Id,
                            IsActive = true,
                            CreatedBy = "Seeder"
                        };
                        _context.UserAssignments.Add(assignment);
                        await _context.SaveChangesAsync();
                        Console.WriteLine("✅ [SeedController] Asignación de usuario creada");
                    }
                }

                // Más mesas
                for (int i = 4; i <= 10; i++)
                {
                    await EnsureTableAsync($"T-{i:00}", i % 2 == 0 ? areaSalon : areaTerraza);
                }

                // Cliente de prueba
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == "cliente@example.com");
                if (customer == null)
                {
                    customer = new Customer
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = company.Id,
                        BranchId = branch.Id,
                        FullName = "Cliente Demo",
                        Email = "cliente@example.com",
                        Phone = "+507 123-4567",
                        LoyaltyPoints = 0,
                        CreatedBy = "Seeder"
                    };
                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("✅ [SeedController] Cliente demo creado");
                }

                Console.WriteLine("✅ [SeedController] SeedDemoData() - Completado exitosamente");
                return Json(new { success = true, message = "Datos de prueba creados" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [SeedController] SeedDemoData() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [SeedController] SeedDemoData() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
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
        [AllowAnonymous]
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
                        // ✅ Fechas se manejan automáticamente por el modelo
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
                        // ✅ Fechas se manejan automáticamente por el modelo
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
                    // ✅ Fechas se manejan automáticamente por el modelo
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