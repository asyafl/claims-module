using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Events;

public sealed class ReserveRejectedEvent(Guid claimId, Guid reserveHistoryId, string reason) : BaseDomainEvent
{
    public Guid ClaimId { get; } = claimId;
    public Guid ReserveHistoryId { get; } = reserveHistoryId;
    public string Reason { get; } = reason;
}
