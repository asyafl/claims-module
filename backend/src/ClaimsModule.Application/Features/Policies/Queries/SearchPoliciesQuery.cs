using AutoMapper;
using ClaimsModule.Application.Behaviors;
using ClaimsModule.Application.DTOs;
using ClaimsModule.Application.Interfaces;
using MediatR;

namespace ClaimsModule.Application.Features.Policies.Queries;

public record SearchPoliciesQuery(string Query) : IQuery<IReadOnlyList<PolicyDto>>;

public class SearchPoliciesQueryHandler(
    IPolicyRepository policyRepository,
    IMapper mapper) : IRequestHandler<SearchPoliciesQuery, IReadOnlyList<PolicyDto>>
{
    public async Task<IReadOnlyList<PolicyDto>> Handle(SearchPoliciesQuery request, CancellationToken cancellationToken)
    {
        var policies = await policyRepository.SearchAsync(request.Query, cancellationToken);
        return mapper.Map<List<PolicyDto>>(policies);
    }
}
