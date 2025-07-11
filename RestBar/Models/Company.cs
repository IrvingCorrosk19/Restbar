using System;
using System.Collections.Generic;

namespace RestBar.Models;

public partial class Company
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? LegalId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
}
