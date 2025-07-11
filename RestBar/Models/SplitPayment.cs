using System;
using System.Collections.Generic;

namespace RestBar.Models;

public partial class SplitPayment
{
    public Guid Id { get; set; }

    public Guid? PaymentId { get; set; }

    public string? PersonName { get; set; }

    public decimal? Amount { get; set; }

    public virtual Payment? Payment { get; set; }
}
