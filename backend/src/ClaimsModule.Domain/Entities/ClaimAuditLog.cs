using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Entities;

public class ClaimAuditLog : BaseEntity
{
    public Guid ClaimId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? CorrelationId { get; set; }
    public Guid? CreatedByUserId { get; set; }
}

public static class AuditEventTypes
{
    public const string ClaimCreated = "CLAIM_CREATED";
    public const string StatusChanged = "STATUS_CHANGED";
    public const string PartyAdded = "PARTY_ADDED";
    public const string PartyRemoved = "PARTY_REMOVED";
    public const string ReserveCreated = "RESERVE_CREATED";
    public const string ReserveAutoApproved = "RESERVE_AUTO_APPROVED";
    public const string ReserveApproved = "RESERVE_APPROVED";
    public const string ReserveRejected = "RESERVE_REJECTED";
    public const string ReserveRetracted = "RESERVE_RETRACTED";
    public const string GlPostingSimulated = "GL_POSTING_SIMULATED";
    public const string GlPostingFailed = "GL_POSTING_FAILED";
    public const string DocumentUploaded = "DOCUMENT_UPLOADED";
    public const string ClaimClosed = "CLAIM_CLOSED";
    public const string ClaimReopened = "CLAIM_REOPENED";
    public const string SlaBreachDetected = "SLA_BREACH_DETECTED";
    public const string ValidationIssueAdded = "VALIDATION_ISSUE_ADDED";
}
