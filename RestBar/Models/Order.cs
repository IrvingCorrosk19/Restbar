using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models;

public partial class Order : ITrackableEntity
{
    public Guid Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public Guid? TableId { get; set; }

    public Guid? CustomerId { get; set; }

    public Guid? UserId { get; set; }

    public OrderType OrderType { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public decimal? TotalAmount { get; set; }

    [Column(TypeName = "timestamp with time zone")]
    public DateTime? OpenedAt { get; set; }

    [Column(TypeName = "timestamp with time zone")]
    public DateTime? ClosedAt { get; set; }

    public string? Notes { get; set; }

    public Guid? CompanyId { get; set; }

    // Tracking fields
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public virtual Company? Company { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Table? Table { get; set; }

    public virtual User? User { get; set; }
}
