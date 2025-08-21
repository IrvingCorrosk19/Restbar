using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface IOperatingHoursService
    {
        Task<IEnumerable<OperatingHours>> GetAllAsync(Guid? companyId = null);
        Task<OperatingHours?> GetByIdAsync(Guid id);
        Task<OperatingHours> CreateAsync(OperatingHours operatingHours);
        Task<OperatingHours> UpdateAsync(OperatingHours operatingHours);
        Task<bool> DeleteAsync(Guid id);
        Task<OperatingHours?> GetByDayOfWeekAsync(string dayOfWeek, Guid? companyId = null);
        Task<bool> IsOpenAsync(DateTime dateTime, Guid? companyId = null);
        Task<TimeSpan?> GetNextOpenTimeAsync(DateTime dateTime, Guid? companyId = null);
        Task<TimeSpan?> GetNextCloseTimeAsync(DateTime dateTime, Guid? companyId = null);
    }
} 