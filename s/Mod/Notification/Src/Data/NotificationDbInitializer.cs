using FSH.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FSH.Mod.Notification.Data;

public sealed class NotificationDbInitializer(
    NotificationDbContext dbContext,
    ILogger<NotificationDbInitializer> logger) : IDbInitializer
{
    public async Task MigrateAsync(CancellationToken ct)
    {
        if ((await dbContext.Database.GetPendingMigrationsAsync(ct).ConfigureAwait(false)).Any())
        {
            await dbContext.Database.MigrateAsync(ct).ConfigureAwait(false);
            logger.LogInformation("[Notification] applied migrations");
        }
    }

    public Task SeedAsync(CancellationToken ct) => Task.CompletedTask;
}