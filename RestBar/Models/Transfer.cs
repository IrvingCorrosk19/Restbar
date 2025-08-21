using System.ComponentModel.DataAnnotations;

namespace RestBar.Models
{
    public class Transfer
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string TransferNumber { get; set; } = string.Empty;
        
        [Required]
        public Guid SourceBranchId { get; set; }
        
        [Required]
        public Guid DestinationBranchId { get; set; }
        
        [Required]
        public Guid CompanyId { get; set; }
        
        [Required]
        public Guid CreatedById { get; set; }
        
        public Guid? ApprovedById { get; set; }
        
        public Guid? ReceivedById { get; set; }
        
        [Required]
        public DateTime TransferDate { get; set; }
        
        public DateTime? ExpectedDeliveryDate { get; set; }
        
        public DateTime? ActualDeliveryDate { get; set; }
        
        [Required]
        public decimal Subtotal { get; set; }
        
        [Required]
        public decimal TaxAmount { get; set; }
        
        [Required]
        public decimal TotalAmount { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        [Required]
        public TransferStatus Status { get; set; }
        
        [Required]
        public bool IsActive { get; set; } = true;
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual Branch SourceBranch { get; set; } = null!;
        public virtual Branch DestinationBranch { get; set; } = null!;
        public virtual Company Company { get; set; } = null!;
        public virtual User CreatedBy { get; set; } = null!;
        public virtual User? ApprovedBy { get; set; }
        public virtual User? ReceivedBy { get; set; }
        public virtual ICollection<TransferItem> Items { get; set; } = new List<TransferItem>();
    }
    
    public enum TransferStatus
    {
        Pending = 0,
        Approved = 1,
        InTransit = 2,
        Delivered = 3,
        Cancelled = 4,
        Rejected = 5
    }
} 