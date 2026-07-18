using FSH.Framework.Persistence.Context;
using FSH.Mods.Ticket.Domain;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mods.Ticket.Data;

public sealed class TicketDbContext(DbContextOptions<TicketDbContext> options)
    : BaseDbContext(options)
{
    public const string Schema = "tickets";

    public DbSet<Domain.Ticket> Tickets => Set<Domain.Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TicketDbContext).Assembly);
        // base.OnModelCreating runs LAST so BaseDbContext's auto-apply sees
        // fully-configured entities (including HasMany child types).
        base.OnModelCreating(modelBuilder);
    }
}