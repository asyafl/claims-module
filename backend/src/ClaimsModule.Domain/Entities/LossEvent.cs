using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Entities;

public class LossEvent : BaseEntity
{
    public Guid ClaimId { get; set; }
    public DateTimeOffset LossDate { get; set; }
    public string LossDescription { get; set; } = string.Empty;
    public string? LossLocation { get; set; }
    public string CauseOfLossCode { get; set; } = string.Empty;
    public decimal? EstimatedLossAmount { get; set; }
    public DateTimeOffset ReportDate { get; set; }
    public string? PoliceReportNumber { get; set; }
}
