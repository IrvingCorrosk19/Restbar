using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using RestBar.Models;

namespace RestBar.Services;

/// <summary>
/// Siembra Restaurante Costa, Norte y Sur para certificación multitenant 3 empresas.
/// </summary>
public class ThreeCompaniesCertSeeder
{
    private const string Creator = "ThreeCompaniesSeeder";
    private readonly RestBarContext _context;

    public ThreeCompaniesCertSeeder(RestBarContext context) => _context = context;

    public async Task<ThreeCompaniesSeedResult> SeedAsync()
    {
        var costa = await SeedCostaAsync();
        var norte = await SeedNorteAsync();
        var sur = await SeedSurAsync();
        await EnsureSuperAdminAsync(costa.BranchId);
        return new ThreeCompaniesSeedResult
        {
            Costa = costa,
            Norte = norte,
            Sur = sur
        };
    }

    private async Task<CompanySeedSummary> SeedCostaAsync() =>
        await SeedCompanyAsync(
            companyName: "Restaurante Costa",
            branchName: "Costa Centro",
            emailPrefix: "costa",
            tablePrefix: "C",
            tableCount: 10,
            areas: new[] { "Piso 1 Salón", "Piso 1 Terraza", "Piso 2 Salón", "Piso 2 Terraza" },
            stations: new (string name, string type, string area)[]
            {
                ("Cocina Piso 1", "kitchen", "Piso 1 Salón"),
                ("Bar Piso 1", "bar", "Piso 1 Salón"),
                ("Cocina Piso 2", "kitchen", "Piso 2 Salón"),
                ("Bar Piso 2", "bar", "Piso 2 Salón")
            },
            exclusiveProduct: "Producto Exclusivo Costa",
            waiterAreaNames: new[] { ("mesero1", "Piso 1 Salón"), ("mesero2", "Piso 2 Salón") });

    private async Task<CompanySeedSummary> SeedNorteAsync() =>
        await SeedCompanyAsync(
            companyName: "Restaurante Norte",
            branchName: "Norte Mall",
            emailPrefix: "norte",
            tablePrefix: "NM",
            tableCount: 10,
            areas: new[] { "Piso 1 Principal", "Piso 2 VIP" },
            stations: new (string name, string type, string area)[]
            {
                ("Cocina Principal", "kitchen", "Piso 1 Principal"),
                ("Bar Principal", "bar", "Piso 1 Principal"),
                ("Parrilla Norte", "kitchen", "Piso 2 VIP")
            },
            exclusiveProduct: "Producto Exclusivo Norte",
            waiterAreaNames: new[] { ("mesero1", "Piso 1 Principal"), ("mesero2", "Piso 2 VIP") });

    private async Task<CompanySeedSummary> SeedSurAsync() =>
        await SeedCompanyAsync(
            companyName: "Restaurante Sur",
            branchName: "Sur Hotel",
            emailPrefix: "sur",
            tablePrefix: "S",
            tableCount: 15,
            areas: new[] { "Piso 1 Hotel", "Piso 2 Hotel", "Piso 3 Rooftop" },
            stations: new (string name, string type, string area)[]
            {
                ("Cocina Principal Sur", "kitchen", "Piso 1 Hotel"),
                ("Bar Hotel Sur", "bar", "Piso 1 Hotel"),
                ("Cocina Rooftop", "kitchen", "Piso 3 Rooftop"),
                ("Bar Rooftop", "bar", "Piso 3 Rooftop")
            },
            exclusiveProduct: "Producto Exclusivo Sur",
            waiterAreaNames: new[] { ("mesero1", "Piso 1 Hotel"), ("mesero2", "Piso 3 Rooftop") });

    private async Task<CompanySeedSummary> SeedCompanyAsync(
        string companyName, string branchName, string emailPrefix, string tablePrefix, int tableCount,
        string[] areas,
        (string name, string type, string area)[] stations,
        string exclusiveProduct,
        (string waiterKey, string areaName)[] waiterAreaNames)
    {
        var company = await EnsureCompanyAsync(companyName);
        var branch = await EnsureBranchAsync(company, branchName);
        var areaMap = new Dictionary<string, Area>();
        foreach (var aName in areas)
        {
            areaMap[aName] = await EnsureAreaAsync(company, branch, aName);
        }

        var stationMap = new Dictionary<string, Station>();
        foreach (var (sName, sType, aName) in stations)
        {
            stationMap[sName] = await EnsureStationAsync(company, branch, areaMap[aName], sName, sType);
        }

        var tables = new List<Table>();
        for (int i = 1; i <= tableCount; i++)
        {
            var area = areaMap.Values.ElementAt((i - 1) % areaMap.Count);
            var num = $"{tablePrefix}-{i:D2}";
            tables.Add(await EnsureTableAsync(company, branch, area, num));
        }

        var cat = await EnsureCategoryAsync(company, branch, "Menú Principal");
        var burger = await EnsureProductAsync(company, branch, cat, $"Hamburguesa {companyName.Split(' ').Last()}", 12m);
        var pizza = await EnsureProductAsync(company, branch, cat, $"Pizza {companyName.Split(' ').Last()}", 14m);
        var beer = await EnsureProductAsync(company, branch, cat, $"Cerveza {companyName.Split(' ').Last()}", 4m);
        var mojito = await EnsureProductAsync(company, branch, cat, $"Mojito {companyName.Split(' ').Last()}", 8m);
        var dessert = await EnsureProductAsync(company, branch, cat, $"Postre {companyName.Split(' ').Last()}", 6m);
        await EnsureProductAsync(company, branch, cat, exclusiveProduct, 99m);

        var kitchen = stationMap.Values.First(s => s.Type == "kitchen");
        var bar = stationMap.Values.First(s => s.Type == "bar");
        var grill = stationMap.Values.FirstOrDefault(s => s.Name.Contains("Parrilla")) ?? kitchen;
        await AssignProductAsync(company, branch, burger, grill, 20);
        await AssignProductAsync(company, branch, pizza, kitchen, 20);
        await AssignProductAsync(company, branch, beer, bar, 20);
        await AssignProductAsync(company, branch, mojito, bar, 20);
        await AssignProductAsync(company, branch, dessert, kitchen, 15);

        var admin = await EnsureUserAsync($"admin@{emailPrefix}.restbar.com", $"Admin {companyName}", UserRole.admin, branch.Id);
        await EnsureUserAsync($"manager@{emailPrefix}.restbar.com", $"Manager {companyName}", UserRole.manager, branch.Id);
        await EnsureUserAsync($"cajero@{emailPrefix}.restbar.com", $"Cajero {companyName}", UserRole.cashier, branch.Id);
        var chef = await EnsureUserAsync($"chef@{emailPrefix}.restbar.com", $"Chef {companyName}", UserRole.chef, branch.Id);
        var bartender = await EnsureUserAsync($"bartender@{emailPrefix}.restbar.com", $"Bartender {companyName}", UserRole.bartender, branch.Id);

        foreach (var (waiterKey, areaName) in waiterAreaNames)
        {
            var waiter = await EnsureUserAsync($"{waiterKey}@{emailPrefix}.restbar.com", $"Mesero {waiterKey} {companyName}", UserRole.waiter, branch.Id);
            await EnsureWaiterAssignmentAsync(company, branch, waiter, areaMap[areaName], tables.Where(t => t.AreaId == areaMap[areaName].Id).Select(t => t.Id).Take(3).ToList());
        }

        await EnsureStationAssignmentAsync(company, branch, chef, kitchen);
        await EnsureStationAssignmentAsync(company, branch, bartender, bar);

        return new CompanySeedSummary
        {
            CompanyId = company.Id,
            BranchId = branch.Id,
            CompanyName = company.Name,
            BranchName = branch.Name,
            TableCount = tables.Count,
            AdminEmail = admin.Email,
            FirstTableId = tables[0].Id,
            BurgerProductId = burger.Id,
            ExclusiveProductName = exclusiveProduct
        };
    }

    private async Task EnsureSuperAdminAsync(Guid branchId)
    {
        if (await _context.Users.AnyAsync(u => u.Email == "superadmin@restbar.com")) return;
        _context.Users.Add(new User
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440099"),
            BranchId = branchId,
            FullName = "Super Administrador",
            Email = "superadmin@restbar.com",
            PasswordHash = HashPassword("123456"),
            Role = UserRole.superadmin,
            IsActive = true,
            CreatedBy = Creator
        });
        await _context.SaveChangesAsync();
    }

    private async Task<Company> EnsureCompanyAsync(string name)
    {
        var c = await _context.Companies.FirstOrDefaultAsync(x => x.Name == name);
        if (c != null) return c;
        c = new Company { Id = Guid.NewGuid(), Name = name, LegalId = Guid.NewGuid().ToString("N")[..9], IsActive = true, CreatedBy = Creator };
        _context.Companies.Add(c);
        await _context.SaveChangesAsync();
        return c;
    }

    private async Task<Branch> EnsureBranchAsync(Company company, string name)
    {
        var b = await _context.Branches.FirstOrDefaultAsync(x => x.Name == name && x.CompanyId == company.Id);
        if (b != null) return b;
        b = new Branch { Id = Guid.NewGuid(), CompanyId = company.Id, Name = name, IsActive = true, CreatedBy = Creator };
        _context.Branches.Add(b);
        await _context.SaveChangesAsync();
        return b;
    }

    private async Task<Area> EnsureAreaAsync(Company company, Branch branch, string name)
    {
        var a = await _context.Areas.FirstOrDefaultAsync(x => x.Name == name && x.BranchId == branch.Id);
        if (a != null) return a;
        a = new Area { Id = Guid.NewGuid(), CompanyId = company.Id, BranchId = branch.Id, Name = name, IsActive = true, CreatedBy = Creator };
        _context.Areas.Add(a);
        await _context.SaveChangesAsync();
        return a;
    }

    private async Task<Station> EnsureStationAsync(Company company, Branch branch, Area area, string name, string type)
    {
        var s = await _context.Stations.FirstOrDefaultAsync(x => x.Name == name && x.BranchId == branch.Id);
        if (s != null) return s;
        s = new Station { Id = Guid.NewGuid(), CompanyId = company.Id, BranchId = branch.Id, AreaId = area.Id, Name = name, Type = type, IsActive = true, CreatedBy = Creator };
        _context.Stations.Add(s);
        await _context.SaveChangesAsync();
        return s;
    }

    private async Task<Table> EnsureTableAsync(Company company, Branch branch, Area area, string number)
    {
        var t = await _context.Tables.FirstOrDefaultAsync(x => x.TableNumber == number && x.BranchId == branch.Id);
        if (t != null) return t;
        t = new Table
        {
            Id = Guid.NewGuid(), CompanyId = company.Id, BranchId = branch.Id, AreaId = area.Id,
            TableNumber = number, Capacity = 4, Status = TableStatus.Disponible, IsActive = true, CreatedBy = Creator
        };
        _context.Tables.Add(t);
        await _context.SaveChangesAsync();
        return t;
    }

    private async Task<Category> EnsureCategoryAsync(Company company, Branch branch, string name)
    {
        var c = await _context.Categories.FirstOrDefaultAsync(x => x.Name == name && x.BranchId == branch.Id);
        if (c != null) return c;
        c = new Category { Id = Guid.NewGuid(), CompanyId = company.Id, BranchId = branch.Id, Name = name, IsActive = true, CreatedBy = Creator };
        _context.Categories.Add(c);
        await _context.SaveChangesAsync();
        return c;
    }

    private async Task<Product> EnsureProductAsync(Company company, Branch branch, Category cat, string name, decimal price)
    {
        var p = await _context.Products.FirstOrDefaultAsync(x => x.Name == name && x.BranchId == branch.Id);
        if (p != null) return p;
        p = new Product
        {
            Id = Guid.NewGuid(), CompanyId = company.Id, BranchId = branch.Id, CategoryId = cat.Id,
            Name = name, Price = price, Stock = 100, MinStock = 5, TrackInventory = true, IsActive = true, CreatedBy = Creator
        };
        _context.Products.Add(p);
        await _context.SaveChangesAsync();
        return p;
    }

    private async Task AssignProductAsync(Company company, Branch branch, Product product, Station station, int priority)
    {
        if (await _context.ProductStockAssignments.AnyAsync(a => a.ProductId == product.Id && a.StationId == station.Id)) return;
        _context.ProductStockAssignments.Add(new ProductStockAssignment
        {
            Id = Guid.NewGuid(), ProductId = product.Id, StationId = station.Id,
            Stock = 100, MinStock = 5, Priority = priority, IsActive = true,
            CompanyId = company.Id, BranchId = branch.Id, CreatedBy = Creator
        });
        await _context.SaveChangesAsync();
    }

    private async Task<User> EnsureUserAsync(string email, string name, UserRole role, Guid branchId)
    {
        var u = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
        if (u != null) return u;
        u = new User
        {
            Id = Guid.NewGuid(), BranchId = branchId, FullName = name, Email = email,
            PasswordHash = HashPassword("123456"), Role = role, IsActive = true, CreatedBy = Creator
        };
        _context.Users.Add(u);
        await _context.SaveChangesAsync();
        return u;
    }

    private async Task EnsureWaiterAssignmentAsync(Company company, Branch branch, User waiter, Area area, List<Guid> tableIds)
    {
        var existing = await _context.UserAssignments.FirstOrDefaultAsync(a => a.UserId == waiter.Id && a.IsActive);
        if (existing != null) return;
        _context.UserAssignments.Add(new UserAssignment
        {
            Id = Guid.NewGuid(), UserId = waiter.Id, AreaId = area.Id, AssignedTableIds = tableIds,
            AssignedAt = DateTime.UtcNow, IsActive = true, CompanyId = company.Id, BranchId = branch.Id, CreatedBy = Creator
        });
        await _context.SaveChangesAsync();
    }

    private async Task EnsureStationAssignmentAsync(Company company, Branch branch, User user, Station station)
    {
        var existing = await _context.UserAssignments.FirstOrDefaultAsync(a => a.UserId == user.Id && a.IsActive);
        if (existing != null)
        {
            if (!existing.StationId.HasValue)
            {
                existing.StationId = station.Id;
                await _context.SaveChangesAsync();
            }
            return;
        }
        _context.UserAssignments.Add(new UserAssignment
        {
            Id = Guid.NewGuid(), UserId = user.Id, StationId = station.Id,
            AssignedAt = DateTime.UtcNow, IsActive = true, CompanyId = company.Id, BranchId = branch.Id, CreatedBy = Creator
        });
        await _context.SaveChangesAsync();
    }

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
    }
}

public class ThreeCompaniesSeedResult
{
    public CompanySeedSummary Costa { get; set; } = null!;
    public CompanySeedSummary Norte { get; set; } = null!;
    public CompanySeedSummary Sur { get; set; } = null!;
}

public class CompanySeedSummary
{
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    public string CompanyName { get; set; } = "";
    public string BranchName { get; set; } = "";
    public int TableCount { get; set; }
    public string AdminEmail { get; set; } = "";
    public Guid FirstTableId { get; set; }
    public Guid BurgerProductId { get; set; }
    public string ExclusiveProductName { get; set; } = "";
}
