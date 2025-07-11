using System;
using System.Collections.Generic;

namespace RestBar.Models;

public partial class User
{
    public Guid Id { get; set; }

    public Guid? BranchId { get; set; }

    public string? FullName { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public UserRole Role { get; set; }  // Aquí el cambio importante

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual Branch? Branch { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}

public enum UserRole
{
    admin,          // Administrador del Sistema
    manager,        // Gerente de Sucursal
    supervisor,     // Supervisor
    waiter,         // Mesero
    cashier,        // Cajero
    chef,           // Cocinero
    bartender,      // Bartender
    inventory,      // Inventarista
    accountant,     // Contador
    support         // Soporte Técnico
}