using Microsoft.EntityFrameworkCore;

using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services
{
    public class SplitPaymentService : ISplitPaymentService
    {
        private readonly RestBarContext _context;

        public SplitPaymentService(RestBarContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SplitPayment>> GetAllAsync()
        {
            return await _context.SplitPayments
                .Include(sp => sp.Payment)
                .ToListAsync();
        }

        public async Task<SplitPayment?> GetByIdAsync(Guid id)
        {
            return await _context.SplitPayments
                .Include(sp => sp.Payment)
                .FirstOrDefaultAsync(sp => sp.Id == id);
        }

        public async Task<SplitPayment> CreateAsync(SplitPayment splitPayment)
        {
            try
            {
                Console.WriteLine($"[SplitPaymentService] Creando split payment:");
                Console.WriteLine($"[SplitPaymentService] Id: {splitPayment.Id}");
                Console.WriteLine($"[SplitPaymentService] PaymentId: {splitPayment.PaymentId}");
                Console.WriteLine($"[SplitPaymentService] PersonName: {splitPayment.PersonName}");
                Console.WriteLine($"[SplitPaymentService] Amount: ${splitPayment.Amount}");
                
                _context.SplitPayments.Add(splitPayment);
                Console.WriteLine($"[SplitPaymentService] Split payment agregado al contexto");
                
                await _context.SaveChangesAsync();
                Console.WriteLine($"[SplitPaymentService] ✅ Split payment guardado exitosamente");
                
                return splitPayment;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SplitPaymentService] ❌ ERROR al crear split payment:");
                Console.WriteLine($"[SplitPaymentService] Error Type: {ex.GetType().Name}");
                Console.WriteLine($"[SplitPaymentService] Error Message: {ex.Message}");
                Console.WriteLine($"[SplitPaymentService] Stack Trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[SplitPaymentService] Inner Exception: {ex.InnerException.Message}");
                }
                
                throw; // Re-lanzar la excepción para que sea manejada por el controlador
            }
        }

        public async Task UpdateAsync(SplitPayment splitPayment)
        {
            _context.Entry(splitPayment).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var splitPayment = await _context.SplitPayments.FindAsync(id);
            if (splitPayment != null)
            {
                _context.SplitPayments.Remove(splitPayment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<SplitPayment>> GetByPaymentIdAsync(Guid paymentId)
        {
            return await _context.SplitPayments
                .Include(sp => sp.Payment)
                .Where(sp => sp.PaymentId == paymentId)
                .ToListAsync();
        }

        public async Task<SplitPayment?> GetSplitPaymentWithDetailsAsync(Guid id)
        {
            return await _context.SplitPayments
                .Include(sp => sp.Payment)
                .FirstOrDefaultAsync(sp => sp.Id == id);
        }

        public async Task<decimal> GetTotalSplitAmountAsync(Guid paymentId)
        {
            return await _context.SplitPayments
                .Where(sp => sp.PaymentId == paymentId)
                .SumAsync(sp => sp.Amount ?? 0);
        }

        public async Task<IEnumerable<SplitPayment>> GetByAmountRangeAsync(decimal minAmount, decimal maxAmount)
        {
            return await _context.SplitPayments
                .Include(sp => sp.Payment)
                .Where(sp => sp.Amount >= minAmount && sp.Amount <= maxAmount)
                .ToListAsync();
        }

        public async Task<IEnumerable<SplitPayment>> GetByPersonNameAsync(string personName)
        {
            return await _context.SplitPayments
                .Include(sp => sp.Payment)
                .Where(sp => sp.PersonName == personName)
                .ToListAsync();
        }
    }
} 