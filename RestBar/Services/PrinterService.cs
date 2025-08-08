using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Claims;

namespace RestBar.Services
{
    public class PrinterService : IPrinterService
    {
        private readonly RestBarContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PrinterService(RestBarContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<Printer>> GetAllAsync(Guid? companyId = null)
        {
            var targetCompanyId = companyId ?? await GetCurrentUserCompanyIdAsync();
            
            return await _context.Printers
                .Where(p => p.CompanyId == targetCompanyId && p.IsActive)
                .OrderBy(p => p.PrinterType)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Printer?> GetByIdAsync(Guid id)
        {
            return await _context.Printers
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        }

        public async Task<Printer> CreateAsync(Printer printer)
        {
            printer.Id = Guid.NewGuid();
            printer.CreatedAt = DateTime.UtcNow;
            printer.IsActive = true;

            _context.Printers.Add(printer);
            await _context.SaveChangesAsync();

            return printer;
        }

        public async Task<Printer> UpdateAsync(Printer printer)
        {
            printer.UpdatedAt = DateTime.UtcNow;
            _context.Printers.Update(printer);
            await _context.SaveChangesAsync();

            return printer;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var printer = await _context.Printers.FindAsync(id);
                if (printer != null)
                {
                    printer.IsActive = false;
                    printer.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Printer?> GetDefaultPrinterAsync(string printerType, Guid? companyId = null)
        {
            var targetCompanyId = companyId ?? await GetCurrentUserCompanyIdAsync();
            
            return await _context.Printers
                .FirstOrDefaultAsync(p => p.PrinterType == printerType && p.IsDefault && p.CompanyId == targetCompanyId && p.IsActive);
        }

        public async Task<bool> SetDefaultPrinterAsync(Guid id, Guid? companyId = null)
        {
            try
            {
                var targetCompanyId = companyId ?? await GetCurrentUserCompanyIdAsync();
                var printer = await _context.Printers.FindAsync(id);
                
                if (printer != null && printer.CompanyId == targetCompanyId)
                {
                    // Desactivar otros printers del mismo tipo como default
                    var otherPrinters = await _context.Printers
                        .Where(p => p.PrinterType == printer.PrinterType && p.CompanyId == targetCompanyId && p.IsActive)
                        .ToListAsync();

                    foreach (var otherPrinter in otherPrinters)
                    {
                        otherPrinter.IsDefault = false;
                        otherPrinter.UpdatedAt = DateTime.UtcNow;
                    }

                    // Establecer el nuevo default
                    printer.IsDefault = true;
                    printer.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Printer>> GetByTypeAsync(string printerType, Guid? companyId = null)
        {
            var targetCompanyId = companyId ?? await GetCurrentUserCompanyIdAsync();
            
            return await _context.Printers
                .Where(p => p.PrinterType == printerType && p.CompanyId == targetCompanyId && p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<bool> TestPrinterAsync(Guid id)
        {
            // Implementación básica de prueba de impresora
            // En un entorno real, aquí se conectaría a la impresora y enviaría una página de prueba
            try
            {
                var printer = await _context.Printers.FindAsync(id);
                if (printer != null && printer.IsActive)
                {
                    // Simular prueba de impresora
                    await Task.Delay(1000); // Simular tiempo de conexión
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private async Task<Guid?> GetCurrentUserCompanyIdAsync()
        {
            var userEmail = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail)) return null;

            var user = await _context.Users
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            return user?.Branch?.CompanyId;
        }
    }
} 