using System;
using System.Collections.Generic;

namespace RestBar.Models;

public partial class Invoice
{
    public Guid Id { get; set; }

    public Guid? OrderId { get; set; }

    public Guid? CustomerId { get; set; }

    public decimal? Total { get; set; }

    public decimal? Tax { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Order? Order { get; set; }
}
