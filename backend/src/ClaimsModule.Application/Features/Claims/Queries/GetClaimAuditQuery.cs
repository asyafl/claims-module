using AutoMapper;
using ClaimsModule.Application.Behaviors;
using ClaimsModule.Application.DTOs;
using ClaimsModule.Application.Interfaces;
using MediatR;

namespace ClaimsModule.Application.Features.Claims.Queries;

public record GetClaimAuditQuery(Guid ClaimId, int Page = 1, int PageSize = 50) : IQuery<PagedResult<AuditLogEntryDto>>;

public class GetClaimAuditQueryHandler(
    IAuditLogService auditLogService,
    IMapper mapper) : IRequestHandler<GetClaimAuditQuery, PagedResult<AuditLogEntryDto>>
{
    public async Task<PagedResult<AuditLogEntryDto>> Handle(GetClaimAuditQuery request, CancellationToken cancellationToken)
    {
        var logs = await auditLogService.GetByClaimIdAsync(request.ClaimId, request.Page, request.PageSize, cancellationToken);
        var dtos = mapper.Map<List<AuditLogEntryDto>>(logs);
        return new PagedResult<AuditLogEntryDto>(dtos, dtos.Count, request.Page, request.PageSize);
    }
}
