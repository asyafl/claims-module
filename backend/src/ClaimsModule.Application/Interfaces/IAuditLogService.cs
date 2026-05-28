using ClaimsModule.Domain.Entities;

namespace ClaimsModule.Application.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(
        Guid claimId,
        string eventType,
        string description,
        Guid? userId = null,
        string? oldValue = null,
        string? newValue = null,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null,
        Guid? correlationId = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ClaimAuditLog>> GetByClaimIdAsync(
        Guid claimId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
