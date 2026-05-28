using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Domain.Events;

public sealed class ReserveCreatedEvent(Guid claimId, Guid reserveHistoryId, decimal amount, ApprovalStatus approvalStatus) : BaseDomainEvent
{
    public Guid ClaimId { get; } = claimId;
    public Guid ReserveHistoryId { get; } = reserveHistoryId;
    public decimal Amount { get; } = amount;
    public ApprovalStatus ApprovalStatus { get; } = approvalStatus;
}
