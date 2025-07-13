using System;
using System.Collections.Generic;

namespace RestBar.Models;

public partial class Branch : ITrackableEntity
{
    public Guid Id { get; set; }

    public Guid? CompanyId { get; set; }

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public bool IsActive { get; set; } = true;

    // ✅ CAMPOS DE AUDITORÍA ESTANDARIZADOS
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public virtual ICollection<Area> Areas { get; set; } = new List<Area>();

    public virtual Company? Company { get; set; }



    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
