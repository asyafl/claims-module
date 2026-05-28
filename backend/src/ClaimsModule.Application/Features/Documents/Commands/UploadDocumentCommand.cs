using ClaimsModule.Application.Behaviors;
using ClaimsModule.Application.Interfaces;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.Features.Documents.Commands;

public record UploadDocumentCommand(
    Guid ClaimId,
    string DocumentType,
    string FileName,
    string ContentType,
    Stream Content,
    long FileSizeBytes,
    string? Notes
) : ICommand<Guid>;

public class UploadDocumentCommandHandler(
    IClaimRepository claimRepository,
    IStorageService storageService,
    IAuditLogService auditLogService,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<UploadDocumentCommand, Guid>
{
    private static readonly Guid OrgId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private static readonly HashSet<string> AllowedMimeTypes =
    [
        "application/pdf", "image/jpeg", "image/png",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "text/plain", "text/csv"
    ];

    public async Task<Guid> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        if (!AllowedMimeTypes.Contains(request.ContentType))
            throw new DomainException($"File type '{request.ContentType}' is not allowed.");

        if (request.FileSizeBytes > 50 * 1024 * 1024)
            throw new DomainException("File size exceeds the 50 MB limit.");

        var claim = await claimRepository.GetByIdAsync(request.ClaimId, cancellationToken)
            ?? throw new NotFoundException(nameof(Claim), request.ClaimId);

        // Sanitise filename to prevent path traversal
        var safeFileName = Path.GetFileName(request.FileName);
        var blobPath = $"{OrgId}/{claim.Id}/{safeFileName}";

        await storageService.UploadAsync(request.Content, blobPath, request.ContentType, cancellationToken);

        var document = new ClaimDocument
        {
            ClaimId = claim.Id,
            DocumentType = request.DocumentType,
            DocumentName = safeFileName,
            BlobPath = blobPath,
            ContentType = request.ContentType,
            FileSizeBytes = request.FileSizeBytes,
            UploadedAt = DateTimeOffset.UtcNow,
            UploadedByUserId = currentUser.UserId,
            Notes = request.Notes,
            OrganisationId = OrgId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        claim.AddDocument(document);
        claimRepository.Update(claim);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(claim.Id, Domain.Entities.AuditEventTypes.DocumentUploaded,
            $"Document '{safeFileName}' ({request.DocumentType}) uploaded",
            currentUser.UserId, relatedEntityId: document.Id, relatedEntityType: "ClaimDocument",
            correlationId: currentUser.CorrelationId, cancellationToken: cancellationToken);

        return document.Id;
    }
}

public class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
{
    public UploadDocumentCommandValidator()
    {
        RuleFor(x => x.ClaimId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.DocumentType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ContentType).NotEmpty();
    }
}
