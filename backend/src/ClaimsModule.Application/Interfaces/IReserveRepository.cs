using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Application.Interfaces;

public interface IReserveRepository
{
    Task<ClaimReserveComponent?> GetComponentByIdAsync(Guid componentId, CancellationToken cancellationToken = default);
    Task<ReserveHistory?> GetHistoryByIdAsync(Guid historyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ClaimReserveComponent>> GetComponentsByClaimIdAsync(Guid claimId, CancellationToken cancellationToken = default);
    Task<int> GetNextChangeSequenceAsync(Guid componentId, CancellationToken cancellationToken = default);
    Task AddComponentAsync(ClaimReserveComponent component, CancellationToken cancellationToken = default);
    Task AddHistoryAsync(ReserveHistory history, CancellationToken cancellationToken = default);
    void UpdateHistory(ReserveHistory history);
    Task<decimal> GetTotalApprovedReservesAsync(Guid claimId, CancellationToken cancellationToken = default);
    Task<bool> HasPendingApprovalAsync(Guid claimId, CancellationToken cancellationToken = default);
    Task<DateTimeOffset?> GetLastSlaBreachDateAsync(Guid claimId, CancellationToken cancellationToken = default);
}
