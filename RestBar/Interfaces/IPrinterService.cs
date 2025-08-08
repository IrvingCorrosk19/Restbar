using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface IPrinterService
    {
        Task<IEnumerable<Printer>> GetAllAsync(Guid? companyId = null);
        Task<Printer?> GetByIdAsync(Guid id);
        Task<Printer> CreateAsync(Printer printer);
        Task<Printer> UpdateAsync(Printer printer);
        Task<bool> DeleteAsync(Guid id);
        Task<Printer?> GetDefaultPrinterAsync(string printerType, Guid? companyId = null);
        Task<bool> SetDefaultPrinterAsync(Guid id, Guid? companyId = null);
        Task<IEnumerable<Printer>> GetByTypeAsync(string printerType, Guid? companyId = null);
        Task<bool> TestPrinterAsync(Guid id);
    }
} 