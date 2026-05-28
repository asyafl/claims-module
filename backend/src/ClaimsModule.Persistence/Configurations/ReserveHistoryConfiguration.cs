using ClaimsModule.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

public class ReserveHistoryConfiguration : IEntityTypeConfiguration<ReserveHistory>
{
    public void Configure(EntityTypeBuilder<ReserveHistory> builder)
    {
        builder.ToTable("ReserveHistories");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(h => h.TransactionType).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(h => h.Amount).HasPrecision(19, 4).IsRequired();
        builder.Property(h => h.PreviousBalance).HasPrecision(19, 4).IsRequired();
        builder.Property(h => h.NewBalance).HasPrecision(19, 4).IsRequired();
        builder.Property(h => h.ApprovalStatus).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(h => h.PostingStatus).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(h => h.ChangeReason).HasMaxLength(500).IsRequired();
        builder.Property(h => h.RejectionReason).HasMaxLength(500);
        builder.Property(h => h.IdempotencyKey).HasMaxLength(200).IsRequired();
        builder.Property(h => h.PostingJobId).HasMaxLength(100);
        builder.Property(h => h.OrganisationId).IsRequired();

        // Append-only: no soft delete filter needed — records are never deleted
        builder.HasQueryFilter(h => !h.IsDeleted);
    }
}
