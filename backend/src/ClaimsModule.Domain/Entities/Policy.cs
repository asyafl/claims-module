using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Entities;

public class Policy : BaseEntity
{
    public string PolicyNumber { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public DateOnly EffectiveDate { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public string Status { get; set; } = "Active";
    public string CoverageTypes { get; set; } = string.Empty;

    public bool IsInForce(DateOnly lossDate) =>
        lossDate >= EffectiveDate && lossDate <= ExpirationDate;
}
