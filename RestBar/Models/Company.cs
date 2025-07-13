using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestBar.Models;

public partial class Company : ITrackableEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? LegalId { get; set; }

    [StringLength(50)]
    public string? TaxId { get; set; }
    
    [StringLength(500)]
    public string? Address { get; set; }
    
    [StringLength(20)]
    public string? Phone { get; set; }
    
    [StringLength(100)]
    public string? Email { get; set; }
    
    public bool IsActive { get; set; } = true;

    // ✅ CAMPOS DE AUDITORÍA ESTANDARIZADOS
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
}
