using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using RestBar.Services;
using RestBar.Hubs;
using RestBar.Middleware;
using RestBar.Helpers;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // Configurar el serializador JSON para manejar referencias circulares
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.MaxDepth = 64; // Aumentar la profundidad máxima
        options.JsonSerializerOptions.WriteIndented = true; // Para debugging
    });

// Agregar SignalR
builder.Services.AddSignalR();

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
    options.AddPolicy("OrderAccess", policy => policy.RequireRole("admin", "manager", "supervisor", "waiter", "cashier"));
    
    // Políticas para área de cocina
    options.AddPolicy("KitchenAccess", policy => policy.RequireRole("admin", "manager", "supervisor", "chef", "bartender"));
    
    // Políticas para área de pagos
    options.AddPolicy("PaymentAccess", policy => policy.RequireRole("admin", "manager", "supervisor", "cashier", "accountant"));
    

    
    // Políticas para área de productos
    options.AddPolicy("ProductAccess", policy => policy.RequireRole("admin", "manager", "inventory"));
    
    // Políticas para área de usuarios
    options.AddPolicy("UserManagement", policy => policy.RequireRole("admin", "manager", "support"));
    
    // Políticas para área de reportes
    options.AddPolicy("ReportAccess", policy => policy.RequireRole("admin", "manager", "accountant"));
    

    
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
builder.Services.AddScoped<IStationService, StationService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ITableService, TableService>();
builder.Services.AddScoped<IAreaService, AreaService>();
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
// Registrar OrderService con sus dependencias
builder.Services.AddScoped<IOrderService>(provider =>
{
    var context = provider.GetRequiredService<RestBarContext>();
    var productService = provider.GetRequiredService<IProductService>();
    var orderHubService = provider.GetRequiredService<IOrderHubService>();
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    return new OrderService(context, productService, orderHubService, httpContextAccessor);
});
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





// ✅ NUEVO: Agregar servicio de transferencias
builder.Services.AddScoped<ITransferService, TransferService>();

// ✅ NUEVO: Servicios de Ajustes Avanzados
builder.Services.AddScoped<ISystemSettingsService, SystemSettingsService>();
builder.Services.AddScoped<IPrinterService, PrinterService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<ITaxRateService, TaxRateService>();
builder.Services.AddScoped<IDiscountPolicyService, DiscountPolicyService>();
builder.Services.AddScoped<IOperatingHoursService, OperatingHoursService>();
builder.Services.AddScoped<INotificationSettingsService, NotificationSettingsService>();
builder.Services.AddScoped<IBackupSettingsService, BackupSettingsService>();

// ✅ NUEVO: Servicio de Reportes Avanzados
builder.Services.AddScoped<IAdvancedReportsService, AdvancedReportsService>();

// Agregar servicio de SignalR
builder.Services.AddScoped<IOrderHubService, OrderHubService>();

// ✅ NUEVO: Agregar servicios de logging y auditoría
builder.Services.AddScoped<IGlobalLoggingService, GlobalLoggingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ NUEVO: Agregar middlewares de auditoría y manejo de errores
app.UseErrorHandling();
app.UseAuditLogging();

// ✅ NUEVO: Configurar sesiones
app.UseSession();

// Configurar middleware de autenticación (orden importante)
app.UseAuthentication();
app.UseAuthorization();

// Middleware personalizado para validación de permisos
app.UsePermissionValidation();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}"); // Cambiar página de inicio a Login

// Mapear el hub de SignalR
app.MapHub<OrderHub>("/orderHub");






app.Run();
