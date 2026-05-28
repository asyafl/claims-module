using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Entities;

public class CauseOfLossCode : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PerilCategory { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
