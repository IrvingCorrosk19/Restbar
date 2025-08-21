using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services
{
    public class InventoryMovementService : IInventoryMovementService
    {
        private readonly RestBarContext _context;
        private readonly IInventoryService _inventoryService;

        public InventoryMovementService(RestBarContext context, IInventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
        }

        public async Task<IEnumerable<InventoryMovement>> GetAllAsync()
        {
            return await _context.InventoryMovements
                .Include(m => m.Inventory)
                .Include(m => m.Product)
                .Include(m => m.Branch)
                .Include(m => m.User)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<InventoryMovement?> GetByIdAsync(Guid id)
        {
            return await _context.InventoryMovements
                .Include(m => m.Inventory)
                .Include(m => m.Product)
                .Include(m => m.Branch)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<InventoryMovement> CreateAsync(InventoryMovement movement)
        {
            movement.CreatedAt = DateTime.UtcNow;
            _context.InventoryMovements.Add(movement);
            await _context.SaveChangesAsync();
            return movement;
        }

        public async Task UpdateAsync(InventoryMovement movement)
        {
            _context.InventoryMovements.Update(movement);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var movement = await _context.InventoryMovements.FindAsync(id);
            if (movement != null)
            {
                _context.InventoryMovements.Remove(movement);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<InventoryMovement>> GetMovementsByInventoryAsync(Guid inventoryId)
        {
            return await _context.InventoryMovements
                .Include(m => m.Inventory)
                .Include(m => m.Product)
                .Include(m => m.Branch)
                .Include(m => m.User)
                .Where(m => m.InventoryId == inventoryId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryMovement>> GetMovementsByProductAsync(Guid productId)
        {
            return await _context.InventoryMovements
                .Include(m => m.Inventory)
                .Include(m => m.Product)
                .Include(m => m.Branch)
                .Include(m => m.User)
                .Where(m => m.ProductId == productId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryMovement>> GetMovementsByBranchAsync(Guid branchId)
        {
            return await _context.InventoryMovements
                .Include(m => m.Inventory)
                .Include(m => m.Product)
                .Include(m => m.Branch)
                .Include(m => m.User)
                .Where(m => m.BranchId == branchId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryMovement>> GetMovementsByUserAsync(Guid userId)
        {
            return await _context.InventoryMovements
                .Include(m => m.Inventory)
                .Include(m => m.Product)
                .Include(m => m.Branch)
                .Include(m => m.User)
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryMovement>> GetMovementsByTypeAsync(MovementType type)
        {
            return await _context.InventoryMovements
                .Include(m => m.Inventory)
                .Include(m => m.Product)
                .Include(m => m.Branch)
                .Include(m => m.User)
                .Where(m => m.Type == type)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryMovement>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.InventoryMovements
                .Include(m => m.Inventory)
                .Include(m => m.Product)
                .Include(m => m.Branch)
                .Include(m => m.User)
                .Where(m => m.CreatedAt >= startDate && m.CreatedAt <= endDate)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalMovementsByTypeAsync(Guid productId, MovementType type, DateTime startDate, DateTime endDate)
        {
            return await _context.InventoryMovements
                .Where(m => m.ProductId == productId && 
                           m.Type == type && 
                           m.CreatedAt >= startDate && 
                           m.CreatedAt <= endDate)
                .SumAsync(m => m.Quantity);
        }

        public async Task<IEnumerable<object>> GetMovementSummaryAsync(DateTime startDate, DateTime endDate)
        {
            var summary = await _context.InventoryMovements
                .Include(m => m.Product)
                .Include(m => m.Branch)
                .Where(m => m.CreatedAt >= startDate && m.CreatedAt <= endDate)
                .GroupBy(m => new { m.Type, ProductName = m.Product.Name, BranchName = m.Branch.Name })
                .Select(g => new
                {
                    MovementType = g.Key.Type,
                    ProductName = g.Key.ProductName,
                    BranchName = g.Key.BranchName,
                    TotalQuantity = g.Sum(m => m.Quantity),
                    MovementCount = g.Count(),
                    LastMovement = g.Max(m => m.CreatedAt)
                })
                .ToListAsync();

            return summary;
        }

        public async Task<IEnumerable<object>> GetProductMovementHistoryAsync(Guid productId, DateTime startDate, DateTime endDate)
        {
            var history = await _context.InventoryMovements
                .Include(m => m.Branch)
                .Include(m => m.User)
                .Where(m => m.ProductId == productId && 
                           m.CreatedAt >= startDate && 
                           m.CreatedAt <= endDate)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new
                {
                    m.Id,
                    m.Type,
                    m.Quantity,
                    m.PreviousStock,
                    m.NewStock,
                    m.Reason,
                    m.Reference,
                    m.CreatedAt,
                    BranchName = m.Branch.Name,
                    UserName = m.User.FullName
                })
                .ToListAsync();

            return history;
        }

        public async Task<InventoryMovement> CreatePurchaseMovementAsync(Guid inventoryId, Guid productId, Guid branchId, Guid userId, decimal quantity, decimal unitCost, string? reason = null)
        {
            var inventory = await _context.Inventories.FindAsync(inventoryId);
            if (inventory == null)
                throw new ArgumentException("Inventario no encontrado");

            var previousStock = inventory.Stock ?? 0;
            var newStock = previousStock + (int)quantity;

            var movement = new InventoryMovement
            {
                Id = Guid.NewGuid(),
                InventoryId = inventoryId,
                ProductId = productId,
                BranchId = branchId,
                UserId = userId,
                Type = MovementType.Purchase,
                Quantity = quantity,
                PreviousStock = previousStock,
                NewStock = newStock,
                Reason = reason ?? "Compra de inventario",
                Reference = $"PO-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}"
            };

            // Actualizar el inventario
            inventory.Stock = newStock;
            inventory.LastUpdated = DateTime.UtcNow;
            inventory.UnitCost = unitCost;
            inventory.TotalValue = newStock * unitCost;

            _context.Inventories.Update(inventory);
            await CreateAsync(movement);

            return movement;
        }

        public async Task<InventoryMovement> CreateSaleMovementAsync(Guid inventoryId, Guid productId, Guid branchId, Guid userId, decimal quantity, string? reason = null)
        {
            var inventory = await _context.Inventories.FindAsync(inventoryId);
            if (inventory == null)
                throw new ArgumentException("Inventario no encontrado");

            if ((inventory.Stock ?? 0) < quantity)
                throw new InvalidOperationException("Stock insuficiente para la venta");

            var previousStock = inventory.Stock ?? 0;
            var newStock = Math.Max(0, previousStock - (int)quantity);

            var movement = new InventoryMovement
            {
                Id = Guid.NewGuid(),
                InventoryId = inventoryId,
                ProductId = productId,
                BranchId = branchId,
                UserId = userId,
                Type = MovementType.Sale,
                Quantity = -quantity, // Negativo para ventas
                PreviousStock = previousStock,
                NewStock = newStock,
                Reason = reason ?? "Venta de inventario",
                Reference = $"SALE-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}"
            };

            // Actualizar el inventario
            inventory.Stock = newStock;
            inventory.LastUpdated = DateTime.UtcNow;
            if (inventory.UnitCost.HasValue)
                inventory.TotalValue = newStock * inventory.UnitCost.Value;

            _context.Inventories.Update(inventory);
            await CreateAsync(movement);

            return movement;
        }

        public async Task<InventoryMovement> CreateAdjustmentMovementAsync(Guid inventoryId, Guid productId, Guid branchId, Guid userId, decimal quantity, string reason)
        {
            var inventory = await _context.Inventories.FindAsync(inventoryId);
            if (inventory == null)
                throw new ArgumentException("Inventario no encontrado");

            var previousStock = inventory.Stock ?? 0;
            var newStock = Math.Max(0, previousStock + (int)quantity);

            var movement = new InventoryMovement
            {
                Id = Guid.NewGuid(),
                InventoryId = inventoryId,
                ProductId = productId,
                BranchId = branchId,
                UserId = userId,
                Type = MovementType.Adjustment,
                Quantity = quantity,
                PreviousStock = previousStock,
                NewStock = newStock,
                Reason = reason,
                Reference = $"ADJ-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}"
            };

            // Actualizar el inventario
            inventory.Stock = newStock;
            inventory.LastUpdated = DateTime.UtcNow;
            if (inventory.UnitCost.HasValue)
                inventory.TotalValue = newStock * inventory.UnitCost.Value;

            _context.Inventories.Update(inventory);
            await CreateAsync(movement);

            return movement;
        }

        public async Task<InventoryMovement> CreateTransferMovementAsync(Guid fromInventoryId, Guid toInventoryId, Guid productId, Guid fromBranchId, Guid toBranchId, Guid userId, decimal quantity, string? reason = null)
        {
            // Crear movimiento de salida
            var fromInventory = await _context.Inventories.FindAsync(fromInventoryId);
            if (fromInventory == null)
                throw new ArgumentException("Inventario de origen no encontrado");

            if ((fromInventory.Stock ?? 0) < quantity)
                throw new InvalidOperationException("Stock insuficiente para la transferencia");

            var fromPreviousStock = fromInventory.Stock ?? 0;
            var fromNewStock = Math.Max(0, fromPreviousStock - (int)quantity);

            var fromMovement = new InventoryMovement
            {
                Id = Guid.NewGuid(),
                InventoryId = fromInventoryId,
                ProductId = productId,
                BranchId = fromBranchId,
                UserId = userId,
                Type = MovementType.Transfer,
                Quantity = -quantity,
                PreviousStock = fromPreviousStock,
                NewStock = fromNewStock,
                Reason = reason ?? $"Transferencia a {toBranchId}",
                Reference = $"TRF-OUT-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}"
            };

            // Actualizar inventario de origen
            fromInventory.Stock = fromNewStock;
            fromInventory.LastUpdated = DateTime.UtcNow;
            if (fromInventory.UnitCost.HasValue)
                fromInventory.TotalValue = fromNewStock * fromInventory.UnitCost.Value;

            _context.Inventories.Update(fromInventory);
            await CreateAsync(fromMovement);

            // Crear movimiento de entrada
            var toInventory = await _context.Inventories.FindAsync(toInventoryId);
            if (toInventory == null)
                throw new ArgumentException("Inventario de destino no encontrado");

            var toPreviousStock = toInventory.Stock ?? 0;
            var toNewStock = toPreviousStock + (int)quantity;

            var toMovement = new InventoryMovement
            {
                Id = Guid.NewGuid(),
                InventoryId = toInventoryId,
                ProductId = productId,
                BranchId = toBranchId,
                UserId = userId,
                Type = MovementType.Transfer,
                Quantity = quantity,
                PreviousStock = toPreviousStock,
                NewStock = toNewStock,
                Reason = reason ?? $"Transferencia desde {fromBranchId}",
                Reference = $"TRF-IN-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}"
            };

            // Actualizar inventario de destino
            toInventory.Stock = toNewStock;
            toInventory.LastUpdated = DateTime.UtcNow;
            if (toInventory.UnitCost.HasValue)
                toInventory.TotalValue = toNewStock * toInventory.UnitCost.Value;

            _context.Inventories.Update(toInventory);
            await CreateAsync(toMovement);

            return toMovement; // Retornamos el movimiento de entrada
        }

        public async Task<InventoryMovement> CreateWasteMovementAsync(Guid inventoryId, Guid productId, Guid branchId, Guid userId, decimal quantity, string reason)
        {
            var inventory = await _context.Inventories.FindAsync(inventoryId);
            if (inventory == null)
                throw new ArgumentException("Inventario no encontrado");

            if ((inventory.Stock ?? 0) < quantity)
                throw new InvalidOperationException("Stock insuficiente para registrar pérdida");

            var previousStock = inventory.Stock ?? 0;
            var newStock = Math.Max(0, previousStock - (int)quantity);

            var movement = new InventoryMovement
            {
                Id = Guid.NewGuid(),
                InventoryId = inventoryId,
                ProductId = productId,
                BranchId = branchId,
                UserId = userId,
                Type = MovementType.Waste,
                Quantity = -quantity, // Negativo para pérdidas
                PreviousStock = previousStock,
                NewStock = newStock,
                Reason = reason,
                Reference = $"WASTE-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}"
            };

            // Actualizar el inventario
            inventory.Stock = newStock;
            inventory.LastUpdated = DateTime.UtcNow;
            if (inventory.UnitCost.HasValue)
                inventory.TotalValue = newStock * inventory.UnitCost.Value;

            _context.Inventories.Update(inventory);
            await CreateAsync(movement);

            return movement;
        }

        public async Task<bool> ValidateMovementAsync(Guid inventoryId, MovementType type, decimal quantity)
        {
            var inventory = await _context.Inventories.FindAsync(inventoryId);
            if (inventory == null)
                return false;

            // Para movimientos que reducen stock (ventas, transferencias, pérdidas)
            if (type == MovementType.Sale || type == MovementType.Transfer || type == MovementType.Waste)
            {
                return (inventory.Stock ?? 0) >= quantity;
            }

            // Para otros tipos de movimientos (compras, ajustes)
            return true;
        }

        public async Task<decimal> GetCurrentStockAsync(Guid inventoryId)
        {
            var inventory = await _context.Inventories.FindAsync(inventoryId);
            return inventory?.Stock ?? 0;
        }
    }
} 