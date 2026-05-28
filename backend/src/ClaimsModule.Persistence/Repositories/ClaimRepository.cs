using ClaimsModule.Application.Interfaces;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Repositories;

public class ClaimRepository(ClaimsDbContext db) : IClaimRepository
{
    public async Task<Claim?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await db.Claims.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<Claim?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await db.Claims
            .Include(c => c.LossEvent)
            .Include(c => c.Parties)
            .Include(c => c.RiskObjects)
            .Include(c => c.Documents)
            .Include(c => c.ReserveComponents)
                .ThenInclude(rc => rc.History)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<(IEnumerable<Claim> Items, int TotalCount)> ListAsync(
        ClaimStatus? status, DateTimeOffset? dateFrom, DateTimeOffset? dateTo,
        Guid? assignedHandlerId, string? causeOfLossCode, Guid? policyId,
        string? search, int page, int pageSize, Guid organisationId,
        CancellationToken cancellationToken = default)
    {
        var query = db.Claims
            .Include(c => c.LossEvent)
            .Include(c => c.ReserveComponents)
            .Where(c => c.OrganisationId == organisationId);

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        if (dateFrom.HasValue)
            query = query.Where(c => c.LossEvent != null && c.LossEvent.LossDate >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(c => c.LossEvent != null && c.LossEvent.LossDate <= dateTo.Value);

        if (assignedHandlerId.HasValue)
            query = query.Where(c => c.AssignedHandlerId == assignedHandlerId.Value);

        if (!string.IsNullOrWhiteSpace(causeOfLossCode))
            query = query.Where(c => c.LossEvent != null && c.LossEvent.CauseOfLossCode == causeOfLossCode);

        if (policyId.HasValue)
            query = query.Where(c => c.PolicyId == policyId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.ClaimNumber.Contains(search) || (c.ClientName != null && c.ClientName.Contains(search)));

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<bool> ClaimNumberExistsAsync(string claimNumber, Guid organisationId, CancellationToken cancellationToken = default) =>
        await db.Claims.IgnoreQueryFilters()
            .AnyAsync(c => c.ClaimNumber == claimNumber && c.OrganisationId == organisationId, cancellationToken);

    public async Task AddAsync(Claim claim, CancellationToken cancellationToken = default) =>
        await db.Claims.AddAsync(claim, cancellationToken);

    public void Update(Claim claim) => db.Claims.Update(claim);

    public async Task<IEnumerable<Claim>> GetStaleDraftOrOpenClaimsAsync(DateTimeOffset threshold, Guid organisationId, CancellationToken cancellationToken = default) =>
        await db.Claims
            .Where(c => c.OrganisationId == organisationId
                && (c.Status == ClaimStatus.Draft || c.Status == ClaimStatus.Open)
                && c.UpdatedAt < threshold)
            .ToListAsync(cancellationToken);
}
