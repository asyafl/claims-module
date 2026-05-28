namespace ClaimsModule.Application.Interfaces;

public interface IClaimNumberGenerator
{
    Task<string> GenerateAsync(Guid organisationId, int year, CancellationToken cancellationToken = default);
}
