using ClaimsModule.Application.Interfaces;
using ClaimsModule.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Repositories;

public class ReferenceRepository(ClaimsDbContext db) : IReferenceRepository
{
    public async Task<IEnumerable<CauseOfLossCode>> GetActiveCauseOfLossCodesAsync(string? perilCategory = null, CancellationToken cancellationToken = default)
    {
        var query = db.CauseOfLossCodes.Where(c => c.IsActive);
        if (!string.IsNullOrWhiteSpace(perilCategory))
            query = query.Where(c => c.PerilCategory == perilCategory);
        return await query.OrderBy(c => c.SortOrder).ToListAsync(cancellationToken);
    }

    public async Task<CauseOfLossCode?> GetCauseOfLossCodeAsync(string code, Guid organisationId, CancellationToken cancellationToken = default) =>
        await db.CauseOfLossCodes
            .FirstOrDefaultAsync(c => c.Code == code && c.IsActive, cancellationToken);
}
