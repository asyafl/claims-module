using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Domain.Events;

public sealed class ClaimStatusChangedEvent(Guid claimId, ClaimStatus from, ClaimStatus to, Guid changedBy) : BaseDomainEvent
{
    public Guid ClaimId { get; } = claimId;
    public ClaimStatus From { get; } = from;
    public ClaimStatus To { get; } = to;
    public Guid ChangedBy { get; } = changedBy;
}
