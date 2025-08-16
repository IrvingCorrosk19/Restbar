using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestBar.Models
{
    public class Station : ITrackableEntity
    {
        public Guid Id { get; set; }
        
        [Required(ErrorMessage = "El nombre de la estación es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        [Display(Name = "Nombre de la Estación")]
        public string Name { get; set; } = null!;
        
        [Required(ErrorMessage = "El tipo de estación es requerido")]
        [StringLength(50, ErrorMessage = "El tipo no puede tener más de 50 caracteres")]
        [Display(Name = "Tipo de Estación")]
        public string Type { get; set; } = null!; // Cocina, Bar, Café, etc.
        
        [StringLength(50)]
        [Display(Name = "Ícono")]
        public string? Icon { get; set; }

        [Display(Name = "Área")]
        public Guid? AreaId { get; set; }

        [Display(Name = "Compañía")]
        public Guid? CompanyId { get; set; }

        [Display(Name = "Sucursal")]
        public Guid? BranchId { get; set; }

        [Display(Name = "Estado Activo")]
        public bool IsActive { get; set; } = true;

        // Propiedades de tracking (ITrackableEntity)
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        // Propiedades de navegación
        public virtual Area? Area { get; set; }
        public virtual Company? Company { get; set; }
        public virtual Branch? Branch { get; set; }
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        
        // Items preparados por esta estación
        public virtual ICollection<OrderItem> PreparedItems { get; set; } = new List<OrderItem>();
    }
} 