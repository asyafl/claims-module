namespace ClaimsModule.Application.DTOs;

public record ClaimDetailDto(
    Guid Id,
    string ClaimNumber,
    Guid? PolicyId,
    string? PolicyNumber,
    string? ClientName,
    string Status,
    string? Severity,
    DateTimeOffset ReportedDate,
    Guid? AssignedHandlerId,
    DateTimeOffset? ClosedAt,
    string? ClosureReason,
    string? Notes,
    bool ManagerOverrideFlag,
    LossEventDto? LossEvent,
    List<ClaimPartyDto> Parties,
    List<ClaimRiskObjectDto> RiskObjects,
    List<ReserveComponentDto> ReserveComponents,
    decimal TotalReserves
);

public record LossEventDto(
    Guid Id,
    DateTimeOffset LossDate,
    string LossDescription,
    string? LossLocation,
    string CauseOfLossCode,
    decimal? EstimatedLossAmount,
    string? PoliceReportNumber
);

public record ClaimPartyDto(
    Guid Id,
    string PartyRole,
    string PartyType,
    string? FirstName,
    string? LastName,
    string? CompanyName,
    string? Email,
    string? Phone,
    string? Notes,
    bool IsActive
);

public record ClaimRiskObjectDto(
    Guid Id,
    string AssetType,
    string AssetDescription,
    string? DamageDescription,
    bool IsPrimary,
    string? AssetReference
);

public record ReserveComponentDto(
    Guid Id,
    string Component,
    decimal CurrentAmount,
    string Status,
    string? Notes,
    List<ReserveTransactionDto> History
);

public record ReserveTransactionDto(
    Guid Id,
    string TransactionType,
    decimal Amount,
    decimal PreviousBalance,
    decimal NewBalance,
    string ApprovalStatus,
    string PostingStatus,
    string ChangeReason,
    string? RejectionReason,
    Guid? SubmittedByUserId,
    Guid? ApprovedByUserId,
    DateTimeOffset? ApprovedAt,
    Guid? RejectedByUserId,
    DateTimeOffset? RejectedAt,
    string IdempotencyKey,
    int ChangeSequence,
    DateTimeOffset CreatedAt
);
