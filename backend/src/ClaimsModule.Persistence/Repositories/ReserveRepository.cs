using ClaimsModule.Application.Interfaces;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Repositories;

public class ReserveRepository(ClaimsDbContext db) : IReserveRepository
{
    public async Task<ClaimReserveComponent?> GetComponentByIdAsync(Guid componentId, CancellationToken cancellationToken = default) =>
        await db.ClaimReserveComponents
            .Include(rc => rc.History)
            .FirstOrDefaultAsync(rc => rc.Id == componentId, cancellationToken);

    public async Task<ReserveHistory?> GetHistoryByIdAsync(Guid historyId, CancellationToken cancellationToken = default) =>
        await db.ReserveHistories
            .Include(h => h.ReserveComponent)
            .FirstOrDefaultAsync(h => h.Id == historyId, cancellationToken);

    public async Task<IEnumerable<ClaimReserveComponent>> GetComponentsByClaimIdAsync(Guid claimId, CancellationToken cancellationToken = default) =>
        await db.ClaimReserveComponents
            .Include(rc => rc.History.OrderBy(h => h.ChangeSequence))
            .Where(rc => rc.ClaimId == claimId)
            .ToListAsync(cancellationToken);

    public async Task<int> GetNextChangeSequenceAsync(Guid componentId, CancellationToken cancellationToken = default)
    {
        var max = await db.ReserveHistories
            .Where(h => h.ReserveComponentId == componentId)
            .MaxAsync(h => (int?)h.ChangeSequence, cancellationToken);
        return (max ?? 0) + 1;
    }

    public async Task AddComponentAsync(ClaimReserveComponent component, CancellationToken cancellationToken = default) =>
        await db.ClaimReserveComponents.AddAsync(component, cancellationToken);

    public async Task AddHistoryAsync(ReserveHistory history, CancellationToken cancellationToken = default) =>
        await db.ReserveHistories.AddAsync(history, cancellationToken);

    public void UpdateHistory(ReserveHistory history) => db.ReserveHistories.Update(history);

    public async Task<decimal> GetTotalApprovedReservesAsync(Guid claimId, CancellationToken cancellationToken = default) =>
        await db.ClaimReserveComponents
            .Where(rc => rc.ClaimId == claimId)
            .SumAsync(rc => rc.CurrentAmount, cancellationToken);

    public async Task<bool> HasPendingApprovalAsync(Guid claimId, CancellationToken cancellationToken = default) =>
        await db.ReserveHistories
            .AnyAsync(h => h.ClaimId == claimId && h.ApprovalStatus == ApprovalStatus.PendingApproval, cancellationToken);

    public async Task<DateTimeOffset?> GetLastSlaBreachDateAsync(Guid claimId, CancellationToken cancellationToken = default) =>
        await db.ClaimAuditLogs
            .Where(a => a.ClaimId == claimId && a.EventType == Domain.Entities.AuditEventTypes.SlaBreachDetected)
            .MaxAsync(a => (DateTimeOffset?)a.CreatedAt, cancellationToken);
}
