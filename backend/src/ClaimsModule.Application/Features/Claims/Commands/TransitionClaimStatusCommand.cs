using ClaimsModule.Application.Behaviors;
using ClaimsModule.Application.Interfaces;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.Features.Claims.Commands;

public record TransitionClaimStatusCommand(
    Guid ClaimId,
    string TargetStatus,
    string? Reason
) : ICommand<Unit>;

public class TransitionClaimStatusCommandHandler(
    IClaimRepository claimRepository,
    IReserveRepository reserveRepository,
    IAuditLogService auditLogService,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<TransitionClaimStatusCommand, Unit>
{
    public async Task<Unit> Handle(TransitionClaimStatusCommand request, CancellationToken cancellationToken)
    {
        var claim = await claimRepository.GetByIdWithDetailsAsync(request.ClaimId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Claim), request.ClaimId);

        if (!Enum.TryParse<ClaimStatus>(request.TargetStatus, out var targetStatus))
            throw new DomainException($"Invalid claim status: {request.TargetStatus}");

        // BR-ST-02: Open requires at least one Claimant
        if (targetStatus == ClaimStatus.Open && !claim.HasActiveClaimant())
            throw new DomainException("At least one Claimant party is required to open a claim.");

        // BR-ST-03: Closed requires no pending reserves
        if (targetStatus == ClaimStatus.Closed)
        {
            var hasPending = await reserveRepository.HasPendingApprovalAsync(claim.Id, cancellationToken);
            if (hasPending)
                throw new DomainException("CC-01: Claim cannot be closed — pending approval reserves exist.");
        }

        // BR-ST-04: Reopened requires supervisor role and reason
        if (targetStatus == ClaimStatus.Reopened)
        {
            if (currentUser.Role != "supervisor" && currentUser.Role != "manager")
                throw new DomainException("Reopening a claim requires Supervisor role.");
            if (string.IsNullOrWhiteSpace(request.Reason))
                throw new DomainException("A reopen reason is required.");
        }

        var previousStatus = claim.Status.ToString();
        claim.TransitionTo(targetStatus, currentUser.UserId ?? Guid.Empty, request.Reason);
        claimRepository.Update(claim);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var eventType = targetStatus == ClaimStatus.Closed
            ? Domain.Entities.AuditEventTypes.ClaimClosed
            : targetStatus == ClaimStatus.Reopened
                ? Domain.Entities.AuditEventTypes.ClaimReopened
                : Domain.Entities.AuditEventTypes.StatusChanged;

        await auditLogService.LogAsync(claim.Id, eventType,
            $"Status changed from {previousStatus} to {targetStatus}",
            currentUser.UserId, oldValue: previousStatus, newValue: targetStatus.ToString(),
            correlationId: currentUser.CorrelationId, cancellationToken: cancellationToken);

        // If Reopened → immediately transition to Open
        if (targetStatus == ClaimStatus.Reopened)
        {
            claim.TransitionTo(ClaimStatus.Open, currentUser.UserId ?? Guid.Empty);
            claimRepository.Update(claim);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await auditLogService.LogAsync(claim.Id, Domain.Entities.AuditEventTypes.StatusChanged,
                "Status automatically transitioned from Reopened to Open",
                currentUser.UserId, cancellationToken: cancellationToken);
        }

        return Unit.Value;
    }
}

public class TransitionClaimStatusCommandValidator : AbstractValidator<TransitionClaimStatusCommand>
{
    public TransitionClaimStatusCommandValidator()
    {
        RuleFor(x => x.ClaimId).NotEmpty();
        RuleFor(x => x.TargetStatus)
            .NotEmpty()
            .Must(s => Enum.TryParse<ClaimStatus>(s, out _))
            .WithMessage(x => $"'{x.TargetStatus}' is not a valid claim status.");
    }
}
