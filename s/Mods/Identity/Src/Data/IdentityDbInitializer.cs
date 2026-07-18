using FSH.Framework.Persistence;
using FSH.Framework.Shared.Identity;
using FSH.Framework.Web.Origin;
using FSH.Mods.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FSH.Mods.Identity.Data;

internal sealed class IdentityDbInitializer(
    ILogger<IdentityDbInitializer> logger,
    IdentityDbContext context,
    RoleManager<FshRole> roleManager,
    UserManager<FshUser> userManager,
    TimeProvider timeProvider,
    IOptions<OriginOptions> originSettings,
    IConfiguration configuration) : IDbInitializer
{
    public async Task MigrateAsync(CancellationToken ct)
    {
        var pendingMigrations = (await context.Database.GetPendingMigrationsAsync(ct).ConfigureAwait(false)).ToList();
        if (pendingMigrations.Count > 0)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Applying {Count} pending migration(s) for Identity module: {Migrations}",
                    pendingMigrations.Count, string.Join(", ", pendingMigrations));
            }
            await context.Database.MigrateAsync(ct).ConfigureAwait(false);
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Applied database migrations for Identity module");
            }
        }
    }

    public async Task SeedAsync(CancellationToken ct)
    {
        await SeedRolesAsync(ct);
        await SeedSystemGroupsAsync(ct);
        await SeedAdminUserAsync(ct);
    }

    private async Task SeedRolesAsync(CancellationToken ct = default)
    {
        foreach (string roleName in RoleConstants.DefaultRoles)
        {
            if (await roleManager.Roles.SingleOrDefaultAsync(r => r.Name == roleName, ct)
                is not FshRole role)
            {
                // create role - roles are global (shared across tenants)
                role = new FshRole(roleName, $"{roleName} Role");
                await roleManager.CreateAsync(role);
            }

            // Assign permissions
            if (roleName == RoleConstants.Basic)
            {
                await AssignPermissionsToRoleAsync(context, PermissionConstants.Basic, role, ct);
            }
            else if (roleName == RoleConstants.Admin)
            {
                await AssignPermissionsToRoleAsync(context, PermissionConstants.Admin, role, ct);
                await AssignPermissionsToRoleAsync(context, PermissionConstants.Root, role, ct);
            }
        }
    }

    private async Task AssignPermissionsToRoleAsync(IdentityDbContext dbContext, IReadOnlyList<FshPermission> permissions, FshRole role, CancellationToken ct = default)
    {
        var currentClaims = await roleManager.GetClaimsAsync(role);
        var newClaims = permissions
            .Where(permission => !currentClaims.Any(c => c.Type == ClaimConstants.Permission && c.Value == permission.Name))
            .Select(permission => new FshRoleClaim
            {
                RoleId = role.Id,
                ClaimType = ClaimConstants.Permission,
                ClaimValue = permission.Name,
                CreatedBy = "application",
                CreatedOn = timeProvider.GetUtcNow()
            })
            .ToList();

        foreach (var claim in newClaims)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Seeding {Role} Permission '{Permission}'.", role.Name, claim.ClaimValue);
            }
            await dbContext.RoleClaims.AddAsync(claim, ct);
        }

        // Save changes to the database context
        if (newClaims.Count != 0)
        {
            await dbContext.SaveChangesAsync(ct);
        }

    }

    private async Task SeedSystemGroupsAsync(CancellationToken ct = default)
    {
        // Seed "All Users" default group - all new users are automatically added to this group
        const string allUsersGroupName = "All Users";
        var allUsersGroup = await context.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Name == allUsersGroupName && g.IsSystemGroup, ct);

        if (allUsersGroup is null)
        {
            allUsersGroup = Group.Create(
                name: allUsersGroupName,
                description: "Default group for all users. New users are automatically added to this group.",
                isDefault: true,
                isSystemGroup: true,
                createdBy: "System");

            await context.Groups.AddAsync(allUsersGroup, ct);
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Seeding '{GroupName}' system group.", allUsersGroupName);
            }
        }

        // Seed "Administrators" group with Admin role
        const string administratorsGroupName = "Administrators";
        var administratorsGroup = await context.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Name == administratorsGroupName && g.IsSystemGroup, ct);

        if (administratorsGroup is null)
        {
            administratorsGroup = Group.Create(
                name: administratorsGroupName,
                description: "System group for administrators with full administrative privileges.",
                isDefault: false,
                isSystemGroup: true,
                createdBy: "System");

            await context.Groups.AddAsync(administratorsGroup, ct);
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Seeding '{GroupName}' system group.", administratorsGroupName);
            }
        }

        await context.SaveChangesAsync(ct);

        // Assign Admin role to Administrators group
        var adminRole = await roleManager.FindByNameAsync(RoleConstants.Admin);
        if (adminRole is not null)
        {
            var existingGroupRole = await context.GroupRoles
                .AsNoTracking()
                .FirstOrDefaultAsync(gr => gr.GroupId == administratorsGroup.Id && gr.RoleId == adminRole.Id, ct);

            if (existingGroupRole is null)
            {
                context.GroupRoles.Add(GroupRole.Create(administratorsGroup.Id, adminRole.Id));

                await context.SaveChangesAsync(ct);
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Assigned Admin role to '{GroupName}' group.", administratorsGroupName);
                }
            }
        }
    }

    private async Task SeedAdminUserAsync(CancellationToken ct = default)
    {
        var adminEmail = configuration["Seed:DefaultAdminEmail"];
        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            return;
        }

        if (await userManager.Users.FirstOrDefaultAsync(u => u.Email == adminEmail, ct)
            is not FshUser adminUser)
        {
            string adminUserName = $"{RoleConstants.Admin}".ToUpperInvariant();
            adminUser = new FshUser
            {
                FirstName = RoleConstants.Admin,
                LastName = RoleConstants.Admin,
                Email = adminEmail,
                UserName = adminUserName,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                NormalizedEmail = adminEmail.ToUpperInvariant(),
                NormalizedUserName = adminUserName.ToUpperInvariant(),
                ImageUrl = new Uri(originSettings.Value.OriginUrl! + "/default-profile.png"),
                IsActive = true
            };

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Seeding Default Admin User");
            }
            var initialPassword = ResolveInitialAdminPassword();
            var password = new PasswordHasher<FshUser>();
            adminUser.PasswordHash = password.HashPassword(adminUser, initialPassword);
            // The IdentityResult MUST be checked: a silent failure here (e.g. a password-policy
            // rejection or a transient DB error) would otherwise mark provisioning "Completed" with no
            // admin user — an unrecoverable tenant with no login. Throwing surfaces it as a Failed
            // provisioning step that the operator can retry.
            var createResult = await userManager.CreateAsync(adminUser);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to seed admin user: "
                    + string.Join("; ", createResult.Errors.Select(e => e.Description)));
            }
        }

        // Assign role to user
        if (!await userManager.IsInRoleAsync(adminUser, RoleConstants.Admin))
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Assigning Admin Role to Admin User");
            }
            await userManager.AddToRoleAsync(adminUser, RoleConstants.Admin);
        }
    }

    /// <summary>
    /// Resolve the initial password for the admin user being seeded into a tenant.
    /// Lookup order:
    ///   1. <c>Seed:DefaultAdminPassword</c> from configuration — covers the framework's
    ///      root-tenant seed at startup and any test-host bootstrap. Operators set this
    ///      via env var / user-secrets / production secrets manager.
    /// Throws if neither source supplies a password — refusing to seed is safer than
    /// minting an admin user with a predictable secret.
    /// </summary>
    private string ResolveInitialAdminPassword()
    {
        var fromConfig = configuration["Seed:DefaultAdminPassword"];
        if (!string.IsNullOrWhiteSpace(fromConfig))
        {
            return fromConfig;
        }

        throw new InvalidOperationException(
            $"No initial admin password available. " +
            "Supply AdminPassword on the CreateTenant request, or set " +
            "'Seed:DefaultAdminPassword' in configuration for the root/startup seed.");
    }
}