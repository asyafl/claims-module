namespace ClaimsModule.Application.DTOs;

public record ClaimListItemDto(
    Guid Id,
    string ClaimNumber,
    string? PolicyNumber,
    string? ClientName,
    DateTimeOffset? LossDate,
    string? CauseOfLossCode,
    string Status,
    decimal TotalReserves,
    DateTimeOffset ReportedDate,
    Guid? AssignedHandlerId
);
