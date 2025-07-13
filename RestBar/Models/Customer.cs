using System;
using System.Collections.Generic;

namespace RestBar.Models;

public partial class Customer : ITrackableEntity
{
    public Guid Id { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public int? LoyaltyPoints { get; set; }

    public string? Notes { get; set; }

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

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
