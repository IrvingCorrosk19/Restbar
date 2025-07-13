using RestBar.Models;

namespace RestBar.ViewModels
{
    public class AuditLogViewModel
    {
        public IEnumerable<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public string? ModuleFilter { get; set; }
        public string? ActionFilter { get; set; }
        public string? LogLevelFilter { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool ShowErrorsOnly { get; set; }
        
        // Opciones para filtros
        public IEnumerable<string> AvailableModules { get; set; } = new List<string>();
        public IEnumerable<string> AvailableActions { get; set; } = new List<string>();
        public IEnumerable<string> AvailableLogLevels { get; set; } = new List<string>();
        
        // Paginación
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 20;
        public int TotalRecords { get; set; }
        
        // Estadísticas
        public int TotalLogs { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public int InfoCount { get; set; }
    }
} 