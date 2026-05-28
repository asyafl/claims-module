using ClaimsModule.Application.Features.Documents.Commands;
using ClaimsModule.Application.Features.Documents.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Controllers;

[ApiController]
[Route("api/claims/{claimId:guid}/documents")]
[Authorize]
public class ClaimDocumentsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(Guid claimId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetClaimDocumentsQuery(claimId), ct);
        return Ok(result);
    }

    [HttpPost]
    [RequestSizeLimit(52_428_800)] // 50 MB
    public async Task<IActionResult> Upload(Guid claimId, IFormFile file,
        [FromForm] string documentType, [FromForm] string? notes, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file provided.");

        await using var stream = file.OpenReadStream();
        var documentId = await mediator.Send(new UploadDocumentCommand(
            claimId, documentType, file.FileName,
            file.ContentType, stream, file.Length, notes), ct);

        return CreatedAtAction(nameof(List), new { claimId }, new { documentId });
    }
}
