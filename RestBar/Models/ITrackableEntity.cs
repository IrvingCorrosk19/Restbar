using System;

namespace RestBar.Models
{
    /// <summary>
    /// Interfaz base para entidades que requieren tracking automático de fechas y usuarios
    /// </summary>
    public interface ITrackableEntity
    {
        /// <summary>
        /// Fecha y hora de creación del registro
        /// </summary>
        DateTime CreatedAt { get; set; }

        /// <summary>
        /// Fecha y hora de la última actualización del registro
        /// </summary>
        DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Usuario que creó el registro
        /// </summary>
        string? CreatedBy { get; set; }

        /// <summary>
        /// Usuario que realizó la última actualización del registro
        /// </summary>
        string? UpdatedBy { get; set; }
    }
} 