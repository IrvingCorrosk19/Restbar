using System;
using System.Collections.Generic;

namespace RestBar.Models;

public partial class User : ITrackableEntity
{
    public Guid Id { get; set; }

    public Guid? BranchId { get; set; }

    public Guid? CompanyId { get; set; } // ✅ Agregado - coincide con company_id en la BD

    public string? Username { get; set; } // ✅ Agregado - coincide con "Username" en la BD

    public string? FirstName { get; set; } // ✅ Agregado - coincide con "FirstName" en la BD

    public string? LastName { get; set; } // ✅ Agregado - coincide con "LastName" en la BD

    public string? FullName { get; set; } // ✅ Ya existe - coincide con full_name en la BD

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public UserRole Role { get; set; }  // Aquí el cambio importante

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual Branch? Branch { get; set; }

    public virtual Company? Company { get; set; } // ✅ Agregado - navegación hacia Company

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
    
    accountant,     // Contador
    support         // Soporte Técnico
}