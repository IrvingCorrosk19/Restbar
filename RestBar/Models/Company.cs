using System;
using System.Collections.Generic;

namespace RestBar.Models;

public partial class Company : ITrackableEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? LegalId { get; set; }

    public DateTime? CreatedAt { get; set; }

    // ✅ NUEVOS CAMPOS AGREGADOS
    public string? TaxId { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
    
    // ✅ NUEVO: Campo UpdatedAt
    public DateTime? UpdatedAt { get; set; }

    // ✅ NUEVOS CAMPOS: Tracking de usuarios
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
}
