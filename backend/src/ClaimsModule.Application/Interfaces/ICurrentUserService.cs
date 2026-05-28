namespace ClaimsModule.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    string? Role { get; }
    Guid? CorrelationId { get; }
}
