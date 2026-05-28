using ClaimsModule.Domain.Entities;

namespace ClaimsModule.Application.Interfaces;

public interface IReferenceRepository
{
    Task<IEnumerable<CauseOfLossCode>> GetActiveCauseOfLossCodesAsync(string? perilCategory = null, CancellationToken cancellationToken = default);
    Task<CauseOfLossCode?> GetCauseOfLossCodeAsync(string code, Guid organisationId, CancellationToken cancellationToken = default);
}
