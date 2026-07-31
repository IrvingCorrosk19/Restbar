using RestBar.Services;
using Xunit;

namespace RestBar.Tests.Inventory;

public class InventoryRecipeQtyTests
{
    [Fact]
    public void ComputeRecipeIngredientQty_no_waste_full_yield()
    {
        var q = InventoryOperationsService.ComputeRecipeIngredientQty(2m, 3m, 0m, 100m);
        Assert.Equal(6m, q);
    }

    [Fact]
    public void ComputeRecipeIngredientQty_applies_waste_percent()
    {
        var q = InventoryOperationsService.ComputeRecipeIngredientQty(1m, 1m, 10m, 100m);
        Assert.Equal(1.1m, q);
    }

    [Fact]
    public void ComputeRecipeIngredientQty_applies_yield_factor()
    {
        // 1 unit recipe, 80% yield → need 100/80 = 1.25 physical
        var q = InventoryOperationsService.ComputeRecipeIngredientQty(1m, 1m, 0m, 80m);
        Assert.Equal(1.25m, q);
    }

    [Fact]
    public void ComputeRecipeIngredientQty_waste_and_yield_combined()
    {
        // (1 * 1.1) * (100/80) = 1.375
        var q = InventoryOperationsService.ComputeRecipeIngredientQty(1m, 1m, 10m, 80m);
        Assert.Equal(1.375m, q);
    }
}
