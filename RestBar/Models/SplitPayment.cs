using System;
using System.Collections.Generic;

namespace RestBar.Models;

public partial class SplitPayment : ITrackableEntity
{
    public Guid Id { get; set; }

    public Guid? PaymentId { get; set; }

    public string? PersonName { get; set; }

    public decimal? Amount { get; set; }

    public string? Method { get; set; }

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

    public virtual Payment? Payment { get; set; }
}
