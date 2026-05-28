using ClaimsModule.Domain.Entities;

namespace ClaimsModule.Application.Interfaces;

public interface IPolicyRepository
{
    Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Policy>> SearchAsync(string query, CancellationToken cancellationToken = default);
}
