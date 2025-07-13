using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models;

public partial class Invoice : ITrackableEntity
{
    public Guid Id { get; set; }

    public Guid? OrderId { get; set; }

    public Guid? CustomerId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "El total no puede ser negativo")]
    public decimal Total { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "El impuesto no puede ser negativo")]
    public decimal Tax { get; set; } = 0;

    [StringLength(50)]
    public string? InvoiceNumber { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "PENDING"; // PENDING, PAID, CANCELLED

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
    public virtual Customer? Customer { get; set; }
    public virtual Order? Order { get; set; }
}
