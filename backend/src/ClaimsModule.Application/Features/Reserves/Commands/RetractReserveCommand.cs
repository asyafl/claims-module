using ClaimsModule.Application.Behaviors;
using ClaimsModule.Application.Interfaces;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Domain.Exceptions;
using MediatR;

namespace ClaimsModule.Application.Features.Reserves.Commands;

public record RetractReserveCommand(Guid ClaimId, Guid HistoryId) : ICommand<Unit>;

public class RetractReserveCommandHandler(
    IReserveRepository reserveRepository,
    IAuditLogService auditLogService,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<RetractReserveCommand, Unit>
{
    public async Task<Unit> Handle(RetractReserveCommand request, CancellationToken cancellationToken)
    {
        var history = await reserveRepository.GetHistoryByIdAsync(request.HistoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.ReserveHistory), request.HistoryId);

        if (history.ClaimId != request.ClaimId)
            throw new DomainException("Reserve does not belong to the specified claim.");

        if (history.ApprovalStatus != ApprovalStatus.PendingApproval)
            throw new DomainException("Only PendingApproval reserves can be retracted.");

        if (history.SubmittedByUserId != currentUser.UserId)
            throw new DomainException("Only the submitter can retract a reserve.");

        history.ApprovalStatus = ApprovalStatus.Cancelled;
        history.PostingStatus = PostingStatus.Cancelled;

        reserveRepository.UpdateHistory(history);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(history.ClaimId, Domain.Entities.AuditEventTypes.ReserveRetracted,
            $"Reserve {history.Amount:C} retracted by submitter",
            currentUser.UserId, relatedEntityId: history.Id, relatedEntityType: "ReserveHistory",
            correlationId: currentUser.CorrelationId, cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
