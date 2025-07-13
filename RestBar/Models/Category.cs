using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestBar.Models
{
    public class Category : ITrackableEntity
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // ✅ NUEVO: Propiedades multi-tenant
        [Display(Name = "Compañía")]
        public Guid? CompanyId { get; set; }

        [Display(Name = "Sucursal")]
        public Guid? BranchId { get; set; }

        // ✅ CAMPOS DE AUDITORÍA ESTANDARIZADOS
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        // Relación con Productos
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        
        // Propiedades de navegación multi-tenant
        public virtual Company? Company { get; set; }
        public virtual Branch? Branch { get; set; }
    }
} 