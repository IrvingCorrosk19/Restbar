using System.ComponentModel.DataAnnotations;

namespace RestBar.ViewModel
{
    public class PaymentRequestDto
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Method { get; set; } = string.Empty;

        public bool IsShared { get; set; } = false;

        public string? PayerName { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "La propina no puede ser negativa")]
        public decimal TipAmount { get; set; } = 0;

        public List<SplitPaymentRequestDto>? SplitPayments { get; set; }

        /// <summary>
        /// UUID v4 único por intento de pago. Generado por el cliente antes de enviar.
        /// Permite reintentos seguros sin duplicar pagos.
        /// Formato: GUID string, ej: "f47ac10b-58cc-4372-a567-0e02b2c3d479"
        /// </summary>
        [StringLength(100)]
        public string? IdempotencyKey { get; set; }
    }

    public class SplitPaymentRequestDto
    {
        [Required]
        public string PersonName { get; set; } = string.Empty;
        
        [Required]
        public decimal Amount { get; set; }
        
        [Required]
        public string Method { get; set; } = string.Empty;
    }

    public class PaymentResponseDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public DateTime PaidAt { get; set; }
        public bool IsVoided { get; set; }
        public bool IsShared { get; set; }
        public string? PayerName { get; set; }
        public decimal TipAmount { get; set; }
        public List<SplitPaymentResponseDto> SplitPayments { get; set; } = new();
    }

    public class SplitPaymentResponseDto
    {
        public Guid Id { get; set; }
        public string PersonName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
    }

    public class OrderPaymentSummaryDto
    {
        public Guid OrderId { get; set; }
        public decimal TotalOrderAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public List<PaymentResponseDto> Payments { get; set; } = new();
        /// <summary>OBS-2: true cuando totalPaid &gt; orderTotal (sobrepago). Opcional para clientes existentes.</summary>
        public bool? IsOverpaid { get; set; }
        /// <summary>OBS-2: monto en exceso cuando hay sobrepago. Opcional.</summary>
        public decimal? OverpaidAmount { get; set; }
        /// <summary>OBS-2: "OVERPAID" cuando hay sobrepago. Opcional.</summary>
        public string? WarningCode { get; set; }
    }
} 