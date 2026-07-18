using FSH.Mod.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Mod.Identity.Data.Configurations;

public class ReferralConfig : IEntityTypeConfiguration<Referral>
{
    public void Configure(EntityTypeBuilder<Referral> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("Referrals", IdentityModuleConstants.SchemaName);

        // Composite key: (ReferrerUserId, NewReferredUserId) prevents duplicate referrals
        builder.HasKey(r => new { r.ReferrerUserId, r.NewReferredUserId });

        builder.Property(r => r.ReferrerUserId)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(r => r.NewReferredUserId)
            .IsRequired()
            .HasMaxLength(64);

        // Navigation: Referral belongs to ReferrerUser (many-to-one)
        builder.HasOne(r => r.ReferrerUser)
            .WithMany(u => u.Referrals)
            .HasForeignKey(r => r.ReferrerUserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation: Referral belongs to NewReferredUser (many-to-one)
        builder.HasOne(r => r.NewReferredUser)
            .WithMany()
            .HasForeignKey(r => r.NewReferredUserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}