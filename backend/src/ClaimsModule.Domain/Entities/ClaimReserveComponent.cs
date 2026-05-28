using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Domain.Entities;

public class ClaimReserveComponent : AggregateRoot
{
    public Guid ClaimId { get; set; }
    public ReserveComponentType Component { get; set; }
    public decimal CurrentAmount { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }

    private readonly List<ReserveHistory> _history = [];
    public IReadOnlyCollection<ReserveHistory> History => _history.AsReadOnly();
}
