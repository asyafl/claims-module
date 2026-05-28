using AutoMapper;
using ClaimsModule.Application.Behaviors;
using ClaimsModule.Application.DTOs;
using ClaimsModule.Application.Interfaces;
using ClaimsModule.Domain.Exceptions;
using MediatR;

namespace ClaimsModule.Application.Features.Claims.Queries;

public record GetClaimDetailQuery(Guid ClaimId) : IQuery<ClaimDetailDto>;

public class GetClaimDetailQueryHandler(
    IClaimRepository claimRepository,
    IMapper mapper) : IRequestHandler<GetClaimDetailQuery, ClaimDetailDto>
{
    public async Task<ClaimDetailDto> Handle(GetClaimDetailQuery request, CancellationToken cancellationToken)
    {
        var claim = await claimRepository.GetByIdWithDetailsAsync(request.ClaimId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Claim), request.ClaimId);

        return mapper.Map<ClaimDetailDto>(claim);
    }
}
