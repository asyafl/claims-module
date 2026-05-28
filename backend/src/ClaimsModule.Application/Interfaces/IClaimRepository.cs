using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Application.Interfaces;

public interface IClaimRepository
{
    Task<Claim?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Claim?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Claim> Items, int TotalCount)> ListAsync(
        ClaimStatus? status,
        DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo,
        Guid? assignedHandlerId,
        string? causeOfLossCode,
        Guid? policyId,
        string? search,
        int page,
        int pageSize,
        Guid organisationId,
        CancellationToken cancellationToken = default);
    Task<bool> ClaimNumberExistsAsync(string claimNumber, Guid organisationId, CancellationToken cancellationToken = default);
    Task AddAsync(Claim claim, CancellationToken cancellationToken = default);
    void Update(Claim claim);
    Task<IEnumerable<Claim>> GetStaleDraftOrOpenClaimsAsync(DateTimeOffset threshold, Guid organisationId, CancellationToken cancellationToken = default);
}
