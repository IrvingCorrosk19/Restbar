using System;
using System.ComponentModel.DataAnnotations;

namespace RestBar.Models
{
    public class NotificationSettings
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string NotificationType { get; set; } = null!; // Email, SMS, Push, etc.
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        public bool IsEnabled { get; set; } = true;
        
        [StringLength(500)]
        public string? Configuration { get; set; } // JSON configuration
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public Guid? CompanyId { get; set; }
        public virtual Company? Company { get; set; }
    }
} 