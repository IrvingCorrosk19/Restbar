using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestBar.Models;

public partial class Table : ITrackableEntity
{
    public Guid Id { get; set; }

    public Guid? AreaId { get; set; }

    public Guid? CompanyId { get; set; }

    public Guid? BranchId { get; set; }

    [Required]
    [StringLength(20)]
    public string TableNumber { get; set; } = null!;

    [Range(1, 20, ErrorMessage = "La capacidad debe estar entre 1 y 20 personas")]
    public int Capacity { get; set; } = 4;

    public TableStatus Status { get; set; } = TableStatus.Disponible;

    public bool IsActive { get; set; } = true;

    // ✅ CAMPOS DE AUDITORÍA ESTANDARIZADOS
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // Propiedades de navegación
    public virtual Area? Area { get; set; }
    public virtual Company? Company { get; set; }
    public virtual Branch? Branch { get; set; }

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
