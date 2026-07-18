using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Storage.Services;
using FSH.Mod.File.Data;
using FSH.Mod.File.Services;
using FSH.Mod.File.Spec;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FSH.Mod.File.Features.v1;

public static class GetFileDownloadUrlEndpoint
{
    internal static RouteHandlerBuilder MapGetFileDownloadUrlEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapGet("/{id:guid}/url",
                async (Guid id, bool? inline, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new GetFileDownloadUrlQuery(id, inline ?? false), ct)))
            .WithName("GetFileDownloadUrl")
            .WithSummary("Mint a short-lived presigned download URL")
            .WithDescription("Default disposition is attachment (click-to-save). Pass ?inline=true to get an inline disposition for browser preview (PDF viewer, image render, etc.).")
            .RequireAuthorization();
}

public sealed class GetFileDownloadUrlQueryHandler(
    FileDbContext db,
    IStorageService storage,
    FileAccessPolicyRegistry policies,
    ICurrentUser currentUser,
    IOptions<FileOptions> options)
    : IQueryHandler<GetFileDownloadUrlQuery, PresignedDownloadResponse>
{
    public async ValueTask<PresignedDownloadResponse> Handle(GetFileDownloadUrlQuery q, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(q);

        var f = await db.FileAssets.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == q.FileAssetId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("file not found");

        var userId = currentUser.GetUserId().ToString();
        var policy = policies.Resolve(f.OwnerType)
            ?? throw new NotFoundException("file not found");

        var ctx = new FileAccessContext(f.Id, f.OwnerType, f.OwnerId, f.CreatedByUserId, (int)f.Visibility);
        if (!await policy.CanReadAsync(ctx, userId, cancellationToken).ConfigureAwait(false))
        {
            throw new NotFoundException("file not found");
        }

        var ttl = TimeSpan.FromMinutes(options.Value.DownloadUrlTtlMinutes);
        var mode = q.Inline ? "inline" : "attachment";
        var disposition = $"{mode}; filename=\"{f.OriginalFileName}\"";
        var url = await storage.GenerateDownloadUrlAsync(f.StorageKey, ttl, disposition, cancellationToken).ConfigureAwait(false);
        return new PresignedDownloadResponse(url, DateTimeOffset.UtcNow.Add(ttl));
    }
}