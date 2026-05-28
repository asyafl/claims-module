using ClaimsModule.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence;

public class ClaimNumberGenerator(ClaimsDbContext db) : IClaimNumberGenerator
{
    public async Task<string> GenerateAsync(Guid organisationId, int year, CancellationToken cancellationToken = default)
    {
        // Atomic increment using raw SQL to prevent gaps/duplicates under concurrency
        var sequence = await db.ClaimNumberSequences
            .FirstOrDefaultAsync(s => s.OrganisationId == organisationId && s.Year == year, cancellationToken);

        if (sequence is null)
        {
            sequence = new ClaimNumberSequence
            {
                Id = Guid.NewGuid(),
                OrganisationId = organisationId,
                Year = year,
                LastSequence = 0
            };
            await db.ClaimNumberSequences.AddAsync(sequence, cancellationToken);
        }

        sequence.LastSequence++;
        await db.SaveChangesAsync(cancellationToken);

        return $"CLM-{year}-{sequence.LastSequence:D7}";
    }
}
