using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services.Cash;

public class CashRegisterService : ICashRegisterService
{
    private readonly RestBarContext _context;

    public CashRegisterService(RestBarContext context) => _context = context;

    public async Task<CashRegister> CreateRegisterAsync(CashRegister register, CancellationToken ct = default)
    {
        register.Id = register.Id == Guid.Empty ? Guid.NewGuid() : register.Id;
        register.CreatedAt = DateTime.UtcNow;
        register.UpdatedAt = DateTime.UtcNow;
        _context.CashRegisters.Add(register);
        await _context.SaveChangesAsync(ct);
        return register;
    }

    public async Task<IReadOnlyList<CashRegister>> GetBranchRegistersAsync(Guid branchId, CancellationToken ct = default)
    {
        return await _context.CashRegisters.AsNoTracking()
            .Where(r => r.BranchId == branchId && r.IsActive)
            .OrderBy(r => r.Code)
            .ToListAsync(ct);
    }

    public async Task<CashRegister?> GetByIdAsync(Guid registerId, CancellationToken ct = default)
    {
        return await _context.CashRegisters.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == registerId, ct);
    }
}
