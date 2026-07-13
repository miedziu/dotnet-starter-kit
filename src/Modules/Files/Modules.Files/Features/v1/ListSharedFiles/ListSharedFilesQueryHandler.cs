using FSH.Framework.Storage.Services;
using FSH.Modules.Files.Contracts.v1.DTOs;
using FSH.Modules.Files.Contracts.v1.Queries;
using FSH.Modules.Files.Data;
using FSH.Modules.Files.Features.v1.Internal;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace FSH.Modules.Files.Features.v1.ListSharedFiles;

/// <summary>
/// Returns Public, Available files belonging to the built-in owner types so the SPA
/// can render a "Shared files" surface alongside "My files". Scoping is handled by
/// the framework's BaseDbContext, not by a WHERE clause here.
/// </summary>
public sealed class ListSharedFilesQueryHandler(FilesDbContext db, IStorageService storage)
    : IQueryHandler<ListSharedFilesQuery, ReadOnlyCollection<FileAssetDto>>
{
    // Owner types that represent "free-standing" files (not bound to a domain entity).
    // Catalog/Tickets/Chat attachments are intentionally excluded — their visibility is a
    // function of their owning entity's access policy, not a free-standing share decision.
    private static readonly string[] SharedOwnerTypes = ["MyFiles", "User"];

    public async ValueTask<ReadOnlyCollection<FileAssetDto>> Handle(ListSharedFilesQuery q, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(q);

        var page = Math.Max(1, q.Page);
        var pageSize = Math.Clamp(q.PageSize, 1, 100);

        var rows = await db.FileAssets.AsNoTracking()
            .Where(f => f.Visibility == Visibility.Public
                && f.Status == FileAssetStatus.Available
                && SharedOwnerTypes.Contains(f.OwnerType))
            .OrderByDescending(f => f.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
            .Select(f => FileAssetMapper.ToDto(f, storage.BuildPublicUrl(f.StorageKey)))
            .ToList()
            .AsReadOnly();
    }
}