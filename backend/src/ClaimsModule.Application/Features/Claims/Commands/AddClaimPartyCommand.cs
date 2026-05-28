using ClaimsModule.Application.Behaviors;
using ClaimsModule.Application.Interfaces;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.Features.Claims.Commands;

public record AddClaimPartyCommand(
    Guid ClaimId,
    string PartyRole,
    string PartyType,
    string? FirstName,
    string? LastName,
    string? CompanyName,
    string? Email,
    string? Phone,
    string? Notes
) : ICommand<Guid>;

public class AddClaimPartyCommandHandler(
    IClaimRepository claimRepository,
    IAuditLogService auditLogService,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<AddClaimPartyCommand, Guid>
{
    private static readonly Guid OrgId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public async Task<Guid> Handle(AddClaimPartyCommand request, CancellationToken cancellationToken)
    {
        var claim = await claimRepository.GetByIdWithDetailsAsync(request.ClaimId, cancellationToken)
            ?? throw new NotFoundException(nameof(Claim), request.ClaimId);

        if (!Enum.TryParse<PartyRole>(request.PartyRole, out var role))
            throw new DomainException($"Invalid party role: {request.PartyRole}");
        if (!Enum.TryParse<PartyType>(request.PartyType, out var partyType))
            throw new DomainException($"Invalid party type: {request.PartyType}");

        var party = new ClaimParty
        {
            PartyRole = role,
            PartyType = partyType,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CompanyName = request.CompanyName,
            Email = request.Email,
            Phone = request.Phone,
            Notes = request.Notes,
            OrganisationId = OrgId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        claim.AddParty(party);
        claimRepository.Update(claim);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(claim.Id, Domain.Entities.AuditEventTypes.PartyAdded,
            $"Party {role} '{party.DisplayName}' added",
            currentUser.UserId, relatedEntityId: party.Id, relatedEntityType: "ClaimParty",
            correlationId: currentUser.CorrelationId, cancellationToken: cancellationToken);

        return party.Id;
    }
}

public class AddClaimPartyCommandValidator : AbstractValidator<AddClaimPartyCommand>
{
    public AddClaimPartyCommandValidator()
    {
        RuleFor(x => x.ClaimId).NotEmpty();
        RuleFor(x => x.PartyRole)
            .NotEmpty()
            .Must(r => Enum.TryParse<PartyRole>(r, out _))
            .WithMessage(x => $"Invalid party role: {x.PartyRole}");
        RuleFor(x => x.PartyType)
            .NotEmpty()
            .Must(t => Enum.TryParse<PartyType>(t, out _))
            .WithMessage(x => $"Invalid party type: {x.PartyType}");
    }
}
