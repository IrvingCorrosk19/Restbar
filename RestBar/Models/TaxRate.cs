using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models
{
    public class TaxRate
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal Rate { get; set; } = 0.0m;
        
        [StringLength(20)]
        public string? TaxCode { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public Guid? CompanyId { get; set; }
        public virtual Company? Company { get; set; }
    }
} 