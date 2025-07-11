using System;
using System.Collections.Generic;

namespace RestBar.Models;

public partial class Table
{
    public Guid Id { get; set; }

    public Guid? AreaId { get; set; }

    public string? TableNumber { get; set; }

    public int? Capacity { get; set; }

    public string? Status { get; set; }

    public bool? IsActive { get; set; }

    public virtual Area? Area { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}

public enum TableStatus
{
    Disponible,
    Ocupada,
    Reservada,
    EnEspera,
    Atendida,
    EnPreparacion,
    Servida,
    ParaPago,
    Pagada,
    Bloqueada
}
