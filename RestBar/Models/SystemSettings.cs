using System;
using System.ComponentModel.DataAnnotations;

namespace RestBar.Models
{
    public class SystemSettings
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string SettingKey { get; set; } = null!;
        
        [StringLength(500)]
        public string? SettingValue { get; set; }
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        [StringLength(50)]
        public string? Category { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public Guid? CompanyId { get; set; }
        public virtual Company? Company { get; set; }
    }
} 