namespace ClaimsModule.Application.DTOs;

public record AuditLogEntryDto(
    Guid Id,
    string EventType,
    string Description,
    string? OldValue,
    string? NewValue,
    Guid? RelatedEntityId,
    string? RelatedEntityType,
    Guid? CreatedByUserId,
    DateTimeOffset CreatedAt
);
