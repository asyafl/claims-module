using ClaimsModule.Application.Behaviors;
using ClaimsModule.Application.DTOs;
using ClaimsModule.Application.Interfaces;
using ClaimsModule.Domain.Exceptions;
using MediatR;

namespace ClaimsModule.Application.Features.Documents.Queries;

public record GetClaimDocumentsQuery(Guid ClaimId) : IQuery<IReadOnlyList<ClaimDocumentDto>>;

public record ClaimDocumentDto(
    Guid Id,
    string DocumentType,
    string DocumentName,
    string ContentType,
    long FileSizeBytes,
    DateTimeOffset UploadedAt,
    Guid? UploadedByUserId,
    string? Notes,
    string DownloadUrl
);

public class GetClaimDocumentsQueryHandler(
    IClaimRepository claimRepository,
    IStorageService storageService) : IRequestHandler<GetClaimDocumentsQuery, IReadOnlyList<ClaimDocumentDto>>
{
    public async Task<IReadOnlyList<ClaimDocumentDto>> Handle(GetClaimDocumentsQuery request, CancellationToken cancellationToken)
    {
        var claim = await claimRepository.GetByIdWithDetailsAsync(request.ClaimId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Claim), request.ClaimId);

        var results = new List<ClaimDocumentDto>();
        foreach (var doc in claim.Documents)
        {
            var url = await storageService.GetDownloadUrlAsync(doc.BlobPath, TimeSpan.FromHours(1), cancellationToken);
            results.Add(new ClaimDocumentDto(
                doc.Id, doc.DocumentType, doc.DocumentName,
                doc.ContentType, doc.FileSizeBytes, doc.UploadedAt,
                doc.UploadedByUserId, doc.Notes, url));
        }
        return results;
    }
}
