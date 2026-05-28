using AutoMapper;
using ClaimsModule.Application.Behaviors;
using ClaimsModule.Application.DTOs;
using ClaimsModule.Application.Interfaces;
using MediatR;

namespace ClaimsModule.Application.Features.Claims.Queries;

public record ListClaimsQuery(
    string? Status,
    DateTimeOffset? DateFrom,
    DateTimeOffset? DateTo,
    Guid? AssignedHandlerId,
    string? CauseOfLossCode,
    Guid? PolicyId,
    string? Search,
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<ClaimListItemDto>>;

public class ListClaimsQueryHandler(
    IClaimRepository claimRepository,
    IMapper mapper) : IRequestHandler<ListClaimsQuery, PagedResult<ClaimListItemDto>>
{
    private static readonly Guid OrgId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public async Task<PagedResult<ClaimListItemDto>> Handle(ListClaimsQuery request, CancellationToken cancellationToken)
    {
        Domain.Enumerations.ClaimStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<Domain.Enumerations.ClaimStatus>(request.Status, out var parsed))
            status = parsed;

        var (items, total) = await claimRepository.ListAsync(
            status, request.DateFrom, request.DateTo,
            request.AssignedHandlerId, request.CauseOfLossCode, request.PolicyId,
            request.Search, request.Page, request.PageSize, OrgId, cancellationToken);

        return new PagedResult<ClaimListItemDto>(
            mapper.Map<List<ClaimListItemDto>>(items),
            total, request.Page, request.PageSize);
    }
}
