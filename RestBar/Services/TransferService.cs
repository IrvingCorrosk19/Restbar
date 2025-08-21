using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Claims;

namespace RestBar.Services
{
    public class TransferService : ITransferService
    {
        private readonly RestBarContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TransferService(RestBarContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<Transfer>> GetAllAsync()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) throw new InvalidOperationException("Usuario no encontrado");

            return await _context.Transfers
                .Include(t => t.SourceBranch)
                .Include(t => t.DestinationBranch)
                .Include(t => t.CreatedBy)
                .Include(t => t.ApprovedBy)
                .Include(t => t.ReceivedBy)
                .Include(t => t.Items)
                    .ThenInclude(ti => ti.Product)
                .Where(t => t.CompanyId == user.Branch.CompanyId && t.IsActive == true)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Transfer?> GetByIdAsync(Guid id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) throw new InvalidOperationException("Usuario no encontrado");

            return await _context.Transfers
                .Include(t => t.SourceBranch)
                .Include(t => t.DestinationBranch)
                .Include(t => t.CreatedBy)
                .Include(t => t.ApprovedBy)
                .Include(t => t.ReceivedBy)
                .Include(t => t.Items)
                    .ThenInclude(ti => ti.Product)
                .FirstOrDefaultAsync(t => t.Id == id && t.CompanyId == user.Branch.CompanyId);
        }

        public async Task<Transfer> CreateAsync(Transfer transfer)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) throw new InvalidOperationException("Usuario no encontrado");

            transfer.Id = Guid.NewGuid();
            transfer.CompanyId = user.Branch.CompanyId ?? throw new InvalidOperationException("Sucursal sin empresa asignada");
            transfer.CreatedById = user.Id;
            transfer.TransferNumber = await GenerateTransferNumberAsync();
            transfer.Status = TransferStatus.Pending;
            transfer.CreatedAt = DateTime.UtcNow;
            transfer.IsActive = true;

            // Calcular totales
            CalculateTransferTotals(transfer);

            _context.Transfers.Add(transfer);
            await _context.SaveChangesAsync();

            return transfer;
        }

        public async Task<Transfer> UpdateAsync(Transfer transfer)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) throw new InvalidOperationException("Usuario no encontrado");

            var existingTransfer = await _context.Transfers
                .Include(t => t.Items)
                .FirstOrDefaultAsync(t => t.Id == transfer.Id && t.CompanyId == user.Branch.CompanyId);

            if (existingTransfer == null)
                throw new InvalidOperationException("Transferencia no encontrada");

            // Solo permitir actualización si está pendiente
            if (existingTransfer.Status != TransferStatus.Pending)
                throw new InvalidOperationException("No se puede modificar una transferencia que no esté pendiente");

            existingTransfer.ExpectedDeliveryDate = transfer.ExpectedDeliveryDate;
            existingTransfer.Notes = transfer.Notes;
            existingTransfer.UpdatedAt = DateTime.UtcNow;

            // Actualizar items si se proporcionan
            if (transfer.Items != null && transfer.Items.Any())
            {
                // Eliminar items existentes
                _context.TransferItems.RemoveRange(existingTransfer.Items);

                // Agregar nuevos items
                foreach (var item in transfer.Items)
                {
                    item.Id = Guid.NewGuid();
                    item.TransferId = existingTransfer.Id;
                    item.CreatedAt = DateTime.UtcNow;
                    item.IsActive = true;
                }
                existingTransfer.Items = transfer.Items;
            }

            // Recalcular totales
            CalculateTransferTotals(existingTransfer);

            await _context.SaveChangesAsync();
            return existingTransfer;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) throw new InvalidOperationException("Usuario no encontrado");

            var transfer = await _context.Transfers
                .FirstOrDefaultAsync(t => t.Id == id && t.CompanyId == user.Branch.CompanyId);

            if (transfer == null) return false;

            // Solo permitir eliminación si está pendiente
            if (transfer.Status != TransferStatus.Pending)
                throw new InvalidOperationException("No se puede eliminar una transferencia que no esté pendiente");

            transfer.IsActive = false;
            transfer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Transfer>> GetByStatusAsync(TransferStatus status)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) throw new InvalidOperationException("Usuario no encontrado");

            return await _context.Transfers
                .Include(t => t.SourceBranch)
                .Include(t => t.DestinationBranch)
                .Include(t => t.CreatedBy)
                .Where(t => t.Status == status && t.CompanyId == user.Branch.CompanyId && t.IsActive)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transfer>> GetPendingTransfersAsync()
        {
            return await GetByStatusAsync(TransferStatus.Pending);
        }

        public async Task<IEnumerable<Transfer>> GetApprovedTransfersAsync()
        {
            return await GetByStatusAsync(TransferStatus.Approved);
        }

        public async Task<IEnumerable<Transfer>> GetInTransitTransfersAsync()
        {
            return await GetByStatusAsync(TransferStatus.InTransit);
        }

        public async Task<IEnumerable<Transfer>> GetBySourceBranchAsync(Guid branchId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) throw new InvalidOperationException("Usuario no encontrado");

            return await _context.Transfers
                .Include(t => t.SourceBranch)
                .Include(t => t.DestinationBranch)
                .Include(t => t.CreatedBy)
                .Where(t => t.SourceBranchId == branchId && t.CompanyId == user.Branch.CompanyId && t.IsActive)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transfer>> GetByDestinationBranchAsync(Guid branchId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) throw new InvalidOperationException("Usuario no encontrado");

            return await _context.Transfers
                .Include(t => t.SourceBranch)
                .Include(t => t.DestinationBranch)
                .Include(t => t.CreatedBy)
                .Where(t => t.DestinationBranchId == branchId && t.CompanyId == user.Branch.CompanyId && t.IsActive)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transfer>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) throw new InvalidOperationException("Usuario no encontrado");

            return await _context.Transfers
                .Include(t => t.SourceBranch)
                .Include(t => t.DestinationBranch)
                .Include(t => t.CreatedBy)
                .Where(t => t.TransferDate >= startDate && t.TransferDate <= endDate && 
                           t.CompanyId == user.Branch.CompanyId && t.IsActive)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Transfer> ApproveAsync(Guid transferId, Guid approvedById)
        {
            var transfer = await _context.Transfers
                .Include(t => t.Items)
                .FirstOrDefaultAsync(t => t.Id == transferId);

            if (transfer == null)
                throw new InvalidOperationException("Transferencia no encontrada");

            if (transfer.Status != TransferStatus.Pending)
                throw new InvalidOperationException("Solo se pueden aprobar transferencias pendientes");

            transfer.Status = TransferStatus.Approved;
            transfer.ApprovedById = approvedById;
            transfer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return transfer;
        }

        public async Task<Transfer> RejectAsync(Guid transferId, Guid rejectedById, string reason)
        {
            var transfer = await _context.Transfers
                .FirstOrDefaultAsync(t => t.Id == transferId);

            if (transfer == null)
                throw new InvalidOperationException("Transferencia no encontrada");

            if (transfer.Status != TransferStatus.Pending)
                throw new InvalidOperationException("Solo se pueden rechazar transferencias pendientes");

            transfer.Status = TransferStatus.Rejected;
            transfer.Notes = $"Rechazada: {reason}";
            transfer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return transfer;
        }

        public async Task<Transfer> CancelAsync(Guid transferId, Guid cancelledById, string reason)
        {
            var transfer = await _context.Transfers
                .FirstOrDefaultAsync(t => t.Id == transferId);

            if (transfer == null)
                throw new InvalidOperationException("Transferencia no encontrada");

            if (transfer.Status != TransferStatus.Pending && transfer.Status != TransferStatus.Approved)
                throw new InvalidOperationException("Solo se pueden cancelar transferencias pendientes o aprobadas");

            transfer.Status = TransferStatus.Cancelled;
            transfer.Notes = $"Cancelada: {reason}";
            transfer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return transfer;
        }

        public async Task<Transfer> MarkInTransitAsync(Guid transferId)
        {
            var transfer = await _context.Transfers
                .FirstOrDefaultAsync(t => t.Id == transferId);

            if (transfer == null)
                throw new InvalidOperationException("Transferencia no encontrada");

            if (transfer.Status != TransferStatus.Approved)
                throw new InvalidOperationException("Solo se pueden marcar en tránsito transferencias aprobadas");

            transfer.Status = TransferStatus.InTransit;
            transfer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return transfer;
        }

        public async Task<Transfer> ReceiveAsync(Guid transferId, Guid receivedById)
        {
            var transfer = await _context.Transfers
                .Include(t => t.Items)
                .FirstOrDefaultAsync(t => t.Id == transferId);

            if (transfer == null)
                throw new InvalidOperationException("Transferencia no encontrada");

            if (transfer.Status != TransferStatus.InTransit)
                throw new InvalidOperationException("Solo se pueden recibir transferencias en tránsito");

            transfer.Status = TransferStatus.Delivered;
            transfer.ReceivedById = receivedById;
            transfer.ActualDeliveryDate = DateTime.UtcNow;
            transfer.UpdatedAt = DateTime.UtcNow;

            // Actualizar inventario en la sucursal destino
            foreach (var item in transfer.Items)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && 
                                            i.BranchId == transfer.DestinationBranchId);

                if (inventory != null)
                {
                    inventory.Stock = (inventory.Stock ?? 0) + item.Quantity;
                    inventory.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    // Crear nuevo registro de inventario si no existe
                    inventory = new Inventory
                    {
                        Id = Guid.NewGuid(),
                        ProductId = item.ProductId,
                        BranchId = transfer.DestinationBranchId,
                        Stock = (int)item.Quantity,
                        MinStock = 0,
                        MaxStock = 1000,
                        IsActive = true,
                        LastUpdated = DateTime.UtcNow
                    };
                    _context.Inventories.Add(inventory);
                }

                item.ReceivedQuantity = item.Quantity;
            }

            await _context.SaveChangesAsync();
            return transfer;
        }

        public async Task<string> GenerateTransferNumberAsync()
        {
            var today = DateTime.UtcNow;
            var prefix = $"TRF{today:yyyyMMdd}";
            
            var lastTransfer = await _context.Transfers
                .Where(t => t.TransferNumber.StartsWith(prefix))
                .OrderByDescending(t => t.TransferNumber)
                .FirstOrDefaultAsync();

            if (lastTransfer == null)
            {
                return $"{prefix}001";
            }

            var lastNumber = int.Parse(lastTransfer.TransferNumber.Substring(prefix.Length));
            return $"{prefix}{(lastNumber + 1):D3}";
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return false;

            return await _context.Transfers
                .AnyAsync(t => t.Id == id && t.CompanyId == user.Branch.CompanyId);
        }

        public async Task<int> GetCountAsync()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return 0;

            return await _context.Transfers
                .CountAsync(t => t.CompanyId == user.Branch.CompanyId && t.IsActive);
        }

        public async Task<decimal> GetTotalAmountAsync()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return 0;

            return await _context.Transfers
                .Where(t => t.CompanyId == user.Branch.CompanyId && t.IsActive)
                .SumAsync(t => t.TotalAmount);
        }

        public async Task<object> GetTransferStatisticsAsync()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return new { };

            var transfers = await _context.Transfers
                .Where(t => t.CompanyId == user.Branch.CompanyId && t.IsActive)
                .ToListAsync();

            return new
            {
                TotalTransfers = transfers.Count,
                PendingTransfers = transfers.Count(t => t.Status == TransferStatus.Pending),
                ApprovedTransfers = transfers.Count(t => t.Status == TransferStatus.Approved),
                InTransitTransfers = transfers.Count(t => t.Status == TransferStatus.InTransit),
                DeliveredTransfers = transfers.Count(t => t.Status == TransferStatus.Delivered),
                CancelledTransfers = transfers.Count(t => t.Status == TransferStatus.Cancelled),
                RejectedTransfers = transfers.Count(t => t.Status == TransferStatus.Rejected),
                TotalAmount = transfers.Sum(t => t.TotalAmount)
            };
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var userEmail = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail)) return null;

            return await _context.Users
                .Include(u => u.Branch)
                .ThenInclude(b => b.Company)
                .FirstOrDefaultAsync(u => u.Email == userEmail);
        }

        private void CalculateTransferTotals(Transfer transfer)
        {
            if (transfer.Items == null || !transfer.Items.Any())
            {
                transfer.Subtotal = 0;
                transfer.TaxAmount = 0;
                transfer.TotalAmount = 0;
                return;
            }

            transfer.Subtotal = transfer.Items.Sum(i => i.Subtotal);
            transfer.TaxAmount = transfer.Items.Sum(i => i.TaxAmount);
            transfer.TotalAmount = transfer.Items.Sum(i => i.TotalAmount);
        }
    }
} 