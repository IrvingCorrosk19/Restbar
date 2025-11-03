using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestBar.Services
{
    public class ProductService : BaseTrackingService, IProductService
    {
        public ProductService(RestBarContext context, IHttpContextAccessor httpContextAccessor) 
            : base(context, httpContextAccessor)
        {
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.Include(p => p.Category).Include(p => p.Station).OrderBy(p => p.Name).ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _context.Products.Include(p => p.Category).Include(p => p.Station).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> CreateAsync(Product product)
        {
            try
            {
                Console.WriteLine($"🔍 [ProductService] CreateAsync() - Iniciando creación de producto: {product.Name}");
                
                if (product == null)
                    throw new ArgumentNullException(nameof(product), "El producto no puede ser null.");

                // ✅ Obtener usuario actual para CompanyId y BranchId
                var userIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    var user = await _context.Users
                        .Include(u => u.Branch)
                        .FirstOrDefaultAsync(u => u.Id == userId);
                    
                    if (user != null && user.Branch != null)
                    {
                        product.CompanyId = user.Branch.CompanyId;
                        product.BranchId = user.BranchId;
                        Console.WriteLine($"✅ [ProductService] CreateAsync() - Asignando CompanyId: {product.CompanyId}, BranchId: {product.BranchId}");
                    }
                }

                // ✅ Generar ID si no existe
                if (product.Id == Guid.Empty)
                {
                    product.Id = Guid.NewGuid();
                }
                
                // ✅ Usar SetCreatedTracking para establecer todos los campos de auditoría
                SetCreatedTracking(product);
                
                // Si el controlador ya estableció CreatedBy, mantenerlo
                var existingCreatedBy = product.CreatedBy;
                if (!string.IsNullOrWhiteSpace(existingCreatedBy))
                {
                    product.CreatedBy = existingCreatedBy;
                    product.UpdatedBy = existingCreatedBy;
                }
                
                Console.WriteLine($"✅ [ProductService] CreateAsync() - Campos establecidos: CreatedBy={product.CreatedBy}, CreatedAt={product.CreatedAt}, UpdatedAt={product.UpdatedAt}");

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ [ProductService] CreateAsync() - Producto creado exitosamente: {product.Name} (ID: {product.Id})");
                return product;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductService] CreateAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [ProductService] CreateAsync() - StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<Product> UpdateAsync(Guid id, Product product)
        {
            try
            {
                Console.WriteLine($"🔍 [ProductService] UpdateAsync() - Actualizando producto: {product.Name} (ID: {id})");
                
                var existing = await _context.Products.FindAsync(id);
                if (existing == null)
                    throw new KeyNotFoundException($"Producto con ID {id} no encontrado");
                
                // Actualizar campos
                existing.Name = product.Name;
                existing.Description = product.Description;
                existing.Price = product.Price;
                existing.Cost = product.Cost;
                existing.TaxRate = product.TaxRate;
                existing.Unit = product.Unit;
                existing.ImageUrl = product.ImageUrl;
                existing.IsActive = product.IsActive;
                existing.CategoryId = product.CategoryId;
                existing.StationId = product.StationId;
                
                // ✅ NUEVO: Actualizar campos de inventario
                existing.Stock = product.Stock;
                existing.MinStock = product.MinStock;
                existing.TrackInventory = product.TrackInventory;
                existing.AllowNegativeStock = product.AllowNegativeStock;
                
                // ✅ Usar SetUpdatedTracking para establecer campos de auditoría de actualización
                SetUpdatedTracking(existing);
                
                Console.WriteLine($"✅ [ProductService] UpdateAsync() - Campos actualizados: UpdatedBy={existing.UpdatedBy}, UpdatedAt={existing.UpdatedAt}");

                await _context.SaveChangesAsync();
                
                Console.WriteLine($"✅ [ProductService] UpdateAsync() - Producto actualizado exitosamente");
                return existing;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductService] UpdateAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [ProductService] UpdateAsync() - StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return false;
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Product>> GetByCategoryIdAsync(Guid categoryId)
        {
            return await _context.Products
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive == true)
                .Include(p => p.Category)
                .ToListAsync();
        }



        public async Task<Product?> GetProductWithModifiersAsync(Guid id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Modifiers)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _context.Products
                .Where(p => p.Price >= minPrice && p.Price <= maxPrice && p.IsActive == true)
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            return await _context.Products
                .Where(p => (p.Name.Contains(searchTerm) || 
                           (p.Description != null && p.Description.Contains(searchTerm))) && 
                           p.IsActive == true)
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetActiveProductsForViewBagAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetProductsWithDetailsAsync(Guid? categoryId = null)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Station)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            return await query
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    price = p.Price,
                    imageUrl = p.ImageUrl,
                    categoryId = p.CategoryId,
                    categoryName = p.Category.Name,
                    stationId = p.StationId,
                    stationName = p.Station.Name,

                    taxRate = p.TaxRate  // ✅ AGREGADO: Campo taxRate
                })
                .ToListAsync();
        }

        // ✅ NUEVO: MÉTODOS DE INVENTARIO

        /// <summary>
        /// Obtiene el stock disponible total de un producto (stock global + stock por estaciones)
        /// </summary>
        public async Task<decimal> GetAvailableStockAsync(Guid productId, Guid? branchId = null)
        {
            try
            {
                Console.WriteLine($"🔍 [ProductService] GetAvailableStockAsync() - ProductId: {productId}, BranchId: {branchId}");
                
                var product = await _context.Products
                    .Include(p => p.StockAssignments.Where(sa => sa.IsActive))
                    .FirstOrDefaultAsync(p => p.Id == productId);
                
                if (product == null)
                {
                    Console.WriteLine($"⚠️ [ProductService] GetAvailableStockAsync() - Producto no encontrado");
                    return 0;
                }

                // Si no se controla inventario, retornar stock ilimitado (representado por -1)
                if (!product.TrackInventory)
                {
                    Console.WriteLine($"✅ [ProductService] GetAvailableStockAsync() - Producto no controla inventario, stock ilimitado");
                    return -1; // -1 representa stock ilimitado
                }

                // Filtrar asignaciones por sucursal si se especifica
                var stockAssignments = product.StockAssignments.Where(sa => sa.IsActive);
                if (branchId.HasValue)
                {
                    stockAssignments = stockAssignments.Where(sa => sa.BranchId == branchId.Value);
                }
                var assignmentsList = stockAssignments.ToList();

                decimal totalStock = 0;

                // ✅ LÓGICA CORREGIDA: Si hay asignaciones por estación, usar la suma de esas asignaciones
                // Si NO hay asignaciones, usar el stock global
                if (assignmentsList.Any())
                {
                    // Hay asignaciones por estación: sumar todas las asignaciones
                    totalStock = assignmentsList.Sum(sa => sa.Stock);
                    Console.WriteLine($"✅ [ProductService] GetAvailableStockAsync() - Stock total de asignaciones: {totalStock} (de {assignmentsList.Count} asignaciones)");
                }
                else
                {
                    // No hay asignaciones: usar stock global
                    totalStock = product.Stock ?? 0;
                    Console.WriteLine($"✅ [ProductService] GetAvailableStockAsync() - Stock global disponible: {totalStock}");
                }

                Console.WriteLine($"✅ [ProductService] GetAvailableStockAsync() - Stock total disponible: {totalStock}");
                return totalStock;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductService] GetAvailableStockAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [ProductService] GetAvailableStockAsync() - StackTrace: {ex.StackTrace}");
                return 0;
            }
        }

        /// <summary>
        /// Obtiene el stock disponible de un producto en una estación específica
        /// </summary>
        public async Task<decimal> GetStockInStationAsync(Guid productId, Guid stationId, Guid? branchId = null)
        {
            try
            {
                Console.WriteLine($"🔍 [ProductService] GetStockInStationAsync() - ProductId: {productId}, StationId: {stationId}, BranchId: {branchId}");
                
                var assignment = await _context.ProductStockAssignments
                    .Where(sa => sa.ProductId == productId && 
                                 sa.StationId == stationId && 
                                 sa.IsActive &&
                                 (!branchId.HasValue || sa.BranchId == branchId.Value))
                    .FirstOrDefaultAsync();
                
                if (assignment == null)
                {
                    Console.WriteLine($"⚠️ [ProductService] GetStockInStationAsync() - No hay asignación para esta estación");
                    // Si no hay asignación específica, retornar stock global del producto
                    var product = await GetByIdAsync(productId);
                    return product?.Stock ?? 0;
                }

                Console.WriteLine($"✅ [ProductService] GetStockInStationAsync() - Stock en estación: {assignment.Stock}");
                return assignment.Stock;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductService] GetStockInStationAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [ProductService] GetStockInStationAsync() - StackTrace: {ex.StackTrace}");
                return 0;
            }
        }

        /// <summary>
        /// Encuentra la mejor estación para asignar un producto basándose en stock disponible y prioridad
        /// </summary>
        public async Task<Guid?> FindBestStationForProductAsync(Guid productId, decimal requiredQuantity, Guid? branchId = null)
        {
            try
            {
                Console.WriteLine($"🔍 [ProductService] FindBestStationForProductAsync() - ProductId: {productId}, RequiredQuantity: {requiredQuantity}, BranchId: {branchId}");
                
                var product = await _context.Products
                    .Include(p => p.StockAssignments.Where(sa => sa.IsActive))
                    .Include(p => p.Station)
                    .FirstOrDefaultAsync(p => p.Id == productId);
                
                if (product == null)
                {
                    Console.WriteLine($"⚠️ [ProductService] FindBestStationForProductAsync() - Producto no encontrado");
                    return null;
                }

                // Si el producto no controla inventario, retornar estación predeterminada
                if (!product.TrackInventory)
                {
                    Console.WriteLine($"✅ [ProductService] FindBestStationForProductAsync() - Producto no controla inventario, usando estación predeterminada: {product.StationId}");
                    return product.StationId;
                }

                // Obtener asignaciones de stock por estación con stock suficiente
                var assignments = product.StockAssignments
                    .Where(sa => sa.IsActive && 
                                 (!branchId.HasValue || sa.BranchId == branchId.Value) &&
                                 sa.Stock >= requiredQuantity)
                    .OrderByDescending(sa => sa.Priority) // Mayor prioridad primero
                    .ThenByDescending(sa => sa.Stock) // Mayor stock después
                    .ToList();

                if (assignments.Any())
                {
                    var bestAssignment = assignments.First();
                    Console.WriteLine($"✅ [ProductService] FindBestStationForProductAsync() - Estación seleccionada: {bestAssignment.StationId} (Prioridad: {bestAssignment.Priority}, Stock: {bestAssignment.Stock})");
                    return bestAssignment.StationId;
                }

                // Si no hay asignaciones por estación pero hay stock global suficiente
                if (product.Stock.HasValue && product.Stock.Value >= requiredQuantity)
                {
                    Console.WriteLine($"✅ [ProductService] FindBestStationForProductAsync() - Usando estación predeterminada con stock global: {product.StationId}");
                    return product.StationId;
                }

                // Si se permite stock negativo o hay stock global insuficiente pero se permite
                if (product.AllowNegativeStock)
                {
                    Console.WriteLine($"⚠️ [ProductService] FindBestStationForProductAsync() - Stock insuficiente pero permitido negativo, usando estación predeterminada: {product.StationId}");
                    return product.StationId;
                }

                Console.WriteLine($"⚠️ [ProductService] FindBestStationForProductAsync() - No hay stock suficiente en ninguna estación");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductService] FindBestStationForProductAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [ProductService] FindBestStationForProductAsync() - StackTrace: {ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Reduce el stock de un producto (global o por estación)
        /// </summary>
        public async Task<bool> ReduceStockAsync(Guid productId, decimal quantity, Guid? stationId = null, Guid? branchId = null)
        {
            try
            {
                Console.WriteLine($"🔍 [ProductService] ReduceStockAsync() - ProductId: {productId}, Quantity: {quantity}, StationId: {stationId}, BranchId: {branchId}");
                
                var product = await _context.Products
                    .Include(p => p.StockAssignments.Where(sa => sa.IsActive))
                    .FirstOrDefaultAsync(p => p.Id == productId);
                
                if (product == null)
                {
                    Console.WriteLine($"⚠️ [ProductService] ReduceStockAsync() - Producto no encontrado");
                    return false;
                }

                // Si no se controla inventario, no reducir stock
                if (!product.TrackInventory)
                {
                    Console.WriteLine($"✅ [ProductService] ReduceStockAsync() - Producto no controla inventario, skip");
                    return true;
                }

                // Si hay estación específica, reducir stock de esa estación
                if (stationId.HasValue)
                {
                    var assignment = product.StockAssignments
                        .FirstOrDefault(sa => sa.StationId == stationId.Value && 
                                              (!branchId.HasValue || sa.BranchId == branchId.Value));
                    
                    if (assignment != null)
                    {
                        if (assignment.Stock < quantity && !product.AllowNegativeStock)
                        {
                            Console.WriteLine($"❌ [ProductService] ReduceStockAsync() - Stock insuficiente en estación: {assignment.Stock} < {quantity}");
                            throw new InvalidOperationException($"Stock insuficiente en estación. Disponible: {assignment.Stock}, Requerido: {quantity}");
                        }
                        
                        assignment.Stock -= quantity;
                        SetUpdatedTracking(assignment);
                        Console.WriteLine($"✅ [ProductService] ReduceStockAsync() - Stock reducido en estación: {assignment.Stock + quantity} -> {assignment.Stock}");
                    }
                    else
                    {
                        // No hay asignación específica, reducir stock global
                        if (product.Stock.HasValue)
                        {
                            if (product.Stock.Value < quantity && !product.AllowNegativeStock)
                            {
                                Console.WriteLine($"❌ [ProductService] ReduceStockAsync() - Stock global insuficiente: {product.Stock.Value} < {quantity}");
                                throw new InvalidOperationException($"Stock insuficiente. Disponible: {product.Stock.Value}, Requerido: {quantity}");
                            }
                            
                            product.Stock = (product.Stock ?? 0) - quantity;
                            SetUpdatedTracking(product);
                            Console.WriteLine($"✅ [ProductService] ReduceStockAsync() - Stock global reducido: {product.Stock + quantity} -> {product.Stock}");
                        }
                    }
                }
                else
                {
                    // Reducir stock global
                    if (product.Stock.HasValue)
                    {
                        if (product.Stock.Value < quantity && !product.AllowNegativeStock)
                        {
                            Console.WriteLine($"❌ [ProductService] ReduceStockAsync() - Stock global insuficiente: {product.Stock.Value} < {quantity}");
                            throw new InvalidOperationException($"Stock insuficiente. Disponible: {product.Stock.Value}, Requerido: {quantity}");
                        }
                        
                        product.Stock = (product.Stock ?? 0) - quantity;
                        SetUpdatedTracking(product);
                        Console.WriteLine($"✅ [ProductService] ReduceStockAsync() - Stock global reducido: {product.Stock + quantity} -> {product.Stock}");
                    }
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ [ProductService] ReduceStockAsync() - Stock reducido exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductService] ReduceStockAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [ProductService] ReduceStockAsync() - StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Restaura stock de un producto (al cancelar una orden)
        /// </summary>
        public async Task<bool> RestoreStockAsync(Guid productId, decimal quantity, Guid? stationId = null, Guid? branchId = null)
        {
            try
            {
                Console.WriteLine($"🔍 [ProductService] RestoreStockAsync() - ProductId: {productId}, Quantity: {quantity}, StationId: {stationId}, BranchId: {branchId}");
                
                var product = await _context.Products
                    .Include(p => p.StockAssignments.Where(sa => sa.IsActive))
                    .FirstOrDefaultAsync(p => p.Id == productId);
                
                if (product == null || !product.TrackInventory)
                {
                    Console.WriteLine($"✅ [ProductService] RestoreStockAsync() - Producto no encontrado o no controla inventario, skip");
                    return true;
                }

                // Si hay estación específica, restaurar stock de esa estación
                if (stationId.HasValue)
                {
                    var assignment = product.StockAssignments
                        .FirstOrDefault(sa => sa.StationId == stationId.Value && 
                                              (!branchId.HasValue || sa.BranchId == branchId.Value));
                    
                    if (assignment != null)
                    {
                        assignment.Stock += quantity;
                        SetUpdatedTracking(assignment);
                        Console.WriteLine($"✅ [ProductService] RestoreStockAsync() - Stock restaurado en estación: {assignment.Stock - quantity} -> {assignment.Stock}");
                    }
                    else
                    {
                        // No hay asignación específica, restaurar stock global
                        product.Stock = (product.Stock ?? 0) + quantity;
                        SetUpdatedTracking(product);
                        Console.WriteLine($"✅ [ProductService] RestoreStockAsync() - Stock global restaurado: {product.Stock - quantity} -> {product.Stock}");
                    }
                }
                else
                {
                    // Restaurar stock global
                    product.Stock = (product.Stock ?? 0) + quantity;
                    SetUpdatedTracking(product);
                    Console.WriteLine($"✅ [ProductService] RestoreStockAsync() - Stock global restaurado: {product.Stock - quantity} -> {product.Stock}");
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ [ProductService] RestoreStockAsync() - Stock restaurado exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductService] RestoreStockAsync() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [ProductService] RestoreStockAsync() - StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Verifica si un producto tiene stock suficiente
        /// </summary>
        public async Task<bool> HasStockAvailableAsync(Guid productId, decimal quantity, Guid? branchId = null)
        {
            try
            {
                var availableStock = await GetAvailableStockAsync(productId, branchId);
                
                // -1 significa stock ilimitado
                if (availableStock == -1)
                    return true;
                
                return availableStock >= quantity;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProductService] HasStockAvailableAsync() - Error: {ex.Message}");
                return false;
            }
        }
    }
} 