using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using RestBar.ViewModels;

namespace RestBar.Controllers
{
    [Authorize(Policy = "ManagerOrAbove")]
    public class AuditController : Controller
    {
        private readonly IAuditLogService _auditLogService;
        private readonly IGlobalLoggingService _loggingService;
        private readonly RestBarContext _context;

        public AuditController(IAuditLogService auditLogService, IGlobalLoggingService loggingService, RestBarContext context)
        {
            _auditLogService = auditLogService;
            _loggingService = loggingService;
            _context = context;
        }

        // GET: Audit
        public async Task<IActionResult> Index(string? module = null, [FromQuery(Name = "action")] string? actionFilter = null, string? logLevel = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                IEnumerable<AuditLog> logs;

                // Filtrar por compañía del usuario
                if (user?.Branch?.CompanyId != null)
                {
                    logs = await _auditLogService.GetByCompanyAsync(user.Branch.CompanyId.Value);
                }
                else
                {
                    logs = await _auditLogService.GetAllAsync();
                }

                // Aplicar filtros adicionales
                if (!string.IsNullOrEmpty(module))
                {
                    logs = logs.Where(l => l.Module == module);
                }

                if (!string.IsNullOrEmpty(actionFilter))
                {
                    logs = logs.Where(l => l.Action == actionFilter);
                }

                if (!string.IsNullOrEmpty(logLevel))
                {
                    logs = logs.Where(l => l.LogLevel == logLevel);
                }

                if (startDate.HasValue)
                {
                    logs = logs.Where(l => l.Timestamp >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    logs = logs.Where(l => l.Timestamp <= endDate.Value);
                }

                // Ordenar por fecha más reciente
                logs = logs.OrderByDescending(l => l.Timestamp).Take(1000); // Limitar a 1000 registros

                var viewModel = new AuditLogViewModel
                {
                    AuditLogs = logs.ToList(),
                    ModuleFilter = module,
                    ActionFilter = actionFilter,
                    LogLevelFilter = logLevel,
                    StartDate = startDate,
                    EndDate = endDate,
                    AvailableModules = GetAvailableModules(),
                    AvailableActions = GetAvailableActions(),
                    AvailableLogLevels = GetAvailableLogLevels(),
                    TotalLogs = logs.Count()
                };

                // ✅ NUEVO: Logging de consulta de auditoría
                await _loggingService.LogReportActivityAsync(
                    action: "AUDIT_VIEW",
                    description: "Consulta de logs de auditoría",
                    reportType: "AuditLog",
                    parameters: new { module, action = actionFilter, logLevel, startDate, endDate, recordCount = logs.Count() }
                );

                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _loggingService.LogModuleErrorAsync(
                    module: AuditModule.SYSTEM.ToString(),
                    description: "Error al cargar logs de auditoría",
                    exception: ex
                );

                TempData["Error"] = "Error al cargar los logs de auditoría.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Audit/Errors
        public async Task<IActionResult> Errors()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                IEnumerable<AuditLog> errors;

                if (user?.Branch?.CompanyId != null)
                {
                    errors = await _auditLogService.GetErrorsAsync(companyId: user.Branch.CompanyId.Value);
                }
                else
                {
                    errors = await _auditLogService.GetErrorsAsync();
                }

                var viewModel = new AuditLogViewModel
                {
                    AuditLogs = errors.Take(500).ToList(),
                    ShowErrorsOnly = true
                };

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                await _loggingService.LogModuleErrorAsync(
                    module: AuditModule.SYSTEM.ToString(),
                    description: "Error al cargar logs de errores",
                    exception: ex
                );

                TempData["Error"] = "Error al cargar los logs de errores.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Audit/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var log = await _auditLogService.GetByIdAsync(id);
                if (log == null)
                {
                    return NotFound();
                }

                // Verificar que el usuario tenga acceso al log (misma compañía)
                var user = await GetCurrentUserAsync();
                if (user?.Branch?.CompanyId != null && log.CompanyId != user.Branch.CompanyId)
                {
                    return Forbid();
                }

                return View(log);
            }
            catch (Exception ex)
            {
                await _loggingService.LogModuleErrorAsync(
                    module: AuditModule.SYSTEM.ToString(),
                    description: $"Error al ver detalles del log: {id}",
                    exception: ex,
                    recordId: id
                );

                TempData["Error"] = "Error al cargar los detalles del log.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Audit/ByModule
        public async Task<IActionResult> ByModule(string module)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                IEnumerable<AuditLog> logs;

                if (user?.Branch?.CompanyId != null)
                {
                    logs = await _auditLogService.GetByModuleAsync(module, user.Branch.CompanyId.Value);
                }
                else
                {
                    logs = await _auditLogService.GetByModuleAsync(module);
                }

                var viewModel = new AuditLogViewModel
                {
                    AuditLogs = logs.Take(500).ToList(),
                    ModuleFilter = module
                };

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                await _loggingService.LogModuleErrorAsync(
                    module: AuditModule.SYSTEM.ToString(),
                    description: $"Error al cargar logs por módulo: {module}",
                    exception: ex
                );

                TempData["Error"] = "Error al cargar los logs del módulo.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Audit/Export
        public async Task<IActionResult> Export(string? module = null, [FromQuery(Name = "action")] string? actionFilter = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                IEnumerable<AuditLog> logs;

                if (user?.Branch?.CompanyId != null)
                {
                    logs = await _auditLogService.GetByCompanyAsync(user.Branch.CompanyId.Value);
                }
                else
                {
                    logs = await _auditLogService.GetAllAsync();
                }

                // Aplicar filtros
                if (!string.IsNullOrEmpty(module))
                    logs = logs.Where(l => l.Module == module);
                if (!string.IsNullOrEmpty(actionFilter))
                    logs = logs.Where(l => l.Action == actionFilter);
                if (startDate.HasValue)
                    logs = logs.Where(l => l.Timestamp >= startDate.Value);
                if (endDate.HasValue)
                    logs = logs.Where(l => l.Timestamp <= endDate.Value);

                // ✅ NUEVO: Logging de exportación
                await _loggingService.LogDataExportAsync(
                    module: AuditModule.SYSTEM.ToString(),
                    description: "Exportación de logs de auditoría",
                    exportType: "CSV",
                    parameters: new { module, action = actionFilter, startDate, endDate, recordCount = logs.Count() }
                );

                // Generar CSV
                var csv = GenerateCsv(logs);
                var fileName = $"audit_logs_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                await _loggingService.LogModuleErrorAsync(
                    module: AuditModule.SYSTEM.ToString(),
                    description: "Error al exportar logs de auditoría",
                    exception: ex
                );

                TempData["Error"] = "Error al exportar los logs de auditoría.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Métodos auxiliares
        private async Task<User?> GetCurrentUserAsync()
        {
            var userIdValue = User?.FindFirst("UserId")?.Value
                ?? User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdValue) || !Guid.TryParse(userIdValue, out var userId))
                return null;

            return await _context.Users
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        private List<string> GetAvailableModules()
        {
            return Enum.GetValues<AuditModule>().Select(m => m.ToString()).ToList();
        }

        private List<string> GetAvailableActions()
        {
            return Enum.GetValues<AuditAction>().Select(a => a.ToString()).ToList();
        }

        private List<string> GetAvailableLogLevels()
        {
            return Enum.GetValues<LogLevel>().Select(l => l.ToString()).ToList();
        }

        private string GenerateCsv(IEnumerable<AuditLog> logs)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Timestamp,Module,Action,Description,User,Company,Branch,LogLevel,IsError,IPAddress");
            
            foreach (var log in logs)
            {
                csv.AppendLine($"\"{log.Timestamp}\",\"{log.Module}\",\"{log.Action}\",\"{log.Description}\",\"{log.User?.FullName}\",\"{log.Company?.Name}\",\"{log.Branch?.Name}\",\"{log.LogLevel}\",\"{log.IsError}\",\"{log.IpAddress}\"");
            }
            
            return csv.ToString();
        }
    }
} 