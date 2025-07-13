using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models;

public partial class OrderItem : ITrackableEntity
{
    public Guid Id { get; set; }

    public Guid? OrderId { get; set; }

    public Guid? ProductId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "El precio unitario no puede ser negativo")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "El descuento no puede ser negativo")]
    public decimal Discount { get; set; } = 0;

    [StringLength(500)]
    public string? Notes { get; set; }

    public OrderItemStatus Status { get; set; } = OrderItemStatus.Pending;

    public Guid? PreparedByStationId { get; set; }

    public DateTime? PreparedAt { get; set; }

    // Nuevos campos para la lógica de cocina
    public KitchenStatus KitchenStatus { get; set; } = KitchenStatus.Pending;

    public DateTime? SentAt { get; set; }

    // ✅ CAMPOS PARA CUENTAS SEPARADAS
    public Guid? AssignedToPersonId { get; set; }
    
    [StringLength(100)]
    public string? AssignedToPersonName { get; set; }
    
    public bool IsShared { get; set; } = false;

    // ✅ CAMPOS MULTI-TENANT
    public Guid? CompanyId { get; set; }
    public Guid? BranchId { get; set; }

    // ✅ CAMPOS DE AUDITORÍA ESTANDARIZADOS
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // Propiedades de navegación multi-tenant
    public virtual Company? Company { get; set; }
    public virtual Branch? Branch { get; set; }

    // Propiedades de navegación
    public virtual Order? Order { get; set; }
    public virtual Product? Product { get; set; }
    public virtual Station? PreparedByStation { get; set; }
    public virtual Person? AssignedToPerson { get; set; }
}
