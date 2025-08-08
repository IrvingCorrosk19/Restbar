using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models
{
    public class Currency
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(10)]
        public string Code { get; set; } = null!; // USD, EUR, etc.
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = null!;
        
        [StringLength(10)]
        public string? Symbol { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal ExchangeRate { get; set; } = 1.0m;
        
        public bool IsDefault { get; set; } = false;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public Guid? CompanyId { get; set; }
        public virtual Company? Company { get; set; }
    }
} 