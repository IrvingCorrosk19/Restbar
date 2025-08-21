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

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public virtual ICollection<Area> Areas { get; set; } = new List<Area>();

    public virtual Company? Company { get; set; }

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
