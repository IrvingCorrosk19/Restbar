using RestBar.Models;

namespace RestBar.Interfaces
{
    public interface IBackupSettingsService
    {
        Task<IEnumerable<BackupSettings>> GetAllAsync(Guid? companyId = null);
        Task<BackupSettings?> GetByIdAsync(Guid id);
        Task<BackupSettings> CreateAsync(BackupSettings backupSettings);
        Task<BackupSettings> UpdateAsync(BackupSettings backupSettings);
        Task<bool> DeleteAsync(Guid id);
        Task<BackupSettings?> GetByTypeAsync(string backupType, Guid? companyId = null);
        Task<bool> IsEnabledAsync(string backupType, Guid? companyId = null);
        Task<bool> EnableBackupAsync(string backupType, Guid? companyId = null);
        Task<bool> DisableBackupAsync(string backupType, Guid? companyId = null);
        Task<bool> ExecuteBackupAsync(string backupType, Guid? companyId = null);
        Task<DateTime?> GetLastBackupDateAsync(string backupType, Guid? companyId = null);
    }
} 