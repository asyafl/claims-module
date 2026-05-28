using ClaimsModule.Application.Behaviors;
using ClaimsModule.Application.Interfaces;
using ClaimsModule.Domain.Entities;
using MediatR;

namespace ClaimsModule.Application.Features.Reference.Queries;

public record GetCauseOfLossCodesQuery(string? PerilCategory = null) : IQuery<IReadOnlyList<CauseOfLossCode>>;

public class GetCauseOfLossCodesQueryHandler(IReferenceRepository referenceRepository)
    : IRequestHandler<GetCauseOfLossCodesQuery, IReadOnlyList<CauseOfLossCode>>
{
    public async Task<IReadOnlyList<CauseOfLossCode>> Handle(GetCauseOfLossCodesQuery request, CancellationToken cancellationToken)
    {
        var codes = await referenceRepository.GetActiveCauseOfLossCodesAsync(request.PerilCategory, cancellationToken);
        return codes.ToList().AsReadOnly();
    }
}
