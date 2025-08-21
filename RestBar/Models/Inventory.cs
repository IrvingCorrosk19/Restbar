using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models;

public partial class Inventory : ITrackableEntity
{
    public Guid Id { get; set; }

    public Guid? BranchId { get; set; }

    public Guid? ProductId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Quantity { get; set; }

    [StringLength(50)]
    public string? Unit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinThreshold { get; set; }

    public DateTime? LastUpdated { get; set; }

    // ✅ NUEVAS PROPIEDADES para las columnas adicionales en la BD
    public int? Stock { get; set; }

    public int? MinStock { get; set; }

    public int? MaxStock { get; set; }

    // ❌ NUEVAS PROPIEDADES FALTANTES
    [Column(TypeName = "decimal(18,2)")]
    public decimal? ReorderPoint { get; set; }    // Punto de reorden

    [Column(TypeName = "decimal(18,2)")]
    public decimal? ReorderQuantity { get; set; } // Cantidad a reordenar

    public DateTime? ExpiryDate { get; set; }     // Fecha de caducidad

    [StringLength(100)]
    public string? Location { get; set; }         // Ubicación en almacén

    [StringLength(50)]
    public string? Barcode { get; set; }          // Código de barras

    [Column(TypeName = "decimal(18,2)")]
    public decimal? UnitCost { get; set; }        // Costo unitario

    [Column(TypeName = "decimal(18,2)")]
    public decimal? TotalValue { get; set; }      // Valor total del stock

    [StringLength(500)]
    public string? Notes { get; set; }            // Notas adicionales

    public bool IsActive { get; set; } = true;    // Estado activo/inactivo

    // Tracking fields
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // Relaciones existentes
    public virtual Branch? Branch { get; set; }

    public virtual Product? Product { get; set; }

    // ❌ NUEVA RELACIÓN para movimientos
    public virtual ICollection<InventoryMovement> Movements { get; set; } = new List<InventoryMovement>();
}
