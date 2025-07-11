using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models;

public partial class OrderItem
{
    public Guid Id { get; set; }

    public Guid? OrderId { get; set; }

    public Guid? ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal? Discount { get; set; }

    public string? Notes { get; set; }

    public OrderItemStatus Status { get; set; } = OrderItemStatus.Pending;

    public Guid? PreparedByStationId { get; set; }

    [Column(TypeName = "timestamp with time zone")]
    public DateTime? PreparedAt { get; set; }

    // Nuevos campos para la lógica de cocina
    public KitchenStatus KitchenStatus { get; set; } = KitchenStatus.Pending;

    [Column(TypeName = "timestamp with time zone")]
    public DateTime? SentAt { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Product? Product { get; set; }

    public virtual Station? PreparedByStation { get; set; }
}
