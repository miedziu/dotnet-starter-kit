using FSH.Framework.Eventing.Inbox;
using FSH.Framework.Eventing.Outbox;
using FSH.Framework.Persistence.Context;
using FSH.Mod.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mod.Identity.Data;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : BaseDbContext(options)
{

    public DbSet<FshUser> Users => Set<FshUser>();
    public DbSet<FshRole> Roles => Set<FshRole>();
    public DbSet<IdentityUserClaim<string>> UserClaims => Set<IdentityUserClaim<string>>();
    public DbSet<IdentityUserRole<string>> UserRoles => Set<IdentityUserRole<string>>();
    public DbSet<IdentityUserLogin<string>> UserLogins => Set<IdentityUserLogin<string>>();
    public DbSet<FshRoleClaim> RoleClaims => Set<FshRoleClaim>();
    public DbSet<IdentityUserToken<string>> UserTokens => Set<IdentityUserToken<string>>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    public DbSet<PasswordHistory> PasswordHistories => Set<PasswordHistory>();

    public DbSet<UserSession> UserSessions => Set<UserSession>();

    public DbSet<Group> Groups => Set<Group>();

    public DbSet<GroupRole> GroupRoles => Set<GroupRole>();

    public DbSet<UserGroup> UserGroups => Set<UserGroup>();

    public DbSet<ImpersonationGrant> ImpersonationGrants => Set<ImpersonationGrant>();

    public DbSet<Referral> Referrals => Set<Referral>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration(IdentityModuleConstants.SchemaName));
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration(IdentityModuleConstants.SchemaName));
    }
}