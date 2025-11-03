using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models;

/// <summary>
/// Asignación de stock de un producto a una estación específica
/// Permite tener el mismo producto con stock diferente en distintas estaciones
/// </summary>
public class ProductStockAssignment : ITrackableEntity
{
    public Guid Id { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid StationId { get; set; }

    [Required]
    [Display(Name = "Stock en Estación")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Stock { get; set; }

    [Display(Name = "Stock Mínimo en Estación")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinStock { get; set; }

    [Display(Name = "Prioridad")]
    public int Priority { get; set; } = 0; // Mayor número = mayor prioridad para asignación

    [Display(Name = "Activo")]
    public bool IsActive { get; set; } = true;

    // ✅ CAMPOS MULTI-TENANT
    [Display(Name = "Compañía")]
    public Guid? CompanyId { get; set; }

    [Display(Name = "Sucursal")]
    public Guid? BranchId { get; set; }

    // ✅ CAMPOS DE AUDITORÍA ESTANDARIZADOS
    [Display(Name = "Fecha de Creación")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Fecha de Actualización")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Creado Por")]
    [StringLength(255)]
    public string? CreatedBy { get; set; }

    [Display(Name = "Actualizado Por")]
    [StringLength(255)]
    public string? UpdatedBy { get; set; }

    // Propiedades de navegación
    public virtual Product Product { get; set; } = null!;
    public virtual Station Station { get; set; } = null!;
    public virtual Company? Company { get; set; }
    public virtual Branch? Branch { get; set; }
}

