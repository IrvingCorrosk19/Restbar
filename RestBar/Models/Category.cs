using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestBar.Models
{
    public class Category
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Relaciones con Company y Branch
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public virtual Company? Company { get; set; }
        public virtual Branch? Branch { get; set; }

        // Tracking fields
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }

        // Relación con Productos
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
} 