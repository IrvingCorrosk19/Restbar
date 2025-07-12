using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models
{
    public class OrderCancellationLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }
        public Guid? UserId { get; set; } // Quien solicita - ahora nullable
        public Guid? SupervisorId { get; set; } // Quien autoriza
        public string Reason { get; set; }
        
        [Column(TypeName = "timestamp with time zone")]
        public DateTime Date { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified); // Ahora compatible con PostgreSQL
        public string Products { get; set; } // JSON de productos afectados

        // Navigation properties
        public virtual Order Order { get; set; }
        public virtual User? User { get; set; } // Ahora nullable
        public virtual User? Supervisor { get; set; }
    }
} 