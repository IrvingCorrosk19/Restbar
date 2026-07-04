using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models;

public partial class Payment : ITrackableEntity
{
    public Guid Id { get; set; }

    public Guid? OrderId { get; set; }

    [Required]
    [StringLength(50)]
    public string Method { get; set; } = null!; // Efectivo, Tarjeta, Transferencia, etc.

    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Amount { get; set; }

    /// <summary>Propina asociada a este pago (configurable por operación).</summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "La propina no puede ser negativa")]
    public decimal TipAmount { get; set; } = 0;

    /// <summary>Usuario (cajero/mesero) que procesó el cobro.</summary>
    public Guid? ProcessedByUserId { get; set; }

    public DateTime PaidAt { get; set; } = DateTime.UtcNow;

    public bool IsVoided { get; set; } = false;

    public bool IsShared { get; set; } = false;

    [StringLength(100)]
    public string? PayerName { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "COMPLETED";

    /// <summary>
    /// Clave de idempotencia para prevenir pagos duplicados por reintento de red.
    /// El cliente genera un UUID v4 único por intento de pago. Unique index en DB.
    /// </summary>
    [StringLength(100)]
    public string? IdempotencyKey { get; set; }

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
    public virtual User? ProcessedByUser { get; set; }

    public virtual ICollection<SplitPayment> SplitPayments { get; set; } = new List<SplitPayment>();
}
