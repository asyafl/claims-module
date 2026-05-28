using ClaimsModule.Application.Interfaces;
using ClaimsModule.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Repositories;

public class AuditLogRepository(ClaimsDbContext db) : IAuditLogService
{
    public async Task LogAsync(
        Guid claimId, string eventType, string description,
        Guid? userId = null, string? oldValue = null, string? newValue = null,
        Guid? relatedEntityId = null, string? relatedEntityType = null,
        Guid? correlationId = null, CancellationToken cancellationToken = default)
    {
        var log = new ClaimAuditLog
        {
            ClaimId = claimId,
            EventType = eventType,
            Description = description,
            OldValue = oldValue,
            NewValue = newValue,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            CorrelationId = correlationId,
            CreatedByUserId = userId,
            CreatedAt = DateTimeOffset.UtcNow
        };
        await db.ClaimAuditLogs.AddAsync(log, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<ClaimAuditLog>> GetByClaimIdAsync(
        Guid claimId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await db.ClaimAuditLogs
            .Where(a => a.ClaimId == claimId)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
}
