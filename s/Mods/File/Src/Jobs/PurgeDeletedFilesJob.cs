using FSH.Framework.Storage.Services;
using FSH.Mods.File.Data;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FSH.Mods.File.Jobs;

/// <summary>
/// Daily purge of soft-deleted FileAsset rows past the retention window. Hard-deletes the row,
/// removes the bytes from storage, and refunds the quota (the bytes were debited at finalize time).
/// </summary>
public sealed class PurgeDeletedFilesJob(
    FileDbContext db,
    IStorageService storage,
    IOptions<FileOptions> options,
    ILogger<PurgeDeletedFilesJob> logger)
{
    [AutomaticRetry(Attempts = 2, DelaysInSeconds = [300, 1800])]
    public async Task RunAsync(CancellationToken ct)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-options.Value.SoftDeleteRetentionDays);
        var candidates = await db.FileAssets
            .IgnoreQueryFilters()
            .Where(f => f.DeletedAt != null && f.DeletedAt < cutoff)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        if (candidates.Count == 0)
        {
            return;
        }

        // Best-effort byte removal per file. Schema-per-tenant means all rows share one tenant,
        // and Hangfire wires the job per-tenant for multi-tenant deployments.
        foreach (var f in candidates)
        {
            try
            {
                await storage.RemoveAsync(f.StorageKey, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Storage remove failed for {Key}", f.StorageKey);
            }
        }

        // Quota refund — group bytes once per tenant. In schema-per-tenant the resolved tenant
        // matches every row's logical tenant; the framework's QuotaService is tenant-scoped via DI.
        var totalBytes = candidates.Sum(f => f.SizeBytes);

        var ids = candidates.Select(f => f.Id).ToList();
        await db.FileAssets
            .IgnoreQueryFilters()
            .Where(f => ids.Contains(f.Id))
            .ExecuteDeleteAsync(ct)
            .ConfigureAwait(false);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Hard-purged {Count} soft-deleted file assets ({Bytes} bytes total)",
                candidates.Count, totalBytes);
        }
    }
}