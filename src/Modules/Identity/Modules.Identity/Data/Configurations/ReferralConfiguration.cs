using FSH.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Identity.Data.Configurations;

public class ReferralConfiguration : IEntityTypeConfiguration<Referral>
{
    public void Configure(EntityTypeBuilder<Referral> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("Referrals", IdentityModuleConstants.SchemaName);

        builder.HasKey(r => new { r.ReferrerUserId, r.NewReferredUserId });

        builder
            .Property(r => r.ReferrerUserId)
            .IsRequired();

        builder
            .Property(r => r.NewReferredUserId)
            .IsRequired();

        // Configure the foreign key relationships
        builder
            .HasOne(r => r.ReferrerUser)
            .WithMany((FshUser u) => u.Referrals)
            .HasForeignKey(r => r.ReferrerUserId)
            .HasPrincipalKey((FshUser u) => u.IntId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(r => r.NewReferredUser)
            .WithMany()
            .HasForeignKey(r => r.NewReferredUserId)
            .HasPrincipalKey((FshUser u) => u.IntId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for efficient lookups
        builder.HasIndex(r => r.ReferrerUserId);
    }
}