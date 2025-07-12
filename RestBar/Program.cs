using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using RestBar.Services;
using RestBar.Hubs;
using RestBar.Middleware;
using RestBar.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Agregar SignalR
builder.Services.AddSignalR();

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
    
    // Políticas para área de inventario
    options.AddPolicy("InventoryAccess", policy => policy.RequireRole("admin", "manager", "inventory"));
    
    // Políticas para área de productos
    options.AddPolicy("ProductAccess", policy => policy.RequireRole("admin", "manager", "inventory"));
    
    // Políticas para área de usuarios
    options.AddPolicy("UserManagement", policy => policy.RequireRole("admin", "manager", "support"));
    
    // Políticas para área de reportes
    options.AddPolicy("ReportAccess", policy => policy.RequireRole("admin", "manager", "accountant"));
    
    // Políticas para área de configuración
    options.AddPolicy("SystemConfig", policy => policy.RequireRole("admin"));
});

// Agregar HttpContextAccessor para el AuthService
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<RestBarContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.MapEnum<UserRole>("user_role_enum"))
);


// Registrar servicios
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IStationService, StationService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ITableService, TableService>();
builder.Services.AddScoped<IAreaService, AreaService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();
builder.Services.AddScoped<IKitchenService, KitchenService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IModifierService, ModifierService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
builder.Services.AddScoped<ISplitPaymentService, SplitPaymentService>();

// Agregar servicio de autenticación
builder.Services.AddScoped<IAuthService, AuthService>();

// Agregar servicio de SignalR
builder.Services.AddScoped<IOrderHubService, OrderHubService>();

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
