using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestBar.Models;

namespace RestBar.Controllers;

[Authorize(Policy = "ManagerOrAbove")]
public class RecipeController : Controller
{
    private readonly RestBarContext _context;

    public RecipeController(RestBarContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> ByProduct(Guid productId)
    {
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
        public List<RecipeLineDto> Lines { get; set; } = new();
    }

    public class RecipeLineDto
    {
        public Guid IngredientProductId { get; set; }
        public decimal Quantity { get; set; }
        public Guid? StationId { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] RecipeDto dto)
    {
        var existing = await _context.Recipes
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.ProductId == dto.ProductId);

        if (existing != null)
        {
            _context.RecipeLines.RemoveRange(existing.Lines.ToList());
            existing.Name = dto.Name;
            existing.IsActive = true;
            foreach (var l in dto.Lines)
            {
                _context.RecipeLines.Add(new RecipeLine
                {
                    Id = Guid.NewGuid(),
                    RecipeId = existing.Id,
                    IngredientProductId = l.IngredientProductId,
                    Quantity = l.Quantity,
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
                Lines = dto.Lines.Select(l => new RecipeLine
                {
                    Id = Guid.NewGuid(),
                    IngredientProductId = l.IngredientProductId,
                    Quantity = l.Quantity,
                    StationId = l.StationId
                }).ToList()
            };
            _context.Recipes.Add(recipe);
        }

        await _context.SaveChangesAsync();
        return Json(new { success = true });
    }
}
