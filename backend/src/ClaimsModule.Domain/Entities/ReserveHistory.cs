using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Domain.Entities;

public class ReserveHistory : BaseEntity
{
    public Guid ReserveComponentId { get; set; }
    public Guid ClaimId { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public decimal PreviousBalance { get; set; }
    public decimal NewBalance { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public Guid? RejectedByUserId { get; set; }
    public DateTimeOffset? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string ChangeReason { get; set; } = string.Empty;
    public PostingStatus PostingStatus { get; set; } = PostingStatus.Pending;
    public string? PostingJobId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public int ChangeSequence { get; set; }
    public Guid? SubmittedByUserId { get; set; }

    public ClaimReserveComponent ReserveComponent { get; set; } = null!;
}
