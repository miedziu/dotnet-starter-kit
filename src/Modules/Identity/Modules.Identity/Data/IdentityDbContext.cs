using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Eventing.Inbox;
using FSH.Framework.Eventing.Outbox;
using FSH.Framework.Persistence;
using FSH.Framework.Shared.Multitenancy;
using FSH.Framework.Shared.Persistence;
using FSH.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace FSH.Modules.Identity.Data;

public class IdentityDbContext : IdentityDbContext<
    FshUser,
    FshRole,
    string,
    IdentityUserClaim<string>,
    IdentityUserRole<string>,
    IdentityUserLogin<string>,
    FshRoleClaim,
    IdentityUserToken<string>,
    IdentityUserPasskey<string>>
{
    private readonly DatabaseOptions _settings;
    private readonly IHostEnvironment _environment;
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    public DbSet<PasswordHistory> PasswordHistories => Set<PasswordHistory>();

    public DbSet<UserSession> UserSessions => Set<UserSession>();

    public DbSet<Group> Groups => Set<Group>();

    public DbSet<GroupRole> GroupRoles => Set<GroupRole>();

    public DbSet<UserGroup> UserGroups => Set<UserGroup>();

    public DbSet<ImpersonationGrant> ImpersonationGrants => Set<ImpersonationGrant>();
    public DbSet<Referral> Referrals => Set<Referral>();

    // Tenant info is set by the DbMigrator for logging purposes
    public AppTenantInfo? TenantInfo { get; set; }

    public IdentityDbContext(
        IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor,
        DbContextOptions<IdentityDbContext> options,
        IOptions<DatabaseOptions> settings,
        IHostEnvironment environment) : base(options)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _environment = environment;
        _settings = settings.Value;

        // Try to get tenant info from the accessor for logging
        TenantInfo = multiTenantContextAccessor?.MultiTenantContext?.TenantInfo;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        base.OnModelCreating(builder); //?
        builder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);

        builder.ApplyConfiguration(new OutboxMessageConfiguration(IdentityModuleConstants.SchemaName));
        builder.ApplyConfiguration(new InboxMessageConfiguration(IdentityModuleConstants.SchemaName));

        // Tenant isolation disabled - Identity entities are global (shared across tenants).
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //optionsBuilder.LogTo(message => System.Diagnostics.Debug.WriteLine(message)).EnableDetailedErrors();

        var connectionString = _settings.ConnectionString;
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            optionsBuilder.ConfigureHeroDatabase(
                _settings.Provider,
                connectionString,
                _settings.MigrationsAssembly,
                _environment.IsDevelopment());
        }
    }
}