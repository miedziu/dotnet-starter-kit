using FSH.Mods.Billing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Mods.Billing.Data.Configurations;

public sealed class BillingPlanConfiguration : IEntityTypeConfiguration<BillingPlan>
{
    public void Configure(EntityTypeBuilder<BillingPlan> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("Plans");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Key).IsRequired().HasMaxLength(64);
        builder.HasIndex(x => x.Key).IsUnique();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(128);
        builder.Ignore(x => x.Currency);
        builder.OwnsOne(x => x.MonthlyBasePrice, m =>
        {
            m.Property(p => p.Amount).HasColumnName("MonthlyBasePrice").HasPrecision(18, 4).IsRequired();
            m.Property(p => p.Currency).HasColumnName("Currency").HasMaxLength(8).IsRequired();
        });
        builder.Navigation(x => x.MonthlyBasePrice).IsRequired();
        builder.Property(x => x.Interval).HasConversion<int>().HasDefaultValue(Spec.PlanInterval.Monthly);
        builder.OwnsOne(x => x.AnnualPrice, m =>
        {
            m.Property(p => p.Amount).HasColumnName("AnnualPrice").HasPrecision(18, 4);
            m.Property(p => p.Currency).HasColumnName("AnnualPriceCurrency").HasMaxLength(8);
        });

        builder.Ignore(x => x.DomainEvents);
    }
}