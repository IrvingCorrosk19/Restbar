using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Claims;

namespace RestBar.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly RestBarContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PurchaseOrderService(RestBarContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var userEmail = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
                return null;

            return await _context.Users
                .Include(u => u.Branch)
                .ThenInclude(b => b.Company)
                .FirstOrDefaultAsync(u => u.Email == userEmail);
        }

        public async Task<IEnumerable<PurchaseOrder>> GetAllAsync()
        {
            var user = await GetCurrentUserAsync();
            var companyId = user?.Branch?.CompanyId;

            return await _context.PurchaseOrders
                .Where(po => po.CompanyId == companyId)
                .Include(po => po.Supplier)
                .Include(po => po.Branch)
                .Include(po => po.CreatedBy)
                .Include(po => po.Items)
                .ThenInclude(item => item.Product)
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();
        }

        public async Task<PurchaseOrder?> GetByIdAsync(Guid id)
        {
            var user = await GetCurrentUserAsync();
            var companyId = user?.Branch?.CompanyId;

            return await _context.PurchaseOrders
                .Where(po => po.Id == id && po.CompanyId == companyId)
                .Include(po => po.Supplier)
                .Include(po => po.Branch)
                .Include(po => po.CreatedBy)
                .Include(po => po.Items)
                .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync();
        }

        public async Task<PurchaseOrder> CreateAsync(PurchaseOrder purchaseOrder)
        {
            var user = await GetCurrentUserAsync();
            
            purchaseOrder.Id = Guid.NewGuid();
            purchaseOrder.CompanyId = user?.Branch?.CompanyId ?? Guid.Empty;
            purchaseOrder.BranchId = user?.BranchId ?? Guid.Empty;
            purchaseOrder.CreatedById = user?.Id ?? Guid.Empty;
            purchaseOrder.OrderDate = DateTime.UtcNow;
            purchaseOrder.Status = PurchaseOrderStatus.Draft;
            purchaseOrder.CreatedAt = DateTime.UtcNow;
            purchaseOrder.IsActive = true;

            // Generar número de orden
            purchaseOrder.OrderNumber = await GenerateOrderNumberAsync();

            // Calcular totales
            CalculateOrderTotals(purchaseOrder);

            _context.PurchaseOrders.Add(purchaseOrder);
            await _context.SaveChangesAsync();

            return purchaseOrder;
        }

        public async Task<PurchaseOrder> UpdateAsync(PurchaseOrder purchaseOrder)
        {
            var user = await GetCurrentUserAsync();
            var companyId = user?.Branch?.CompanyId;

            var existingOrder = await _context.PurchaseOrders
                .Include(po => po.Items)
                .FirstOrDefaultAsync(po => po.Id == purchaseOrder.Id && po.CompanyId == companyId);

            if (existingOrder == null)
                throw new InvalidOperationException("Orden de compra no encontrada");

            // Actualizar propiedades básicas
            existingOrder.ExpectedDeliveryDate = purchaseOrder.ExpectedDeliveryDate;
            existingOrder.Notes = purchaseOrder.Notes;
            existingOrder.UpdatedAt = DateTime.UtcNow;

            // Actualizar items
            _context.PurchaseOrderItems.RemoveRange(existingOrder.Items);
            existingOrder.Items = purchaseOrder.Items;

            // Recalcular totales
            CalculateOrderTotals(existingOrder);

            await _context.SaveChangesAsync();
            return existingOrder;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await GetCurrentUserAsync();
            var companyId = user?.Branch?.CompanyId;

            var purchaseOrder = await _context.PurchaseOrders
                .FirstOrDefaultAsync(po => po.Id == id && po.CompanyId == companyId);

            if (purchaseOrder == null)
                return false;

            // Solo permitir eliminar órdenes en estado Draft
            if (purchaseOrder.Status != PurchaseOrderStatus.Draft)
                return false;

            purchaseOrder.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<PurchaseOrder>> GetByStatusAsync(PurchaseOrderStatus status)
        {
            var user = await GetCurrentUserAsync();
            var companyId = user?.Branch?.CompanyId;

            return await _context.PurchaseOrders
                .Where(po => po.Status == status && po.CompanyId == companyId)
                .Include(po => po.Supplier)
                .Include(po => po.Branch)
                .Include(po => po.CreatedBy)
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PurchaseOrder>> GetBySupplierAsync(Guid supplierId)
        {
            var user = await GetCurrentUserAsync();
            var companyId = user?.Branch?.CompanyId;

            return await _context.PurchaseOrders
                .Where(po => po.SupplierId == supplierId && po.CompanyId == companyId)
                .Include(po => po.Supplier)
                .Include(po => po.Branch)
                .Include(po => po.CreatedBy)
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PurchaseOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var user = await GetCurrentUserAsync();
            var companyId = user?.Branch?.CompanyId;

            return await _context.PurchaseOrders
                .Where(po => po.OrderDate >= startDate && po.OrderDate <= endDate && po.CompanyId == companyId)
                .Include(po => po.Supplier)
                .Include(po => po.Branch)
                .Include(po => po.CreatedBy)
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();
        }

        public async Task<PurchaseOrder> ApproveAsync(Guid id)
        {
            var user = await GetCurrentUserAsync();
            var companyId = user?.Branch?.CompanyId;

            var purchaseOrder = await _context.PurchaseOrders
                .FirstOrDefaultAsync(po => po.Id == id && po.CompanyId == companyId);

            if (purchaseOrder == null)
                throw new InvalidOperationException("Orden de compra no encontrada");

            if (purchaseOrder.Status != PurchaseOrderStatus.Draft)
                throw new InvalidOperationException("Solo se pueden aprobar órdenes en estado Draft");

            // Cambiar a Pending primero, luego a Approved
            purchaseOrder.Status = PurchaseOrderStatus.Pending;
            purchaseOrder.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Simular proceso de aprobación (en un sistema real, aquí iría la lógica de aprobación)
            await Task.Delay(100); // Simular procesamiento

            purchaseOrder.Status = PurchaseOrderStatus.Approved;
            purchaseOrder.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return purchaseOrder;
        }

        public async Task<PurchaseOrder> OrderAsync(Guid id)
        {
            var user = await GetCurrentUserAsync();
            var companyId = user?.Branch?.CompanyId;

            var purchaseOrder = await _context.PurchaseOrders
                .FirstOrDefaultAsync(po => po.Id == id && po.CompanyId == companyId);

            if (purchaseOrder == null)
                throw new InvalidOperationException("Orden de compra no encontrada");

            if (purchaseOrder.Status != PurchaseOrderStatus.Approved)
                throw new InvalidOperationException("Solo se pueden ordenar órdenes aprobadas");

            purchaseOrder.Status = PurchaseOrderStatus.Ordered;
            purchaseOrder.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return purchaseOrder;
        }

        public async Task<PurchaseOrder> CancelAsync(Guid id)
        {
            var user = await GetCurrentUserAsync();
            var companyId = user?.Branch?.CompanyId;

            var purchaseOrder = await _context.PurchaseOrders
                .FirstOrDefaultAsync(po => po.Id == id && po.CompanyId == companyId);

            if (purchaseOrder == null)
                throw new InvalidOperationException("Orden de compra no encontrada");

            if (purchaseOrder.Status == PurchaseOrderStatus.Received)
                throw new InvalidOperationException("No se puede cancelar una orden ya recibida");

            purchaseOrder.Status = PurchaseOrderStatus.Cancelled;
            purchaseOrder.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return purchaseOrder;
        }

        public async Task<PurchaseOrder> ReceiveAsync(Guid id, List<PurchaseOrderItem> receivedItems)
        {
            var user = await GetCurrentUserAsync();
            var companyId = user?.Branch?.CompanyId;
            var branchId = user?.BranchId;

            Console.WriteLine($"[PurchaseOrderService] ReceiveAsync iniciado para orden {id}");
            Console.WriteLine($"[PurchaseOrderService] Usuario: {user?.Email}, CompanyId: {companyId}, BranchId: {branchId}");

            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.Items)
                .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync(po => po.Id == id && po.CompanyId == companyId);

            if (purchaseOrder == null)
                throw new InvalidOperationException("Orden de compra no encontrada");

            if (purchaseOrder.Status == PurchaseOrderStatus.Received)
                throw new InvalidOperationException("La orden ya fue recibida");

            Console.WriteLine($"[PurchaseOrderService] Orden encontrada. Estado actual: {purchaseOrder.Status}");

            // Actualizar cantidades recibidas y actualizar inventario
            foreach (var receivedItem in receivedItems)
            {
                var orderItem = purchaseOrder.Items.FirstOrDefault(item => item.Id == receivedItem.Id);
                if (orderItem != null)
                {
                    var previousReceived = orderItem.ReceivedQuantity ?? 0;
                    var newReceived = receivedItem.ReceivedQuantity;
                    var quantityToAdd = newReceived - previousReceived;

                    Console.WriteLine($"[PurchaseOrderService] Item {orderItem.Product?.Name}: {previousReceived} -> {newReceived} (+{quantityToAdd})");

                    // Actualizar cantidad recibida
                    orderItem.ReceivedQuantity = newReceived;
                    orderItem.UpdatedAt = DateTime.UtcNow;

                    // Actualizar inventario si hay productos para agregar
                    if (quantityToAdd > 0 && orderItem.Product != null)
                    {
                        await UpdateInventoryAsync(orderItem.Product.Id, branchId.Value, quantityToAdd.Value, orderItem.UnitPrice);
                    }
                }
            }

            // Verificar si todos los items fueron recibidos
            var allReceived = purchaseOrder.Items.All(item => 
                item.ReceivedQuantity.HasValue && item.ReceivedQuantity >= item.Quantity);

            purchaseOrder.Status = allReceived ? PurchaseOrderStatus.Received : PurchaseOrderStatus.PartiallyReceived;
            purchaseOrder.ActualDeliveryDate = DateTime.UtcNow;
            purchaseOrder.UpdatedAt = DateTime.UtcNow;

            Console.WriteLine($"[PurchaseOrderService] Nuevo estado de la orden: {purchaseOrder.Status}");

            await _context.SaveChangesAsync();
            return purchaseOrder;
        }

        private async Task UpdateInventoryAsync(Guid productId, Guid branchId, int quantity, decimal unitPrice)
        {
            Console.WriteLine($"[PurchaseOrderService] Actualizando inventario: Producto {productId}, Sucursal {branchId}, Cantidad {quantity}");

            // Buscar inventario existente
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == productId && i.BranchId == branchId);

            if (inventory == null)
            {
                // Crear nuevo registro de inventario
                inventory = new Inventory
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    BranchId = branchId,
                    Quantity = quantity,
                    MinStock = 10, // Valor por defecto
                    MaxStock = 100, // Valor por defecto
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };

                _context.Inventories.Add(inventory);
                Console.WriteLine($"[PurchaseOrderService] Nuevo inventario creado para producto {productId}");
            }
            else
            {
                // Actualizar inventario existente
                inventory.Quantity += quantity;
                inventory.UpdatedAt = DateTime.UtcNow;
                inventory.UpdatedBy = "System";
                Console.WriteLine($"[PurchaseOrderService] Inventario actualizado: {inventory.Quantity} unidades");
            }

            // Crear movimiento de inventario
            var movement = new InventoryMovement
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                BranchId = branchId,
                Type = MovementType.Purchase,
                Quantity = quantity,
                Reference = "Recepción de Orden de Compra",
                Reason = $"Recepción automática de {quantity} unidades",
                CreatedAt = DateTime.UtcNow
            };

            _context.InventoryMovements.Add(movement);
            Console.WriteLine($"[PurchaseOrderService] Movimiento de inventario registrado: {movement.Id}");

            // Verificar stock bajo (usando MinStock en lugar de MinimumStock)
            if (inventory.MinStock.HasValue && inventory.Quantity <= inventory.MinStock.Value)
            {
                Console.WriteLine($"[PurchaseOrderService] Stock bajo detectado para producto {productId}: {inventory.Quantity} unidades");
                // Aquí se podría implementar una notificación o alerta en el futuro
            }
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            var today = DateTime.UtcNow;
            var prefix = $"PO{today:yyyyMMdd}";
            
            var lastOrder = await _context.PurchaseOrders
                .Where(po => po.OrderNumber.StartsWith(prefix))
                .OrderByDescending(po => po.OrderNumber)
                .FirstOrDefaultAsync();

            if (lastOrder == null)
            {
                return $"{prefix}001";
            }

            var lastNumber = int.Parse(lastOrder.OrderNumber.Substring(prefix.Length));
            return $"{prefix}{(lastNumber + 1):D3}";
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            var user = await GetCurrentUserAsync();
            var companyId = user?.Branch?.CompanyId;

            return await _context.PurchaseOrders.AnyAsync(po => po.Id == id && po.CompanyId == companyId);
        }

        public async Task<int> GetCountAsync()
        {
            var user = await GetCurrentUserAsync();
            var companyId = user?.Branch?.CompanyId;

            return await _context.PurchaseOrders.CountAsync(po => po.CompanyId == companyId);
        }

        public async Task<decimal> GetTotalAmountAsync()
        {
            var user = await GetCurrentUserAsync();
            var companyId = user?.Branch?.CompanyId;

            return await _context.PurchaseOrders
                .Where(po => po.CompanyId == companyId)
                .SumAsync(po => po.TotalAmount);
        }

        private void CalculateOrderTotals(PurchaseOrder purchaseOrder)
        {
            decimal subtotal = 0;
            decimal totalTax = 0;

            foreach (var item in purchaseOrder.Items)
            {
                item.Subtotal = item.UnitPrice * item.Quantity;
                item.TaxAmount = item.Subtotal * (item.TaxRate / 100);
                item.TotalAmount = item.Subtotal + item.TaxAmount;

                subtotal += item.Subtotal;
                totalTax += item.TaxAmount;
            }

            purchaseOrder.Subtotal = subtotal;
            purchaseOrder.TaxAmount = totalTax;
            purchaseOrder.TotalAmount = subtotal + totalTax;
        }
    }
} 