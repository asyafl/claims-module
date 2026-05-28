using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Events;

public sealed class ReserveApprovedEvent(Guid claimId, Guid reserveHistoryId, string idempotencyKey) : BaseDomainEvent
{
    public Guid ClaimId { get; } = claimId;
    public Guid ReserveHistoryId { get; } = reserveHistoryId;
    public string IdempotencyKey { get; } = idempotencyKey;
}
