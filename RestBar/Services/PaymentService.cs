using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly RestBarContext _context;

        public PaymentService(RestBarContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Include(p => p.SplitPayments)
                .ToListAsync();
        }

        public async Task<Payment?> GetByIdAsync(Guid id)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Include(p => p.SplitPayments)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Payment> CreateAsync(Payment payment)
        {
            payment.PaidAt = DateTime.UtcNow;
            
            // Validación de desarrollo para asegurar que las fechas sean UTC
            if (payment.PaidAt.HasValue && payment.PaidAt.Value.Kind == DateTimeKind.Unspecified)
                throw new InvalidOperationException("PaidAt no debe ser Unspecified para columnas timestamp with time zone");
            
            payment.IsVoided = false;
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task UpdateAsync(Payment payment)
        {
            _context.Entry(payment).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Payment>> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Include(p => p.SplitPayments)
                .Where(p => p.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByMethodAsync(string method)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Include(p => p.SplitPayments)
                .Where(p => p.Method == method)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Include(p => p.SplitPayments)
                .Where(p => p.PaidAt >= startDate && p.PaidAt <= endDate)
                .ToListAsync();
        }

        public async Task<Payment?> GetPaymentWithSplitsAsync(Guid id)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Include(p => p.SplitPayments)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<decimal> GetTotalPaymentsByOrderAsync(Guid orderId)
        {
            return await _context.Payments
                .Where(p => p.OrderId == orderId && p.IsVoided == false)
                .SumAsync(p => p.Amount);
        }

        public async Task VoidPaymentAsync(Guid id)
        {
            try
            {
                Console.WriteLine($"[PaymentService] VoidPaymentAsync iniciado para paymentId: {id}");
                
                var payment = await _context.Payments
                    .Include(p => p.Order)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (payment == null)
                {
                    Console.WriteLine($"[PaymentService] ERROR: Pago no encontrado con ID {id}");
                    throw new KeyNotFoundException($"No se encontró el pago con ID {id}");
                }

                if (payment.IsVoided == true)
                {
                    Console.WriteLine($"[PaymentService] ERROR: Pago ya está anulado");
                    throw new InvalidOperationException("El pago ya está anulado");
                }

                Console.WriteLine($"[PaymentService] Anulando pago - Amount: {payment.Amount}, Method: {payment.Method}");
                
                payment.IsVoided = true;
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"[PaymentService] Pago anulado exitosamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PaymentService] ERROR en VoidPaymentAsync: {ex.Message}");
                Console.WriteLine($"[PaymentService] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<IEnumerable<Payment>> GetVoidedPaymentsAsync()
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Include(p => p.SplitPayments)
                .Where(p => p.IsVoided == true)
                .ToListAsync();
        }
    }
} 