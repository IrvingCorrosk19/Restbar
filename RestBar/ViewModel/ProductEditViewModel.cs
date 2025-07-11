using System;
using System.ComponentModel.DataAnnotations;

namespace RestBar.ViewModel
{
    public class ProductEditViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser mayor o igual a 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "El costo es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El costo debe ser mayor o igual a 0")]
        public decimal Cost { get; set; }

        [Range(0, 100, ErrorMessage = "El impuesto debe estar entre 0 y 100")]
        public decimal TaxRate { get; set; }

        public string Unit { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? StationId { get; set; }
    }
} 