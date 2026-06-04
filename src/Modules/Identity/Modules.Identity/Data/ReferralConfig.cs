using FSH.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Identity.Data;

public class ReferralConfig : IEntityTypeConfiguration<Referral>
{
    public void Configure(EntityTypeBuilder<Referral> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("Referrals", IdentityModuleConstants.SchemaName);

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReferralCode)
            .IsRequired()
            .HasMaxLength(16);

        builder.HasIndex(r => r.ReferralCode)
            .IsUnique();

        builder.Property(r => r.ReferrerUserId)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(r => r.ReferredUserId)
            .HasMaxLength(64);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UsedAt);

        // Navigation: ReferrerUser -> Referral (one-to-one)
        builder.HasOne(r => r.ReferrerUser)
            .WithOne(u => u.Referral)
            .HasForeignKey<Referral>(r => r.ReferrerUserId)
            .HasPrincipalKey<FshUser>(u => u.Id)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation: Referral -> ReferredUser (many-to-one, optional)
        builder.HasOne(r => r.ReferredUser)
            .WithMany(u => u.ReferredUsers)
            .HasForeignKey(r => r.ReferredUserId)
            .HasPrincipalKey(u => u.Id)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes for common queries
        builder.HasIndex(r => r.ReferrerUserId)
            .HasDatabaseName("IX_Referrals_ReferrerUserId");
        
        builder.HasIndex(r => r.ReferredUserId)
            .HasDatabaseName("IX_Referrals_ReferredUserId");
    }
}