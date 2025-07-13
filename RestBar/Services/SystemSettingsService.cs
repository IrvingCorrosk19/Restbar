using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Claims;

namespace RestBar.Services
{
    public class SystemSettingsService : ISystemSettingsService
    {
        private readonly RestBarContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SystemSettingsService(RestBarContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<SystemSettings?> GetSettingAsync(string key, Guid? companyId = null)
        {
            var targetCompanyId = companyId ?? await GetCurrentUserCompanyIdAsync();
            
            return await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == key && s.CompanyId == targetCompanyId && s.IsActive);
        }

        public async Task<string?> GetSettingValueAsync(string key, Guid? companyId = null)
        {
            var setting = await GetSettingAsync(key, companyId);
            return setting?.SettingValue;
        }

        public async Task<bool> SetSettingAsync(string key, string value, string? description = null, string? category = null, Guid? companyId = null)
        {
            try
            {
                var targetCompanyId = companyId ?? await GetCurrentUserCompanyIdAsync();
                
                var existingSetting = await _context.SystemSettings
                    .FirstOrDefaultAsync(s => s.SettingKey == key && s.CompanyId == targetCompanyId);

                if (existingSetting != null)
                {
                    existingSetting.SettingValue = value;
                    existingSetting.Description = description ?? existingSetting.Description;
                    existingSetting.Category = category ?? existingSetting.Category;
                    // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService
                }
                else
                {
                    var newSetting = new SystemSettings
                    {
                        Id = Guid.NewGuid(),
                        SettingKey = key,
                        SettingValue = value,
                        Description = description,
                        Category = category,
                        CompanyId = targetCompanyId,
                        IsActive = true,
                        // ✅ Fechas se manejan automáticamente por el modelo
                    };
                    
                    _context.SystemSettings.Add(newSetting);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteSettingAsync(string key, Guid? companyId = null)
        {
            try
            {
                var targetCompanyId = companyId ?? await GetCurrentUserCompanyIdAsync();
                
                var setting = await _context.SystemSettings
                    .FirstOrDefaultAsync(s => s.SettingKey == key && s.CompanyId == targetCompanyId);

                if (setting != null)
                {
                    setting.IsActive = false;
                    // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService
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

        public async Task<IEnumerable<SystemSettings>> GetSettingsByCategoryAsync(string category, Guid? companyId = null)
        {
            var targetCompanyId = companyId ?? await GetCurrentUserCompanyIdAsync();
            
            return await _context.SystemSettings
                .Where(s => s.Category == category && s.CompanyId == targetCompanyId && s.IsActive)
                .OrderBy(s => s.SettingKey)
                .ToListAsync();
        }

        public async Task<IEnumerable<SystemSettings>> GetAllSettingsAsync(Guid? companyId = null)
        {
            var targetCompanyId = companyId ?? await GetCurrentUserCompanyIdAsync();
            
            return await _context.SystemSettings
                .Where(s => s.CompanyId == targetCompanyId && s.IsActive)
                .OrderBy(s => s.Category)
                .ThenBy(s => s.SettingKey)
                .ToListAsync();
        }

        public async Task<bool> UpdateSettingAsync(SystemSettings setting)
        {
            try
            {
                // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService
                _context.SystemSettings.Update(setting);
                await _context.SaveChangesAsync();
                return true;
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