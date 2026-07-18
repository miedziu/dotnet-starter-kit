
using FSH.Framework.Persistence.Context;
using FSH.Mod.Chat.Domain;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mod.Chat.Data;

public sealed class ChatDbContext(DbContextOptions<ChatDbContext> options)
    : BaseDbContext(options)
{
    public const string Schema = "chat";

    public DbSet<ChatChannel> Channels => Set<ChatChannel>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChatDbContext).Assembly);
        // base.OnModelCreating runs LAST so BaseDbContext's auto-apply sees
        // fully-configured entities (including HasMany child types).
        base.OnModelCreating(modelBuilder);
    }
}