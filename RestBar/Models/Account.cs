using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestBar.Models
{
    public class Account
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public AccountType Type { get; set; }

        [Required]
        public AccountCategory Category { get; set; }

        [Required]
        public AccountNature Nature { get; set; }

        public Guid? ParentAccountId { get; set; }

        [ForeignKey("ParentAccountId")]
        public Account? ParentAccount { get; set; }

        public ICollection<Account> SubAccounts { get; set; } = new List<Account>();

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public bool IsSystem { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public string? CreatedBy { get; set; }

        public string? UpdatedBy { get; set; }

        // Propiedades calculadas
        [NotMapped]
        public string FullCode => ParentAccount != null ? $"{ParentAccount.FullCode}.{Code}" : Code;

        [NotMapped]
        public string FullName => ParentAccount != null ? $"{ParentAccount.FullName} - {Name}" : Name;

        [NotMapped]
        public int Level => ParentAccount?.Level + 1 ?? 0;

        [NotMapped]
        public bool HasChildren => SubAccounts.Any();
    }

    public enum AccountType
    {
        Asset = 1,        // Activo
        Liability = 2,    // Pasivo
        Equity = 3,       // Capital
        Income = 4,       // Ingreso
        Expense = 5       // Gasto
    }

    public enum AccountCategory
    {
        // Activos
        CurrentAssets = 1,        // Activos Corrientes
        NonCurrentAssets = 2,     // Activos No Corrientes
        
        // Pasivos
        CurrentLiabilities = 3,   // Pasivos Corrientes
        NonCurrentLiabilities = 4, // Pasivos No Corrientes
        
        // Capital
        Capital = 5,              // Capital
        RetainedEarnings = 6,     // Utilidades Retenidas
        
        // Ingresos
        OperatingIncome = 7,      // Ingresos Operativos
        NonOperatingIncome = 8,   // Ingresos No Operativos
        
        // Gastos
        OperatingExpenses = 9,    // Gastos Operativos
        NonOperatingExpenses = 10 // Gastos No Operativos
    }

    public enum AccountNature
    {
        Debit = 1,   // Deudora
        Credit = 2   // Acredora
    }
} 