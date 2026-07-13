using FSH.Framework.Persistence.Context;
using FSH.Modules.Tickets.Domain;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Tickets.Data;

public sealed class TicketsDbContext(DbContextOptions<TicketsDbContext> options)
    : BaseDbContext(options)
{
    public const string Schema = "tickets";

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TicketsDbContext).Assembly);
        // base.OnModelCreating runs LAST so BaseDbContext's auto-apply sees
        // fully-configured entities (including HasMany child types).
        base.OnModelCreating(modelBuilder);
    }
}