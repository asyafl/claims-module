using ClaimsModule.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClaimsModule.Persistence.Configurations;

public class ClaimAuditLogConfiguration : IEntityTypeConfiguration<ClaimAuditLog>
{
    public void Configure(EntityTypeBuilder<ClaimAuditLog> builder)
    {
        builder.ToTable("ClaimAuditLogs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(a => a.EventType).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Description).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(a => a.OldValue).HasColumnType("nvarchar(max)");
        builder.Property(a => a.NewValue).HasColumnType("nvarchar(max)");
        builder.Property(a => a.RelatedEntityType).HasMaxLength(100);
        builder.Property(a => a.OrganisationId).IsRequired();
        builder.Property(a => a.CreatedAt).IsRequired();

        // Audit log is never soft-deleted — no query filter
        builder.HasIndex(a => new { a.ClaimId, a.CreatedAt });
    }
}
