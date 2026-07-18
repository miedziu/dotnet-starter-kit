using FluentValidation;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Shared.Persistence;
using FSH.Mod.File.Data;
using FSH.Mod.File.Features.v1.Internal;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mod.File.Features.v1;

public static class ListTrashedFilesEndpoint
{
    internal static RouteHandlerBuilder MapListTrashedFilesEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapGet("/trash",
                async (int? pageNumber, int? pageSize, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new ListTrashedFilesQuery(pageNumber ?? 1, pageSize ?? 20), ct)))
            .WithName("ListTrashedFiles")
            .WithSummary("List soft-deleted files (admin/trash view)")
            .RequirePermission(FilePermissions.ViewTrash);
}

public sealed class ListTrashedFilesQueryValidator : AbstractValidator<ListTrashedFilesQuery>
{
    public ListTrashedFilesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}

public sealed class ListTrashedFilesQueryHandler(FileDbContext db)
    : IQueryHandler<ListTrashedFilesQuery, PagedResponse<FileAssetDto>>
{
    public async ValueTask<PagedResponse<FileAssetDto>> Handle(ListTrashedFilesQuery q, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(q);

        int page = q.PageNumber < 1 ? 1 : q.PageNumber;
        int size = q.PageSize is < 1 or > 200 ? 20 : q.PageSize;

        // IgnoreQueryFilters because the SoftDelete filter would otherwise hide deleted rows —
        // exactly what we DO want here. Scoping is preserved via the DbContext.
        var baseQuery = db.FileAssets
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(f => f.DeletedAt != null)
            .OrderByDescending(f => f.DeletedAt);

        long total = await baseQuery.LongCountAsync(cancellationToken).ConfigureAwait(false);

        var rows = await baseQuery
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResponse<FileAssetDto>
        {
            Items = rows.Select(f => FileAssetMapper.ToDto(f)).ToList(),
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size),
        };
    }
}