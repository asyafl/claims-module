using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Domain.Events;
using ClaimsModule.Domain.Exceptions;

namespace ClaimsModule.Domain.Entities;

public class Claim : AggregateRoot
{
    // Valid transitions per state machine (FRS Section 4.2)
    private static readonly Dictionary<ClaimStatus, HashSet<ClaimStatus>> ValidTransitions = new()
    {
        [ClaimStatus.Draft] = [ClaimStatus.Open, ClaimStatus.Withdrawn],
        [ClaimStatus.Open] = [ClaimStatus.UnderInvestigation, ClaimStatus.PendingPayment, ClaimStatus.Closed, ClaimStatus.Withdrawn],
        [ClaimStatus.UnderInvestigation] = [ClaimStatus.Open, ClaimStatus.PendingPayment, ClaimStatus.Closed, ClaimStatus.Withdrawn],
        [ClaimStatus.PendingPayment] = [ClaimStatus.Closed],
        [ClaimStatus.Closed] = [ClaimStatus.Reopened],
        [ClaimStatus.Reopened] = [ClaimStatus.Open],
        [ClaimStatus.Withdrawn] = []
    };

    public string ClaimNumber { get; private set; } = string.Empty;
    public Guid? PolicyId { get; set; }
    public string? PolicyNumber { get; set; }
    public string? ClientName { get; set; }
    public ClaimStatus Status { get; private set; } = ClaimStatus.Draft;
    public ClaimSeverity? Severity { get; set; }
    public DateTimeOffset ReportedDate { get; set; }
    public Guid? AssignedHandlerId { get; set; }
    public DateTimeOffset? ClosedAt { get; private set; }
    public string? ClosureReason { get; private set; }
    public string? Notes { get; set; }
    public bool ManagerOverrideFlag { get; set; }

    private readonly List<ClaimParty> _parties = [];
    private readonly List<ClaimRiskObject> _riskObjects = [];
    private readonly List<ClaimDocument> _documents = [];
    private readonly List<ClaimReserveComponent> _reserveComponents = [];

    public IReadOnlyCollection<ClaimParty> Parties => _parties.AsReadOnly();
    public IReadOnlyCollection<ClaimRiskObject> RiskObjects => _riskObjects.AsReadOnly();
    public IReadOnlyCollection<ClaimDocument> Documents => _documents.AsReadOnly();
    public IReadOnlyCollection<ClaimReserveComponent> ReserveComponents => _reserveComponents.AsReadOnly();

    public LossEvent? LossEvent { get; set; }

    public static Claim Create(string claimNumber, Guid organisationId, Guid? createdByUserId)
    {
        var claim = new Claim
        {
            ClaimNumber = claimNumber,
            OrganisationId = organisationId,
            Status = ClaimStatus.Draft,
            ReportedDate = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UserCreated = createdByUserId
        };

        claim.AddDomainEvent(new ClaimCreatedEvent(claim.Id, claimNumber, organisationId));
        return claim;
    }

    // Used only by EF Core seed data — bypasses domain events and state machine
    public static Claim CreateForSeed(
        Guid id, string claimNumber, Guid organisationId,
        ClaimStatus status, Guid? policyId, string? policyNumber, string? clientName,
        DateTimeOffset reportedDate, DateTimeOffset? closedAt = null, string? closureReason = null)
    {
        return new Claim
        {
            Id = id,
            ClaimNumber = claimNumber,
            OrganisationId = organisationId,
            Status = status,
            PolicyId = policyId,
            PolicyNumber = policyNumber,
            ClientName = clientName,
            ReportedDate = reportedDate,
            ClosedAt = closedAt,
            ClosureReason = closureReason,
            CreatedAt = reportedDate
        };
    }

    public void TransitionTo(ClaimStatus targetStatus, Guid userId, string? reason = null)
    {
        if (!ValidTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(targetStatus))
        {
            var validOnes = ValidTransitions.TryGetValue(Status, out var v)
                ? string.Join(", ", v)
                : "none";
            throw new DomainException(
                $"Transition from {Status} to {targetStatus} is not permitted. Valid transitions: {validOnes}.");
        }

        if (targetStatus == ClaimStatus.Closed)
            ValidateClosureConditions();

        var previous = Status;
        Status = targetStatus;
        UpdatedAt = DateTimeOffset.UtcNow;
        UserModified = userId;

        if (targetStatus == ClaimStatus.Closed)
        {
            ClosedAt = DateTimeOffset.UtcNow;
            ClosureReason = reason;
        }

        AddDomainEvent(new ClaimStatusChangedEvent(Id, previous, targetStatus, userId));
    }

    private void ValidateClosureConditions()
    {
        // CC-01: no PendingApproval reserves
        var hasPendingReserves = _reserveComponents
            .SelectMany(rc => rc.History)
            .Any(h => h.ApprovalStatus == ApprovalStatus.PendingApproval);

        if (hasPendingReserves)
            throw new DomainException("CC-01: Claim cannot be closed — pending approval reserves exist.");

        // CC-03: at least one active Claimant
        var hasClaimant = _parties.Any(p => p.PartyRole == PartyRole.Claimant && p.IsActive);
        if (!hasClaimant)
            throw new DomainException("CC-03: Claim cannot be closed — at least one Claimant party is required.");
    }

    public void AddParty(ClaimParty party)
    {
        party.ClaimId = Id;
        _parties.Add(party);
    }

    public ClaimParty RemoveParty(Guid partyId)
    {
        var party = _parties.FirstOrDefault(p => p.Id == partyId && p.IsActive)
            ?? throw new NotFoundException(nameof(ClaimParty), partyId);

        // Prevent removing the last active Claimant
        var activeClaimants = _parties.Count(p => p.PartyRole == PartyRole.Claimant && p.IsActive);
        if (party.PartyRole == PartyRole.Claimant && activeClaimants == 1)
            throw new DomainException("Cannot remove the last active Claimant from the claim.");

        party.IsActive = false;
        return party;
    }

    public void AddRiskObject(ClaimRiskObject riskObject)
    {
        riskObject.ClaimId = Id;
        _riskObjects.Add(riskObject);
    }

    public void AddDocument(ClaimDocument document)
    {
        document.ClaimId = Id;
        _documents.Add(document);
        AddDomainEvent(new DocumentUploadedEvent(Id, document.Id, document.DocumentName));
    }

    public void AddReserveComponent(ClaimReserveComponent component)
    {
        component.ClaimId = Id;
        _reserveComponents.Add(component);
    }

    public bool HasActiveClaimant() =>
        _parties.Any(p => p.PartyRole == PartyRole.Claimant && p.IsActive);

    public decimal GetTotalApprovedReserves() =>
        _reserveComponents.Sum(rc => rc.CurrentAmount);
}
