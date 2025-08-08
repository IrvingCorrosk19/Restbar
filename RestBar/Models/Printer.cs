using System;
using System.ComponentModel.DataAnnotations;

namespace RestBar.Models
{
    public class Printer
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        [Required]
        [StringLength(100)]
        public string PrinterType { get; set; } = null!; // Ticket, Report, Kitchen
        
        [StringLength(100)]
        public string? IpAddress { get; set; }
        
        [StringLength(50)]
        public string? Port { get; set; }
        
        [StringLength(100)]
        public string? DriverName { get; set; }
        
        public bool IsDefault { get; set; } = false;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public Guid? CompanyId { get; set; }
        public virtual Company? Company { get; set; }
    }
} 