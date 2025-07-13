using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Claims;

namespace RestBar.Services
{
    public class OperatingHoursService : IOperatingHoursService
    {
        private readonly RestBarContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OperatingHoursService(RestBarContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<OperatingHours>> GetAllAsync(Guid? companyId = null)
        {
            var targetCompanyId = companyId ?? await GetCurrentUserCompanyIdAsync();
            
            return await _context.OperatingHours
                .Where(o => o.CompanyId == targetCompanyId)
                .OrderBy(o => o.DayOfWeek)
                .ToListAsync();
        }

        public async Task<OperatingHours?> GetByIdAsync(Guid id)
        {
            return await _context.OperatingHours
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<OperatingHours> CreateAsync(OperatingHours operatingHours)
        {
            operatingHours.Id = Guid.NewGuid();
            // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService

            _context.OperatingHours.Add(operatingHours);
            await _context.SaveChangesAsync();

            return operatingHours;
        }

        public async Task<OperatingHours> UpdateAsync(OperatingHours operatingHours)
        {
            // ✅ Fechas se manejan automáticamente por el modelo y BaseTrackingService
            _context.OperatingHours.Update(operatingHours);
            await _context.SaveChangesAsync();

            return operatingHours;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var operatingHours = await _context.OperatingHours.FindAsync(id);
                if (operatingHours != null)
                {
                    _context.OperatingHours.Remove(operatingHours);
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

        public async Task<OperatingHours?> GetByDayOfWeekAsync(string dayOfWeek, Guid? companyId = null)
        {
            var targetCompanyId = companyId ?? await GetCurrentUserCompanyIdAsync();
            
            return await _context.OperatingHours
                .FirstOrDefaultAsync(o => o.DayOfWeek == dayOfWeek && o.CompanyId == targetCompanyId);
        }

        public async Task<bool> IsOpenAsync(DateTime dateTime, Guid? companyId = null)
        {
            var dayOfWeek = dateTime.DayOfWeek.ToString();
            var time = dateTime.TimeOfDay;
            
            var operatingHours = await GetByDayOfWeekAsync(dayOfWeek, companyId);
            
            if (operatingHours != null && operatingHours.IsOpen)
            {
                if (operatingHours.OpenTime.HasValue && operatingHours.CloseTime.HasValue)
                {
                    return time >= operatingHours.OpenTime.Value && time <= operatingHours.CloseTime.Value;
                }
                return true; // Si no hay horarios específicos, está abierto todo el día
            }
            
            return false;
        }

        public async Task<TimeSpan?> GetNextOpenTimeAsync(DateTime dateTime, Guid? companyId = null)
        {
            var dayOfWeek = dateTime.DayOfWeek.ToString();
            var operatingHours = await GetByDayOfWeekAsync(dayOfWeek, companyId);
            
            if (operatingHours?.OpenTime.HasValue == true)
            {
                return operatingHours.OpenTime.Value;
            }
            
            return null;
        }

        public async Task<TimeSpan?> GetNextCloseTimeAsync(DateTime dateTime, Guid? companyId = null)
        {
            var dayOfWeek = dateTime.DayOfWeek.ToString();
            var operatingHours = await GetByDayOfWeekAsync(dayOfWeek, companyId);
            
            if (operatingHours?.CloseTime.HasValue == true)
            {
                return operatingHours.CloseTime.Value;
            }
            
            return null;
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