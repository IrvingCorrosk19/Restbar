using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RestBar.Infrastructure.Foundation;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Controllers;

[Authorize(Policy = "CostingAccess")]
public class RecipeController : Controller
{
    private readonly RestBarContext _context;
    private readonly IRecipeProfitabilityService _profit;
    private readonly IFoodCostEngine _engine;
    private readonly FeatureFlags _flags;

    public RecipeController(
        RestBarContext context,
        IRecipeProfitabilityService profit,
        IFoodCostEngine engine,
        IOptions<FeatureFlags> flags)
    {
        _context = context;
        _profit = profit;
        _engine = engine;
        _flags = flags.Value;
    }

    public async Task<IActionResult> Index()
    {
        if (!_flags.EnableFoodCostModule)
            return View("~/Views/FoodCostDashboard/ModuleDisabled.cshtml");

        var companyId = Guid.Parse(User.FindFirst("CompanyId")!.Value);
        var recipes = await _context.Recipes.AsNoTracking()
            .Include(r => r.Product)
            .Where(r => r.IsActive && (r.CompanyId == null || r.CompanyId == companyId))
            .OrderBy(r => r.Name)
            .Take(100)
            .ToListAsync();
        return View(recipes);
    }

    [HttpGet]
    public async Task<IActionResult> Cost(Guid productId)
    {
        if (!_flags.EnableFoodCostModule)
            return View("~/Views/FoodCostDashboard/ModuleDisabled.cshtml");

        var cost = await _engine.GetPlateCostAsync(productId);
        if (Request.Headers["Accept"].ToString().Contains("application/json") || Request.Query["json"] == "1")
            return Json(cost);
        ViewBag.Cost = cost;
        return View(cost);
    }

    [HttpGet]
    public async Task<IActionResult> ByProduct(Guid productId)
    {
        if (!_flags.EnableFoodCostModule)
            return Json(new { success = false, message = "Food Cost module disabled" });

        var recipe = await _context.Recipes
            .Include(r => r.Lines)
                .ThenInclude(l => l.IngredientProduct)
            .FirstOrDefaultAsync(r => r.ProductId == productId && r.IsActive);
        return Json(new { success = true, recipe });
    }

    public class RecipeDto
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal YieldPercent { get; set; } = 100m;
        public decimal? TargetFoodCostPercent { get; set; }
        public List<RecipeLineDto> Lines { get; set; } = new();
    }

    public class RecipeLineDto
    {
        public Guid IngredientProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal WastePercent { get; set; }
        public Guid? StationId { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] RecipeDto dto)
    {
        if (!_flags.EnableFoodCostModule)
            return Json(new { success = false, message = "Food Cost module disabled" });

        var companyId = Guid.TryParse(User.FindFirst("CompanyId")?.Value, out var cid) ? cid : Guid.Empty;
        var existing = await _context.Recipes
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.ProductId == dto.ProductId);

        if (existing != null)
        {
            _context.RecipeLines.RemoveRange(existing.Lines.ToList());
            existing.Name = dto.Name;
            existing.IsActive = true;
            existing.YieldPercent = dto.YieldPercent;
            existing.TargetFoodCostPercent = dto.TargetFoodCostPercent;
            existing.Version++;
            foreach (var l in dto.Lines)
            {
                _context.RecipeLines.Add(new RecipeLine
                {
                    Id = Guid.NewGuid(),
                    RecipeId = existing.Id,
                    IngredientProductId = l.IngredientProductId,
                    Quantity = l.Quantity,
                    WastePercent = l.WastePercent,
                    StationId = l.StationId
                });
            }
        }
        else
        {
            var recipe = new Recipe
            {
                Id = Guid.NewGuid(),
                ProductId = dto.ProductId,
                Name = dto.Name,
                IsActive = true,
                YieldPercent = dto.YieldPercent,
                TargetFoodCostPercent = dto.TargetFoodCostPercent,
                CompanyId = companyId == Guid.Empty ? null : companyId,
                Lines = dto.Lines.Select(l => new RecipeLine
                {
                    Id = Guid.NewGuid(),
                    IngredientProductId = l.IngredientProductId,
                    Quantity = l.Quantity,
                    WastePercent = l.WastePercent,
                    StationId = l.StationId
                }).ToList()
            };
            _context.Recipes.Add(recipe);
        }

        await _context.SaveChangesAsync();
        var cost = await _profit.RecalcAndHistoryAsync(dto.ProductId, companyId);
        return Json(new { success = true, cost });
    }
}
