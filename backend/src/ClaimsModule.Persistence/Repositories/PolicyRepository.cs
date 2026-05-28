using ClaimsModule.Application.Interfaces;
using ClaimsModule.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Repositories;

public class PolicyRepository(ClaimsDbContext db) : IPolicyRepository
{
    public async Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await db.Policies.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IEnumerable<Policy>> SearchAsync(string query, CancellationToken cancellationToken = default) =>
        await db.Policies
            .Where(p => p.PolicyNumber.Contains(query) || p.ClientName.Contains(query))
            .OrderBy(p => p.PolicyNumber)
            .Take(20)
            .ToListAsync(cancellationToken);
}
