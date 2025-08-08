using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services
{
    public class JournalEntryService : IJournalEntryService
    {
        private readonly RestBarContext _context;
        private readonly ILogger<JournalEntryService> _logger;

        public JournalEntryService(RestBarContext context, ILogger<JournalEntryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<JournalEntry>> GetAllJournalEntriesAsync()
        {
            return await _context.JournalEntries
                .Include(j => j.Details)
                .ThenInclude(d => d.Account)
                .OrderByDescending(j => j.EntryDate)
                .ThenBy(j => j.EntryNumber)
                .ToListAsync();
        }

        public async Task<JournalEntry?> GetJournalEntryByIdAsync(Guid id)
        {
            return await _context.JournalEntries
                .Include(j => j.Details)
                .ThenInclude(d => d.Account)
                .Include(j => j.Order)
                .Include(j => j.Payment)
                .FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task<JournalEntry?> GetJournalEntryByNumberAsync(string entryNumber)
        {
            return await _context.JournalEntries
                .Include(j => j.Details)
                .ThenInclude(d => d.Account)
                .FirstOrDefaultAsync(j => j.EntryNumber == entryNumber);
        }

        public async Task<JournalEntry> CreateJournalEntryAsync(JournalEntry journalEntry)
        {
            // Validar el asiento
            if (!await ValidateJournalEntryAsync(journalEntry))
            {
                throw new InvalidOperationException("El asiento contable no es válido.");
            }

            // Verificar que esté balanceado
            if (!await IsJournalEntryBalancedAsync(journalEntry))
            {
                throw new InvalidOperationException("El asiento contable no está balanceado.");
            }

            // Generar número de asiento si no se proporciona
            if (string.IsNullOrEmpty(journalEntry.EntryNumber))
            {
                journalEntry.EntryNumber = await GenerateEntryNumberAsync(journalEntry.Type);
            }
            else
            {
                // Verificar que el número sea único
                if (!await IsEntryNumberUniqueAsync(journalEntry.EntryNumber))
                {
                    throw new InvalidOperationException($"El número de asiento '{journalEntry.EntryNumber}' ya existe.");
                }
            }

            journalEntry.Id = Guid.NewGuid();
            journalEntry.CreatedAt = DateTime.UtcNow;
            journalEntry.Status = JournalEntryStatus.Draft;

            // Calcular totales
            journalEntry.TotalDebit = journalEntry.Details.Sum(d => d.DebitAmount);
            journalEntry.TotalCredit = journalEntry.Details.Sum(d => d.CreditAmount);

            // Asignar IDs a los detalles
            foreach (var detail in journalEntry.Details)
            {
                detail.Id = Guid.NewGuid();
                detail.JournalEntryId = journalEntry.Id;
            }

            _context.JournalEntries.Add(journalEntry);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Asiento contable creado: {journalEntry.EntryNumber} - {journalEntry.Description}");
            return journalEntry;
        }

        public async Task<JournalEntry> UpdateJournalEntryAsync(JournalEntry journalEntry)
        {
            var existingEntry = await GetJournalEntryByIdAsync(journalEntry.Id);
            if (existingEntry == null)
            {
                throw new InvalidOperationException("El asiento contable no existe.");
            }

            // No permitir modificar asientos registrados
            if (existingEntry.Status == JournalEntryStatus.Posted)
            {
                throw new InvalidOperationException("No se pueden modificar asientos ya registrados.");
            }

            // Validar el asiento
            if (!await ValidateJournalEntryAsync(journalEntry))
            {
                throw new InvalidOperationException("El asiento contable no es válido.");
            }

            // Verificar que esté balanceado
            if (!await IsJournalEntryBalancedAsync(journalEntry))
            {
                throw new InvalidOperationException("El asiento contable no está balanceado.");
            }

            // Actualizar propiedades básicas
            existingEntry.EntryDate = journalEntry.EntryDate;
            existingEntry.Type = journalEntry.Type;
            existingEntry.Description = journalEntry.Description;
            existingEntry.Reference = journalEntry.Reference;
            existingEntry.UpdatedAt = DateTime.UtcNow;
            existingEntry.UpdatedBy = journalEntry.UpdatedBy;

            // Calcular totales
            existingEntry.TotalDebit = journalEntry.Details.Sum(d => d.DebitAmount);
            existingEntry.TotalCredit = journalEntry.Details.Sum(d => d.CreditAmount);

            // Actualizar detalles
            _context.JournalEntryDetails.RemoveRange(existingEntry.Details);
            
            foreach (var detail in journalEntry.Details)
            {
                detail.Id = Guid.NewGuid();
                detail.JournalEntryId = existingEntry.Id;
                _context.JournalEntryDetails.Add(detail);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Asiento contable actualizado: {existingEntry.EntryNumber}");
            return existingEntry;
        }

        public async Task<bool> DeleteJournalEntryAsync(Guid id)
        {
            var journalEntry = await GetJournalEntryByIdAsync(id);
            if (journalEntry == null)
            {
                return false;
            }

            // No permitir eliminar asientos registrados
            if (journalEntry.Status == JournalEntryStatus.Posted)
            {
                throw new InvalidOperationException("No se pueden eliminar asientos ya registrados.");
            }

            _context.JournalEntries.Remove(journalEntry);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Asiento contable eliminado: {journalEntry.EntryNumber}");
            return true;
        }

        public async Task<IEnumerable<JournalEntry>> GetJournalEntriesByTypeAsync(JournalEntryType type)
        {
            return await _context.JournalEntries
                .Include(j => j.Details)
                .ThenInclude(d => d.Account)
                .Where(j => j.Type == type)
                .OrderByDescending(j => j.EntryDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<JournalEntry>> GetJournalEntriesByStatusAsync(JournalEntryStatus status)
        {
            return await _context.JournalEntries
                .Include(j => j.Details)
                .ThenInclude(d => d.Account)
                .Where(j => j.Status == status)
                .OrderByDescending(j => j.EntryDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<JournalEntry>> GetJournalEntriesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.JournalEntries
                .Include(j => j.Details)
                .ThenInclude(d => d.Account)
                .Where(j => j.EntryDate >= startDate && j.EntryDate <= endDate)
                .OrderByDescending(j => j.EntryDate)
                .ToListAsync();
        }

        public async Task<bool> PostJournalEntryAsync(Guid id, string postedBy)
        {
            var journalEntry = await GetJournalEntryByIdAsync(id);
            if (journalEntry == null)
            {
                return false;
            }

            if (!await CanPostJournalEntryAsync(id))
            {
                throw new InvalidOperationException("No se puede registrar el asiento contable.");
            }

            journalEntry.Status = JournalEntryStatus.Posted;
            journalEntry.PostedAt = DateTime.UtcNow;
            journalEntry.PostedBy = postedBy;
            journalEntry.UpdatedAt = DateTime.UtcNow;
            journalEntry.UpdatedBy = postedBy;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Asiento contable registrado: {journalEntry.EntryNumber} por {postedBy}");
            return true;
        }

        public async Task<bool> VoidJournalEntryAsync(Guid id, string voidedBy, string reason)
        {
            var journalEntry = await GetJournalEntryByIdAsync(id);
            if (journalEntry == null)
            {
                return false;
            }

            if (!await CanVoidJournalEntryAsync(id))
            {
                throw new InvalidOperationException("No se puede anular el asiento contable.");
            }

            journalEntry.Status = JournalEntryStatus.Voided;
            journalEntry.UpdatedAt = DateTime.UtcNow;
            journalEntry.UpdatedBy = voidedBy;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Asiento contable anulado: {journalEntry.EntryNumber} por {voidedBy}. Razón: {reason}");
            return true;
        }

        public async Task<bool> UnpostJournalEntryAsync(Guid id, string unpostedBy)
        {
            var journalEntry = await GetJournalEntryByIdAsync(id);
            if (journalEntry == null)
            {
                return false;
            }

            if (journalEntry.Status != JournalEntryStatus.Posted)
            {
                throw new InvalidOperationException("Solo se pueden desregistrar asientos que estén registrados.");
            }

            journalEntry.Status = JournalEntryStatus.Draft;
            journalEntry.PostedAt = null;
            journalEntry.PostedBy = null;
            journalEntry.UpdatedAt = DateTime.UtcNow;
            journalEntry.UpdatedBy = unpostedBy;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Asiento contable desregistrado: {journalEntry.EntryNumber} por {unpostedBy}");
            return true;
        }

        public async Task<string> GenerateEntryNumberAsync(JournalEntryType type)
        {
            var prefix = type switch
            {
                JournalEntryType.Opening => "AP",
                JournalEntryType.Regular => "AS",
                JournalEntryType.Adjustment => "AJ",
                JournalEntryType.Closing => "CI",
                JournalEntryType.Recurring => "RC",
                _ => "AS"
            };

            var today = DateTime.Today;
            var year = today.Year;
            var month = today.Month;

            // Buscar el último número del mes actual
            var lastEntry = await _context.JournalEntries
                .Where(j => j.EntryNumber.StartsWith($"{prefix}{year:D4}{month:D2}"))
                .OrderByDescending(j => j.EntryNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastEntry != null)
            {
                var lastSequence = int.Parse(lastEntry.EntryNumber.Substring(lastEntry.EntryNumber.Length - 4));
                sequence = lastSequence + 1;
            }

            return $"{prefix}{year:D4}{month:D2}{sequence:D4}";
        }

        public async Task<bool> IsEntryNumberUniqueAsync(string entryNumber)
        {
            return !await _context.JournalEntries.AnyAsync(j => j.EntryNumber == entryNumber);
        }

        public async Task<bool> ValidateJournalEntryAsync(JournalEntry journalEntry)
        {
            // Validar que tenga al menos dos detalles
            if (journalEntry.Details == null || journalEntry.Details.Count() < 2)
            {
                return false;
            }

            // Validar que todos los detalles tengan cuentas válidas
            foreach (var detail in journalEntry.Details)
            {
                var account = await _context.Accounts.FindAsync(detail.AccountId);
                if (account == null || !account.IsActive)
                {
                    return false;
                }
            }

            // Validar que no haya detalles con montos negativos
            if (journalEntry.Details.Any(d => d.DebitAmount < 0 || d.CreditAmount < 0))
            {
                return false;
            }

            return true;
        }

        public async Task<bool> IsJournalEntryBalancedAsync(JournalEntry journalEntry)
        {
            var totalDebit = journalEntry.Details.Sum(d => d.DebitAmount);
            var totalCredit = journalEntry.Details.Sum(d => d.CreditAmount);

            return Math.Abs(totalDebit - totalCredit) < 0.01m; // Tolerancia de 1 centavo
        }

        public async Task<bool> CanPostJournalEntryAsync(Guid id)
        {
            var journalEntry = await GetJournalEntryByIdAsync(id);
            if (journalEntry == null)
            {
                return false;
            }

            // Solo se pueden registrar asientos en borrador
            if (journalEntry.Status != JournalEntryStatus.Draft)
            {
                return false;
            }

            // Verificar que esté balanceado
            return await IsJournalEntryBalancedAsync(journalEntry);
        }

        public async Task<bool> CanVoidJournalEntryAsync(Guid id)
        {
            var journalEntry = await GetJournalEntryByIdAsync(id);
            if (journalEntry == null)
            {
                return false;
            }

            // Solo se pueden anular asientos registrados
            return journalEntry.Status == JournalEntryStatus.Posted;
        }

        public async Task<decimal> GetAccountBalanceAsync(Guid accountId, DateTime? asOfDate = null)
        {
            var query = _context.JournalEntryDetails
                .Include(d => d.JournalEntry)
                .Where(d => d.AccountId == accountId && d.JournalEntry.Status == JournalEntryStatus.Posted);

            if (asOfDate.HasValue)
            {
                query = query.Where(d => d.JournalEntry.EntryDate <= asOfDate.Value);
            }

            var details = await query.ToListAsync();

            var balance = details.Sum(d => d.DebitAmount - d.CreditAmount);

            // Ajustar según la naturaleza de la cuenta
            var account = await _context.Accounts.FindAsync(accountId);
            if (account?.Nature == AccountNature.Credit)
            {
                balance = -balance;
            }

            return balance;
        }

        public async Task<Dictionary<Guid, decimal>> GetAccountsBalanceAsync(IEnumerable<Guid> accountIds, DateTime? asOfDate = null)
        {
            var balances = new Dictionary<Guid, decimal>();

            foreach (var accountId in accountIds)
            {
                var balance = await GetAccountBalanceAsync(accountId, asOfDate);
                balances[accountId] = balance;
            }

            return balances;
        }

        public async Task<IEnumerable<JournalEntry>> GetJournalEntriesForTrialBalanceAsync(DateTime asOfDate)
        {
            return await _context.JournalEntries
                .Include(j => j.Details)
                .ThenInclude(d => d.Account)
                .Where(j => j.Status == JournalEntryStatus.Posted && j.EntryDate <= asOfDate)
                .OrderBy(j => j.EntryDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<JournalEntry>> GetJournalEntriesForGeneralLedgerAsync(Guid accountId, DateTime startDate, DateTime endDate)
        {
            return await _context.JournalEntries
                .Include(j => j.Details)
                .ThenInclude(d => d.Account)
                .Where(j => j.Status == JournalEntryStatus.Posted && 
                           j.EntryDate >= startDate && 
                           j.EntryDate <= endDate &&
                           j.Details.Any(d => d.AccountId == accountId))
                .OrderBy(j => j.EntryDate)
                .ToListAsync();
        }

        public async Task<JournalEntry> CreateAutomaticEntryAsync(string description, IEnumerable<JournalEntryDetail> details, JournalEntryType type = JournalEntryType.Regular)
        {
            var journalEntry = new JournalEntry
            {
                Description = description,
                Type = type,
                EntryDate = DateTime.Today,
                Details = details.ToList()
            };

            return await CreateJournalEntryAsync(journalEntry);
        }

        public async Task<JournalEntry> CreateRecurringEntryAsync(JournalEntry template, DateTime entryDate)
        {
            var journalEntry = new JournalEntry
            {
                Description = template.Description,
                Type = template.Type,
                EntryDate = entryDate,
                Reference = template.Reference,
                Details = template.Details.Select(d => new JournalEntryDetail
                {
                    AccountId = d.AccountId,
                    DebitAmount = d.DebitAmount,
                    CreditAmount = d.CreditAmount,
                    Description = d.Description,
                    Reference = d.Reference
                }).ToList()
            };

            return await CreateJournalEntryAsync(journalEntry);
        }

        public async Task<JournalEntry?> CreateEntryFromOrderAsync(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return null;
            }

            var details = new List<JournalEntryDetail>();

            // Buscar cuentas del sistema
            var cashAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Code == "1110"); // Efectivo
            var salesAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Code == "4110"); // Ventas de Comida
            var costAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Code == "5110"); // Costo de Ventas
            var inventoryAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Code == "1130"); // Inventarios

            if (cashAccount != null && salesAccount != null)
            {
                // Debito: Efectivo
                details.Add(new JournalEntryDetail
                {
                    AccountId = cashAccount.Id,
                    DebitAmount = order.TotalAmount ?? 0,
                    CreditAmount = 0,
                    Description = $"Venta de orden #{order.OrderNumber}"
                });

                // Crédito: Ventas
                details.Add(new JournalEntryDetail
                {
                    AccountId = salesAccount.Id,
                    DebitAmount = 0,
                    CreditAmount = order.TotalAmount ?? 0,
                    Description = $"Venta de orden #{order.OrderNumber}"
                });
            }

            if (details.Any())
            {
                var journalEntry = new JournalEntry
                {
                    Description = $"Venta de orden #{order.OrderNumber}",
                    Type = JournalEntryType.Regular,
                    EntryDate = order.OpenedAt ?? DateTime.UtcNow,
                    OrderId = orderId,
                    Details = details
                };

                return await CreateJournalEntryAsync(journalEntry);
            }

            return null;
        }

        public async Task<JournalEntry?> CreateEntryFromPaymentAsync(Guid paymentId)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return null;
            }

            var details = new List<JournalEntryDetail>();

            // Buscar cuentas del sistema
            var cashAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Code == "1110"); // Efectivo
            var accountsReceivableAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Code == "1120"); // Cuentas por Cobrar

            if (cashAccount != null && accountsReceivableAccount != null)
            {
                // Debito: Efectivo
                details.Add(new JournalEntryDetail
                {
                    AccountId = cashAccount.Id,
                    DebitAmount = payment.Amount,
                    CreditAmount = 0,
                    Description = $"Pago de orden #{payment.Order?.OrderNumber}"
                });

                // Crédito: Cuentas por Cobrar
                details.Add(new JournalEntryDetail
                {
                    AccountId = accountsReceivableAccount.Id,
                    DebitAmount = 0,
                    CreditAmount = payment.Amount,
                    Description = $"Pago de orden #{payment.Order?.OrderNumber}"
                });
            }

            if (details.Any())
            {
                var journalEntry = new JournalEntry
                {
                    Description = $"Pago de orden #{payment.Order?.OrderNumber}",
                    Type = JournalEntryType.Regular,
                    EntryDate = DateTime.UtcNow,
                    PaymentId = paymentId,
                    Details = details
                };

                return await CreateJournalEntryAsync(journalEntry);
            }

            return null;
        }

        public async Task<IEnumerable<JournalEntry>> GetJournalEntriesByUserAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.JournalEntries
                .Include(j => j.Details)
                .ThenInclude(d => d.Account)
                .Where(j => j.CreatedBy == userId || j.UpdatedBy == userId);

            if (startDate.HasValue)
            {
                query = query.Where(j => j.EntryDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(j => j.EntryDate <= endDate.Value);
            }

            return await query.OrderByDescending(j => j.EntryDate).ToListAsync();
        }

        public async Task<IEnumerable<JournalEntry>> GetModifiedJournalEntriesAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.JournalEntries
                .Include(j => j.Details)
                .ThenInclude(d => d.Account)
                .Where(j => j.UpdatedAt >= startDate && j.UpdatedAt <= endDate)
                .OrderByDescending(j => j.UpdatedAt)
                .ToListAsync();
        }
    }
} 