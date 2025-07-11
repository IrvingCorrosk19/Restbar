using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models;

public partial class Payment
{
    public Guid Id { get; set; }

    public Guid? OrderId { get; set; }

    public string? Method { get; set; }

    public decimal Amount { get; set; }

    [Column(TypeName = "timestamp with time zone")]
    public DateTime? PaidAt { get; set; }

    public bool? IsVoided { get; set; }

    public virtual Order? Order { get; set; }

    public virtual ICollection<SplitPayment> SplitPayments { get; set; } = new List<SplitPayment>();
}
