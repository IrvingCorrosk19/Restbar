using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface IJournalEntryService
    {
        // Operaciones básicas de asientos
        Task<IEnumerable<JournalEntry>> GetAllJournalEntriesAsync();
        Task<JournalEntry?> GetJournalEntryByIdAsync(Guid id);
        Task<JournalEntry?> GetJournalEntryByNumberAsync(string entryNumber);
        Task<JournalEntry> CreateJournalEntryAsync(JournalEntry journalEntry);
        Task<JournalEntry> UpdateJournalEntryAsync(JournalEntry journalEntry);
        Task<bool> DeleteJournalEntryAsync(Guid id);
        
        // Operaciones por tipo y estado
        Task<IEnumerable<JournalEntry>> GetJournalEntriesByTypeAsync(JournalEntryType type);
        Task<IEnumerable<JournalEntry>> GetJournalEntriesByStatusAsync(JournalEntryStatus status);
        Task<IEnumerable<JournalEntry>> GetJournalEntriesByDateRangeAsync(DateTime startDate, DateTime endDate);
        
        // Operaciones de posting
        Task<bool> PostJournalEntryAsync(Guid id, string postedBy);
        Task<bool> VoidJournalEntryAsync(Guid id, string voidedBy, string reason);
        Task<bool> UnpostJournalEntryAsync(Guid id, string unpostedBy);
        
        // Operaciones de numeración
        Task<string> GenerateEntryNumberAsync(JournalEntryType type);
        Task<bool> IsEntryNumberUniqueAsync(string entryNumber);
        
        // Operaciones de validación
        Task<bool> ValidateJournalEntryAsync(JournalEntry journalEntry);
        Task<bool> IsJournalEntryBalancedAsync(JournalEntry journalEntry);
        Task<bool> CanPostJournalEntryAsync(Guid id);
        Task<bool> CanVoidJournalEntryAsync(Guid id);
        
        // Operaciones de balance
        Task<decimal> GetAccountBalanceAsync(Guid accountId, DateTime? asOfDate = null);
        Task<Dictionary<Guid, decimal>> GetAccountsBalanceAsync(IEnumerable<Guid> accountIds, DateTime? asOfDate = null);
        
        // Operaciones de reportes
        Task<IEnumerable<JournalEntry>> GetJournalEntriesForTrialBalanceAsync(DateTime asOfDate);
        Task<IEnumerable<JournalEntry>> GetJournalEntriesForGeneralLedgerAsync(Guid accountId, DateTime startDate, DateTime endDate);
        
        // Operaciones automáticas
        Task<JournalEntry> CreateAutomaticEntryAsync(string description, IEnumerable<JournalEntryDetail> details, JournalEntryType type = JournalEntryType.Regular);
        Task<JournalEntry> CreateRecurringEntryAsync(JournalEntry template, DateTime entryDate);
        
        // Operaciones de integración
        Task<JournalEntry?> CreateEntryFromOrderAsync(Guid orderId);
        Task<JournalEntry?> CreateEntryFromPaymentAsync(Guid paymentId);
        
        // Operaciones de auditoría
        Task<IEnumerable<JournalEntry>> GetJournalEntriesByUserAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<JournalEntry>> GetModifiedJournalEntriesAsync(DateTime startDate, DateTime endDate);
    }
} 