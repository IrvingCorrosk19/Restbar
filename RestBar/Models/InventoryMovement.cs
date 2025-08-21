using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models;

public partial class InventoryMovement
{
    public Guid Id { get; set; }

    public Guid InventoryId { get; set; }

    public Guid ProductId { get; set; }

    public Guid BranchId { get; set; }

    public Guid? UserId { get; set; }

    public MovementType Type { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PreviousStock { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? NewStock { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }

    [StringLength(100)]
    public string? Reference { get; set; }

    public DateTime CreatedAt { get; set; }

    // Relaciones
    public virtual Inventory Inventory { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public virtual Branch Branch { get; set; } = null!;
    public virtual User? User { get; set; }
}

public enum MovementType
{
    Purchase,        // Compra
    Sale,           // Venta
    Adjustment,     // Ajuste manual
    Transfer,       // Transferencia entre sucursales
    Return,         // Devolución
    Waste,          // Pérdida/Merma
    Initial,        // Stock inicial
    Correction      // Corrección de inventario
} 