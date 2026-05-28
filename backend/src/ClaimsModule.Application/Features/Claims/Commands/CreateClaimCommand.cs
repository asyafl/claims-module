using ClaimsModule.Application.Behaviors;
using ClaimsModule.Application.DTOs;
using ClaimsModule.Application.Interfaces;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Domain.Exceptions;
using FluentValidation;
using MediatR;
namespace ClaimsModule.Application.Features.Claims.Commands;

public record CreateClaimCommand(
    Guid? PolicyId,
    DateTimeOffset LossDate,
    string LossDescription,
    string? LossLocation,
    string CauseOfLossCode,
    decimal? EstimatedLossAmount,
    string? PoliceReportNumber,
    List<CreateClaimPartyRequest> Parties,
    List<CreateClaimRiskObjectRequest> RiskObjects,
    CreateInitialReserveRequest? InitialReserve
) : ICommand<CreateClaimResult>;

public record CreateClaimPartyRequest(
    string PartyRole,
    string PartyType,
    string? FirstName,
    string? LastName,
    string? CompanyName,
    string? Email,
    string? Phone,
    string? Notes
);

public record CreateClaimRiskObjectRequest(
    string AssetType,
    string AssetDescription,
    string? DamageDescription,
    bool IsPrimary,
    string? AssetReference
);

public record CreateInitialReserveRequest(
    string Component,
    decimal Amount,
    string ChangeReason
);

public record CreateClaimResult(Guid ClaimId, string ClaimNumber, List<string> Warnings);

public class CreateClaimCommandHandler(
    IClaimRepository claimRepository,
    IReserveRepository reserveRepository,
    IPolicyRepository policyRepository,
    IReferenceRepository referenceRepository,
    IAuditLogService auditLogService,
    IClaimNumberGenerator claimNumberGenerator,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateClaimCommand, CreateClaimResult>
{
    private static readonly Guid OrgId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public async Task<CreateClaimResult> Handle(CreateClaimCommand request, CancellationToken cancellationToken)
    {
        var warnings = new List<string>();
        var year = DateTimeOffset.UtcNow.Year;
        var claimNumber = await claimNumberGenerator.GenerateAsync(OrgId, year, cancellationToken);

        var claim = Claim.Create(claimNumber, OrgId, currentUser.UserId);
        claim.OrganisationId = OrgId;

        // Policy linkage
        if (request.PolicyId.HasValue)
        {
            var policy = await policyRepository.GetByIdAsync(request.PolicyId.Value, cancellationToken);
            if (policy is not null)
            {
                claim.PolicyId = policy.Id;
                claim.PolicyNumber = policy.PolicyNumber;
                claim.ClientName = policy.ClientName;

                // BR-C-02: loss date must be within policy effective dates
                var lossDateOnly = DateOnly.FromDateTime(request.LossDate.DateTime);
                if (!policy.IsInForce(lossDateOnly))
                    warnings.Add("Loss date is outside the policy effective period.");
            }
        }
        else
        {
            // BR-C-06: no policy linked
            warnings.Add("No policy linked — claim requires policy association before financial actions are permitted.");
        }

        // Loss Event
        claim.LossEvent = new LossEvent
        {
            ClaimId = claim.Id,
            LossDate = request.LossDate,
            LossDescription = request.LossDescription,
            LossLocation = request.LossLocation,
            CauseOfLossCode = request.CauseOfLossCode,
            EstimatedLossAmount = request.EstimatedLossAmount,
            ReportDate = DateTimeOffset.UtcNow,
            PoliceReportNumber = request.PoliceReportNumber,
            OrganisationId = OrgId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Parties
        foreach (var p in request.Parties)
        {
            if (!Enum.TryParse<PartyRole>(p.PartyRole, out var role))
                throw new DomainException($"Invalid party role: {p.PartyRole}");
            if (!Enum.TryParse<PartyType>(p.PartyType, out var pType))
                throw new DomainException($"Invalid party type: {p.PartyType}");

            claim.AddParty(new ClaimParty
            {
                PartyRole = role,
                PartyType = pType,
                FirstName = p.FirstName,
                LastName = p.LastName,
                CompanyName = p.CompanyName,
                Email = p.Email,
                Phone = p.Phone,
                Notes = p.Notes,
                OrganisationId = OrgId,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        // Risk Objects
        foreach (var r in request.RiskObjects)
        {
            if (!Enum.TryParse<AssetType>(r.AssetType, out var assetType))
                throw new DomainException($"Invalid asset type: {r.AssetType}");

            claim.AddRiskObject(new ClaimRiskObject
            {
                AssetType = assetType,
                AssetDescription = r.AssetDescription,
                DamageDescription = r.DamageDescription,
                IsPrimary = r.IsPrimary,
                AssetReference = r.AssetReference,
                OrganisationId = OrgId,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await claimRepository.AddAsync(claim, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Initial reserve (optional)
        if (request.InitialReserve is not null)
        {
            if (!Enum.TryParse<ReserveComponentType>(request.InitialReserve.Component, out var compType))
                throw new DomainException($"Invalid reserve component type: {request.InitialReserve.Component}");

            var component = new ClaimReserveComponent
            {
                ClaimId = claim.Id,
                Component = compType,
                CurrentAmount = 0,
                OrganisationId = OrgId,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await reserveRepository.AddComponentAsync(component, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var amount = request.InitialReserve.Amount;
            var approvalStatus = amount <= 10000m ? ApprovalStatus.AutoApproved : ApprovalStatus.PendingApproval;
            var seq = await reserveRepository.GetNextChangeSequenceAsync(component.Id, cancellationToken);
            var idempKey = $"Reserve:{component.Id}:Change:{seq}";

            var history = new ReserveHistory
            {
                ReserveComponentId = component.Id,
                ClaimId = claim.Id,
                TransactionType = TransactionType.Add,
                Amount = amount,
                PreviousBalance = 0,
                NewBalance = amount,
                ApprovalStatus = approvalStatus,
                ChangeReason = request.InitialReserve.ChangeReason,
                PostingStatus = approvalStatus == ApprovalStatus.AutoApproved ? PostingStatus.Pending : PostingStatus.Cancelled,
                IdempotencyKey = idempKey,
                ChangeSequence = seq,
                SubmittedByUserId = currentUser.UserId,
                OrganisationId = OrgId,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await reserveRepository.AddHistoryAsync(history, cancellationToken);

            if (approvalStatus == ApprovalStatus.AutoApproved)
                component.CurrentAmount = amount;

            await unitOfWork.SaveChangesAsync(cancellationToken);

            var eventType = approvalStatus == ApprovalStatus.AutoApproved
                ? Domain.Entities.AuditEventTypes.ReserveAutoApproved
                : Domain.Entities.AuditEventTypes.ReserveCreated;

            await auditLogService.LogAsync(claim.Id, eventType,
                $"Initial reserve {compType} of {amount:C} created with status {approvalStatus}",
                currentUser.UserId, relatedEntityId: history.Id, relatedEntityType: "ReserveHistory",
                correlationId: currentUser.CorrelationId, cancellationToken: cancellationToken);
        }

        await auditLogService.LogAsync(
            claim.Id, Domain.Entities.AuditEventTypes.ClaimCreated,
            $"Claim {claimNumber} created",
            currentUser.UserId, correlationId: currentUser.CorrelationId,
            cancellationToken: cancellationToken);

        // Log warnings as validation issues
        foreach (var warning in warnings)
            await auditLogService.LogAsync(claim.Id, AuditEventTypes.ValidationIssueAdded,
                $"Warning: {warning}", currentUser.UserId, cancellationToken: cancellationToken);

        return new CreateClaimResult(claim.Id, claimNumber, warnings);
    }
}

public class CreateClaimCommandValidator : AbstractValidator<CreateClaimCommand>
{
    public CreateClaimCommandValidator(IReferenceRepository referenceRepository)
    {
        RuleFor(x => x.LossDate)
            .NotEmpty().WithMessage("Loss date is required.")
            .Must(d => d <= DateTimeOffset.UtcNow).WithMessage("Loss date cannot be in the future.");

        RuleFor(x => x.LossDescription)
            .NotEmpty().WithMessage("Loss description is required and must be at least 20 characters.")
            .MinimumLength(20).WithMessage("Loss description is required and must be at least 20 characters.");

        RuleFor(x => x.CauseOfLossCode)
            .NotEmpty()
            .MustAsync(async (code, ct) =>
            {
                var orgId = Guid.Parse("00000000-0000-0000-0000-000000000001");
                var found = await referenceRepository.GetCauseOfLossCodeAsync(code, orgId, ct);
                return found is not null;
            }).WithMessage("Cause of loss code is not recognised or is inactive.");

        RuleFor(x => x.Parties)
            .NotNull();

        RuleFor(x => x.InitialReserve!.Amount)
            .GreaterThan(0).WithMessage("Reserve amount must be greater than zero.")
            .When(x => x.InitialReserve is not null && x.InitialReserve.Component != "SubrogationRecoverable");
    }
}
