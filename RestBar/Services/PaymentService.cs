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
            // FIX: Una única transacción ACID — antes había doble SaveChanges sin transacción
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var payment = await _context.Payments
                    .Include(p => p.Order)
                        .ThenInclude(o => o.Table)
                    .Include(p => p.Order)
                        .ThenInclude(o => o.OrderItems)
                    .Include(p => p.SplitPayments)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (payment == null)
                    throw new KeyNotFoundException($"No se encontró el pago con ID {id}");

                if (payment.IsVoided)
                    throw new InvalidOperationException("El pago ya está anulado");

                if (payment.OrderId == null)
                    throw new InvalidOperationException("El pago no tiene orden asociada");

                var order = payment.Order!;

                // Anular el pago (y sus splits, que son informativos; no tienen estado propio)
                payment.IsVoided = true;

                // FIX: Calcular totalPaid restante EXCLUYENDO el pago actual (ya marcado IsVoided=true en memoria)
                // y EXCLUYENDO otros pagos ya anulados. Se hace con query para evitar datos stale.
                var totalPaidRemaining = await _context.Payments
                    .Where(p => p.OrderId == order.Id && p.Id != id && !p.IsVoided)
                    .SumAsync(p => p.Amount);

                // FIX: orderTotal correcto — excluye ítems cancelados y aplica descuentos
                var orderTotal = order.OrderItems?
                    .Where(oi => oi.Status != OrderItemStatus.Cancelled)
                    .Sum(oi => oi.Quantity * oi.UnitPrice - oi.Discount) ?? 0;

                // Actualizar estado de orden si estaba cerrada y ahora hay saldo pendiente
                if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Served)
                {
                    if (totalPaidRemaining < orderTotal - 0.01m)
                    {
                        order.Status = OrderStatus.ReadyToPay;
                        if (order.Table != null)
                            order.Table.Status = TableStatus.ParaPago;
                    }
                    // Si totalPaidRemaining >= orderTotal, la orden sigue correctamente cerrada
                }
                // ReadyToPay se mantiene: ya indica que hay saldo pendiente

                order.Version++;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
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