using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FSH.Modules.Identity.Authorization;

/// <summary>
/// Runs once on host startup:adds any permission claims that
/// have been registered via <see cref="FSH.Framework.Shared.Constants.PermissionConstants"/>
/// but are missing from the role claims table. Idempotent and lightweight —
/// only writes when there's something new, so it's safe to run unconditionally.
/// </summary>
/// <remarks>
/// Implemented as a <see cref="BackgroundService"/> so it does not block host startup.
/// In production, the tenant catalog is migrated by the standalone <c>FSH.Starter.DbMigrator</c>
/// console application before the API process starts, so the tenant store is already
/// populated when this service runs. The polling loop covers test environments and the
/// brief window during local Aspire startup where catalog migration may overlap with
/// other startup work.
/// </remarks>
internal sealed class RolePermissionSyncHostedService(
    IServiceProvider serviceProvider,
    ILogger<RolePermissionSyncHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var syncer = scope.ServiceProvider.GetRequiredService<RolePermissionSyncer>();
            await syncer.SyncAsync(stoppingToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Per-tenant failure must not stop the rest of the loop.
            logger.LogError(ex, "Role permission sync failed");
        }
    }
}
