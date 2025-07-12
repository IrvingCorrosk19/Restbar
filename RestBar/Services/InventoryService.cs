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
            return await _context.Inventories
                .Include(i => i.Branch)
                .Include(i => i.Product)
                .ToListAsync();
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
            inventory.LastUpdated = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
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

                inventory.LastUpdated = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                
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
                .Where(i => i.Quantity <= i.MinThreshold)
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
                    LastUpdated = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                };
                _context.Inventories.Add(inventory);
            }
            else
            {
                inventory.Quantity = quantity;
                inventory.LastUpdated = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
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
                    LastUpdated = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                };
                _context.Inventories.Add(inventory);
            }
            else
            {
                inventory.Quantity += adjustment;
                inventory.LastUpdated = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            }

            await _context.SaveChangesAsync();
            return inventory;
        }
    }
} 