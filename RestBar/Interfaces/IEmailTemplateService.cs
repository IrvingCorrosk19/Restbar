using RestBar.Models;

namespace RestBar.Interfaces
{
    /// <summary>
    /// Servicio para gestionar templates de email
    /// </summary>
    public interface IEmailTemplateService
    {
        Task<IEnumerable<EmailTemplate>> GetAllAsync(Guid? companyId = null);
        Task<EmailTemplate?> GetByIdAsync(Guid id);
        Task<EmailTemplate?> GetByNameAsync(string name, Guid? companyId = null);
        Task<EmailTemplate> CreateAsync(EmailTemplate template);
        Task<EmailTemplate> UpdateAsync(EmailTemplate template);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> InitializeDefaultTemplatesAsync();
    }
}
