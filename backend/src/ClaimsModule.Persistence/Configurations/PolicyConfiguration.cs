using ClaimsModule.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    private static readonly Guid OrgId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("Policies");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(p => p.PolicyNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(p => p.PolicyNumber).IsUnique();
        builder.Property(p => p.ClientName).HasMaxLength(255).IsRequired();
        builder.Property(p => p.Status).HasMaxLength(50).IsRequired();
        builder.Property(p => p.CoverageTypes).HasMaxLength(500).IsRequired();
        builder.Property(p => p.OrganisationId).IsRequired();

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.HasData(SeedData());
    }

    private static Policy[] SeedData()
    {
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        Policy Make(string id, string num, string client, DateOnly eff, DateOnly exp, string status, string coverage) => new()
        {
            Id = Guid.Parse(id), OrganisationId = OrgId, PolicyNumber = num, ClientName = client,
            EffectiveDate = eff, ExpirationDate = exp, Status = status, CoverageTypes = coverage,
            CreatedAt = now
        };
        return
        [
            Make("22000000-0000-0000-0000-000000000001", "POL-2024-001001", "Meridian Transport LLC",
                new DateOnly(2024, 1, 1), new DateOnly(2026, 12, 31), "Active", "Vehicle,Cargo"),
            Make("22000000-0000-0000-0000-000000000002", "POL-2024-001002", "Harborview Properties Inc",
                new DateOnly(2024, 6, 1), new DateOnly(2026, 5, 31), "Active", "Property,Liability"),
            Make("22000000-0000-0000-0000-000000000003", "POL-2025-002001", "Coastal Builders Group",
                new DateOnly(2025, 3, 1), new DateOnly(2027, 2, 28), "Active", "Property,Equipment"),
            Make("22000000-0000-0000-0000-000000000004", "POL-2025-002002", "Stanton Medical Group",
                new DateOnly(2025, 1, 1), new DateOnly(2026, 12, 31), "Active", "Liability,Vehicle"),
            Make("22000000-0000-0000-0000-000000000005", "POL-2023-000099", "Archived Corp",
                new DateOnly(2020, 1, 1), new DateOnly(2021, 12, 31), "Expired", "Property"),
        ];
    }
}
