using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestBar.Interfaces;
using RestBar.Models;
using System.Text.Json;

namespace RestBar.Controllers
{
    [Authorize(Policy = "UserManagement")] // Roles: admin, manager, supervisor
    public class UserAssignmentController : Controller
    {
        private readonly IUserAssignmentService _userAssignmentService;
        private readonly IUserService _userService;
        private readonly IStationService _stationService;
        private readonly IAreaService _areaService;
        private readonly ITableService _tableService;
        private readonly IAuthService _authService;

        public UserAssignmentController(
            IUserAssignmentService userAssignmentService,
            IUserService userService,
            IStationService stationService,
            IAreaService areaService,
            ITableService tableService,
            IAuthService authService)
        {
            _userAssignmentService = userAssignmentService;
            _userService = userService;
            _stationService = stationService;
            _areaService = areaService;
            _tableService = tableService;
            _authService = authService;
        }

        private async Task<bool> HasAssignmentPermissionAsync()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync(User);
                if (currentUser == null) return false;
                
                return currentUser.Role == UserRole.admin || 
                       currentUser.Role == UserRole.manager || 
                       currentUser.Role == UserRole.supervisor;
            }
            catch
            {
                return false;
            }
        }

        private async Task<IActionResult> CheckAssignmentPermissionAsync()
        {
            if (!await HasAssignmentPermissionAsync())
            {
                return Json(new { success = false, message = "No tienes permisos para gestionar asignaciones" });
            }
            return null;
        }

        [Authorize(Policy = "UserManagement")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAssignments(string role = "", string assignmentType = "")
        {
            var permissionCheck = await CheckAssignmentPermissionAsync();
            if (permissionCheck != null) return permissionCheck;

            try
            {
                var assignments = await _userAssignmentService.GetAllAsync();

                // Filtrar por rol si se especifica
                if (!string.IsNullOrEmpty(role) && Enum.TryParse<UserRole>(role, true, out var userRole))
                {
                    assignments = assignments.Where(a => a.User.Role == userRole);
                }

                // Filtrar por tipo de asignación
                if (!string.IsNullOrEmpty(assignmentType))
                {
                    assignments = assignmentType.ToLower() switch
                    {
                        "station" => assignments.Where(a => a.StationId.HasValue),
                        "area" => assignments.Where(a => a.AreaId.HasValue && (a.AssignedTableIds?.Count == 0 || a.AssignedTableIds == null)),
                        "tables" => assignments.Where(a => a.AssignedTableIds?.Count > 0),
                        _ => assignments
                    };
                }

                var assignmentData = assignments.Select(a => new {
                    id = a.Id,
                    userId = a.UserId,
                    userName = a.User.FullName ?? a.User.Email,
                    userEmail = a.User.Email,
                    userRole = a.User.Role.ToString().ToLower(),
                    stationId = a.StationId,
                    stationName = a.Station?.Name,
                    areaId = a.AreaId,
                    areaName = a.Area?.Name,
                    assignedTableIds = a.AssignedTableIds ?? new List<Guid>(),
                    assignedAt = a.AssignedAt,
                    isActive = a.IsActive,
                    notes = a.Notes
                }).ToList();

                return Json(new { success = true, data = assignmentData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUnassignedUsers(string role)
        {
            var permissionCheck = await CheckAssignmentPermissionAsync();
            if (permissionCheck != null) return permissionCheck;

            try
            {
                if (!Enum.TryParse<UserRole>(role, true, out var userRole))
                {
                    return Json(new { success = false, message = "Rol no válido" });
                }

                var users = await _userAssignmentService.GetUnassignedUsersByRoleAsync(userRole);
                
                var userData = users.Select(u => new {
                    id = u.Id,
                    fullName = u.FullName ?? u.Email,
                    email = u.Email,
                    role = u.Role.ToString().ToLower()
                }).ToList();

                return Json(new { success = true, data = userData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStations()
        {
            var permissionCheck = await CheckAssignmentPermissionAsync();
            if (permissionCheck != null) return permissionCheck;

            try
            {
                var stations = await _userAssignmentService.GetAvailableStationsAsync();
                
                var stationData = stations.Select(s => new {
                    id = s.Id,
                    name = s.Name,
                    type = s.Type,
                    icon = s.Icon,
                    areaId = s.AreaId,
                    areaName = s.Area?.Name
                }).ToList();

                return Json(new { success = true, data = stationData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAreas()
        {
            var permissionCheck = await CheckAssignmentPermissionAsync();
            if (permissionCheck != null) return permissionCheck;

            try
            {
                var areas = await _userAssignmentService.GetAvailableAreasAsync();
                
                var areaData = areas.Select(a => new {
                    id = a.Id,
                    name = a.Name,
                    description = a.Description,
                    branchId = a.BranchId,
                    branchName = a.Branch?.Name
                }).ToList();

                return Json(new { success = true, data = areaData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTables(Guid? areaId = null)
        {
            var permissionCheck = await CheckAssignmentPermissionAsync();
            if (permissionCheck != null) return permissionCheck;

            try
            {
                var tables = await _userAssignmentService.GetAvailableTablesAsync(areaId);
                
                var tableData = tables.Select(t => new {
                    id = t.Id,
                    tableNumber = t.TableNumber,
                    capacity = t.Capacity,
                    areaId = t.AreaId,
                    areaName = t.Area?.Name,
                    status = t.Status,
                    isActive = t.IsActive
                }).ToList();

                return Json(new { success = true, data = tableData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAssignment([FromForm] UserAssignment assignment, [FromForm] string assignedTableIdsJson = "")
        {
            var permissionCheck = await CheckAssignmentPermissionAsync();
            if (permissionCheck != null) return permissionCheck;

            try
            {
                // Validar usuario
                var user = await _userService.GetByIdAsync(assignment.UserId);
                if (user == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                // Validar que el usuario no tenga asignación activa
                if (await _userAssignmentService.HasActiveAssignmentAsync(assignment.UserId))
                {
                    return Json(new { success = false, message = "El usuario ya tiene una asignación activa" });
                }

                // Procesar mesas asignadas si es mesero
                if (user.Role == UserRole.waiter && !string.IsNullOrEmpty(assignedTableIdsJson))
                {
                    try
                    {
                        assignment.AssignedTableIds = JsonSerializer.Deserialize<List<Guid>>(assignedTableIdsJson) ?? new List<Guid>();
                    }
                    catch
                    {
                        assignment.AssignedTableIds = new List<Guid>();
                    }
                }

                // Validar según el rol
                switch (user.Role)
                {
                    case UserRole.chef:
                    case UserRole.bartender:
                        if (!assignment.StationId.HasValue)
                        {
                            return Json(new { success = false, message = "Debe asignar una estación" });
                        }
                        // Verificar que la estación exista
                        var station = await _stationService.GetStationByIdAsync(assignment.StationId.Value);
                        if (station == null)
                        {
                            return Json(new { success = false, message = "Estación no encontrada" });
                        }
                        break;

                    case UserRole.waiter:
                        if (!assignment.AreaId.HasValue)
                        {
                            return Json(new { success = false, message = "Debe asignar un área" });
                        }
                        // Verificar que el área exista
                        var area = await _areaService.GetByIdAsync(assignment.AreaId.Value);
                        if (area == null)
                        {
                            return Json(new { success = false, message = "Área no encontrada" });
                        }
                        break;

                    default:
                        return Json(new { success = false, message = "Rol no válido para asignación" });
                }

                var createdAssignment = await _userAssignmentService.CreateAsync(assignment);
                
                return Json(new { 
                    success = true, 
                    data = new {
                        id = createdAssignment.Id,
                        userId = createdAssignment.UserId,
                        userName = user.FullName ?? user.Email,
                        userRole = user.Role.ToString().ToLower(),
                        stationId = createdAssignment.StationId,
                        areaId = createdAssignment.AreaId,
                        assignedTableIds = createdAssignment.AssignedTableIds,
                        assignedAt = createdAssignment.AssignedAt,
                        isActive = createdAssignment.IsActive,
                        notes = createdAssignment.Notes
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al crear la asignación: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAssignment([FromForm] UserAssignment assignment, [FromForm] string assignedTableIdsJson = "")
        {
            var permissionCheck = await CheckAssignmentPermissionAsync();
            if (permissionCheck != null) return permissionCheck;

            try
            {
                var existingAssignment = await _userAssignmentService.GetByIdAsync(assignment.Id);
                if (existingAssignment == null)
                {
                    return Json(new { success = false, message = "Asignación no encontrada" });
                }

                // Procesar mesas asignadas si es mesero
                if (!string.IsNullOrEmpty(assignedTableIdsJson))
                {
                    try
                    {
                        assignment.AssignedTableIds = JsonSerializer.Deserialize<List<Guid>>(assignedTableIdsJson) ?? new List<Guid>();
                    }
                    catch
                    {
                        assignment.AssignedTableIds = new List<Guid>();
                    }
                }

                // Mantener campos que no deben ser modificados
                assignment.UserId = existingAssignment.UserId;
                assignment.AssignedAt = existingAssignment.AssignedAt;

                await _userAssignmentService.UpdateAsync(assignment);

                return Json(new { success = true, message = "Asignación actualizada exitosamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al actualizar la asignación: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UnassignUser(Guid userId)
        {
            var permissionCheck = await CheckAssignmentPermissionAsync();
            if (permissionCheck != null) return permissionCheck;

            try
            {
                var result = await _userAssignmentService.UnassignUserAsync(userId);
                
                if (result)
                {
                    return Json(new { success = true, message = "Usuario desasignado exitosamente" });
                }
                else
                {
                    return Json(new { success = false, message = "No se encontró asignación activa para este usuario" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al desasignar usuario: {ex.Message}" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAssignment(Guid id)
        {
            var permissionCheck = await CheckAssignmentPermissionAsync();
            if (permissionCheck != null) return permissionCheck;

            try
            {
                await _userAssignmentService.DeleteAsync(id);
                return Json(new { success = true, message = "Asignación eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al eliminar la asignación: {ex.Message}" });
            }
        }
    }
} 