using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models;

public partial class Product : ITrackableEntity
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Cost { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? TaxRate { get; set; }

    [StringLength(20)]
    public string? Unit { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    // ✅ CAMPOS DE AUDITORÍA ESTANDARIZADOS
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public Guid? CategoryId { get; set; }

    public virtual Category? Category { get; set; }



    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Modifier> Modifiers { get; set; } = new List<Modifier>();

    public Guid? StationId { get; set; }
    public virtual Station? Station { get; set; }

    // ✅ NUEVO: Propiedades multi-tenant
    [Display(Name = "Compañía")]
    public Guid? CompanyId { get; set; }

    [Display(Name = "Sucursal")]
    public Guid? BranchId { get; set; }

    // Propiedades de navegación multi-tenant
    public virtual Company? Company { get; set; }
    public virtual Branch? Branch { get; set; }

    // ✅ NUEVO: CAMPOS DE INVENTARIO
    [Display(Name = "Stock Disponible")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Stock { get; set; }

    [Display(Name = "Stock Mínimo")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinStock { get; set; }

    [Display(Name = "Controlar Inventario")]
    public bool TrackInventory { get; set; } = false;

    [Display(Name = "Permitir Stock Negativo")]
    public bool AllowNegativeStock { get; set; } = false;

    // ✅ NUEVO: Navegación a asignaciones de stock por estación
    public virtual ICollection<ProductStockAssignment> StockAssignments { get; set; } = new List<ProductStockAssignment>();
}
