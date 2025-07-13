using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestBar.Models;

public partial class Area : ITrackableEntity
{
    public Guid Id { get; set; }

    public Guid? BranchId { get; set; }

    // ✅ NUEVO: Propiedad multi-tenant
    public Guid? CompanyId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // ✅ CAMPOS DE AUDITORÍA ESTANDARIZADOS
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // Propiedades de navegación
    public virtual Branch? Branch { get; set; }
    public virtual Company? Company { get; set; }

    public virtual ICollection<Table> Tables { get; set; } = new List<Table>();
    
    // Relación con estaciones ubicadas en esta área
    public virtual ICollection<Station> Stations { get; set; } = new List<Station>();
}
