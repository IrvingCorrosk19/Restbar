using Microsoft.EntityFrameworkCore;
using RestBar.Models;

namespace RestBar.Services;

/// <summary>
/// Genera dataset comercial vendible: 30 mesas, 100 productos, staff completo, historial de ventas.
/// </summary>
public class CommercialDemoSeeder
{
    private readonly RestBarContext _context;

    public CommercialDemoSeeder(RestBarContext context) => _context = context;

    public async Task<CommercialDemoResult> SeedAsync()
    {
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Name == "RestBar Principal")
            ?? throw new InvalidOperationException("Ejecute SeedDemoData primero");

        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Name == "RestBar Centro" && b.CompanyId == company.Id)
            ?? throw new InvalidOperationException("Ejecute SeedDemoData primero");

        var areas = await _context.Areas.Where(a => a.BranchId == branch.Id).ToListAsync();
        if (areas.Count < 3)
        {
            foreach (var name in new[] { "Piso 1 - Salón Comercial", "Piso 2 - Terraza VIP", "Piso 3 - Lounge" })
            {
                if (!areas.Any(a => a.Name == name))
                {
                    var a = new Area { Id = Guid.NewGuid(), CompanyId = company.Id, BranchId = branch.Id, Name = name, IsActive = true, CreatedBy = "CommercialSeeder" };
                    _context.Areas.Add(a);
                    areas.Add(a);
                }
            }
            await _context.SaveChangesAsync();
        }

        var tableCount = await _context.Tables.CountAsync(t => t.BranchId == branch.Id);
        var targetTables = 30;
        for (int i = tableCount + 1; i <= targetTables; i++)
        {
            var area = areas[i % areas.Count];
            var num = $"C-{i:D2}";
            if (!await _context.Tables.AnyAsync(t => t.TableNumber == num && t.BranchId == branch.Id))
            {
                _context.Tables.Add(new Table
                {
                    Id = Guid.NewGuid(), CompanyId = company.Id, BranchId = branch.Id, AreaId = area.Id,
                    TableNumber = num, Capacity = 4, Status = TableStatus.Disponible, IsActive = true, CreatedBy = "CommercialSeeder"
                });
            }
        }
        await _context.SaveChangesAsync();

        var categories = await _context.Categories.Where(c => c.BranchId == branch.Id).ToListAsync();
        if (!categories.Any())
        {
            categories.Add(new Category { Id = Guid.NewGuid(), CompanyId = company.Id, BranchId = branch.Id, Name = "Menú Comercial", IsActive = true, CreatedBy = "CommercialSeeder" });
            _context.Categories.Add(categories[0]);
            await _context.SaveChangesAsync();
        }

        var productCount = await _context.Products.CountAsync(p => p.BranchId == branch.Id);
        for (int i = productCount + 1; i <= 100; i++)
        {
            var cat = categories[i % categories.Count];
            var name = $"Producto Demo {i:D3}";
            if (!await _context.Products.AnyAsync(p => p.Name == name && p.BranchId == branch.Id))
            {
                _context.Products.Add(new Product
                {
                    Id = Guid.NewGuid(), CompanyId = company.Id, BranchId = branch.Id, CategoryId = cat.Id,
                    Name = name, Price = 3m + (i % 25), Stock = 100, MinStock = 5, TrackInventory = true, IsActive = true, CreatedBy = "CommercialSeeder"
                });
            }
        }
        await _context.SaveChangesAsync();

        async Task EnsureStaffAsync(string email, string name, UserRole role)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email)) return;
            _context.Users.Add(new User
            {
                Id = Guid.NewGuid(), BranchId = branch.Id, FullName = name, Email = email,
                PasswordHash = HashPassword("123456"), Role = role, IsActive = true, CreatedBy = "CommercialSeeder"
            });
        }

        for (int i = 2; i <= 10; i++) await EnsureStaffAsync($"mesero{i}@restbar.com", $"Mesero {i}", UserRole.waiter);
        for (int i = 2; i <= 5; i++) await EnsureStaffAsync($"chef{i}@restbar.com", $"Chef {i}", UserRole.chef);
        for (int i = 2; i <= 3; i++) await EnsureStaffAsync($"bartender{i}@restbar.com", $"Bartender {i}", UserRole.bartender);
        for (int i = 2; i <= 3; i++) await EnsureStaffAsync($"cajero{i}@restbar.com", $"Cajero {i}", UserRole.cashier);
        await EnsureStaffAsync("gerente2@restbar.com", "Gerente Secundario", UserRole.manager);
        await _context.SaveChangesAsync();

        var waiter = await _context.Users.FirstOrDefaultAsync(u => u.Email == "mesero@restbar.com");
        var cashier = await _context.Users.FirstOrDefaultAsync(u => u.Email == "cajero@restbar.com");
        var table = await _context.Tables.FirstOrDefaultAsync(t => t.BranchId == branch.Id);
        var products = await _context.Products.Where(p => p.BranchId == branch.Id).Take(20).ToListAsync();

        var existingHistory = await _context.Orders.CountAsync(o => o.BranchId == branch.Id && o.Status == OrderStatus.Completed);
        var historyToCreate = Math.Max(0, 40 - existingHistory);

        for (int h = 0; h < historyToCreate; h++)
        {
            var prod = products[h % products.Count];
            var daysAgo = h % 14;
            var opened = DateTime.UtcNow.AddDays(-daysAgo).AddHours(-(h % 8));
            var total = prod.Price * 2;
            var order = new Order
            {
                Id = Guid.NewGuid(), CompanyId = company.Id, BranchId = branch.Id, TableId = table?.Id,
                OrderNumber = $"HIST-{1000 + h}", OrderType = OrderType.DineIn, Status = OrderStatus.Completed,
                OpenedAt = opened, ClosedAt = opened.AddHours(1), TotalAmount = total,
                UserId = waiter?.Id, CreatedBy = "CommercialSeeder"
            };
            _context.Orders.Add(order);
            _context.OrderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(), OrderId = order.Id, ProductId = prod.Id, Quantity = 2,
                UnitPrice = prod.Price, Status = OrderItemStatus.Served, KitchenStatus = KitchenStatus.Ready
            });
            _context.Payments.Add(new Payment
            {
                Id = Guid.NewGuid(), OrderId = order.Id, Amount = total, Method = h % 2 == 0 ? "Efectivo" : "Tarjeta",
                PaidAt = opened.AddHours(1), ProcessedByUserId = cashier?.Id, IsVoided = false, Status = "COMPLETED",
                CreatedBy = "CommercialSeeder"
            });
        }
        await _context.SaveChangesAsync();

        return new CommercialDemoResult
        {
            Tables = await _context.Tables.CountAsync(t => t.BranchId == branch.Id),
            Products = await _context.Products.CountAsync(p => p.BranchId == branch.Id),
            Areas = await _context.Areas.CountAsync(a => a.BranchId == branch.Id),
            Staff = await _context.Users.CountAsync(u => u.BranchId == branch.Id),
            HistoricalOrders = await _context.Orders.CountAsync(o => o.BranchId == branch.Id && o.Status == OrderStatus.Completed)
        };
    }

    private static string HashPassword(string password)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        return Convert.ToBase64String(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)));
    }
}

public class CommercialDemoResult
{
    public int Tables { get; set; }
    public int Products { get; set; }
    public int Areas { get; set; }
    public int Staff { get; set; }
    public int HistoricalOrders { get; set; }
}
