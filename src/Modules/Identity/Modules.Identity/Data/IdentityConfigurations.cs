using FSH.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Identity.Data;

public class ApplicationUserConfig : IEntityTypeConfiguration<FshUser>
{
    public void Configure(EntityTypeBuilder<FshUser> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("Users", IdentityModuleConstants.SchemaName);

        builder
            .Property(u => u.IntId)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder
            .Property(u => u.ObjectId)
                .HasMaxLength(256);

        // Unique constraint on IntId
        builder
            .HasIndex(u => u.IntId)
            .IsUnique();
    }
}

public class ApplicationRoleConfig : IEntityTypeConfiguration<FshRole>
{
    public void Configure(EntityTypeBuilder<FshRole> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("Roles", IdentityModuleConstants.SchemaName);
    }
}

public class ApplicationRoleClaimConfig : IEntityTypeConfiguration<FshRoleClaim>
{
    public void Configure(EntityTypeBuilder<FshRoleClaim> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("RoleClaims", IdentityModuleConstants.SchemaName);

        //builder.HasKey(rc => rc.Id);
    }
}



public class IdentityUserRoleConfig : IEntityTypeConfiguration<IdentityUserRole<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("UserRoles", IdentityModuleConstants.SchemaName);

        builder.HasKey(r => new { r.UserId, r.RoleId });
    }
}

public class IdentityUserClaimConfig : IEntityTypeConfiguration<IdentityUserClaim<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserClaim<string>> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("UserClaims", IdentityModuleConstants.SchemaName);
    }
}

public class IdentityUserLoginConfig : IEntityTypeConfiguration<IdentityUserLogin<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserLogin<string>> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("UserLogins", IdentityModuleConstants.SchemaName);

        builder.HasKey(login => new { login.LoginProvider, login.ProviderKey });
    }
}

public class IdentityUserTokenConfig : IEntityTypeConfiguration<IdentityUserToken<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserToken<string>> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable("UserTokens", IdentityModuleConstants.SchemaName);

        builder.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });
    }
}
