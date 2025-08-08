using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models
{
    public class JournalEntry
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(20)]
        public string EntryNumber { get; set; } = string.Empty;

        [Required]
        public DateTime EntryDate { get; set; }

        [Required]
        public JournalEntryType Type { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Reference { get; set; }

        [Required]
        public JournalEntryStatus Status { get; set; } = JournalEntryStatus.Draft;

        public DateTime? PostedAt { get; set; }

        public string? PostedBy { get; set; }

        [Required]
        public decimal TotalDebit { get; set; }

        [Required]
        public decimal TotalCredit { get; set; }

        [Required]
        public bool IsBalanced => TotalDebit == TotalCredit;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public string? CreatedBy { get; set; }

        public string? UpdatedBy { get; set; }

        // Relaciones
        public ICollection<JournalEntryDetail> Details { get; set; } = new List<JournalEntryDetail>();

        // Relación con órdenes (opcional)
        public Guid? OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        // Relación con pagos (opcional)
        public Guid? PaymentId { get; set; }

        [ForeignKey("PaymentId")]
        public Payment? Payment { get; set; }
    }

    public class JournalEntryDetail
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid JournalEntryId { get; set; }

        [ForeignKey("JournalEntryId")]
        public JournalEntry JournalEntry { get; set; } = null!;

        [Required]
        public Guid AccountId { get; set; }

        [ForeignKey("AccountId")]
        public Account Account { get; set; } = null!;

        [Required]
        public decimal DebitAmount { get; set; }

        [Required]
        public decimal CreditAmount { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Reference { get; set; }

        // Propiedades calculadas
        [NotMapped]
        public decimal NetAmount => DebitAmount - CreditAmount;

        [NotMapped]
        public bool IsDebit => DebitAmount > 0;

        [NotMapped]
        public bool IsCredit => CreditAmount > 0;
    }

    public enum JournalEntryType
    {
        Opening = 1,      // Apertura
        Regular = 2,      // Regular
        Adjustment = 3,   // Ajuste
        Closing = 4,      // Cierre
        Recurring = 5     // Recurrente
    }

    public enum JournalEntryStatus
    {
        Draft = 1,        // Borrador
        Posted = 2,       // Registrado
        Voided = 3        // Anulado
    }
} 