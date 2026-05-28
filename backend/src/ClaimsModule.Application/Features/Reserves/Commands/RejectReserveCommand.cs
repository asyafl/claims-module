using ClaimsModule.Application.Behaviors;
using ClaimsModule.Application.Interfaces;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.Features.Reserves.Commands;

public record RejectReserveCommand(Guid ClaimId, Guid HistoryId, string RejectionReason) : ICommand<Unit>;

public class RejectReserveCommandHandler(
    IReserveRepository reserveRepository,
    IAuditLogService auditLogService,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<RejectReserveCommand, Unit>
{
    public async Task<Unit> Handle(RejectReserveCommand request, CancellationToken cancellationToken)
    {
        var history = await reserveRepository.GetHistoryByIdAsync(request.HistoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.ReserveHistory), request.HistoryId);

        if (history.ClaimId != request.ClaimId)
            throw new DomainException("Reserve does not belong to the specified claim.");

        if (history.ApprovalStatus != ApprovalStatus.PendingApproval)
            throw new DomainException($"Reserve is not in PendingApproval status (current: {history.ApprovalStatus}).");

        var role = currentUser.Role;
        if (role != "supervisor" && role != "manager")
            throw new DomainException("Your role does not have authority to reject reserves.");

        history.ApprovalStatus = ApprovalStatus.Rejected;
        history.RejectedByUserId = currentUser.UserId;
        history.RejectedAt = DateTimeOffset.UtcNow;
        history.RejectionReason = request.RejectionReason;
        history.PostingStatus = PostingStatus.Cancelled;

        reserveRepository.UpdateHistory(history);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(history.ClaimId, Domain.Entities.AuditEventTypes.ReserveRejected,
            $"Reserve {history.Amount:C} rejected: {request.RejectionReason}",
            currentUser.UserId, newValue: request.RejectionReason,
            relatedEntityId: history.Id, relatedEntityType: "ReserveHistory",
            correlationId: currentUser.CorrelationId, cancellationToken: cancellationToken);

        return Unit.Value;
    }
}

public class RejectReserveCommandValidator : AbstractValidator<RejectReserveCommand>
{
    public RejectReserveCommandValidator()
    {
        RuleFor(x => x.ClaimId).NotEmpty();
        RuleFor(x => x.HistoryId).NotEmpty();
        RuleFor(x => x.RejectionReason).NotEmpty().MaximumLength(500);
    }
}
