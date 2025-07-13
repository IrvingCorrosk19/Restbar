using System;
using System.ComponentModel.DataAnnotations;

namespace RestBar.Models
{
    /// <summary>
    /// Template de email reutilizable
    /// </summary>
    public class EmailTemplate
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!; // Nombre único del template

        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = null!; // Asunto del email

        [Required]
        public string Body { get; set; } = null!; // Cuerpo del email (HTML)

        [StringLength(500)]
        public string? Description { get; set; } // Descripción del template

        [StringLength(100)]
        public string? Category { get; set; } // Categoría: Order, User, Inventory, Report, etc.

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public Guid? CompanyId { get; set; }
        public virtual Company? Company { get; set; }

        // Placeholders disponibles en el template (ej: {{UserName}}, {{OrderNumber}})
        [StringLength(1000)]
        public string? Placeholders { get; set; } // JSON array de placeholders disponibles
    }
}

