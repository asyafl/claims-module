using AutoMapper;
using ClaimsModule.Application.DTOs;
using ClaimsModule.Domain.Entities;

namespace ClaimsModule.Application.Mappings;

public class ClaimProfile : Profile
{
    public ClaimProfile()
    {
        CreateMap<Claim, ClaimListItemDto>()
            .ConstructUsing((src, ctx) => new ClaimListItemDto(
                src.Id, src.ClaimNumber, src.PolicyNumber, src.ClientName,
                src.LossEvent != null ? src.LossEvent.LossDate : null,
                src.LossEvent != null ? src.LossEvent.CauseOfLossCode : null,
                src.Status.ToString(),
                src.ReserveComponents.Sum(rc => rc.CurrentAmount),
                src.ReportedDate, src.AssignedHandlerId))
            .ForAllMembers(o => o.Ignore());

        CreateMap<Claim, ClaimDetailDto>()
            .ConstructUsing((src, ctx) => new ClaimDetailDto(
                src.Id, src.ClaimNumber, src.PolicyId, src.PolicyNumber, src.ClientName,
                src.Status.ToString(), src.Severity?.ToString(), src.ReportedDate,
                src.AssignedHandlerId, src.ClosedAt, src.ClosureReason, src.Notes,
                src.ManagerOverrideFlag,
                src.LossEvent != null ? ctx.Mapper.Map<LossEventDto>(src.LossEvent) : null,
                ctx.Mapper.Map<List<ClaimPartyDto>>(src.Parties),
                ctx.Mapper.Map<List<ClaimRiskObjectDto>>(src.RiskObjects),
                ctx.Mapper.Map<List<ReserveComponentDto>>(src.ReserveComponents),
                src.ReserveComponents.Sum(rc => rc.CurrentAmount)))
            .ForAllMembers(o => o.Ignore());

        CreateMap<LossEvent, LossEventDto>()
            .ConstructUsing(src => new LossEventDto(
                src.Id, src.LossDate, src.LossDescription, src.LossLocation,
                src.CauseOfLossCode, src.EstimatedLossAmount, src.PoliceReportNumber))
            .ForAllMembers(o => o.Ignore());

        CreateMap<ClaimParty, ClaimPartyDto>()
            .ConstructUsing(src => new ClaimPartyDto(
                src.Id, src.PartyRole.ToString(), src.PartyType.ToString(),
                src.FirstName, src.LastName, src.CompanyName,
                src.Email, src.Phone, src.Notes, src.IsActive))
            .ForAllMembers(o => o.Ignore());

        CreateMap<ClaimRiskObject, ClaimRiskObjectDto>()
            .ConstructUsing(src => new ClaimRiskObjectDto(
                src.Id, src.AssetType.ToString(), src.AssetDescription,
                src.DamageDescription, src.IsPrimary, src.AssetReference))
            .ForAllMembers(o => o.Ignore());

        CreateMap<ClaimReserveComponent, ReserveComponentDto>()
            .ConstructUsing((src, ctx) => new ReserveComponentDto(
                src.Id, src.Component.ToString(), src.CurrentAmount, src.Status, src.Notes,
                ctx.Mapper.Map<List<ReserveTransactionDto>>(src.History)))
            .ForAllMembers(o => o.Ignore());

        CreateMap<ReserveHistory, ReserveTransactionDto>()
            .ConstructUsing(src => new ReserveTransactionDto(
                src.Id, src.TransactionType.ToString(), src.Amount,
                src.PreviousBalance, src.NewBalance,
                src.ApprovalStatus.ToString(), src.PostingStatus.ToString(),
                src.ChangeReason, src.RejectionReason,
                src.SubmittedByUserId, src.ApprovedByUserId, src.ApprovedAt,
                src.RejectedByUserId, src.RejectedAt,
                src.IdempotencyKey, src.ChangeSequence, src.CreatedAt))
            .ForAllMembers(o => o.Ignore());

        CreateMap<ClaimAuditLog, AuditLogEntryDto>()
            .ConstructUsing(src => new AuditLogEntryDto(
                src.Id, src.EventType, src.Description,
                src.OldValue, src.NewValue,
                src.RelatedEntityId, src.RelatedEntityType,
                src.CreatedByUserId, src.CreatedAt))
            .ForAllMembers(o => o.Ignore());

        CreateMap<Policy, PolicyDto>()
            .ConstructUsing(src => new PolicyDto(
                src.Id, src.PolicyNumber, src.ClientName,
                src.EffectiveDate, src.ExpirationDate, src.Status,
                src.CoverageTypes.Split(',', StringSplitOptions.RemoveEmptyEntries)))
            .ForAllMembers(o => o.Ignore());
    }
}
