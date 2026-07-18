using FSH.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FSH.Mod.Webhook.Data;

public sealed class WebhookDbInitializer(
    WebhookDbContext dbContext,
    ILogger<WebhookDbInitializer> logger) : IDbInitializer
{
    public async Task MigrateAsync(CancellationToken ct)
    {
        if ((await dbContext.Database.GetPendingMigrationsAsync(ct).ConfigureAwait(false)).Any())
        {
            await dbContext.Database.MigrateAsync(ct).ConfigureAwait(false);
            logger.LogInformation("[Webhook] applied migrations");
        }
    }

    public Task SeedAsync(CancellationToken ct) => Task.CompletedTask;
}