using FSH.Framework.Persistence.Context;
using FSH.Modules.Webhooks.Domain;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Webhooks.Data;

public sealed class WebhookDbContext(DbContextOptions<WebhookDbContext> options)
    : BaseDbContext(options)
{
    public DbSet<WebhookSubscription> Subscriptions => Set<WebhookSubscription>();
    public DbSet<WebhookDelivery> Deliveries => Set<WebhookDelivery>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema("webhooks");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WebhookDbContext).Assembly);
        // base.OnModelCreating runs LAST so BaseDbContext's auto-apply sees
        // fully-configured entities (including HasMany child types).
        base.OnModelCreating(modelBuilder);
    }
}