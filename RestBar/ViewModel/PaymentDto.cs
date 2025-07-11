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
        
        public List<SplitPaymentRequestDto>? SplitPayments { get; set; }
    }

    public class SplitPaymentRequestDto
    {
        [Required]
        public string PersonName { get; set; } = string.Empty;
        
        [Required]
        public decimal Amount { get; set; }
    }

    public class PaymentResponseDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public DateTime PaidAt { get; set; }
        public bool IsVoided { get; set; }
        public List<SplitPaymentResponseDto> SplitPayments { get; set; } = new();
    }

    public class SplitPaymentResponseDto
    {
        public Guid Id { get; set; }
        public string PersonName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class OrderPaymentSummaryDto
    {
        public Guid OrderId { get; set; }
        public decimal TotalOrderAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public List<PaymentResponseDto> Payments { get; set; } = new();
    }
} 