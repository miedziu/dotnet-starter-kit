using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Files.Contracts.Authorization;
using FSH.Modules.Files.Contracts.v1.Queries;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using FluentValidation;
using FSH.Framework.Storage.Services;
using FSH.Modules.Files.Contracts.v1.DTOs;
using FSH.Modules.Files.Data;
using FSH.Modules.Files.Features.v1.Internal;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace FSH.Modules.Files.Features.v1;

public static class ListSharedFilesEndpoint
{
    internal static RouteHandlerBuilder MapListSharedFilesEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapGet("/shared",
                async (int? page, int? pageSize, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new ListSharedFilesQuery(page ?? 1, pageSize ?? 20), ct)))
            .WithName("ListSharedFiles")
            .WithSummary("List Public files in this tenant (the 'Shared' view)")
            .RequirePermission(FilesPermissions.Upload);
}

public sealed class ListSharedFilesQueryValidator : AbstractValidator<ListSharedFilesQuery>
{
    public ListSharedFilesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

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