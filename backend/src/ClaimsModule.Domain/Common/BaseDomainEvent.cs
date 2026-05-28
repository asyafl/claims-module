using ClaimsModule.Domain.Interfaces;

namespace ClaimsModule.Domain.Common;

public abstract class BaseDomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
