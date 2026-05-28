using ClaimsModule.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

public class LossEventConfiguration : IEntityTypeConfiguration<LossEvent>
{
    public void Configure(EntityTypeBuilder<LossEvent> builder)
    {
        builder.ToTable("LossEvents");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(e => e.LossDescription).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(e => e.LossLocation).HasMaxLength(500);
        builder.Property(e => e.CauseOfLossCode).HasMaxLength(50).IsRequired();
        builder.Property(e => e.EstimatedLossAmount).HasPrecision(19, 4);
        builder.Property(e => e.PoliceReportNumber).HasMaxLength(100);
        builder.Property(e => e.LossDate).IsRequired();
        builder.Property(e => e.ReportDate).IsRequired();

        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasData(ClaimSeedData.LossEvents());
    }
}
