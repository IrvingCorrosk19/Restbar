using System;
using System.Collections.Generic;

namespace RestBar.Models;

public partial class User : ITrackableEntity
{
    public Guid Id { get; set; }

    public Guid? BranchId { get; set; }

    public string? FullName { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public UserRole Role { get; set; }  // Aquí el cambio importante

    public bool IsActive { get; set; } = true;

    // ✅ CAMPOS DE AUDITORÍA ESTANDARIZADOS
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual Branch? Branch { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}

public enum UserRole
{
    superadmin,     // Super Administrador del Sistema
    admin,          // Administrador del Sistema
    manager,        // Gerente de Sucursal
    supervisor,     // Supervisor
    waiter,         // Mesero
    cashier,        // Cajero
    chef,           // Cocinero
    bartender,      // Bartender

    accountant,     // Contador
    support         // Soporte Técnico
}