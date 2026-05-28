using AutoMapper;
using ClaimsModule.Application.Behaviors;
using ClaimsModule.Application.DTOs;
using ClaimsModule.Application.Interfaces;
using MediatR;

namespace ClaimsModule.Application.Features.Reserves.Queries;

public record GetClaimReservesQuery(Guid ClaimId) : IQuery<IReadOnlyList<ReserveComponentDto>>;

public class GetClaimReservesQueryHandler(
    IReserveRepository reserveRepository,
    IMapper mapper) : IRequestHandler<GetClaimReservesQuery, IReadOnlyList<ReserveComponentDto>>
{
    public async Task<IReadOnlyList<ReserveComponentDto>> Handle(GetClaimReservesQuery request, CancellationToken cancellationToken)
    {
        var components = await reserveRepository.GetComponentsByClaimIdAsync(request.ClaimId, cancellationToken);
        return mapper.Map<List<ReserveComponentDto>>(components);
    }
}
