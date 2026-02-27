using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestBar.Helpers;
using RestBar.Interfaces;
using RestBar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestBar.Services
{
    /// <summary>
    /// Servicio para gestionar asignaciones de stock de productos a estaciones
    /// </summary>
    public class ProductStockAssignmentService : BaseTrackingService, IProductStockAssignmentService
    {
        private readonly ILogger<ProductStockAssignmentService>? _logger;

        public ProductStockAssignmentService(
            RestBarContext context, 
            IHttpContextAccessor httpContextAccessor,
            ILogger<ProductStockAssignmentService>? logger = null) 
            : base(context, httpContextAccessor)
        {
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las asignaciones de stock, opcionalmente filtradas por sucursal
        /// </summary>
        public async Task<IEnumerable<ProductStockAssignment>> GetAllAsync(Guid? branchId = null)
        {
            try
            {
                LoggingHelper.LogInfo(_logger, nameof(ProductStockAssignmentService), nameof(GetAllAsync), 
                    $"BranchId: {branchId?.ToString() ?? "null"}");
                
                var query = _context.ProductStockAssignments
                    .Include(psa => psa.Product)
                    .Include(psa => psa.Station)
                    .AsQueryable();

                // Contar total antes del filtro
                var totalCount = await query.CountAsync();
                LoggingHelper.LogData(_logger, nameof(ProductStockAssignmentService), nameof(GetAllAsync), 
                    $"Total asignaciones (sin filtro): {totalCount}");

                if (branchId.HasValue)
                {
                    query = query.Where(psa => psa.BranchId == branchId.Value);
                    var filteredCount = await query.CountAsync();
                    LoggingHelper.LogData(_logger, nameof(ProductStockAssignmentService), nameof(GetAllAsync), 
                        $"Total asignaciones (con filtro BranchId={branchId}): {filteredCount}");
                }

                var assignments = await query
                    .OrderBy(psa => psa.Product != null ? psa.Product.Name : "")
                    .ThenBy(psa => psa.Station != null ? psa.Station.Name : "")
                    .ToListAsync();
                
                LoggingHelper.LogSuccess(_logger, nameof(ProductStockAssignmentService), nameof(GetAllAsync), 
                    $"Total asignaciones retornadas: {assignments.Count}");
                
                // Log de detalles si hay asignaciones
                if (assignments.Count > 0)
                {
                    foreach (var assignment in assignments.Take(3))
                    {
                        LoggingHelper.LogData(_logger, nameof(ProductStockAssignmentService), nameof(GetAllAsync), 
                            $"ID: {assignment.Id}, Producto: {assignment.Product?.Name ?? "N/A"}, Estación: {assignment.Station?.Name ?? "N/A"}, Stock: {assignment.Stock}");
                    }
                }
                else
                {
                    LoggingHelper.LogWarning(_logger, nameof(ProductStockAssignmentService), nameof(GetAllAsync), 
                        "No hay asignaciones para retornar");
                }
                
                return assignments;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogError(_logger, nameof(ProductStockAssignmentService), nameof(GetAllAsync), ex);
                throw;
            }
        }

        /// <summary>
        /// Obtiene una asignación por su ID
        /// </summary>
        public async Task<ProductStockAssignment?> GetByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    throw new ArgumentException("El ID no puede estar vacío", nameof(id));
                }

                LoggingHelper.LogInfo(_logger, nameof(ProductStockAssignmentService), nameof(GetByIdAsync), $"Id: {id}");
                
                var assignment = await _context.ProductStockAssignments
                    .Include(psa => psa.Product)
                    .Include(psa => psa.Station)
                    .AsNoTracking() // Optimización: solo lectura
                    .FirstOrDefaultAsync(psa => psa.Id == id);

                if (assignment == null)
                {
                    LoggingHelper.LogWarning(_logger, nameof(ProductStockAssignmentService), nameof(GetByIdAsync), 
                        "Asignación no encontrada");
                }
                else
                {
                    LoggingHelper.LogSuccess(_logger, nameof(ProductStockAssignmentService), nameof(GetByIdAsync), 
                        "Asignación encontrada");
                }

                return assignment;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogError(_logger, nameof(ProductStockAssignmentService), nameof(GetByIdAsync), ex);
                throw;
            }
        }

        /// <summary>
        /// Obtiene asignaciones por ID de producto
        /// </summary>
        public async Task<IEnumerable<ProductStockAssignment>> GetByProductIdAsync(Guid productId, Guid? branchId = null)
        {
            try
            {
                if (productId == Guid.Empty)
                {
                    throw new ArgumentException("ProductId no puede estar vacío", nameof(productId));
                }

                LoggingHelper.LogParams(_logger, nameof(ProductStockAssignmentService), nameof(GetByProductIdAsync), 
                    $"ProductId: {productId}, BranchId: {branchId?.ToString() ?? "null"}");
                
                var query = _context.ProductStockAssignments
                    .Include(psa => psa.Product)
                    .Include(psa => psa.Station)
                    .Where(psa => psa.ProductId == productId && psa.IsActive);

                if (branchId.HasValue)
                {
                    query = query.Where(psa => psa.BranchId == branchId.Value);
                }

                var assignments = await query.ToListAsync();
                LoggingHelper.LogSuccess(_logger, nameof(ProductStockAssignmentService), nameof(GetByProductIdAsync), 
                    $"Total asignaciones: {assignments.Count}");
                return assignments;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogError(_logger, nameof(ProductStockAssignmentService), nameof(GetByProductIdAsync), ex);
                throw;
            }
        }

        /// <summary>
        /// Obtiene asignaciones por ID de estación
        /// </summary>
        public async Task<IEnumerable<ProductStockAssignment>> GetByStationIdAsync(Guid stationId, Guid? branchId = null)
        {
            try
            {
                if (stationId == Guid.Empty)
                {
                    throw new ArgumentException("StationId no puede estar vacío", nameof(stationId));
                }

                LoggingHelper.LogParams(_logger, nameof(ProductStockAssignmentService), nameof(GetByStationIdAsync), 
                    $"StationId: {stationId}, BranchId: {branchId?.ToString() ?? "null"}");
                
                var query = _context.ProductStockAssignments
                    .Include(psa => psa.Product)
                    .Include(psa => psa.Station)
                    .Where(psa => psa.StationId == stationId && psa.IsActive);

                if (branchId.HasValue)
                {
                    query = query.Where(psa => psa.BranchId == branchId.Value);
                }

                var assignments = await query.ToListAsync();
                LoggingHelper.LogSuccess(_logger, nameof(ProductStockAssignmentService), nameof(GetByStationIdAsync), 
                    $"Total asignaciones: {assignments.Count}");
                return assignments;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogError(_logger, nameof(ProductStockAssignmentService), nameof(GetByStationIdAsync), ex);
                throw;
            }
        }

        /// <summary>
        /// Crea una nueva asignación de stock con validaciones
        /// </summary>
        public async Task<ProductStockAssignment> CreateAsync(ProductStockAssignment assignment)
        {
            try
            {
                // Validaciones de entrada
                if (assignment == null)
                {
                    throw new ArgumentNullException(nameof(assignment), "La asignación no puede ser nula");
                }

                if (assignment.ProductId == Guid.Empty)
                {
                    throw new ArgumentException("ProductId es requerido", nameof(assignment));
                }

                if (assignment.StationId == Guid.Empty)
                {
                    throw new ArgumentException("StationId es requerido", nameof(assignment));
                }

                if (assignment.Stock < 0)
                {
                    throw new ArgumentException("El stock no puede ser negativo", nameof(assignment));
                }

                LoggingHelper.LogParams(_logger, nameof(ProductStockAssignmentService), nameof(CreateAsync), 
                    $"ProductId: {assignment.ProductId}, StationId: {assignment.StationId}, Stock: {assignment.Stock}");
                
                // Obtener usuario actual para CompanyId y BranchId
                var userIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    var user = await _context.Users
                        .Include(u => u.Branch)
                        .FirstOrDefaultAsync(u => u.Id == userId);
                    
                    if (user != null && user.Branch != null)
                    {
                        assignment.CompanyId = user.Branch.CompanyId;
                        assignment.BranchId = user.BranchId;
                        LoggingHelper.LogSuccess(_logger, nameof(ProductStockAssignmentService), nameof(CreateAsync), 
                            $"Asignando CompanyId: {assignment.CompanyId}, BranchId: {assignment.BranchId}");
                    }
                }

                // Generar ID si no existe
                if (assignment.Id == Guid.Empty)
                {
                    assignment.Id = Guid.NewGuid();
                }

                // Verificar si ya existe una asignación para este producto y estación
                var existing = await _context.ProductStockAssignments
                    .FirstOrDefaultAsync(psa => psa.ProductId == assignment.ProductId 
                        && psa.StationId == assignment.StationId 
                        && psa.BranchId == assignment.BranchId);

                if (existing != null)
                {
                    LoggingHelper.LogWarning(_logger, nameof(ProductStockAssignmentService), nameof(CreateAsync), 
                        "Ya existe una asignación para este producto y estación");
                    throw new InvalidOperationException("Ya existe una asignación de stock para este producto en esta estación");
                }

                SetCreatedTracking(assignment);
                
                _context.ProductStockAssignments.Add(assignment);
                await _context.SaveChangesAsync();

                LoggingHelper.LogSuccess(_logger, nameof(ProductStockAssignmentService), nameof(CreateAsync), 
                    $"Asignación creada exitosamente: {assignment.Id}");
                return assignment;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogError(_logger, nameof(ProductStockAssignmentService), nameof(CreateAsync), ex);
                throw;
            }
        }

        /// <summary>
        /// Actualiza una asignación existente con validaciones
        /// </summary>
        public async Task<ProductStockAssignment> UpdateAsync(Guid id, ProductStockAssignment assignment)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    throw new ArgumentException("El ID no puede estar vacío", nameof(id));
                }

                if (assignment == null)
                {
                    throw new ArgumentNullException(nameof(assignment), "La asignación no puede ser nula");
                }

                if (assignment.Stock < 0)
                {
                    throw new ArgumentException("El stock no puede ser negativo", nameof(assignment));
                }

                LoggingHelper.LogInfo(_logger, nameof(ProductStockAssignmentService), nameof(UpdateAsync), $"Id: {id}");
                
                var existing = await _context.ProductStockAssignments.FindAsync(id);
                if (existing == null)
                {
                    LoggingHelper.LogWarning(_logger, nameof(ProductStockAssignmentService), nameof(UpdateAsync), 
                        "Asignación no encontrada");
                    throw new KeyNotFoundException($"Asignación con ID {id} no encontrada");
                }

                // Actualizar campos
                existing.ProductId = assignment.ProductId;
                existing.StationId = assignment.StationId;
                existing.Stock = assignment.Stock;
                existing.MinStock = assignment.MinStock;
                existing.Priority = assignment.Priority;
                existing.IsActive = assignment.IsActive;

                SetUpdatedTracking(existing);
                
                await _context.SaveChangesAsync();

                LoggingHelper.LogSuccess(_logger, nameof(ProductStockAssignmentService), nameof(UpdateAsync), 
                    "Asignación actualizada exitosamente");
                return existing;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogError(_logger, nameof(ProductStockAssignmentService), nameof(UpdateAsync), ex);
                throw;
            }
        }

        /// <summary>
        /// Elimina una asignación por su ID
        /// </summary>
        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    throw new ArgumentException("El ID no puede estar vacío", nameof(id));
                }

                LoggingHelper.LogInfo(_logger, nameof(ProductStockAssignmentService), nameof(DeleteAsync), $"Id: {id}");
                
                var assignment = await _context.ProductStockAssignments.FindAsync(id);
                if (assignment == null)
                {
                    LoggingHelper.LogWarning(_logger, nameof(ProductStockAssignmentService), nameof(DeleteAsync), 
                        "Asignación no encontrada");
                    return false;
                }

                _context.ProductStockAssignments.Remove(assignment);
                await _context.SaveChangesAsync();

                LoggingHelper.LogSuccess(_logger, nameof(ProductStockAssignmentService), nameof(DeleteAsync), 
                    "Asignación eliminada exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                LoggingHelper.LogError(_logger, nameof(ProductStockAssignmentService), nameof(DeleteAsync), ex);
                throw;
            }
        }
    }
}

