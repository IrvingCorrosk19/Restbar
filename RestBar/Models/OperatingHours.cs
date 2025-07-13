using System;
using System.ComponentModel.DataAnnotations;

namespace RestBar.Models
{
    public class OperatingHours
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(20)]
        public string DayOfWeek { get; set; } = null!; // Monday, Tuesday, etc.
        
        public TimeSpan? OpenTime { get; set; }
        
        public TimeSpan? CloseTime { get; set; }
        
        public bool IsOpen { get; set; } = true;
        
        [StringLength(200)]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public Guid? CompanyId { get; set; }
        public virtual Company? Company { get; set; }
    }
} 