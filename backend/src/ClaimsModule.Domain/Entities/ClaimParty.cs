using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Domain.Entities;

public class ClaimParty : BaseEntity
{
    public Guid ClaimId { get; set; }
    public PartyRole PartyRole { get; set; }
    public PartyType PartyType { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CompanyName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public string DisplayName => PartyType == PartyType.Company
        ? CompanyName ?? string.Empty
        : $"{FirstName} {LastName}".Trim();
}
