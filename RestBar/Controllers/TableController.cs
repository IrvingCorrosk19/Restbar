using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestBar.Interfaces;
using System;
using System.Threading.Tasks;
using System.Linq;
using RestBar.Models;
using System.Security.Claims;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace RestBar.Controllers
{
    [Authorize(Roles = "admin,manager,supervisor")]
    public class TableController : Controller
    {
        private readonly ITableService _tableService;
        private readonly IAreaService _areaService;
        private readonly IUserService _userService;

        public TableController(ITableService tableService, IAreaService areaService, IUserService userService)
        {
            _tableService = tableService;
            _areaService = areaService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                Console.WriteLine("🔍 [TableController] Index() - Iniciando carga de mesas...");
                
                // ✅ NUEVO: Obtener CompanyId y BranchId del usuario actual
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    Console.WriteLine("❌ [TableController] Index() - Usuario no autenticado o UserId inválido");
                    return RedirectToAction("Login", "Auth");
                }

                var currentUser = await _userService.GetCurrentUserWithAssignmentsAsync(userId);
                if (currentUser == null)
                {
                    Console.WriteLine("❌ [TableController] Index() - Usuario actual no encontrado");
                    return RedirectToAction("Login", "Auth");
                }

                // ✅ NUEVO: Validar que el usuario tenga CompanyId y BranchId asignados
                if (currentUser.Branch?.CompanyId == null || currentUser.BranchId == null)
                {
                    Console.WriteLine("❌ [TableController] Index() - Usuario sin CompanyId o BranchId asignado");
                    return RedirectToAction("Login", "Auth");
                }

                // ✅ NUEVO: Filtrar mesas por CompanyId y BranchId del usuario actual
                var tables = await _tableService.GetTablesByCompanyAndBranchAsync(currentUser.Branch.CompanyId.Value, currentUser.BranchId.Value);
                
                Console.WriteLine($"✅ [TableController] Index() - Mesas cargadas: {tables.Count()}");
                return View(tables);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [TableController] Index() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [TableController] Index() - StackTrace: {ex.StackTrace}");
                return View(new List<Table>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTables()
        {
            try
            {
                Console.WriteLine("🔍 [TableController] GetTables() - Iniciando carga de mesas...");
                
                // ✅ NUEVO: Obtener CompanyId y BranchId del usuario actual
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    Console.WriteLine("❌ [TableController] GetTables() - Usuario no autenticado o UserId inválido");
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var currentUser = await _userService.GetCurrentUserWithAssignmentsAsync(userId);
                if (currentUser == null)
                {
                    Console.WriteLine("❌ [TableController] GetTables() - Usuario actual no encontrado");
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                // ✅ NUEVO: Validar que el usuario tenga CompanyId y BranchId asignados
                if (currentUser.Branch?.CompanyId == null || currentUser.BranchId == null)
                {
                    Console.WriteLine("❌ [TableController] GetTables() - Usuario sin CompanyId o BranchId asignado");
                    return Json(new { success = false, message = "Usuario sin compañía o sucursal asignada" });
                }

                Console.WriteLine($"🔍 [TableController] GetTables() - Usuario: {currentUser.Email}");
                Console.WriteLine($"🔍 [TableController] GetTables() - CompanyId: {currentUser.Branch.CompanyId}");
                Console.WriteLine($"🔍 [TableController] GetTables() - BranchId: {currentUser.BranchId}");

                // ✅ NUEVO: Filtrar mesas por CompanyId y BranchId del usuario actual
                var tables = await _tableService.GetTablesByCompanyAndBranchAsync(currentUser.Branch.CompanyId.Value, currentUser.BranchId.Value);
                
                var data = tables.Select(t => new {
                    id = t.Id,
                    tableNumber = t.TableNumber,
                    capacity = t.Capacity,
                    status = t.Status,
                    isActive = t.IsActive,
                    areaId = t.AreaId,
                    areaName = t.Area != null ? t.Area.Name : null
                }).ToList();
                
                Console.WriteLine($"✅ [TableController] GetTables() - Mesas cargadas: {data.Count}");
                
                // ✅ NUEVO: Mostrar detalles de las mesas que se envían
                foreach (var table in data)
                {
                    Console.WriteLine($"🔍 [TableController] GetTables() - Mesa a enviar: ID={table.id}, TableNumber={table.tableNumber}, Status={table.status}, Area={table.areaName ?? "Sin área"}");
                }
                
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [TableController] GetTables() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [TableController] GetTables() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAreas()
        {
            try
            {
                Console.WriteLine("🔍 [TableController] GetAreas() - Iniciando carga de áreas...");
                
                // ✅ NUEVO: Obtener CompanyId y BranchId del usuario actual
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    Console.WriteLine("❌ [TableController] GetAreas() - Usuario no autenticado o UserId inválido");
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var currentUser = await _userService.GetCurrentUserWithAssignmentsAsync(userId);
                if (currentUser == null)
                {
                    Console.WriteLine("❌ [TableController] GetAreas() - Usuario actual no encontrado");
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                // ✅ NUEVO: Validar que el usuario tenga CompanyId y BranchId asignados
                if (currentUser.Branch?.CompanyId == null || currentUser.BranchId == null)
                {
                    Console.WriteLine("❌ [TableController] GetAreas() - Usuario sin CompanyId o BranchId asignado");
                    return Json(new { success = false, message = "Usuario sin compañía o sucursal asignada" });
                }

                // ✅ NUEVO: Filtrar áreas por CompanyId y BranchId del usuario actual
                var areas = await _areaService.GetAreasByCompanyAndBranchAsync(currentUser.Branch.CompanyId.Value, currentUser.BranchId.Value);
                
                var data = areas.Select(a => new { id = a.Id, name = a.Name }).ToList();
                
                Console.WriteLine($"✅ [TableController] GetAreas() - Áreas cargadas: {data.Count}");
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [TableController] GetAreas() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [TableController] GetAreas() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(Guid id)
        {
            var table = await _tableService.GetByIdAsync(id);
            if (table == null)
                return Json(new { success = false, message = "Mesa no encontrada" });
            return Json(new { success = true, data = new {
                id = table.Id,
                tableNumber = table.TableNumber,
                capacity = table.Capacity,
                status = table.Status,
                isActive = table.IsActive,
                areaId = table.AreaId
            }});
        }


        // ✅ MÉTODO PARA CORREGIR VALORES DE TABLESTATUS EN LA BASE DE DATOS
        [HttpGet]
        public async Task<IActionResult> FixTableStatus()
        {
            try
            {
                Console.WriteLine("🔍 [TableController] FixTableStatus() - Iniciando corrección de valores...");
                
                // Obtener el contexto de la base de datos
                var context = _tableService.GetContext();
                
                // Ejecutar SQL para actualizar valores de inglés a español
                var updateSql = @"
                    UPDATE public.tables SET status = 'Disponible'    WHERE UPPER(status) = 'AVAILABLE';
                    UPDATE public.tables SET status = 'Ocupada'       WHERE UPPER(status) = 'OCCUPIED';
                    UPDATE public.tables SET status = 'Reservada'     WHERE UPPER(status) = 'RESERVED';
                    UPDATE public.tables SET status = 'EnEspera'      WHERE UPPER(status) = 'WAITING';
                    UPDATE public.tables SET status = 'Atendida'      WHERE UPPER(status) = 'ATTENDED';
                    UPDATE public.tables SET status = 'EnPreparacion' WHERE UPPER(status) = 'PREPARING';
                    UPDATE public.tables SET status = 'Servida'       WHERE UPPER(status) = 'SERVED';
                    UPDATE public.tables SET status = 'ParaPago'      WHERE UPPER(status) = 'READY_FOR_PAYMENT';
                    UPDATE public.tables SET status = 'Pagada'        WHERE UPPER(status) = 'PAID';
                    UPDATE public.tables SET status = 'Bloqueada'     WHERE UPPER(status) = 'BLOCKED';";
                
                var rowsAffected = await context.Database.ExecuteSqlRawAsync(updateSql);
                Console.WriteLine($"✅ [TableController] FixTableStatus() - Actualizadas {rowsAffected} filas");
                
                // Agregar constraint de validación
                var constraintSql = @"
                    ALTER TABLE public.tables
                      ADD CONSTRAINT ck_tables_status_allowed
                      CHECK (status IN (
                        'Disponible',
                        'Ocupada',
                        'Reservada',
                        'EnEspera',
                        'Atendida',
                        'EnPreparacion',
                        'Servida',
                        'ParaPago',
                        'Pagada',
                        'Bloqueada'
                      ));";
                
                try
                {
                    await context.Database.ExecuteSqlRawAsync(constraintSql);
                    Console.WriteLine("✅ [TableController] FixTableStatus() - Constraint de validación agregado");
                }
                catch (Exception constraintEx)
                {
                    Console.WriteLine($"⚠️ [TableController] FixTableStatus() - Constraint ya existe o error: {constraintEx.Message}");
                }
                
                return Json(new { 
                    success = true, 
                    message = $"✅ Corregidos {rowsAffected} registros de TableStatus y agregado constraint de validación" 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [TableController] FixTableStatus() - Error: {ex.Message}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Table model)
        {
            try
            {
                Console.WriteLine("🔍 [TableController] Create() - Iniciando creación de mesa...");
                
                // ✅ NUEVO: Logs detallados para diagnosticar el problema
                Console.WriteLine($"🔍 [TableController] Create() - Request.ContentType: {Request.ContentType}");
                Console.WriteLine($"🔍 [TableController] Create() - Request.ContentLength: {Request.ContentLength}");
                Console.WriteLine($"🔍 [TableController] Create() - Request.Method: {Request.Method}");
                Console.WriteLine($"🔍 [TableController] Create() - Request.Headers: {string.Join(", ", Request.Headers.Select(h => $"{h.Key}={h.Value}"))}");
                
                // ✅ NUEVO: Leer el body raw para ver qué está llegando
                Request.EnableBuffering();
                Request.Body.Position = 0;
                using var reader = new StreamReader(Request.Body, Encoding.UTF8);
                var rawBody = await reader.ReadToEndAsync();
                Request.Body.Position = 0;
                Console.WriteLine($"🔍 [TableController] Create() - Raw body: {rawBody}");
                
                // ✅ NUEVO: Validar que el modelo no sea null
                if (model == null)
                {
                    Console.WriteLine("❌ [TableController] Create() - Model es null");
                    Console.WriteLine("❌ [TableController] Create() - Posibles causas:");
                    Console.WriteLine("   - JSON malformado");
                    Console.WriteLine("   - Content-Type incorrecto");
                    Console.WriteLine("   - Model binding fallido");
                    Console.WriteLine("   - Propiedades del modelo no coinciden");
                    return Json(new { success = false, message = "Datos de la mesa no válidos" });
                }
                
                Console.WriteLine($"🔍 [TableController] Create() - Model recibido: TableNumber='{model.TableNumber}', Capacity={model.Capacity}, AreaId={model.AreaId}");
                
                if (string.IsNullOrWhiteSpace(model.TableNumber))
                    return Json(new { success = false, message = "El número de mesa es requerido" });
                if (model.Capacity <= 0)
                    return Json(new { success = false, message = "La capacidad debe ser mayor a 0" });
                if (model.AreaId == null)
                    return Json(new { success = false, message = "El área es requerida" });

                // ✅ NUEVO: Extraer CompanyId y BranchId del usuario actual
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    Console.WriteLine("❌ [TableController] Create() - Usuario no autenticado o UserId inválido");
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var currentUser = await _userService.GetCurrentUserWithAssignmentsAsync(userId);
                if (currentUser == null)
                {
                    Console.WriteLine("❌ [TableController] Create() - Usuario actual no encontrado");
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                // ✅ NUEVO: Validar que el usuario tenga CompanyId y BranchId asignados
                if (currentUser.Branch?.CompanyId == null || currentUser.BranchId == null)
                {
                    Console.WriteLine("❌ [TableController] Create() - Usuario sin CompanyId o BranchId asignado");
                    return Json(new { success = false, message = "Usuario sin compañía o sucursal asignada" });
                }

                // Asignar CompanyId y BranchId del usuario actual
                model.CompanyId = currentUser.Branch.CompanyId.Value;
                model.BranchId = currentUser.BranchId.Value;

                Console.WriteLine($"✅ [TableController] Create() - CompanyId: {model.CompanyId}, BranchId: {model.BranchId}");
                
                var created = await _tableService.CreateAsync(model);
                Console.WriteLine($"✅ [TableController] Create() - Mesa creada exitosamente con ID: {created.Id}");
                
                return Json(new { success = true, data = created });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [TableController] Create() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [TableController] Create() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Edit(Guid id, [FromBody] Table model)
        {
            try
            {
                Console.WriteLine("🔍 [TableController] Edit() - Iniciando edición de mesa...");
                
                if (id != model.Id)
                    return Json(new { success = false, message = "ID no coincide" });
                if (string.IsNullOrWhiteSpace(model.TableNumber))
                    return Json(new { success = false, message = "El número de mesa es requerido" });
                if (model.Capacity <= 0)
                    return Json(new { success = false, message = "La capacidad debe ser mayor a 0" });
                if (model.AreaId == null)
                    return Json(new { success = false, message = "El área es requerida" });

                // ✅ NUEVO: Extraer CompanyId y BranchId del usuario actual
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    Console.WriteLine("❌ [TableController] Edit() - Usuario no autenticado o UserId inválido");
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var currentUser = await _userService.GetCurrentUserWithAssignmentsAsync(userId);
                if (currentUser == null)
                {
                    Console.WriteLine("❌ [TableController] Edit() - Usuario actual no encontrado");
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                // ✅ NUEVO: Validar que el usuario tenga CompanyId y BranchId asignados
                if (currentUser.Branch?.CompanyId == null || currentUser.BranchId == null)
                {
                    Console.WriteLine("❌ [TableController] Edit() - Usuario sin CompanyId o BranchId asignado");
                    return Json(new { success = false, message = "Usuario sin compañía o sucursal asignada" });
                }

                // Asignar CompanyId y BranchId del usuario actual
                model.CompanyId = currentUser.Branch.CompanyId.Value;
                model.BranchId = currentUser.BranchId.Value;

                Console.WriteLine($"✅ [TableController] Edit() - CompanyId: {model.CompanyId}, BranchId: {model.BranchId}");
                
                await _tableService.UpdateAsync(model);
                Console.WriteLine($"✅ [TableController] Edit() - Mesa actualizada exitosamente");
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [TableController] Edit() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [TableController] Edit() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _tableService.DeleteAsync(id);
            return Json(new { success = true });
        }

        // ✅ Liberar mesas fantasma: OCUPADA sin órdenes activas → Disponible
        [HttpPost]
        public async Task<IActionResult> ReleaseGhostTables()
        {
            try
            {
                Console.WriteLine("🔍 [TableController] ReleaseGhostTables() - Iniciando...");

                var context = _tableService.GetContext();

                // Mesas que están OCUPADA/EnPreparacion/ParaPago pero no tienen
                // ninguna orden en estado activo
                var activeStatuses = new[]
                {
                    OrderStatus.Pending,
                    OrderStatus.SentToKitchen,
                    OrderStatus.Preparing,
                    OrderStatus.Ready,
                    OrderStatus.ReadyToPay,
                    OrderStatus.Served
                };

                var ghostTables = await context.Tables
                    .Where(t => t.Status == TableStatus.Ocupada ||
                                t.Status == TableStatus.EnPreparacion ||
                                t.Status == TableStatus.ParaPago)
                    .Where(t => !context.Orders
                        .Any(o => o.TableId == t.Id && activeStatuses.Contains(o.Status)))
                    .ToListAsync();

                Console.WriteLine($"🔍 [TableController] ReleaseGhostTables() - Mesas fantasma encontradas: {ghostTables.Count}");

                var released = new List<object>();
                foreach (var table in ghostTables)
                {
                    var prev = table.Status.ToString();
                    table.Status = TableStatus.Disponible;
                    released.Add(new { id = table.Id, tableNumber = table.TableNumber, previousStatus = prev });
                    Console.WriteLine($"✅ [TableController] ReleaseGhostTables() - Mesa {table.TableNumber}: {prev} → Disponible");
                }

                await context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    releasedCount = released.Count,
                    tables = released,
                    message = $"Se liberaron {released.Count} mesa(s) fantasma"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [TableController] ReleaseGhostTables() - Error: {ex.Message}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MergeTables([FromBody] MergeTablesDto dto)
        {
            try
            {
                if (dto.PrimaryTableId == Guid.Empty || dto.SecondaryTableId == Guid.Empty)
                    return Json(new { success = false, message = "IDs de mesa requeridos" });

                var result = await _tableService.MergeTablesAsync(dto.PrimaryTableId, dto.SecondaryTableId);
                return Json(new
                {
                    success = true,
                    primaryTableId = result.PrimaryTableId,
                    secondaryTableId = result.SecondaryTableId,
                    combinedCapacity = result.CombinedCapacity,
                    movedOrderId = result.MovedOrderId
                });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SplitTables([FromBody] SplitTablesDto dto)
        {
            try
            {
                if (dto.PrimaryTableId == Guid.Empty)
                    return Json(new { success = false, message = "ID de mesa principal requerido" });

                var result = await _tableService.SplitTablesAsync(dto.PrimaryTableId);
                return Json(new
                {
                    success = true,
                    primaryTableId = result.PrimaryTableId,
                    restoredTables = result.RestoredTables,
                    primaryCapacity = result.PrimaryCapacity
                });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }

    public class MergeTablesDto
    {
        public Guid PrimaryTableId { get; set; }
        public Guid SecondaryTableId { get; set; }
    }

    public class SplitTablesDto
    {
        public Guid PrimaryTableId { get; set; }
    }
} 