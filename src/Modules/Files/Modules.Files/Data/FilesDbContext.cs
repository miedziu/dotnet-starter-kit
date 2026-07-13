
using FSH.Framework.Persistence.Context;
using FSH.Modules.Files.Domain;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Files.Data;

public sealed class FilesDbContext(DbContextOptions<FilesDbContext> options)
    : BaseDbContext(options)
{
    public const string Schema = "files";

    public DbSet<FileAsset> FileAssets => Set<FileAsset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FilesDbContext).Assembly);
        // base.OnModelCreating runs LAST so BaseDbContext's auto-apply sees
        // fully-configured entities (including HasMany child types).
        base.OnModelCreating(modelBuilder);
    }
}