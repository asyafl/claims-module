using ClaimsModule.Application.Behaviors;
using ClaimsModule.Application.Interfaces;
using ClaimsModule.Domain.Exceptions;
using MediatR;

namespace ClaimsModule.Application.Features.Claims.Commands;

public record RemoveClaimPartyCommand(Guid ClaimId, Guid PartyId) : ICommand<Unit>;

public class RemoveClaimPartyCommandHandler(
    IClaimRepository claimRepository,
    IAuditLogService auditLogService,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<RemoveClaimPartyCommand, Unit>
{
    public async Task<Unit> Handle(RemoveClaimPartyCommand request, CancellationToken cancellationToken)
    {
        var claim = await claimRepository.GetByIdWithDetailsAsync(request.ClaimId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Claim), request.ClaimId);

        var party = claim.RemoveParty(request.PartyId);
        claimRepository.Update(claim);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(claim.Id, Domain.Entities.AuditEventTypes.PartyRemoved,
            $"Party {party.PartyRole} '{party.DisplayName}' removed",
            currentUser.UserId, relatedEntityId: party.Id, relatedEntityType: "ClaimParty",
            correlationId: currentUser.CorrelationId, cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
