using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestBar.Models;

namespace RestBar.Controllers;

[Authorize(Policy = "OrderAccess")]
public class ShiftController : Controller
{
    private readonly RestBarContext _context;

    public ShiftController(RestBarContext context) => _context = context;

    [HttpPost]
    public async Task<IActionResult> Start()
    {
        var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
        var branchId = Guid.TryParse(User.FindFirst("BranchId")?.Value, out var bid) ? bid : (Guid?)null;
        var companyId = Guid.TryParse(User.FindFirst("CompanyId")?.Value, out var cid) ? cid : (Guid?)null;

        var active = await _context.Shifts.AnyAsync(s => s.UserId == userId && s.IsActive);
        if (active)
            return BadRequest(new { success = false, message = "Ya tiene un turno activo" });

        var shift = new Shift
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BranchId = branchId,
            CompanyId = companyId,
            StartedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.Shifts.Add(shift);
        await _context.SaveChangesAsync();
        return Json(new { success = true, shiftId = shift.Id });
    }

    public class HandoffDto
    {
        public Guid TableId { get; set; }
        public Guid ToUserId { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> HandoffTable([FromBody] HandoffDto dto)
    {
        var fromUserId = Guid.Parse(User.FindFirst("UserId")!.Value);
        var shift = await _context.Shifts.FirstOrDefaultAsync(s => s.UserId == fromUserId && s.IsActive);
        if (shift == null)
            return BadRequest(new { success = false, message = "No tiene turno activo" });

        _context.ShiftTableHandoffs.Add(new ShiftTableHandoff
        {
            Id = Guid.NewGuid(),
            ShiftId = shift.Id,
            TableId = dto.TableId,
            FromUserId = fromUserId,
            ToUserId = dto.ToUserId,
            HandedOffAt = DateTime.UtcNow
        });

        var assignment = await _context.UserAssignments.FirstOrDefaultAsync(a => a.UserId == dto.ToUserId && a.IsActive);
        if (assignment != null)
        {
            assignment.AssignedTableIds ??= new List<Guid>();
            if (!assignment.AssignedTableIds.Contains(dto.TableId))
                assignment.AssignedTableIds.Add(dto.TableId);
        }

        await _context.SaveChangesAsync();
        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> End()
    {
        var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
        var shift = await _context.Shifts.FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);
        if (shift == null)
            return BadRequest(new { success = false, message = "No tiene turno activo" });

        shift.IsActive = false;
        shift.EndedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Json(new { success = true });
    }
}
