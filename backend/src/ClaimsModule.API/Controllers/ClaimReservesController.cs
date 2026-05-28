using ClaimsModule.Application.Features.Reserves.Commands;
using ClaimsModule.Application.Features.Reserves.Queries;
using Hangfire;
using ClaimsModule.Infrastructure.Jobs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Controllers;

[ApiController]
[Route("api/claims/{claimId:guid}/reserves")]
[Authorize]
public class ClaimReservesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(Guid claimId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetClaimReservesQuery(claimId), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid claimId, [FromBody] CreateReserveRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateReserveCommand(
            claimId, request.Component, request.Amount, request.ChangeReason, request.TransactionType ?? "Add"), ct);

        // Enqueue GL posting job if auto-approved
        if (!result.RequiresApproval)
        {
            BackgroundJob.Enqueue<PostGLReserveChangeJob>(job =>
                job.ExecuteAsync(result.HistoryId, claimId,
                    $"Reserve:{result.HistoryId}:Change:1"));
        }

        return CreatedAtAction(nameof(List), new { claimId }, result);
    }

    [HttpPost("{historyId:guid}/approve")]
    [Authorize(Roles = "supervisor,manager")]
    public async Task<IActionResult> Approve(Guid claimId, Guid historyId, CancellationToken ct)
    {
        await mediator.Send(new ApproveReserveCommand(claimId, historyId), ct);

        BackgroundJob.Enqueue<PostGLReserveChangeJob>(job =>
            job.ExecuteAsync(historyId, claimId, $"Reserve:{historyId}:Change:approved"));

        return NoContent();
    }

    [HttpPost("{historyId:guid}/reject")]
    [Authorize(Roles = "supervisor,manager")]
    public async Task<IActionResult> Reject(Guid claimId, Guid historyId, [FromBody] RejectReserveRequest request, CancellationToken ct)
    {
        await mediator.Send(new RejectReserveCommand(claimId, historyId, request.RejectionReason), ct);
        return NoContent();
    }

    [HttpPost("{historyId:guid}/retract")]
    public async Task<IActionResult> Retract(Guid claimId, Guid historyId, CancellationToken ct)
    {
        await mediator.Send(new RetractReserveCommand(claimId, historyId), ct);
        return NoContent();
    }
}

public record CreateReserveRequest(string Component, decimal Amount, string ChangeReason, string? TransactionType);
public record RejectReserveRequest(string RejectionReason);
