using ClaimsModule.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

public class ClaimReserveComponentConfiguration : IEntityTypeConfiguration<ClaimReserveComponent>
{
    public void Configure(EntityTypeBuilder<ClaimReserveComponent> builder)
    {
        builder.ToTable("ClaimReserveComponents");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(r => r.Component).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(r => r.CurrentAmount).HasPrecision(19, 4).IsRequired();
        builder.Property(r => r.Status).HasMaxLength(50).IsRequired();
        builder.Property(r => r.Notes).HasColumnType("nvarchar(max)");
        builder.Property(r => r.OrganisationId).IsRequired();

        builder.Property(r => r.RowVer).IsRowVersion();

        builder.HasMany(r => r.History)
            .WithOne(h => h.ReserveComponent)
            .HasForeignKey(h => h.ReserveComponentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
