using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FSH.Modules.Identity.Domain;

namespace FSH.Modules.Identity.Data.Configurations;

public class FshUserConfiguration : IEntityTypeConfiguration<FshUser>
{
    public void Configure(EntityTypeBuilder<FshUser> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("Users", IdentityModuleConstants.SchemaName)
            .IsMultiTenant();

        builder.HasKey(u => u.Id);

        // Configure Id as UUID instead of text
        builder
            .Property(u => u.Id)
            .IsRequired()
            .HasColumnType("uuid")
            .HasMaxLength(36);

        builder
            .Property(u => u.FirstName)
            .HasColumnType("text");

        builder
            .Property(u => u.LastName)
            .HasColumnType("text");

        builder
            .Property(u => u.ImageUrl)
            .HasColumnType("text");

        builder
            .Property(u => u.IsActive)
            .IsRequired();

        builder
            .Property(u => u.RefreshToken)
            .HasColumnType("text");

        builder
            .Property(u => u.RefreshTokenExpiryTime)
            .IsRequired();

        builder
            .Property(u => u.ObjectId)
            .HasMaxLength(256)
            .HasColumnType("character varying(256)");

        builder
            .Property(u => u.CreatedAt)
            .IsRequired();

        builder
            .Property(u => u.LastPasswordChangeDate)
            .IsRequired();

        // Configure navigation properties
        builder
            .HasMany(u => u.PasswordHistories)
            .WithOne(ph => ph.User)
            .HasForeignKey(ph => ph.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}