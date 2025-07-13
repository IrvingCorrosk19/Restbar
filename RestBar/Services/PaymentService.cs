using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services
{
    public class PaymentService : BaseTrackingService, IPaymentService
    {


        public PaymentService(RestBarContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
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
            try
            {
                Console.WriteLine($"[PaymentService] Creando payment:");
                Console.WriteLine($"[PaymentService] Id: {payment.Id}");
                Console.WriteLine($"[PaymentService] OrderId: {payment.OrderId}");
                Console.WriteLine($"[PaymentService] Amount: ${payment.Amount}");
                Console.WriteLine($"[PaymentService] Method: {payment.Method}");
                
                payment.PaidAt = DateTime.UtcNow; // ✅ Fecha específica de pago
                Console.WriteLine($"[PaymentService] PaidAt configurado como UTC: {payment.PaidAt}");
                
                payment.IsVoided = false;
                Console.WriteLine($"[PaymentService] Payment configurado - IsVoided: {payment.IsVoided}");
                
                _context.Payments.Add(payment);
                Console.WriteLine($"[PaymentService] Payment agregado al contexto");
                
                await _context.SaveChangesAsync();
                Console.WriteLine($"[PaymentService] ✅ Payment guardado exitosamente");
                

                
                return payment;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PaymentService] ❌ ERROR al crear payment:");
                Console.WriteLine($"[PaymentService] Error Type: {ex.GetType().Name}");
                Console.WriteLine($"[PaymentService] Error Message: {ex.Message}");
                Console.WriteLine($"[PaymentService] Stack Trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[PaymentService] Inner Exception: {ex.InnerException.Message}");
                }
                
                throw; // Re-lanzar la excepción para que sea manejada por el controlador
            }
        }

        public async Task UpdateAsync(Payment payment)
        {
            try
            {
                // Buscar si hay una entidad con el mismo ID siendo rastreada
                var existingEntity = _context.ChangeTracker.Entries<Payment>()
                    .FirstOrDefault(e => e.Entity.Id == payment.Id);

                if (existingEntity != null)
                {
                    // Detach la entidad existente para evitar conflictos
                    existingEntity.State = EntityState.Detached;
                }

                // Usar Update para manejar automáticamente el tracking
                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al actualizar el pago en la base de datos.", ex);
            }
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

        public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Include(p => p.Order.Table)
                .Include(p => p.SplitPayments)
                .Where(p => p.PaidAt >= startDate && p.PaidAt <= endDate && p.IsVoided == false)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(string status)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Include(p => p.Order.Table)
                .Include(p => p.SplitPayments)
                .Where(p => p.Status.ToUpper() == status.ToUpper() && p.IsVoided == false)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByMethodAsync(string method)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Include(p => p.Order.Table)
                .Include(p => p.SplitPayments)
                .Where(p => p.Method == method && p.IsVoided == false)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Payments
                .Where(p => p.PaidAt >= startDate && p.PaidAt <= endDate && p.IsVoided == false)
                .SumAsync(p => p.Amount);
        }

        public async Task<int> GetTotalOrdersPaidAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Payments
                .Where(p => p.PaidAt >= startDate && p.PaidAt <= endDate && p.IsVoided == false)
                .Select(p => p.OrderId)
                .Distinct()
                .CountAsync();
        }
    }
} 