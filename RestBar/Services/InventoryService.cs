using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly RestBarContext _context;

        public InventoryService(RestBarContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Inventory>> GetAllAsync()
        {
            try
            {
                Console.WriteLine($"[InventoryService] GetAllAsync iniciado");
                
                var inventories = await _context.Inventories
                    .Include(i => i.Branch)
                    .Include(i => i.Product)
                    .Include(i => i.Product.Category)
                    .ToListAsync();
                
                Console.WriteLine($"[InventoryService] Inventarios obtenidos: {inventories.Count} items");
                
                foreach (var inv in inventories)
                {
                    Console.WriteLine($"[InventoryService] Item: ProductId={inv.ProductId}, ProductName={inv.Product?.Name}, BranchId={inv.BranchId}, BranchName={inv.Branch?.Name}, Quantity={inv.Quantity}");
                }
                
                return inventories;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryService] ERROR en GetAllAsync: {ex.Message}");
                Console.WriteLine($"[InventoryService] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<Inventory?> GetByIdAsync(Guid id)
        {
            return await _context.Inventories
                .Include(i => i.Branch)
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Inventory> CreateAsync(Inventory inventory)
        {
            inventory.LastUpdated = DateTime.UtcNow;
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
            return inventory;
        }

        public async Task UpdateAsync(Inventory inventory)
        {
            try
            {
                // Buscar si hay una entidad con el mismo ID siendo rastreada
                var existingEntity = _context.ChangeTracker.Entries<Inventory>()
                    .FirstOrDefault(e => e.Entity.Id == inventory.Id);

                if (existingEntity != null)
                {
                                    // Detach la entidad existente para evitar conflictos
                existingEntity.State = EntityState.Detached;
            }

            inventory.LastUpdated = DateTime.UtcNow;
            
            // Usar Update para manejar automáticamente el tracking
            _context.Inventories.Update(inventory);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al actualizar el inventario en la base de datos.", ex);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory != null)
            {
                _context.Inventories.Remove(inventory);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Inventory>> GetByBranchIdAsync(Guid branchId)
        {
            return await _context.Inventories
                .Where(i => i.BranchId == branchId)
                .Include(i => i.Product)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inventory>> GetByProductIdAsync(Guid productId)
        {
            return await _context.Inventories
                .Where(i => i.ProductId == productId)
                .Include(i => i.Branch)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inventory>> GetLowStockItemsAsync()
        {
            return await _context.Inventories
                .Where(i => (i.Stock ?? 0) <= (i.MinStock ?? 0))
                .Include(i => i.Branch)
                .Include(i => i.Product)
                .ToListAsync();
        }

        public async Task<Inventory?> GetByBranchAndProductAsync(Guid branchId, Guid productId)
        {
            return await _context.Inventories
                .Include(i => i.Branch)
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.BranchId == branchId && i.ProductId == productId);
        }

        public async Task UpdateStockAsync(Guid productId, decimal quantity)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == productId);

            if (inventory == null)
            {
                inventory = new Inventory
                {
                    ProductId = productId,
                    Quantity = quantity,
                    LastUpdated = DateTime.UtcNow
                };
                _context.Inventories.Add(inventory);
            }
            else
            {
                inventory.Quantity = quantity;
                inventory.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Inventory> AdjustStockAsync(Guid productId, Guid branchId, decimal adjustment)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == productId && i.BranchId == branchId);

            if (inventory == null)
            {
                // Crear nuevo inventario si no existe
                inventory = new Inventory
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    BranchId = branchId,
                    Quantity = adjustment,
                    MinThreshold = 10,
                    LastUpdated = DateTime.UtcNow
                };
                _context.Inventories.Add(inventory);
            }
            else
            {
                inventory.Quantity += adjustment;
                inventory.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return inventory;
        }

        // ✅ NUEVO: Método para ajustar stock con registro de movimiento
        public async Task<Inventory> AdjustStockWithMovementAsync(Guid productId, Guid branchId, decimal adjustment, MovementType movementType, string reason, Guid? userId = null)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == productId && i.BranchId == branchId);

            var previousStock = inventory?.Stock ?? 0;
            var newStock = Math.Max(0, previousStock + (int)adjustment);

            if (inventory == null)
            {
                // Crear nuevo inventario si no existe
                inventory = new Inventory
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    BranchId = branchId,
                    Stock = newStock,
                    MinThreshold = 10,
                    LastUpdated = DateTime.UtcNow
                };
                _context.Inventories.Add(inventory);
            }
            else
            {
                inventory.Stock = newStock;
                inventory.LastUpdated = DateTime.UtcNow;
            }

            // Crear movimiento de inventario
            var movement = new InventoryMovement
            {
                Id = Guid.NewGuid(),
                InventoryId = inventory.Id,
                ProductId = productId,
                BranchId = branchId,
                UserId = userId,
                Type = movementType,
                Quantity = adjustment,
                PreviousStock = previousStock,
                NewStock = newStock,
                Reason = reason,
                Reference = $"{movementType.ToString().ToUpper()}-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}",
                CreatedAt = DateTime.UtcNow
            };

            _context.InventoryMovements.Add(movement);
            await _context.SaveChangesAsync();
            
            return inventory;
        }

        // ✅ NUEVO: Método para descontar stock de un producto
        public async Task<bool> DecrementStockAsync(Guid productId, decimal quantity, IOrderHubService? orderHubService = null)
        {
            try
            {
                Console.WriteLine($"[InventoryService] DecrementStockAsync iniciado - ProductId: {productId}, Quantity: {quantity}");
                
                // Obtener el producto directamente
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    Console.WriteLine($"[InventoryService] ERROR: Producto no encontrado con ID {productId}");
                    return false;
                }

                // Verificar si el producto tiene stock configurado
                if (product.Stock == null)
                {
                    Console.WriteLine($"[InventoryService] WARNING: Producto {product.Name} no tiene stock configurado");
                    return false;
                }

                // Verificar stock suficiente en el producto
                if (product.Stock < quantity)
                {
                    Console.WriteLine($"[InventoryService] ERROR: Stock insuficiente para {product.Name} - Disponible: {product.Stock}, Requerido: {quantity}");
                    return false;
                }

                // Guardar el stock anterior para notificación
                var oldStock = product.Stock.Value;

                // Descontar el stock del producto (stock global)
                product.Stock -= quantity;
                Console.WriteLine($"[InventoryService] Stock global descontado para {product.Name}: {oldStock} -> {product.Stock}");

                // Actualizar el producto en la base de datos
                _context.Products.Update(product);

                // ✅ NUEVO: También descontar del inventario por sucursal
                // Por ahora asumimos que es la sucursal principal (se puede mejorar para manejar múltiples sucursales)
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == productId);

                if (inventory != null)
                {
                    var oldInventoryStock = inventory.Stock ?? 0;
                    inventory.Stock = Math.Max(0, oldInventoryStock - (int)quantity);
                    inventory.LastUpdated = DateTime.UtcNow;
                    
                    Console.WriteLine($"[InventoryService] Stock de inventario descontado: {oldInventoryStock} -> {inventory.Stock}");
                    _context.Inventories.Update(inventory);
                }

                await _context.SaveChangesAsync();

                // Notificar a través de SignalR si está disponible
                if (orderHubService != null)
                {
                    try
                    {
                        await orderHubService.NotifyStockUpdated(productId, product.Name, product.Stock.Value);
                        Console.WriteLine($"[InventoryService] ✅ Notificación de stock enviada via SignalR");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[InventoryService] WARNING: Error al enviar notificación de stock: {ex.Message}");
                    }
                }

                Console.WriteLine($"[InventoryService] Stock actualizado exitosamente para {product.Name}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryService] ERROR al descontar stock: {ex.Message}");
                return false;
            }
        }

        // ✅ NUEVO: Método para obtener historial de cambios de stock
        public async Task<IEnumerable<object>> GetStockHistoryAsync(Guid productId, Guid branchId)
        {
            // Por ahora retornamos una lista vacía ya que no tenemos tabla de historial
            // En el futuro se puede implementar una tabla InventoryHistory
            return new List<object>();
        }

        // ✅ NUEVO: Método para sincronizar stock global con inventarios
        public async Task SyncGlobalStockAsync(Guid productId)
        {
            try
            {
                Console.WriteLine($"[InventoryService] SyncGlobalStockAsync iniciado para ProductId: {productId}");
                
                // Obtener el producto
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    Console.WriteLine($"[InventoryService] ERROR: Producto no encontrado con ID {productId}");
                    return;
                }

                // Calcular la suma total de stock de todos los inventarios
                var totalInventoryStock = await _context.Inventories
                    .Where(i => i.ProductId == productId)
                    .SumAsync(i => i.Stock ?? 0);

                // Actualizar el stock global del producto
                var oldGlobalStock = product.Stock ?? 0;
                product.Stock = totalInventoryStock;

                Console.WriteLine($"[InventoryService] Stock global sincronizado para {product.Name}: {oldGlobalStock} -> {product.Stock}");

                _context.Products.Update(product);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryService] ERROR al sincronizar stock global: {ex.Message}");
            }
        }

        // ✅ NUEVO: Método para obtener reporte de stock por sucursal
        public async Task<IEnumerable<object>> GetStockReportByBranchAsync()
        {
            try
            {
                var report = await _context.Inventories
                    .Include(i => i.Product)
                    .Include(i => i.Branch)
                    .Select(i => new
                    {
                        ProductName = i.Product.Name,
                        BranchName = i.Branch.Name,
                        Stock = i.Stock ?? 0,
                        MinStock = i.MinStock ?? 0,
                        MaxStock = i.MaxStock ?? 0,
                        LastUpdated = i.LastUpdated
                    })
                    .ToListAsync();

                return report;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryService] ERROR al obtener reporte de stock: {ex.Message}");
                return new List<object>();
            }
        }
    }
} 