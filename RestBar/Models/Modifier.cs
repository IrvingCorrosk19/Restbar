using System;
using System.Collections.Generic;

namespace RestBar.Models;

public partial class Modifier
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public decimal? ExtraCost { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
