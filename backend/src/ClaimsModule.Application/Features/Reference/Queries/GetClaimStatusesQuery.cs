using ClaimsModule.Application.Behaviors;
using ClaimsModule.Domain.Enumerations;
using MediatR;

namespace ClaimsModule.Application.Features.Reference.Queries;

public record ClaimStatusInfo(string Status, string[] ValidNextStatuses);

public record GetClaimStatusesQuery : IQuery<IReadOnlyList<ClaimStatusInfo>>;

public class GetClaimStatusesQueryHandler : IRequestHandler<GetClaimStatusesQuery, IReadOnlyList<ClaimStatusInfo>>
{
    private static readonly Dictionary<ClaimStatus, ClaimStatus[]> Transitions = new()
    {
        [ClaimStatus.Draft] = [ClaimStatus.Open, ClaimStatus.Withdrawn],
        [ClaimStatus.Open] = [ClaimStatus.UnderInvestigation, ClaimStatus.PendingPayment, ClaimStatus.Closed, ClaimStatus.Withdrawn],
        [ClaimStatus.UnderInvestigation] = [ClaimStatus.Open, ClaimStatus.PendingPayment, ClaimStatus.Closed, ClaimStatus.Withdrawn],
        [ClaimStatus.PendingPayment] = [ClaimStatus.Closed],
        [ClaimStatus.Closed] = [ClaimStatus.Reopened],
        [ClaimStatus.Reopened] = [ClaimStatus.Open],
        [ClaimStatus.Withdrawn] = []
    };

    public Task<IReadOnlyList<ClaimStatusInfo>> Handle(GetClaimStatusesQuery request, CancellationToken cancellationToken)
    {
        var result = Transitions
            .Select(kv => new ClaimStatusInfo(kv.Key.ToString(), kv.Value.Select(s => s.ToString()).ToArray()))
            .ToList();

        return Task.FromResult<IReadOnlyList<ClaimStatusInfo>>(result);
    }
}
