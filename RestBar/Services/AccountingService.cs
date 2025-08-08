using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services
{
    public class AccountingService : IAccountingService
    {
        private readonly RestBarContext _context;
        private readonly ILogger<AccountingService> _logger;
        private readonly IJournalEntryService _journalEntryService;

        public AccountingService(
            RestBarContext context, 
            ILogger<AccountingService> logger,
            IJournalEntryService journalEntryService)
        {
            _context = context;
            _logger = logger;
            _journalEntryService = journalEntryService;
        }

        public async Task<FinancialSummaryDto> GetFinancialSummaryAsync(string period)
        {
            var (startDate, endDate) = GetDateRangeFromPeriod(period);
            
            var totalIncome = await GetTotalIncomeAsync(startDate, endDate);
            var totalExpenses = await GetTotalExpensesAsync(startDate, endDate);
            var netProfit = totalIncome - totalExpenses;
            var totalTaxes = await GetTotalTaxesAsync(startDate, endDate);
            
            var totalOrders = await _context.Orders
                .Where(o => o.ClosedAt >= startDate && o.ClosedAt <= endDate && o.Status == OrderStatus.Completed)
                .CountAsync();
                
            var totalPayments = await _context.Payments
                .Where(p => p.PaidAt >= startDate && p.PaidAt <= endDate && p.Status == "COMPLETED")
                .CountAsync();

            return new FinancialSummaryDto
            {
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                NetProfit = netProfit,
                TotalTaxes = totalTaxes,
                ProfitMargin = totalIncome > 0 ? (netProfit / totalIncome) * 100 : 0,
                TotalOrders = totalOrders,
                TotalPayments = totalPayments
            };
        }

        public async Task<IncomeDetailsDto> GetIncomeDetailsAsync(string period)
        {
            var (startDate, endDate) = GetDateRangeFromPeriod(period);
            
            var payments = await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.PaidAt >= startDate && p.PaidAt <= endDate && p.Status == "COMPLETED" && !p.IsVoided.GetValueOrDefault())
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();

            var incomeItems = payments.Select(p => new IncomeItem
            {
                Concept = $"Venta #{p.Order?.OrderNumber ?? "N/A"}",
                Amount = p.Amount,
                Date = p.PaidAt ?? p.CreatedAt,
                Status = "Completado",
                OrderNumber = p.Order?.OrderNumber ?? "N/A"
            }).ToList();

            return new IncomeDetailsDto
            {
                Items = incomeItems,
                TotalAmount = incomeItems.Sum(i => i.Amount),
                TotalCount = incomeItems.Count
            };
        }

        public async Task<ExpenseDetailsDto> GetExpenseDetailsAsync(string period)
        {
            var (startDate, endDate) = GetDateRangeFromPeriod(period);
            
            // Obtener gastos desde asientos contables de tipo gasto
            var expenseEntries = await _context.JournalEntries
                .Include(j => j.Details)
                .ThenInclude(d => d.Account)
                .Where(j => j.EntryDate >= startDate && j.EntryDate <= endDate && 
                           j.Status == JournalEntryStatus.Posted &&
                           j.Type == JournalEntryType.Regular)
                .OrderByDescending(j => j.EntryDate)
                .ToListAsync();

            var expenseItems = new List<ExpenseItem>();
            
            foreach (var entry in expenseEntries)
            {
                foreach (var detail in entry.Details.Where(d => d.DebitAmount > 0))
                {
                    expenseItems.Add(new ExpenseItem
                    {
                        Concept = entry.Description,
                        Amount = detail.DebitAmount,
                        Date = entry.EntryDate,
                        Category = detail.Account?.Category.ToString() ?? "Sin categoría",
                        Supplier = entry.Reference ?? "N/A"
                    });
                }
            }

            return new ExpenseDetailsDto
            {
                Items = expenseItems,
                TotalAmount = expenseItems.Sum(i => i.Amount),
                TotalCount = expenseItems.Count
            };
        }

        public async Task<TaxSummaryDto> GetTaxSummaryAsync(string period)
        {
            var (startDate, endDate) = GetDateRangeFromPeriod(period);
            
            var ivaCollected = await CalculateIVACollectedAsync(startDate, endDate);
            var ivaPaid = await CalculateIVAPaidAsync(startDate, endDate);
            var ivaToPay = ivaCollected - ivaPaid;
            var isr = await CalculateISRAsync(startDate, endDate);

            return new TaxSummaryDto
            {
                IVACollected = ivaCollected,
                IVAPaid = ivaPaid,
                IVAToPay = ivaToPay,
                ISR = isr,
                TotalTaxes = ivaToPay + isr
            };
        }

        public async Task<decimal> GetTotalIncomeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Payments
                .Where(p => p.PaidAt >= startDate && p.PaidAt <= endDate && 
                           p.Status == "COMPLETED" && !p.IsVoided.GetValueOrDefault())
                .SumAsync(p => p.Amount);
        }

        public async Task<decimal> GetTotalExpensesAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.JournalEntries
                .Include(j => j.Details)
                .Where(j => j.EntryDate >= startDate && j.EntryDate <= endDate && 
                           j.Status == JournalEntryStatus.Posted &&
                           j.Type == JournalEntryType.Regular)
                .SumAsync(j => j.TotalDebit);
        }

        public async Task<decimal> GetNetProfitAsync(DateTime startDate, DateTime endDate)
        {
            var income = await GetTotalIncomeAsync(startDate, endDate);
            var expenses = await GetTotalExpensesAsync(startDate, endDate);
            return income - expenses;
        }

        public async Task<decimal> GetTotalTaxesAsync(DateTime startDate, DateTime endDate)
        {
            var ivaToPay = await CalculateIVAToPayAsync(startDate, endDate);
            var isr = await CalculateISRAsync(startDate, endDate);
            return ivaToPay + isr;
        }

        public async Task<IEnumerable<MonthlyFinancialData>> GetMonthlyFinancialDataAsync(int year)
        {
            var monthlyData = new List<MonthlyFinancialData>();
            var monthNames = new[] { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", 
                                   "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };

            for (int month = 1; month <= 12; month++)
            {
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var income = await GetTotalIncomeAsync(startDate, endDate);
                var expenses = await GetTotalExpensesAsync(startDate, endDate);
                var profit = income - expenses;
                var orderCount = await _context.Orders
                    .Where(o => o.ClosedAt >= startDate && o.ClosedAt <= endDate && o.Status == OrderStatus.Completed)
                    .CountAsync();

                monthlyData.Add(new MonthlyFinancialData
                {
                    Month = month,
                    MonthName = monthNames[month - 1],
                    Income = income,
                    Expenses = expenses,
                    Profit = profit,
                    OrderCount = orderCount
                });
            }

            return monthlyData;
        }

        public async Task<IEnumerable<DailyFinancialData>> GetDailyFinancialDataAsync(DateTime startDate, DateTime endDate)
        {
            var dailyData = new List<DailyFinancialData>();
            var currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                var dayStart = currentDate;
                var dayEnd = currentDate.AddDays(1).AddSeconds(-1);

                var income = await GetTotalIncomeAsync(dayStart, dayEnd);
                var expenses = await GetTotalExpensesAsync(dayStart, dayEnd);
                var profit = income - expenses;
                var orderCount = await _context.Orders
                    .Where(o => o.ClosedAt >= dayStart && o.ClosedAt <= dayEnd && o.Status == OrderStatus.Completed)
                    .CountAsync();

                dailyData.Add(new DailyFinancialData
                {
                    Date = currentDate,
                    Income = income,
                    Expenses = expenses,
                    Profit = profit,
                    OrderCount = orderCount
                });

                currentDate = currentDate.AddDays(1);
            }

            return dailyData;
        }

        public async Task<byte[]> GenerateFinancialReportAsync(string reportType, string period, DateTime? startDate = null, DateTime? endDate = null)
        {
            // Implementación básica - en producción usar una librería como EPPlus o iTextSharp
            var reportContent = $"Reporte {reportType} - Período: {period}";
            return System.Text.Encoding.UTF8.GetBytes(reportContent);
        }

        public async Task<byte[]> ExportAccountingDataAsync(string format = "xlsx")
        {
            // Implementación básica - en producción usar EPPlus para Excel
            var data = await GetFinancialSummaryAsync("month");
            var content = $"Ingresos: {data.TotalIncome}, Gastos: {data.TotalExpenses}, Beneficio: {data.NetProfit}";
            return System.Text.Encoding.UTF8.GetBytes(content);
        }

        public async Task<bool> CreateAccountingEntryFromOrderAsync(Guid orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Payments)
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null || order.Status != OrderStatus.Completed)
                    return false;

                var totalAmount = order.TotalAmount ?? 0;
                var ivaRate = 0.16m; // 16% IVA
                var subtotal = totalAmount / (1 + ivaRate);
                var iva = totalAmount - subtotal;

                var details = new List<JournalEntryDetail>
                {
                    new JournalEntryDetail
                    {
                        AccountId = await GetAccountIdByCodeAsync("1101"), // Caja
                        DebitAmount = totalAmount,
                        CreditAmount = 0,
                        Description = $"Venta #{order.OrderNumber}"
                    },
                    new JournalEntryDetail
                    {
                        AccountId = await GetAccountIdByCodeAsync("4101"), // Ventas
                        DebitAmount = 0,
                        CreditAmount = subtotal,
                        Description = $"Venta #{order.OrderNumber}"
                    },
                    new JournalEntryDetail
                    {
                        AccountId = await GetAccountIdByCodeAsync("2101"), // IVA por cobrar
                        DebitAmount = 0,
                        CreditAmount = iva,
                        Description = $"IVA venta #{order.OrderNumber}"
                    }
                };

                var journalEntry = new JournalEntry
                {
                    EntryNumber = await _journalEntryService.GenerateEntryNumberAsync(JournalEntryType.Regular),
                    EntryDate = order.ClosedAt ?? DateTime.UtcNow,
                    Type = JournalEntryType.Regular,
                    Description = $"Registro de venta #{order.OrderNumber}",
                    Status = JournalEntryStatus.Posted,
                    PostedAt = DateTime.UtcNow,
                    PostedBy = "Sistema",
                    TotalDebit = totalAmount,
                    TotalCredit = totalAmount,
                    OrderId = orderId,
                    Details = details
                };

                await _journalEntryService.CreateJournalEntryAsync(journalEntry);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear asiento contable desde orden {OrderId}", orderId);
                return false;
            }
        }

        public async Task<bool> CreateAccountingEntryFromPaymentAsync(Guid paymentId)
        {
            try
            {
                var payment = await _context.Payments
                    .Include(p => p.Order)
                    .FirstOrDefaultAsync(p => p.Id == paymentId);

                if (payment == null || payment.Status != "COMPLETED")
                    return false;

                // Si ya existe un asiento para este pago, no crear otro
                var existingEntry = await _context.JournalEntries
                    .FirstOrDefaultAsync(j => j.PaymentId == paymentId);

                if (existingEntry != null)
                    return true;

                var details = new List<JournalEntryDetail>
                {
                    new JournalEntryDetail
                    {
                        AccountId = await GetAccountIdByCodeAsync("1101"), // Caja
                        DebitAmount = payment.Amount,
                        CreditAmount = 0,
                        Description = $"Pago #{payment.Id}"
                    },
                    new JournalEntryDetail
                    {
                        AccountId = await GetAccountIdByCodeAsync("1201"), // Cuentas por cobrar
                        DebitAmount = 0,
                        CreditAmount = payment.Amount,
                        Description = $"Pago #{payment.Id}"
                    }
                };

                var journalEntry = new JournalEntry
                {
                    EntryNumber = await _journalEntryService.GenerateEntryNumberAsync(JournalEntryType.Regular),
                    EntryDate = payment.PaidAt ?? DateTime.UtcNow,
                    Type = JournalEntryType.Regular,
                    Description = $"Registro de pago #{payment.Id}",
                    Status = JournalEntryStatus.Posted,
                    PostedAt = DateTime.UtcNow,
                    PostedBy = "Sistema",
                    TotalDebit = payment.Amount,
                    TotalCredit = payment.Amount,
                    PaymentId = paymentId,
                    Details = details
                };

                await _journalEntryService.CreateJournalEntryAsync(journalEntry);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear asiento contable desde pago {PaymentId}", paymentId);
                return false;
            }
        }

        public async Task<bool> RecordPaymentAsync(Guid orderId, decimal amount, string method, bool isShared, string? payerName, List<SplitPaymentDto>? splitPayments = null)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Payments)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    return false;

                // Crear el pago
                var payment = new Payment
                {
                    OrderId = orderId,
                    Amount = amount,
                    Method = method,
                    IsShared = isShared,
                    PayerName = payerName,
                    Status = "COMPLETED",
                    PaidAt = DateTime.UtcNow,
                    IsVoided = false
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // Crear asiento contable automáticamente
                await CreateAccountingEntryFromPaymentAsync(payment.Id);

                // Si hay pagos divididos, procesarlos también
                if (splitPayments != null && splitPayments.Any())
                {
                    foreach (var split in splitPayments)
                    {
                        var splitPayment = new Payment
                        {
                            OrderId = orderId,
                            Amount = split.Amount,
                            Method = split.Method,
                            IsShared = true,
                            PayerName = split.PayerName,
                            Status = "COMPLETED",
                            PaidAt = DateTime.UtcNow,
                            IsVoided = false
                        };

                        _context.Payments.Add(splitPayment);
                        await _context.SaveChangesAsync();
                        await CreateAccountingEntryFromPaymentAsync(splitPayment.Id);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar pago para orden {OrderId}", orderId);
                return false;
            }
        }

        public async Task<decimal> CalculateIVACollectedAsync(DateTime startDate, DateTime endDate)
        {
            var totalIncome = await GetTotalIncomeAsync(startDate, endDate);
            var ivaRate = 0.16m; // 16% IVA
            return totalIncome * ivaRate / (1 + ivaRate);
        }

        public async Task<decimal> CalculateIVAPaidAsync(DateTime startDate, DateTime endDate)
        {
            // Obtener IVA pagado desde asientos contables de gastos
            return await _context.JournalEntries
                .Include(j => j.Details)
                .ThenInclude(d => d.Account)
                .Where(j => j.EntryDate >= startDate && j.EntryDate <= endDate && 
                           j.Status == JournalEntryStatus.Posted &&
                           j.Type == JournalEntryType.Regular)
                .SelectMany(j => j.Details)
                .Where(d => d.Account.Code.StartsWith("2102")) // IVA por pagar
                .SumAsync(d => d.DebitAmount);
        }

        public async Task<decimal> CalculateIVAToPayAsync(DateTime startDate, DateTime endDate)
        {
            var ivaCollected = await CalculateIVACollectedAsync(startDate, endDate);
            var ivaPaid = await CalculateIVAPaidAsync(startDate, endDate);
            return ivaCollected - ivaPaid;
        }

        public async Task<decimal> CalculateISRAsync(DateTime startDate, DateTime endDate)
        {
            var netProfit = await GetNetProfitAsync(startDate, endDate);
            var isrRate = 0.30m; // 30% ISR
            return Math.Max(0, netProfit * isrRate);
        }

        private (DateTime startDate, DateTime endDate) GetDateRangeFromPeriod(string period)
        {
            var now = DateTime.UtcNow;
            
            return period.ToLower() switch
            {
                "month" => (new DateTime(now.Year, now.Month, 1), now),
                "quarter" => (new DateTime(now.Year, ((now.Month - 1) / 3) * 3 + 1, 1), now),
                "year" => (new DateTime(now.Year, 1, 1), now),
                _ => (new DateTime(now.Year, now.Month, 1), now)
            };
        }

        private async Task<Guid> GetAccountIdByCodeAsync(string code)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Code == code);
            return account?.Id ?? Guid.Empty;
        }
    }
} 