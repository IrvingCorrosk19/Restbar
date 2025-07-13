using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface ISystemSettingsService
    {
        Task<SystemSettings?> GetSettingAsync(string key, Guid? companyId = null);
        Task<string?> GetSettingValueAsync(string key, Guid? companyId = null);
        Task<bool> SetSettingAsync(string key, string value, string? description = null, string? category = null, Guid? companyId = null);
        Task<bool> DeleteSettingAsync(string key, Guid? companyId = null);
        Task<IEnumerable<SystemSettings>> GetSettingsByCategoryAsync(string category, Guid? companyId = null);
        Task<IEnumerable<SystemSettings>> GetAllSettingsAsync(Guid? companyId = null);
        Task<bool> UpdateSettingAsync(SystemSettings setting);
    }
} 