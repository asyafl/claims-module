using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Events;

public sealed class DocumentUploadedEvent(Guid claimId, Guid documentId, string fileName) : BaseDomainEvent
{
    public Guid ClaimId { get; } = claimId;
    public Guid DocumentId { get; } = documentId;
    public string FileName { get; } = fileName;
}
