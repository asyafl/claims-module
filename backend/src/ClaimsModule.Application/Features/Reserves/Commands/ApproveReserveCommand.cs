using ClaimsModule.Application.Behaviors;
using ClaimsModule.Application.Interfaces;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Domain.Exceptions;
using MediatR;

namespace ClaimsModule.Application.Features.Reserves.Commands;

public record ApproveReserveCommand(Guid ClaimId, Guid HistoryId) : ICommand<Unit>;

public class ApproveReserveCommandHandler(
    IReserveRepository reserveRepository,
    IAuditLogService auditLogService,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<ApproveReserveCommand, Unit>
{
    private const decimal SupervisorThreshold = 100_000m;

    public async Task<Unit> Handle(ApproveReserveCommand request, CancellationToken cancellationToken)
    {
        var history = await reserveRepository.GetHistoryByIdAsync(request.HistoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.ReserveHistory), request.HistoryId);

        if (history.ClaimId != request.ClaimId)
            throw new DomainException("Reserve does not belong to the specified claim.");

        if (history.ApprovalStatus != ApprovalStatus.PendingApproval)
            throw new DomainException($"Reserve is not in PendingApproval status (current: {history.ApprovalStatus}).");

        // BR-R-03: Self-approval not permitted
        if (history.SubmittedByUserId == currentUser.UserId)
            throw new DomainException("Self-approval is not permitted.");

        // Authority validation
        var role = currentUser.Role;
        if (role != "supervisor" && role != "manager")
            throw new DomainException("Your role does not have authority to approve reserves.");

        var absAmount = Math.Abs(history.Amount);
        if (absAmount > SupervisorThreshold && role != "manager")
            throw new DomainException("Reserves above $100,000 require Manager approval.");

        history.ApprovalStatus = ApprovalStatus.Approved;
        history.ApprovedByUserId = currentUser.UserId;
        history.ApprovedAt = DateTimeOffset.UtcNow;
        history.PostingStatus = PostingStatus.Pending;
        history.NewBalance = history.PreviousBalance + history.Amount;

        // Update component current amount
        var component = history.ReserveComponent;
        component.CurrentAmount = history.NewBalance;

        reserveRepository.UpdateHistory(history);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(history.ClaimId, Domain.Entities.AuditEventTypes.ReserveApproved,
            $"Reserve {history.Amount:C} approved by {currentUser.UserName}",
            currentUser.UserId, relatedEntityId: history.Id, relatedEntityType: "ReserveHistory",
            correlationId: currentUser.CorrelationId, cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
