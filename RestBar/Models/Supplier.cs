using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models
{
    public class Supplier : ITrackableEntity
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        [Column("name")]
        public string Name { get; set; } = null!;

        [StringLength(200)]
        [Column("contact_person")]
        public string? ContactPerson { get; set; }

        [StringLength(200)]
        [Column("email")]
        public string? Email { get; set; }

        [StringLength(50)]
        [Column("phone")]
        public string? Phone { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [StringLength(100)]
        [Column("city")]
        public string? City { get; set; }

        [StringLength(100)]
        [Column("state")]
        public string? State { get; set; }

        [StringLength(20)]
        [Column("postal_code")]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        [Column("country")]
        public string? Country { get; set; }

        [StringLength(50)]
        [Column("tax_id")]
        public string? TaxId { get; set; }

        [StringLength(200)]
        [Column("payment_terms")]
        public string? PaymentTerms { get; set; }

        [Column("lead_time_days")]
        public int? LeadTimeDays { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("created_by")]
        public string? CreatedBy { get; set; }

        [Column("updated_by")]
        public string? UpdatedBy { get; set; }

        [Column("company_id")]
        public Guid? CompanyId { get; set; }

        // Propiedades adicionales que están en la base de datos
        [StringLength(500)]
        [Column("Description")]
        public string? Description { get; set; }

        [StringLength(20)]
        [Column("Fax")]
        public string? Fax { get; set; }

        [StringLength(50)]
        [Column("AccountNumber")]
        public string? AccountNumber { get; set; }

        [StringLength(100)]
        [Column("Website")]
        public string? Website { get; set; }

        [StringLength(500)]
        [Column("Notes")]
        public string? Notes { get; set; }

        // Relaciones
        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

        // Relación con órdenes de compra (cuando las implementemos)
        // public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    }
} 