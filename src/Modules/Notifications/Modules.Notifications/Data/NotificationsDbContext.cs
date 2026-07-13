
using FSH.Framework.Persistence.Context;
using FSH.Modules.Notifications.Domain;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Notifications.Data;

public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
    : BaseDbContext(options)
{
    public const string Schema = "notifications";

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);
        // base.OnModelCreating runs LAST so BaseDbContext's auto-apply (ApplyTenantIsolationByDefault)
        // sees fully-configured entities, including child types reached via HasMany navigation.
        base.OnModelCreating(modelBuilder);
    }
}