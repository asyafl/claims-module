using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Events;

public sealed class ClaimCreatedEvent(Guid claimId, string claimNumber, Guid organisationId) : BaseDomainEvent
{
    public Guid ClaimId { get; } = claimId;
    public string ClaimNumber { get; } = claimNumber;
    public Guid OrganisationId { get; } = organisationId;
}
