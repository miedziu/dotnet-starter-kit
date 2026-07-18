using FSH.Mod.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Mod.Identity.Data.Configurations;

public class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
{
    public void Configure(EntityTypeBuilder<UserGroup> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("UserGroups", IdentityModuleConstants.SchemaName);

        builder.HasKey(ug => new { ug.UserId, ug.GroupId });

        builder
            .Property(ug => ug.UserId)
            .IsRequired();

        builder
            .Property(ug => ug.AddedBy)
            .HasMaxLength(450);

        builder
            .Property(ug => ug.AddedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder
            .HasOne(ug => ug.User)
            .WithMany()
            .HasForeignKey(ug => ug.UserId)
            .HasPrincipalKey((FshUser u) => u.IntId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(ug => ug.Group)
            .WithMany(g => g.UserGroups)
            .HasForeignKey(ug => ug.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(ug => ug.UserId);
        builder.HasIndex(ug => ug.GroupId);
    }
}