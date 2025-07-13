using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models;

public partial class AuditLog
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    // ✅ NUEVOS CAMPOS PARA MULTI-TENANT
    public Guid? CompanyId { get; set; }
    public Guid? BranchId { get; set; }

    public string Action { get; set; } = null!;

    public string? TableName { get; set; }

    public Guid? RecordId { get; set; }

    public DateTime? Timestamp { get; set; }

    // ✅ NUEVOS CAMPOS PARA LOGGING DETALLADO
    [StringLength(50)]
    public string? LogLevel { get; set; } // INFO, WARNING, ERROR, CRITICAL

    [StringLength(100)]
    public string? Module { get; set; } // User, Order, Inventory, Supplier, etc.

    [StringLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "text")]
    public string? OldValues { get; set; } // JSON con valores anteriores

    [Column(TypeName = "text")]
    public string? NewValues { get; set; } // JSON con valores nuevos

    [Column(TypeName = "text")]
    public string? ErrorDetails { get; set; } // Detalles del error en JSON

    [StringLength(200)]
    public string? IpAddress { get; set; }

    [StringLength(500)]
    public string? UserAgent { get; set; }

    [StringLength(100)]
    public string? SessionId { get; set; }

    public bool IsError { get; set; } = false;

    public int? ErrorCode { get; set; }

    [StringLength(200)]
    public string? ExceptionType { get; set; }

    [Column(TypeName = "text")]
    public string? StackTrace { get; set; }

    // Relaciones
    public virtual User? User { get; set; }
    public virtual Company? Company { get; set; }
    public virtual Branch? Branch { get; set; }
}

// ✅ NUEVO: Enums para tipos de logging
public enum AuditLogLevel
{
    INFO,
    WARNING,
    ERROR,
    CRITICAL
}

public enum AuditAction
{
    CREATE,
    UPDATE,
    DELETE,
    LOGIN,
    LOGOUT,
    ERROR,
    SYSTEM,
    SECURITY,
    DATA_EXPORT,
    DATA_IMPORT,
    BACKUP,
    RESTORE,
    CONFIGURATION_CHANGE
}

public enum AuditModule
{
    USER,
    ORDER,
    
    PRODUCT,
    PAYMENT,
    ACCOUNTING,
    REPORT,
    SYSTEM,
    SECURITY,
    BACKUP,
    CONFIGURATION
}
