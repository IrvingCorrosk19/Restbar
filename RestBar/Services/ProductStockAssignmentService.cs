using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestBar.Services
{
    public class ProductStockAssignmentService : BaseTrackingService, IProductStockAssignmentService
    {
        public ProductStockAssignmentService(RestBarContext context, IHttpContextAccessor httpContextAccessor) 
            : base(context, httpContextAccessor)
        {
        }

        public async Task<IEnumerable<ProductStockAssignment>> GetAllAsync(Guid? branchId = null)
        {
            try
            {
                Console.WriteLine($"🔍 [ProductStockAssignmentService] GetAllAsync() - BranchId: {branchId}");
                
                var query = _context.ProductStockAssignments
                    .Include(psa => psa.Product)
                    .Include(psa => psa.Station)
                    .Include(psa => psa.Company)
                    .Include(psa => psa.Branch)
                    .AsQueryable();

                if (branchId.HasValue)
                {
                    query = query.Where(psa => psa.BranchId == branchId.Value);
                }

                var assignments = await query.ToListAsync();
                Console.WriteLine($"✅ [ProductStockAssignmentService] GetAllAsync() - Total asignaciones: {assignments.Count}");
                return assignments;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductStockAssignmentService] GetAllAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [ProductStockAssignmentService] GetAllAsync() - StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<ProductStockAssignment?> GetByIdAsync(Guid id)
        {
            try
            {
                Console.WriteLine($"🔍 [ProductStockAssignmentService] GetByIdAsync() - Id: {id}");
                
                var assignment = await _context.ProductStockAssignments
                    .Include(psa => psa.Product)
                    .Include(psa => psa.Station)
                    .Include(psa => psa.Company)
                    .Include(psa => psa.Branch)
                    .FirstOrDefaultAsync(psa => psa.Id == id);

                if (assignment == null)
                {
                    Console.WriteLine($"⚠️ [ProductStockAssignmentService] GetByIdAsync() - Asignación no encontrada");
                }
                else
                {
                    Console.WriteLine($"✅ [ProductStockAssignmentService] GetByIdAsync() - Asignación encontrada");
                }

                return assignment;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductStockAssignmentService] GetByIdAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [ProductStockAssignmentService] GetByIdAsync() - StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<IEnumerable<ProductStockAssignment>> GetByProductIdAsync(Guid productId, Guid? branchId = null)
        {
            try
            {
                Console.WriteLine($"🔍 [ProductStockAssignmentService] GetByProductIdAsync() - ProductId: {productId}, BranchId: {branchId}");
                
                var query = _context.ProductStockAssignments
                    .Include(psa => psa.Product)
                    .Include(psa => psa.Station)
                    .Where(psa => psa.ProductId == productId && psa.IsActive);

                if (branchId.HasValue)
                {
                    query = query.Where(psa => psa.BranchId == branchId.Value);
                }

                var assignments = await query.ToListAsync();
                Console.WriteLine($"✅ [ProductStockAssignmentService] GetByProductIdAsync() - Total asignaciones: {assignments.Count}");
                return assignments;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductStockAssignmentService] GetByProductIdAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [ProductStockAssignmentService] GetByProductIdAsync() - StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<IEnumerable<ProductStockAssignment>> GetByStationIdAsync(Guid stationId, Guid? branchId = null)
        {
            try
            {
                Console.WriteLine($"🔍 [ProductStockAssignmentService] GetByStationIdAsync() - StationId: {stationId}, BranchId: {branchId}");
                
                var query = _context.ProductStockAssignments
                    .Include(psa => psa.Product)
                    .Include(psa => psa.Station)
                    .Where(psa => psa.StationId == stationId && psa.IsActive);

                if (branchId.HasValue)
                {
                    query = query.Where(psa => psa.BranchId == branchId.Value);
                }

                var assignments = await query.ToListAsync();
                Console.WriteLine($"✅ [ProductStockAssignmentService] GetByStationIdAsync() - Total asignaciones: {assignments.Count}");
                return assignments;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductStockAssignmentService] GetByStationIdAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [ProductStockAssignmentService] GetByStationIdAsync() - StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<ProductStockAssignment> CreateAsync(ProductStockAssignment assignment)
        {
            try
            {
                Console.WriteLine($"🔍 [ProductStockAssignmentService] CreateAsync() - ProductId: {assignment.ProductId}, StationId: {assignment.StationId}, Stock: {assignment.Stock}");
                
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
                        Console.WriteLine($"✅ [ProductStockAssignmentService] CreateAsync() - Asignando CompanyId: {assignment.CompanyId}, BranchId: {assignment.BranchId}");
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
                    Console.WriteLine($"⚠️ [ProductStockAssignmentService] CreateAsync() - Ya existe una asignación para este producto y estación");
                    throw new InvalidOperationException("Ya existe una asignación de stock para este producto en esta estación");
                }

                SetCreatedTracking(assignment);
                
                _context.ProductStockAssignments.Add(assignment);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ [ProductStockAssignmentService] CreateAsync() - Asignación creada exitosamente: {assignment.Id}");
                return assignment;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductStockAssignmentService] CreateAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [ProductStockAssignmentService] CreateAsync() - StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<ProductStockAssignment> UpdateAsync(Guid id, ProductStockAssignment assignment)
        {
            try
            {
                Console.WriteLine($"🔍 [ProductStockAssignmentService] UpdateAsync() - Id: {id}");
                
                var existing = await _context.ProductStockAssignments.FindAsync(id);
                if (existing == null)
                {
                    Console.WriteLine($"⚠️ [ProductStockAssignmentService] UpdateAsync() - Asignación no encontrada");
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

                Console.WriteLine($"✅ [ProductStockAssignmentService] UpdateAsync() - Asignación actualizada exitosamente");
                return existing;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductStockAssignmentService] UpdateAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [ProductStockAssignmentService] UpdateAsync() - StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                Console.WriteLine($"🔍 [ProductStockAssignmentService] DeleteAsync() - Id: {id}");
                
                var assignment = await _context.ProductStockAssignments.FindAsync(id);
                if (assignment == null)
                {
                    Console.WriteLine($"⚠️ [ProductStockAssignmentService] DeleteAsync() - Asignación no encontrada");
                    return false;
                }

                _context.ProductStockAssignments.Remove(assignment);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ [ProductStockAssignmentService] DeleteAsync() - Asignación eliminada exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductStockAssignmentService] DeleteAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [ProductStockAssignmentService] DeleteAsync() - StackTrace: {ex.StackTrace}");
                throw;
            }
        }
    }
}

