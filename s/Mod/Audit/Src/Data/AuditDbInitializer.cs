using FSH.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FSH.Modules.Auditing.Persistence;

internal sealed class AuditDbInitializer(
    ILogger<AuditDbInitializer> logger,
    AuditDbContext context) : IDbInitializer
{
    public async Task MigrateAsync(CancellationToken ct)
    {
        if ((await context.Database.GetPendingMigrationsAsync(ct).ConfigureAwait(false)).Any())
        {
            await context.Database.MigrateAsync(ct).ConfigureAwait(false);
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("applied database migrations for audit module");
            }
        }
    }

    public Task SeedAsync(CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}