using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RestBar.Infrastructure.Foundation;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Controllers;

[Authorize(Policy = "CashAccess")]
[Route("api/[controller]")]
[ApiController]
public class CashMovementController : ControllerBase
{
    private readonly ICashMovementService _movements;
    private readonly ICashSessionService _sessions;
    private readonly ICashApprovalService _approvals;
    private readonly FeatureFlags _flags;

    public CashMovementController(
        ICashMovementService movements,
        ICashSessionService sessions,
        ICashApprovalService approvals,
        IOptions<FeatureFlags> flags)
    {
        _movements = movements;
        _sessions = sessions;
        _approvals = approvals;
        _flags = flags.Value;
    }

    [HttpGet("{sessionId:guid}")]
    public async Task<IActionResult> List(Guid sessionId)
    {
        if (!_flags.EnableCashModule)
            return NotFound(new { message = "Cash module disabled" });

        var items = await _movements.GetSessionMovementsAsync(sessionId);
        return Ok(items);
    }

    public record PaidMovementDto(Guid SessionId, decimal Amount, string? ReasonCode, string? Comments);

    [HttpPost("paid-in")]
    public async Task<IActionResult> PaidIn([FromBody] PaidMovementDto dto)
    {
        if (!_flags.EnableCashModule)
            return NotFound(new { message = "Cash module disabled" });

        var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
        var movement = await _movements.RecordMovementAsync(new CashMovementRequest(
            dto.SessionId, CashMovementType.PaidIn, CashMovementDirection.In,
            dto.Amount, userId, ReasonCode: dto.ReasonCode, Comments: dto.Comments));
        return Ok(movement);
    }

    [HttpPost("paid-out")]
    public async Task<IActionResult> PaidOut([FromBody] PaidMovementDto dto)
    {
        if (!_flags.EnableCashModule)
            return NotFound(new { message = "Cash module disabled" });

        var userId = Guid.Parse(User.FindFirst("UserId")!.Value);
        var session = await _sessions.GetByIdAsync(dto.SessionId);
        if (session == null)
            return NotFound();

        if (await _approvals.RequiresDualApprovalAsync(dto.SessionId, CashApprovalType.LargePaidOut, dto.Amount))
        {
            var approval = await _approvals.RequestApprovalAsync(new CashApprovalRequest(
                dto.SessionId, CashApprovalType.LargePaidOut, userId, dto.Amount, dto.Comments));
            return Accepted(new { requiresApproval = true, approvalId = approval.Id });
        }

        var movement = await _movements.RecordMovementAsync(new CashMovementRequest(
            dto.SessionId, CashMovementType.PaidOut, CashMovementDirection.Out,
            dto.Amount, userId, ReasonCode: dto.ReasonCode, Comments: dto.Comments));
        return Ok(movement);
    }
}
