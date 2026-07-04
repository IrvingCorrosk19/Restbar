using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using RestBar.Services;
using System.Threading.RateLimiting;
using RestBar.Hubs;
using RestBar.Middleware;
using RestBar.Helpers;
using Npgsql;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // Configurar el serializador JSON para evitar referencias circulares
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.MaxDepth = 64; // Aumentar la profundidad máxima
        options.JsonSerializerOptions.WriteIndented = true; // Para debugging
    });

// ✅ Configurar cultura y formato de fechas para Panamá (es-PA)
var panamaCulture = new CultureInfo("es-PA");
panamaCulture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
panamaCulture.DateTimeFormat.LongDatePattern = "dddd, d 'de' MMMM 'de' yyyy";
panamaCulture.DateTimeFormat.ShortTimePattern = "HH:mm";
panamaCulture.DateTimeFormat.LongTimePattern = "HH:mm:ss";
CultureInfo.DefaultThreadCurrentCulture = panamaCulture;
CultureInfo.DefaultThreadCurrentUICulture = panamaCulture;

// Agregar SignalR
builder.Services.AddSignalR();

// ✅ SEGURIDAD: Rate limiting para endpoints de autenticación
// Producción: 5 req/min. Desarrollo: límite alto para certificación/pruebas.
var isDevelopment = builder.Environment.IsDevelopment();
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth_endpoints", limiterOptions =>
    {
        limiterOptions.PermitLimit = isDevelopment ? 500 : 5;
        limiterOptions.Window = TimeSpan.FromSeconds(60);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });

    // Respuesta cuando se supera el límite
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = 429; // Too Many Requests
        context.HttpContext.Response.Headers.Append("Retry-After", "60");
        await context.HttpContext.Response.WriteAsync(
            "{\"success\":false,\"message\":\"Demasiados intentos. Espere 60 segundos.\"}",
            cancellationToken);
    };
});

// ✅ NUEVO: Configurar sesiones para el AuditLogService
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configurar autenticación por cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ReturnUrlParameter = "returnUrl";
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Sesión de 8 horas
        options.SlidingExpiration = true; // Renovar automáticamente
        options.Cookie.Name = "RestBarAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

// Configurar autorización con políticas personalizadas
builder.Services.AddAuthorization(options =>
{
    // Políticas por roles específicos
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
    options.AddPolicy("ManagerOrAbove", policy => policy.RequireRole("admin", "manager"));
    options.AddPolicy("SupervisorOrAbove", policy => policy.RequireRole("admin", "manager", "supervisor"));
    
    // Políticas para área de órdenes
    options.AddPolicy("OrderAccess", policy => policy.RequireRole("admin", "manager", "supervisor", "waiter", "cashier", "chef", "bartender"));
    
    // Políticas para área de cocina
    options.AddPolicy("KitchenAccess", policy => policy.RequireRole("admin", "manager", "supervisor", "chef", "bartender"));
    
    // Políticas para área de pagos
    options.AddPolicy("PaymentAccess", policy => policy.RequireRole("admin", "manager", "supervisor", "cashier", "accountant"));
    
    // Políticas para área de inventario — "inventarista" ahora existe en UserRole enum
    options.AddPolicy("InventoryAccess", policy => policy.RequireRole("admin", "manager", "supervisor", "accountant", "inventarista"));
    
    // Políticas para área de productos
    options.AddPolicy("ProductAccess", policy => policy.RequireRole("admin", "manager"));
    
    // Políticas para área de usuarios
    options.AddPolicy("UserManagement", policy => policy.RequireRole("admin", "manager", "support"));
    
    // Políticas para área de reportes
    options.AddPolicy("ReportAccess", policy => policy.RequireRole("admin", "manager", "accountant"));
    
    // Políticas para área de contabilidad
    options.AddPolicy("AccountingAccess", policy => policy.RequireRole("admin", "manager", "accountant"));
    
    // Políticas para área de configuración
    options.AddPolicy("SystemConfig", policy => policy.RequireRole("admin"));
});

// Agregar HttpContextAccessor para el AuthService y tracking automático
builder.Services.AddHttpContextAccessor();

// Habilitar Newtonsoft.Json para Npgsql 8+
AppContext.SetSwitch("Npgsql.EnableLegacyJsonNet", true);

// Habilitar serialización dinámica de JSON para Npgsql 8+
NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();

// Configurar el RestBarContext con HttpContextAccessor
builder.Services.AddDbContext<RestBarContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.MapEnum<UserRole>("user_role_enum"))
);

// Configurar el RestBarContext con HttpContextAccessor para tracking automático
builder.Services.AddScoped<RestBarContext>(provider =>
{
    var options = provider.GetRequiredService<DbContextOptions<RestBarContext>>();
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    
    var context = new RestBarContext(options);
    context.HttpContextAccessor = httpContextAccessor;
    
    return context;
});

// Registrar servicios
builder.Services.AddScoped<ICategoryService, CategoryService>();
// Registrar StationService con IHttpContextAccessor
builder.Services.AddScoped<IStationService>(provider =>
{
    var context = provider.GetRequiredService<RestBarContext>();
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    return new StationService(context, httpContextAccessor);
});
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductStockAssignmentService>(provider =>
{
    var context = provider.GetRequiredService<RestBarContext>();
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    var logger = provider.GetRequiredService<ILogger<ProductStockAssignmentService>>();
    return new ProductStockAssignmentService(context, httpContextAccessor, logger);
});
builder.Services.AddScoped<ITableService, TableService>();
// Registrar AreaService con IHttpContextAccessor
builder.Services.AddScoped<IAreaService>(provider =>
{
    var context = provider.GetRequiredService<RestBarContext>();
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    return new AreaService(context, httpContextAccessor);
});
builder.Services.AddScoped<ICompanyService, CompanyService>();

// Registrar BranchService con IHttpContextAccessor
builder.Services.AddScoped<IBranchService>(provider =>
{
    var context = provider.GetRequiredService<RestBarContext>();
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    return new BranchService(context, httpContextAccessor);
});

// Registrar UserService con IHttpContextAccessor
builder.Services.AddScoped<IUserService>(provider =>
{
    var context = provider.GetRequiredService<RestBarContext>();
    var loggingService = provider.GetRequiredService<IGlobalLoggingService>();
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    return new UserService(context, loggingService, httpContextAccessor);
});

builder.Services.AddScoped<IUserAssignmentService, UserAssignmentService>();
// Registrar CustomerService con IHttpContextAccessor
builder.Services.AddScoped<ICustomerService>(provider =>
{
    var context = provider.GetRequiredService<RestBarContext>();
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    return new CustomerService(context, httpContextAccessor);
});
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();
builder.Services.AddScoped<IKitchenService, KitchenService>();
// Registrar PaymentService con IHttpContextAccessor
builder.Services.AddScoped<IPaymentService>(provider =>
{
    var context = provider.GetRequiredService<RestBarContext>();
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    return new PaymentService(context, httpContextAccessor);
});
// Registrar InvoiceService con IHttpContextAccessor
builder.Services.AddScoped<IInvoiceService>(provider =>
{
    var context = provider.GetRequiredService<RestBarContext>();
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    return new InvoiceService(context, httpContextAccessor);
});

builder.Services.AddScoped<IModifierService, ModifierService>();
// Registrar NotificationService con IHttpContextAccessor
builder.Services.AddScoped<INotificationService>(provider =>
{
    var context = provider.GetRequiredService<RestBarContext>();
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    return new NotificationService(context, httpContextAccessor);
});
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
builder.Services.AddScoped<ISplitPaymentService, SplitPaymentService>();

// ✅ NUEVO: Servicio de Personas para Cuentas Separadas
builder.Services.AddScoped<IPersonService, PersonService>();

// ✅ Agregar servicio de reportes de ventas
builder.Services.AddScoped<ISalesReportService, SalesReportService>();



// Registrar AuthService con IHttpContextAccessor
builder.Services.AddScoped<IAuthService>(provider =>
{
    var context = provider.GetRequiredService<RestBarContext>();
    var userService = provider.GetRequiredService<IUserService>();
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    var logger = provider.GetRequiredService<ILogger<AuthService>>();
    return new AuthService(context, userService, httpContextAccessor, logger);
});





// ✅ NUEVO: Servicios de Ajustes Avanzados
builder.Services.AddScoped<ISystemSettingsService, SystemSettingsService>();

builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<ITaxRateService, TaxRateService>();
builder.Services.AddScoped<IDiscountPolicyService, DiscountPolicyService>();
builder.Services.AddScoped<IOperatingHoursService, OperatingHoursService>();
builder.Services.AddScoped<INotificationSettingsService, NotificationSettingsService>();
builder.Services.AddScoped<IBackupSettingsService, BackupSettingsService>();

// ✅ NUEVO: Servicio de Reportes Avanzados
builder.Services.AddScoped<IAdvancedReportsService, AdvancedReportsService>();

// Agregar servicio de SignalR
builder.Services.AddScoped<IInventoryOperationsService, InventoryOperationsService>();
builder.Services.AddScoped<IOrderHubService, OrderHubService>();

// ✅ NUEVO: Agregar servicios de logging y auditoría
builder.Services.AddScoped<IGlobalLoggingService, GlobalLoggingService>();

// ✅ NUEVO: Servicio de Email
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();

var app = builder.Build();

// Aplicar migraciones pendientes al arranque (VPS/Docker)
if (!args.Contains("--verify-db") && app.Environment.IsProduction())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RestBarContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Migrate at startup failed (DB may not be ready). App will continue.");
    }
}

// Verificación en DB (ejecutar: dotnet run -- --verify-db)
if (args.Contains("--verify-db"))
{
    using var scope = app.Services.CreateScope();
    var ctx = scope.ServiceProvider.GetRequiredService<RestBarContext>();
    var conn = ctx.Database.GetDbConnection();
    await conn.OpenAsync();
    var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT column_name, data_type FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'orders' ORDER BY ordinal_position";
    var columns = new List<string>();
    await using (var reader = await cmd.ExecuteReaderAsync())
    {
        while (await reader.ReadAsync())
            columns.Add($"{reader.GetString(0)} ({reader.GetString(1)})");
    }
    Console.WriteLine("Columnas en tabla 'orders':");
    foreach (var c in columns) Console.WriteLine("  " + c);
    var hasVersion = columns.Any(c => c.StartsWith("version ", StringComparison.OrdinalIgnoreCase));
    Console.WriteLine(hasVersion ? "\n[OK] Columna 'version' existe en la DB." : "\n[ERROR] Columna 'version' NO existe. Ejecute: dotnet ef database update");
    return;
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// ✅ MEJORADO: Configurar archivos estáticos con cache busting para desarrollo
if (app.Environment.IsDevelopment())
{
    // Middleware personalizado para desarrollo
    app.Use(async (context, next) =>
    {
        // Agregar headers de no-cache para todas las respuestas en desarrollo
        context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
        context.Response.Headers.Append("Pragma", "no-cache");
        context.Response.Headers.Append("Expires", "0");
        
        await next();
    });
    
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            var path = ctx.File.Name.ToLowerInvariant();
            
            // Deshabilitar cache para TODOS los archivos estáticos en desarrollo
            ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            ctx.Context.Response.Headers.Append("Pragma", "no-cache");
            ctx.Context.Response.Headers.Append("Expires", "0");
            ctx.Context.Response.Headers.Append("Last-Modified", DateTime.UtcNow.ToString("R"));
            ctx.Context.Response.Headers.Append("ETag", Guid.NewGuid().ToString());
            
            // Log para debugging
            Console.WriteLine($"🔍 [StaticFiles] Sirviendo archivo: {path} - Cache deshabilitado");
        }
    });
}
else
{
    app.UseStaticFiles();
}

app.UseRouting();

// ✅ SEGURIDAD: Rate limiting (debe estar antes de UseAuthentication)
app.UseRateLimiter();

// ✅ NUEVO: Configurar sesiones (debe estar después de UseRouting)
app.UseSession();

// Configurar middleware de autenticación (orden importante)
app.UseAuthentication();
app.UseAuthorization();

// ✅ NUEVO: Agregar middlewares de auditoría y manejo de errores (después de autenticación)
app.UseAuditLogging();
app.UseErrorHandling();

// Middleware personalizado para validación de permisos
app.UsePermissionValidation();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}"); // Cambiar página de inicio a Login

// Mapear el hub de SignalR
app.MapHub<OrderHub>("/orderHub");






app.Run();
