using ClaimsModule.Application.Behaviors;
using ClaimsModule.Application.Interfaces;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.Features.Reserves.Commands;

public record CreateReserveCommand(
    Guid ClaimId,
    string Component,
    decimal Amount,
    string ChangeReason,
    string TransactionType = "Add"
) : ICommand<CreateReserveResult>;

public record CreateReserveResult(
    Guid HistoryId,
    string ApprovalStatus,
    string PostingStatus,
    bool RequiresApproval
);

public class CreateReserveCommandHandler(
    IClaimRepository claimRepository,
    IReserveRepository reserveRepository,
    IAuditLogService auditLogService,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateReserveCommand, CreateReserveResult>
{
    private const decimal SupervisorThreshold = 10_000m;
    private const decimal ManagerThreshold = 100_000m;
    private const decimal MaxTotalReserves = 10_000_000m;
    private static readonly Guid OrgId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public async Task<CreateReserveResult> Handle(CreateReserveCommand request, CancellationToken cancellationToken)
    {
        var claim = await claimRepository.GetByIdAsync(request.ClaimId, cancellationToken)
            ?? throw new NotFoundException(nameof(Claim), request.ClaimId);

        // BR-C-06: Reserves blocked without policy
        if (!claim.PolicyId.HasValue)
            throw new DomainException("Reserve creation is blocked until a policy is linked to the claim.");

        if (!Enum.TryParse<ReserveComponentType>(request.Component, out var compType))
            throw new DomainException($"Invalid reserve component type: {request.Component}");

        if (!Enum.TryParse<TransactionType>(request.TransactionType, out var txnType))
            throw new DomainException($"Invalid transaction type: {request.TransactionType}");

        // BR-R-05: Total approved reserves check
        var totalApproved = await reserveRepository.GetTotalApprovedReservesAsync(claim.Id, cancellationToken);
        if (totalApproved + request.Amount > MaxTotalReserves && !claim.ManagerOverrideFlag)
            throw new DomainException("BR-R-05: Total reserves will exceed $10,000,000. Manager override required.");

        // Find or create reserve component
        var components = await reserveRepository.GetComponentsByClaimIdAsync(claim.Id, cancellationToken);
        var component = components.FirstOrDefault(c => c.Component == compType);
        if (component is null)
        {
            component = new ClaimReserveComponent
            {
                ClaimId = claim.Id,
                Component = compType,
                CurrentAmount = 0,
                OrganisationId = OrgId,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await reserveRepository.AddComponentAsync(component, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // Determine authority level
        var absAmount = Math.Abs(request.Amount);
        ApprovalStatus approvalStatus;
        if (absAmount <= SupervisorThreshold)
            approvalStatus = ApprovalStatus.AutoApproved;
        else if (absAmount <= ManagerThreshold)
            approvalStatus = ApprovalStatus.PendingApproval;
        else
            approvalStatus = ApprovalStatus.PendingApproval;

        var previousBalance = component.CurrentAmount;
        var seq = await reserveRepository.GetNextChangeSequenceAsync(component.Id, cancellationToken);
        var idempKey = $"Reserve:{component.Id}:Change:{seq}";

        var history = new ReserveHistory
        {
            ReserveComponentId = component.Id,
            ClaimId = claim.Id,
            TransactionType = txnType,
            Amount = request.Amount,
            PreviousBalance = previousBalance,
            NewBalance = approvalStatus == ApprovalStatus.AutoApproved ? previousBalance + request.Amount : previousBalance,
            ApprovalStatus = approvalStatus,
            ChangeReason = request.ChangeReason,
            PostingStatus = approvalStatus == ApprovalStatus.AutoApproved ? PostingStatus.Pending : PostingStatus.Cancelled,
            IdempotencyKey = idempKey,
            ChangeSequence = seq,
            SubmittedByUserId = currentUser.UserId,
            OrganisationId = OrgId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await reserveRepository.AddHistoryAsync(history, cancellationToken);

        if (approvalStatus == ApprovalStatus.AutoApproved)
            component.CurrentAmount = previousBalance + request.Amount;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var eventType = approvalStatus == ApprovalStatus.AutoApproved
            ? Domain.Entities.AuditEventTypes.ReserveAutoApproved
            : Domain.Entities.AuditEventTypes.ReserveCreated;

        await auditLogService.LogAsync(claim.Id, eventType,
            $"Reserve {compType} {request.Amount:C} submitted, status: {approvalStatus}",
            currentUser.UserId, relatedEntityId: history.Id, relatedEntityType: "ReserveHistory",
            correlationId: currentUser.CorrelationId, cancellationToken: cancellationToken);

        return new CreateReserveResult(
            history.Id,
            approvalStatus.ToString(),
            history.PostingStatus.ToString(),
            approvalStatus == ApprovalStatus.PendingApproval
        );
    }
}

public class CreateReserveCommandValidator : AbstractValidator<CreateReserveCommand>
{
    public CreateReserveCommandValidator()
    {
        RuleFor(x => x.ClaimId).NotEmpty();

        RuleFor(x => x.Component)
            .NotEmpty()
            .Must(c => Enum.TryParse<ReserveComponentType>(c, out _))
            .WithMessage(x => $"Invalid reserve component type: {x.Component}");

        // BR-R-01: amount > 0, except SubrogationRecoverable which may be negative
        RuleFor(x => x.Amount)
            .Must((cmd, amount) =>
                cmd.Component == nameof(ReserveComponentType.SubrogationRecoverable) || amount > 0)
            .WithMessage("Reserve amount must be greater than zero.");

        RuleFor(x => x.ChangeReason).NotEmpty().MaximumLength(500);
    }
}
