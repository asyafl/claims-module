using ClaimsModule.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

public class CauseOfLossCodeConfiguration : IEntityTypeConfiguration<CauseOfLossCode>
{
    private static readonly Guid OrgId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public void Configure(EntityTypeBuilder<CauseOfLossCode> builder)
    {
        builder.ToTable("CauseOfLossCodes");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(c => c.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(c => c.Code).IsUnique();
        builder.Property(c => c.Name).HasMaxLength(255).IsRequired();
        builder.Property(c => c.PerilCategory).HasMaxLength(50).IsRequired();
        builder.Property(c => c.OrganisationId).IsRequired();

        builder.HasQueryFilter(c => !c.IsDeleted);

        builder.HasData(SeedData());
    }

    private static CauseOfLossCode[] SeedData()
    {
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        CauseOfLossCode Make(string id, string code, string name, string peril, int sort) => new()
        {
            Id = Guid.Parse(id), OrganisationId = OrgId, Code = code, Name = name,
            PerilCategory = peril, IsActive = true, SortOrder = sort,
            CreatedAt = now
        };
        return
        [
            Make("11000000-0000-0000-0000-000000000001", "COL-FIRE", "Fire", "Property", 1),
            Make("11000000-0000-0000-0000-000000000002", "COL-FLOOD", "Flood", "Weather", 2),
            Make("11000000-0000-0000-0000-000000000003", "COL-THEFT", "Theft", "Crime", 3),
            Make("11000000-0000-0000-0000-000000000004", "COL-VEH-COL", "Vehicle Collision", "Auto", 4),
            Make("11000000-0000-0000-0000-000000000005", "COL-VEH-COMP", "Vehicle Comprehensive", "Auto", 5),
            Make("11000000-0000-0000-0000-000000000006", "COL-LIAB", "Third Party Liability", "Liability", 6),
            Make("11000000-0000-0000-0000-000000000007", "COL-EQUIP", "Equipment Breakdown", "Equipment", 7),
            Make("11000000-0000-0000-0000-000000000008", "COL-WIND", "Wind / Storm", "Weather", 8),
            Make("11000000-0000-0000-0000-000000000009", "COL-INJURY", "Bodily Injury", "Liability", 9),
            Make("11000000-0000-0000-0000-000000000010", "COL-OTHER", "Other / Unknown", "General", 10),
        ];
    }
}
