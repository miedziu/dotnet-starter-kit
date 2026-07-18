
using FSH.Framework.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mod.Notification.Data;

public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options)
    : BaseDbContext(options)
{
    public const string Schema = "notifications";

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
        // base.OnModelCreating runs LAST so BaseDbContext's auto-apply (ApplyTenantIsolationByDefault)
        // sees fully-configured entities, including child types reached via HasMany navigation.
        base.OnModelCreating(modelBuilder);
    }
}