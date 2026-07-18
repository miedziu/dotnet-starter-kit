using FSH.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FSH.Mod.Ticket.Data;

public sealed class TicketDbInitializer(
    TicketDbContext dbContext,
    ILogger<TicketDbInitializer> logger) : IDbInitializer
{
    public async Task MigrateAsync(CancellationToken ct)
    {
        if ((await dbContext.Database.GetPendingMigrationsAsync(ct).ConfigureAwait(false)).Any())
        {
            await dbContext.Database.MigrateAsync(ct).ConfigureAwait(false);
            logger.LogInformation("[Ticket] applied migrations");
        }
    }

    public Task SeedAsync(CancellationToken ct) => Task.CompletedTask;
}