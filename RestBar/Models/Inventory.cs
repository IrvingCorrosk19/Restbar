using System;
using System.Collections.Generic;

namespace RestBar.Models;

public partial class Inventory
{
    public Guid Id { get; set; }

    public Guid? BranchId { get; set; }

    public Guid? ProductId { get; set; }

    public decimal Quantity { get; set; }

    public string? Unit { get; set; }

    public decimal? MinThreshold { get; set; }

    public DateTime? LastUpdated { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual Product? Product { get; set; }
}
