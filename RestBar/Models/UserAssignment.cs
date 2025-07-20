using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestBar.Models
{
    public class UserAssignment
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid UserId { get; set; }
        
        // Para cocineros y bartenders
        public Guid? StationId { get; set; }
        
        // Para meseros
        public Guid? AreaId { get; set; }
        
        // Mesas específicas asignadas al mesero (JSON array)
        public List<Guid>? AssignedTableIds { get; set; } = new List<Guid>();
        
        [Required]
        public DateTime AssignedAt { get; set; }
        
        public DateTime? UnassignedAt { get; set; }
        
        [Required]
        public bool IsActive { get; set; } = true;
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        // Navegación
        public virtual User User { get; set; } = null!;
        public virtual Station? Station { get; set; }
        public virtual Area? Area { get; set; }
    }
    
    // Enum para tipos de asignación
    public enum AssignmentType
    {
        Station,    // Cocineros y Bartenders
        Area,       // Meseros - área completa
        Tables      // Meseros - mesas específicas
    }
} 