using System;
using System.Collections.Generic;

namespace RestBar.Models;

public partial class Area
{
    public Guid Id { get; set; }

    public Guid? BranchId { get; set; }

    public Guid? CompanyId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual Company? Company { get; set; }

    public virtual ICollection<Table> Tables { get; set; } = new List<Table>();
    
    // Relación con estaciones ubicadas en esta área
    public virtual ICollection<Station> Stations { get; set; } = new List<Station>();
}
