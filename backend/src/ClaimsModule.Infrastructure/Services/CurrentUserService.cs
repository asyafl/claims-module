using ClaimsModule.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ClaimsModule.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly IHttpContextAccessor _http = httpContextAccessor;

    public Guid? UserId
    {
        get
        {
            var claim = _http.HttpContext?.User.FindFirst("sub")
                ?? _http.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return claim is not null && Guid.TryParse(claim.Value, out var id) ? id : null;
        }
    }

    public string? UserName =>
        _http.HttpContext?.User.FindFirst("name")?.Value
        ?? _http.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

    public string? Role =>
        _http.HttpContext?.User.FindFirst("role")?.Value
        ?? _http.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

    public Guid? CorrelationId
    {
        get
        {
            var header = _http.HttpContext?.Request.Headers["X-Correlation-ID"].FirstOrDefault();
            return header is not null && Guid.TryParse(header, out var id) ? id : null;
        }
    }
}
