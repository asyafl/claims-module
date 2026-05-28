namespace ClaimsModule.Application.DTOs;

public record PolicyDto(
    Guid Id,
    string PolicyNumber,
    string ClientName,
    DateOnly EffectiveDate,
    DateOnly ExpirationDate,
    string Status,
    string[] CoverageTypes
);
