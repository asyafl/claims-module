using ClaimsModule.Application.Features.Claims.Commands;
using ClaimsModule.Application.Features.Claims.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClaimsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClaimCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.ClaimId }, result);
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? status,
        [FromQuery] DateTimeOffset? dateFrom,
        [FromQuery] DateTimeOffset? dateTo,
        [FromQuery] Guid? assignedHandlerId,
        [FromQuery] string? causeOfLossCode,
        [FromQuery] Guid? policyId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new ListClaimsQuery(
            status, dateFrom, dateTo, assignedHandlerId,
            causeOfLossCode, policyId, search, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetClaimDetailQuery(id), ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> TransitionStatus(Guid id, [FromBody] TransitionStatusRequest request, CancellationToken ct)
    {
        await mediator.Send(new TransitionClaimStatusCommand(id, request.TargetStatus, request.Reason), ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/audit")]
    public async Task<IActionResult> GetAudit(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetClaimAuditQuery(id, page, pageSize), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/parties")]
    public async Task<IActionResult> AddParty(Guid id, [FromBody] AddClaimPartyRequest request, CancellationToken ct)
    {
        var partyId = await mediator.Send(new AddClaimPartyCommand(
            id, request.PartyRole, request.PartyType,
            request.FirstName, request.LastName, request.CompanyName,
            request.Email, request.Phone, request.Notes), ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { partyId });
    }

    [HttpDelete("{id:guid}/parties/{partyId:guid}")]
    public async Task<IActionResult> RemoveParty(Guid id, Guid partyId, CancellationToken ct)
    {
        await mediator.Send(new RemoveClaimPartyCommand(id, partyId), ct);
        return NoContent();
    }
}

public record TransitionStatusRequest(string TargetStatus, string? Reason);
public record AddClaimPartyRequest(
    string PartyRole, string PartyType,
    string? FirstName, string? LastName, string? CompanyName,
    string? Email, string? Phone, string? Notes);
