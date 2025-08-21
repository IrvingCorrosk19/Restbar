using System;
using System.ComponentModel.DataAnnotations;

namespace RestBar.Models
{
    public class BackupSettings
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string BackupType { get; set; } = null!; // Database, Files, Full
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        [StringLength(500)]
        public string? BackupPath { get; set; }
        
        public bool IsEnabled { get; set; } = true;
        
        public int? RetentionDays { get; set; }
        
        public string? Schedule { get; set; } // Cron expression or schedule
        
        public DateTime? LastBackupDate { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public Guid? CompanyId { get; set; }
        public virtual Company? Company { get; set; }
    }
} 