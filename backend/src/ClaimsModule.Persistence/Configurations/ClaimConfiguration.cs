using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

public class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        builder.ToTable("Claims");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(c => c.ClaimNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(c => new { c.ClaimNumber, c.OrganisationId }).IsUnique();

        builder.Property(c => c.OrganisationId).IsRequired();
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(c => c.Severity).HasConversion<string>().HasMaxLength(50);
        builder.Property(c => c.PolicyNumber).HasMaxLength(50);
        builder.Property(c => c.ClientName).HasMaxLength(255);
        builder.Property(c => c.ClosureReason).HasMaxLength(500);

        builder.Property(c => c.ReportedDate).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();

        builder.Property(c => c.RowVer).IsRowVersion();

        builder.HasQueryFilter(c => !c.IsDeleted);

        builder.HasOne(c => c.LossEvent)
            .WithOne()
            .HasForeignKey<LossEvent>(le => le.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Parties)
            .WithOne()
            .HasForeignKey(p => p.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.RiskObjects)
            .WithOne()
            .HasForeignKey(r => r.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Documents)
            .WithOne()
            .HasForeignKey(d => d.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.ReserveComponents)
            .WithOne()
            .HasForeignKey(rc => rc.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(ClaimSeedData.Claims());
    }
}
