using FSH.Mod.Billing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Mod.Billing.Data.Configurations;

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("Subscriptions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PlanId).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>();

        builder.HasIndex(x => x.Status);

        builder.Ignore(x => x.DomainEvents);
    }
}