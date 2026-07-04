using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Claims;

namespace RestBar.Services
{
    public class ProductService : BaseTrackingService, IProductService
    {
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            RestBarContext context,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ProductService> logger)
            : base(context, httpContextAccessor)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.Include(p => p.Category).OrderBy(p => p.Name).ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> CreateAsync(Product product)
        {
            try
            {
                _logger.LogDebug("[ProductService] CreateAsync - iniciando creación de producto: {ProductName}", product?.Name);

                if (product == null)
                    throw new ArgumentNullException(nameof(product), "El producto no puede ser null.");

                // Obtener usuario actual para CompanyId y BranchId
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
                        _logger.LogDebug("[ProductService] CreateAsync - asignando CompanyId: {CompanyId}, BranchId: {BranchId}",
                            product.CompanyId, product.BranchId);
                    }
                }

                if (product.Id == Guid.Empty)
                    product.Id = Guid.NewGuid();

                SetCreatedTracking(product);

                // Si el controlador ya estableció CreatedBy, mantenerlo
                var existingCreatedBy = product.CreatedBy;
                if (!string.IsNullOrWhiteSpace(existingCreatedBy))
                {
                    product.CreatedBy = existingCreatedBy;
                    product.UpdatedBy = existingCreatedBy;
                }

                _logger.LogDebug("[ProductService] CreateAsync - campos de auditoría: CreatedBy={CreatedBy}, CreatedAt={CreatedAt}",
                    product.CreatedBy, product.CreatedAt);

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation("[ProductService] CreateAsync - producto creado: {ProductName} (ID: {ProductId})",
                    product.Name, product.Id);
                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProductService] CreateAsync - error creando producto: {ProductName}", product?.Name);
                throw;
            }
        }

        public async Task<Product> UpdateAsync(Guid id, Product product)
        {
            try
            {
                _logger.LogDebug("[ProductService] UpdateAsync - actualizando producto: {ProductName} (ID: {Id})", product.Name, id);

                var existing = await _context.Products.FindAsync(id);
                if (existing == null)
                    throw new KeyNotFoundException($"Producto con ID {id} no encontrado");

                existing.Name = product.Name;
                existing.Description = product.Description;
                existing.Price = product.Price;
                existing.Cost = product.Cost;
                existing.TaxRate = product.TaxRate;
                existing.Unit = product.Unit;
                existing.ImageUrl = product.ImageUrl;
                existing.IsActive = product.IsActive;
                existing.CategoryId = product.CategoryId;
                // StationId eliminado — se usa ProductStockAssignment

                existing.Stock = product.Stock;
                existing.MinStock = product.MinStock;
                existing.TrackInventory = product.TrackInventory;
                existing.AllowNegativeStock = product.AllowNegativeStock;

                SetUpdatedTracking(existing);

                _logger.LogDebug("[ProductService] UpdateAsync - auditoría: UpdatedBy={UpdatedBy}, UpdatedAt={UpdatedAt}",
                    existing.UpdatedBy, existing.UpdatedAt);

                await _context.SaveChangesAsync();

                _logger.LogInformation("[ProductService] UpdateAsync - producto actualizado: {ProductName} (ID: {Id})", existing.Name, id);
                return existing;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProductService] UpdateAsync - error actualizando producto ID: {Id}", id);
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
            var userIdValue = _httpContextAccessor?.HttpContext?.User?.FindFirst("UserId")?.Value
                ?? _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var query = _context.Products
                .Where(p => p.CategoryId == categoryId && p.IsActive);

            if (!string.IsNullOrEmpty(userIdValue) && Guid.TryParse(userIdValue, out var userId))
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .Include(u => u.Branch)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user?.BranchId != null)
                {
                    query = query.Where(p =>
                        p.BranchId == user.BranchId &&
                        (p.CompanyId == null || p.CompanyId == user.Branch!.CompanyId));
                }
            }

            return await query
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
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
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            return await query
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    price = p.Price,
                    imageUrl = p.ImageUrl,
                    categoryId = p.CategoryId,
                    categoryName = p.Category.Name,
                    // StationId/stationName eliminados — se usa ProductStockAssignment
                    taxRate = p.TaxRate
                })
                .ToListAsync();
        }

        // ─── INVENTARIO ────────────────────────────────────────────────────────────

        /// <summary>
        /// Obtiene el stock disponible total de un producto (global + por estaciones).
        /// Retorna -1 cuando el producto no controla inventario (stock ilimitado).
        /// </summary>
        public async Task<decimal> GetAvailableStockAsync(Guid productId, Guid? branchId = null)
        {
            try
            {
                _logger.LogDebug("[ProductService] GetAvailableStockAsync - ProductId: {ProductId}, BranchId: {BranchId}",
                    productId, branchId);

                var product = await _context.Products
                    .Include(p => p.StockAssignments.Where(sa => sa.IsActive))
                    .FirstOrDefaultAsync(p => p.Id == productId);

                if (product == null)
                {
                    _logger.LogWarning("[ProductService] GetAvailableStockAsync - producto no encontrado: {ProductId}", productId);
                    return 0;
                }

                if (!product.TrackInventory)
                {
                    _logger.LogDebug("[ProductService] GetAvailableStockAsync - {ProductName} no controla inventario → stock ilimitado", product.Name);
                    return -1;
                }

                var stockAssignments = product.StockAssignments.Where(sa => sa.IsActive);
                if (branchId.HasValue)
                    stockAssignments = stockAssignments.Where(sa => sa.BranchId == branchId.Value);

                var assignmentsList = stockAssignments.ToList();
                decimal totalStock;

                if (assignmentsList.Any())
                {
                    totalStock = assignmentsList.Sum(sa => sa.Stock);
                    _logger.LogDebug("[ProductService] GetAvailableStockAsync - {ProductName}: stock de {Count} asignaciones = {Total}",
                        product.Name, assignmentsList.Count, totalStock);
                }
                else
                {
                    totalStock = product.Stock ?? 0;
                    _logger.LogDebug("[ProductService] GetAvailableStockAsync - {ProductName}: sin asignaciones → stock global = {Total}",
                        product.Name, totalStock);
                }

                return totalStock;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProductService] GetAvailableStockAsync - error para ProductId: {ProductId}", productId);
                return 0;
            }
        }

        /// <summary>
        /// Obtiene el stock disponible de un producto en una estación específica.
        /// </summary>
        public async Task<decimal> GetStockInStationAsync(Guid productId, Guid stationId, Guid? branchId = null)
        {
            try
            {
                _logger.LogDebug("[ProductService] GetStockInStationAsync - ProductId: {ProductId}, StationId: {StationId}, BranchId: {BranchId}",
                    productId, stationId, branchId);

                var assignment = await _context.ProductStockAssignments
                    .Where(sa => sa.ProductId == productId &&
                                 sa.StationId == stationId &&
                                 sa.IsActive &&
                                 (!branchId.HasValue || sa.BranchId == branchId.Value))
                    .FirstOrDefaultAsync();

                if (assignment == null)
                {
                    _logger.LogWarning("[ProductService] GetStockInStationAsync - sin asignación para ProductId: {ProductId} en StationId: {StationId}. Retornando stock global.",
                        productId, stationId);
                    var product = await GetByIdAsync(productId);
                    return product?.Stock ?? 0;
                }

                _logger.LogDebug("[ProductService] GetStockInStationAsync - stock en estación: {Stock}", assignment.Stock);
                return assignment.Stock;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProductService] GetStockInStationAsync - error para ProductId: {ProductId}, StationId: {StationId}",
                    productId, stationId);
                return 0;
            }
        }

        /// <summary>
        /// Encuentra la mejor estación para asignar un producto basándose en stock disponible y prioridad.
        /// Retorna null si el producto no tiene ProductStockAssignment configurado — el operador debe
        /// configurar la asignación de estaciones para que el KDS pueda rutear el ítem correctamente.
        /// </summary>
        public async Task<Guid?> FindBestStationForProductAsync(Guid productId, decimal requiredQuantity, Guid? branchId = null, Guid? areaId = null)
        {
            try
            {
                _logger.LogDebug("[ProductService] FindBestStationForProductAsync - ProductId: {ProductId}, Qty: {Qty}, BranchId: {BranchId}, AreaId: {AreaId}",
                    productId, requiredQuantity, branchId, areaId);

                var product = await _context.Products
                    .Include(p => p.StockAssignments.Where(sa => sa.IsActive))
                        .ThenInclude(sa => sa.Station)
                    .FirstOrDefaultAsync(p => p.Id == productId);

                if (product == null)
                {
                    _logger.LogWarning("[ProductService] FindBestStationForProductAsync - producto no encontrado: {ProductId}", productId);
                    return null;
                }

                var branchAssignments = product.StockAssignments
                    .Where(sa => sa.IsActive && (!branchId.HasValue || sa.BranchId == branchId.Value))
                    .ToList();

                if (!branchAssignments.Any())
                {
                    _logger.LogWarning(
                        "[KDS] FindBestStationForProductAsync - PRODUCTO SIN ESTACIÓN: '{ProductName}' (ID: {ProductId})",
                        product.Name, productId);
                    return null;
                }

                // Preferir estaciones del mismo área/piso que la mesa
                if (areaId.HasValue)
                {
                    var areaScoped = branchAssignments
                        .Where(sa => sa.Station?.AreaId == areaId.Value)
                        .ToList();
                    if (areaScoped.Any())
                    {
                        branchAssignments = areaScoped;
                        _logger.LogInformation("[ProductService] FindBestStationForProductAsync - '{ProductName}' filtrado por área {AreaId}: {Count} asignaciones",
                            product.Name, areaId, areaScoped.Count);
                    }
                    else
                    {
                        _logger.LogWarning("[ProductService] FindBestStationForProductAsync - '{ProductName}' sin estación en área {AreaId}, usando sucursal completa",
                            product.Name, areaId);
                    }
                }

                Guid? PickBest(IEnumerable<ProductStockAssignment> assignments)
                {
                    if (!product.TrackInventory)
                    {
                        var best = assignments.OrderByDescending(sa => sa.Priority).FirstOrDefault();
                        return best?.StationId;
                    }

                    var withStock = assignments
                        .Where(sa => sa.Stock >= requiredQuantity)
                        .OrderByDescending(sa => sa.Priority)
                        .ThenByDescending(sa => sa.Stock)
                        .ToList();

                    if (withStock.Any())
                        return withStock.First().StationId;

                    if (product.Stock.HasValue && product.Stock.Value >= requiredQuantity)
                        return assignments.OrderByDescending(sa => sa.Priority).FirstOrDefault()?.StationId;

                    if (product.AllowNegativeStock)
                        return assignments.OrderByDescending(sa => sa.Priority).FirstOrDefault()?.StationId;

                    return null;
                }

                var selected = PickBest(branchAssignments);
                if (selected.HasValue)
                {
                    _logger.LogInformation("[ProductService] FindBestStationForProductAsync - '{ProductName}' → estación {StationId}",
                        product.Name, selected);
                }
                return selected;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProductService] FindBestStationForProductAsync - error para ProductId: {ProductId}", productId);
                return null;
            }
        }

        /// <summary>
        /// Reduce el stock de un producto (global o por estación).
        /// </summary>
        public async Task<bool> ReduceStockAsync(Guid productId, decimal quantity, Guid? stationId = null, Guid? branchId = null)
        {
            try
            {
                _logger.LogDebug("[ProductService] ReduceStockAsync - ProductId: {ProductId}, Qty: {Qty}, StationId: {StationId}, BranchId: {BranchId}",
                    productId, quantity, stationId, branchId);

                var product = await _context.Products
                    .Include(p => p.StockAssignments.Where(sa => sa.IsActive))
                    .FirstOrDefaultAsync(p => p.Id == productId);

                if (product == null)
                {
                    _logger.LogWarning("[ProductService] ReduceStockAsync - producto no encontrado: {ProductId}", productId);
                    return false;
                }

                if (!product.TrackInventory)
                {
                    _logger.LogDebug("[ProductService] ReduceStockAsync - {ProductName} no controla inventario → skip", product.Name);
                    return true;
                }

                if (stationId.HasValue)
                {
                    var assignment = product.StockAssignments
                        .FirstOrDefault(sa => sa.StationId == stationId.Value &&
                                              (!branchId.HasValue || sa.BranchId == branchId.Value));

                    if (assignment != null)
                    {
                        if (assignment.Stock < quantity && !product.AllowNegativeStock)
                        {
                            _logger.LogWarning("[ProductService] ReduceStockAsync - stock insuficiente en estación {StationId}: disponible={Stock}, requerido={Qty}",
                                stationId, assignment.Stock, quantity);
                            throw new InvalidOperationException($"Stock insuficiente en estación. Disponible: {assignment.Stock}, Requerido: {quantity}");
                        }

                        var prevStock = assignment.Stock;
                        assignment.Stock -= quantity;
                        SetUpdatedTracking(assignment);
                        _logger.LogInformation("[ProductService] ReduceStockAsync - stock en estación {StationId}: {Prev} → {New}",
                            stationId, prevStock, assignment.Stock);
                    }
                    else
                    {
                        // Sin asignación específica → reducir stock global
                        if (product.Stock.HasValue)
                        {
                            if (product.Stock.Value < quantity && !product.AllowNegativeStock)
                            {
                                _logger.LogWarning("[ProductService] ReduceStockAsync - stock global insuficiente para {ProductName}: disponible={Stock}, requerido={Qty}",
                                    product.Name, product.Stock.Value, quantity);
                                throw new InvalidOperationException($"Stock insuficiente. Disponible: {product.Stock.Value}, Requerido: {quantity}");
                            }

                            var prevStock = product.Stock ?? 0;
                            product.Stock = prevStock - quantity;
                            SetUpdatedTracking(product);
                            _logger.LogInformation("[ProductService] ReduceStockAsync - stock global {ProductName}: {Prev} → {New}",
                                product.Name, prevStock, product.Stock);
                        }
                    }
                }
                else
                {
                    if (product.Stock.HasValue)
                    {
                        if (product.Stock.Value < quantity && !product.AllowNegativeStock)
                        {
                            _logger.LogWarning("[ProductService] ReduceStockAsync - stock global insuficiente para {ProductName}: disponible={Stock}, requerido={Qty}",
                                product.Name, product.Stock.Value, quantity);
                            throw new InvalidOperationException($"Stock insuficiente. Disponible: {product.Stock.Value}, Requerido: {quantity}");
                        }

                        var prevStock = product.Stock ?? 0;
                        product.Stock = prevStock - quantity;
                        SetUpdatedTracking(product);
                        _logger.LogInformation("[ProductService] ReduceStockAsync - stock global {ProductName}: {Prev} → {New}",
                            product.Name, prevStock, product.Stock);
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("[ProductService] ReduceStockAsync - stock reducido exitosamente para {ProductName}", product.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProductService] ReduceStockAsync - error para ProductId: {ProductId}", productId);
                throw;
            }
        }

        /// <summary>
        /// Restaura stock de un producto (al cancelar una orden).
        /// </summary>
        public async Task<bool> RestoreStockAsync(Guid productId, decimal quantity, Guid? stationId = null, Guid? branchId = null)
        {
            try
            {
                _logger.LogDebug("[ProductService] RestoreStockAsync - ProductId: {ProductId}, Qty: {Qty}, StationId: {StationId}, BranchId: {BranchId}",
                    productId, quantity, stationId, branchId);

                var product = await _context.Products
                    .Include(p => p.StockAssignments.Where(sa => sa.IsActive))
                    .FirstOrDefaultAsync(p => p.Id == productId);

                if (product == null || !product.TrackInventory)
                {
                    _logger.LogDebug("[ProductService] RestoreStockAsync - producto no encontrado o sin control de inventario → skip");
                    return true;
                }

                if (stationId.HasValue)
                {
                    var assignment = product.StockAssignments
                        .FirstOrDefault(sa => sa.StationId == stationId.Value &&
                                              (!branchId.HasValue || sa.BranchId == branchId.Value));

                    if (assignment != null)
                    {
                        var prevStock = assignment.Stock;
                        assignment.Stock += quantity;
                        SetUpdatedTracking(assignment);
                        _logger.LogInformation("[ProductService] RestoreStockAsync - stock en estación {StationId}: {Prev} → {New}",
                            stationId, prevStock, assignment.Stock);
                    }
                    else
                    {
                        var prevStock = product.Stock ?? 0;
                        product.Stock = prevStock + quantity;
                        SetUpdatedTracking(product);
                        _logger.LogInformation("[ProductService] RestoreStockAsync - stock global {ProductName}: {Prev} → {New}",
                            product.Name, prevStock, product.Stock);
                    }
                }
                else
                {
                    var prevStock = product.Stock ?? 0;
                    product.Stock = prevStock + quantity;
                    SetUpdatedTracking(product);
                    _logger.LogInformation("[ProductService] RestoreStockAsync - stock global {ProductName}: {Prev} → {New}",
                        product.Name, prevStock, product.Stock);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("[ProductService] RestoreStockAsync - stock restaurado exitosamente para {ProductName}", product.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProductService] RestoreStockAsync - error para ProductId: {ProductId}", productId);
                throw;
            }
        }

        /// <summary>
        /// Verifica si un producto tiene stock suficiente.
        /// </summary>
        public async Task<bool> HasStockAvailableAsync(Guid productId, decimal quantity, Guid? branchId = null)
        {
            try
            {
                var availableStock = await GetAvailableStockAsync(productId, branchId);
                // -1 representa stock ilimitado
                return availableStock == -1 || availableStock >= quantity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProductService] HasStockAvailableAsync - error para ProductId: {ProductId}", productId);
                return false;
            }
        }
    }
}
