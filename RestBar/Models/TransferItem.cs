using System.ComponentModel.DataAnnotations;

namespace RestBar.Models
{
    public class TransferItem
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid TransferId { get; set; }
        
        [Required]
        public Guid ProductId { get; set; }
        
        [Required]
        public decimal UnitPrice { get; set; }
        
        [Required]
        public int Quantity { get; set; }
        
        [Required]
        public decimal Subtotal { get; set; }
        
        [Required]
        public decimal TaxRate { get; set; }
        
        [Required]
        public decimal TaxAmount { get; set; }
        
        [Required]
        public decimal TotalAmount { get; set; }
        
        public int? ReceivedQuantity { get; set; }
        
        [StringLength(200)]
        public string? Notes { get; set; }
        
        [Required]
        public bool IsActive { get; set; } = true;
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual Transfer Transfer { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
} 