using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models
{
    public class PurchaseOrder
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        public Guid SupplierId { get; set; }

        [ForeignKey("SupplierId")]
        public virtual Supplier? Supplier { get; set; }

        [Required]
        public Guid CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }

        [Required]
        public Guid BranchId { get; set; }

        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }

        [Required]
        public Guid CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual User? CreatedBy { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        public DateTime? ActualDeliveryDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public PurchaseOrderStatus Status { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    }

    public enum PurchaseOrderStatus
    {
        Draft = 0,
        Pending = 1,
        Approved = 2,
        Ordered = 3,
        PartiallyReceived = 4,
        Received = 5,
        Cancelled = 6
    }
} 