using System;
using System.Collections.Generic;

namespace RestBar.Models;

public partial class Notification
{
    public Guid Id { get; set; }

    public Guid? OrderId { get; set; }

    public string? Message { get; set; }

    public bool? IsRead { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Order? Order { get; set; }
}
