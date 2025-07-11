using System;
using System.Collections.Generic;

namespace RestBar.Models;

public partial class AuditLog
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string Action { get; set; } = null!;

    public string? TableName { get; set; }

    public Guid? RecordId { get; set; }

    public DateTime? Timestamp { get; set; }

    public virtual User? User { get; set; }
}
