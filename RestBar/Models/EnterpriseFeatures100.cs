using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models;

/// <summary>Mesa secundaria unida a una mesa principal.</summary>
public class TableMergeLink
{
    public Guid Id { get; set; }
    public Guid PrimaryTableId { get; set; }
    public Guid SecondaryTableId { get; set; }
    public int SecondaryCapacitySnapshot { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime MergedAt { get; set; } = DateTime.UtcNow;
    public Guid? CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public virtual Table? PrimaryTable { get; set; }
    public virtual Table? SecondaryTable { get; set; }
}

/// <summary>Paso de preparación multi-estación (ej. parrilla → armado).</summary>
public class ProductPreparationStep
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid StationId { get; set; }
    public int StepOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public virtual Product? Product { get; set; }
    public virtual Station? Station { get; set; }
}

/// <summary>Sustituto cuando un ingrediente está agotado.</summary>
public class IngredientAlternative
{
    public Guid Id { get; set; }
    public Guid IngredientProductId { get; set; }
    public Guid AlternativeProductId { get; set; }
    public int Priority { get; set; } = 10;
    public bool IsActive { get; set; } = true;
    public Guid? CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public virtual Product? IngredientProduct { get; set; }
    public virtual Product? AlternativeProduct { get; set; }
}
