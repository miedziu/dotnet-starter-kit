using FSH.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FSH.Modules.Tickets.Data;

public sealed class TicketsDbInitializer(
    TicketsDbContext dbContext,
    ILogger<TicketsDbInitializer> logger) : IDbInitializer
{
    public async Task MigrateAsync(CancellationToken ct)
    {
        if ((await dbContext.Database.GetPendingMigrationsAsync(ct).ConfigureAwait(false)).Any())
        {
            await dbContext.Database.MigrateAsync(ct).ConfigureAwait(false);
            logger.LogInformation("[Tickets] applied migrations");
        }
    }

    public Task SeedAsync(CancellationToken ct) => Task.CompletedTask;
}