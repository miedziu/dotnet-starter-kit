
using FSH.Framework.Persistence.Context;
using FSH.Mods.File.Domain;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mods.File.Data;

public sealed class FileDbContext(DbContextOptions<FileDbContext> options)
    : BaseDbContext(options)
{
    public const string Schema = "files";

    public DbSet<FileAsset> FileAssets => Set<FileAsset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FileDbContext).Assembly);
        // base.OnModelCreating runs LAST so BaseDbContext's auto-apply sees
        // fully-configured entities (including HasMany child types).
        base.OnModelCreating(modelBuilder);
    }
}