using FSH.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FSH.Modules.Files.Data;

public sealed class FilesDbInitializer(
    FilesDbContext dbContext,
    ILogger<FilesDbInitializer> logger) : IDbInitializer
{
    public async Task MigrateAsync(CancellationToken ct)
    {
        if ((await dbContext.Database.GetPendingMigrationsAsync(ct).ConfigureAwait(false)).Any())
        {
            await dbContext.Database.MigrateAsync(ct).ConfigureAwait(false);
            logger.LogInformation("[Files] applied migrations");
        }
    }

    public Task SeedAsync(CancellationToken ct) => Task.CompletedTask;
}