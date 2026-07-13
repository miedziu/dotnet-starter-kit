using FSH.Modules.Billing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Billing.Data.Configurations;

public sealed class UsageSnapshotConfiguration : IEntityTypeConfiguration<UsageSnapshot>
{
    public void Configure(EntityTypeBuilder<UsageSnapshot> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("UsageSnapshots");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.PeriodYear, x.PeriodMonth })
            .IsUnique()
            .HasDatabaseName("ux_usage_snapshots_period");

        builder.Ignore(x => x.DomainEvents);
    }
}