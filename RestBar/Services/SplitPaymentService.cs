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
            _context.SplitPayments.Add(splitPayment);
            await _context.SaveChangesAsync();
            return splitPayment;
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