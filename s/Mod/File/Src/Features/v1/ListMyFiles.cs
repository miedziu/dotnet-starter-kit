using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Storage.Services;
using FSH.Modules.Files.Contracts.Authorization;
using FSH.Modules.Files.Contracts.v1.Dtos;
using FSH.Modules.Files.Contracts.v1.Queries;
using FSH.Modules.Files.Data;
using FSH.Modules.Files.Features.v1.Internal;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace FSH.Modules.Files.Features.v1;

public static class ListMyFilesEndpoint
{
    internal static RouteHandlerBuilder MapListMyFilesEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapGet("/mine",
                async (int? page, int? pageSize, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new ListMyFilesQuery(page ?? 1, pageSize ?? 20), ct)))
            .WithName("ListMyFiles")
            .WithSummary("List files uploaded by the current user")
            .RequirePermission(FilesPermissions.Upload);
}

public sealed class ListMyFilesQueryValidator : AbstractValidator<ListMyFilesQuery>
{
    public ListMyFilesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class ListMyFilesQueryHandler(
    FilesDbContext db,
    ICurrentUser currentUser,
    IStorageService storage)
    : IQueryHandler<ListMyFilesQuery, ReadOnlyCollection<FileAssetDto>>
{
    public async ValueTask<ReadOnlyCollection<FileAssetDto>> Handle(ListMyFilesQuery q, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(q);
        var userId = currentUser.GetUserId().ToString();
        if (string.IsNullOrEmpty(userId) || userId == Guid.Empty.ToString())
        {
            throw new UnauthorizedException("no current user");
        }

        var page = Math.Max(1, q.Page);
        var pageSize = Math.Clamp(q.PageSize, 1, 100);

        var rows = await db.FileAssets.AsNoTracking()
            .Where(f => f.CreatedByUserId == userId && f.Status == FileAssetStatus.Available)
            .OrderByDescending(f => f.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Seed publicUrl for public files so the preview dialog can paint the image
        // immediately from the list data, without waiting on a metadata refetch to mint it.
        return rows
            .Select(f => FileAssetMapper.ToDto(
                f,
                f.Visibility == Visibility.Public ? storage.BuildPublicUrl(f.StorageKey) : null))
            .ToList()
            .AsReadOnly();
    }
}