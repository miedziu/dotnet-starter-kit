using FSH.Framework.Shared.Constants;
using FSH.Framework.Shared.Identity.Authorization;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Mod.Identity.Features.v1.Permissions;

public static class GetPermissionCatalogEndpoint
{
    internal static RouteHandlerBuilder MapGetPermissionCatalogEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/permissions/catalog", async (IMediator mediator, CancellationToken ct) =>
            TypedResults.Ok(await mediator.Send(new GetPermissionCatalogQuery(), ct)))
        .WithName("GetPermissionCatalog")
        .WithSummary("Get permission catalog")
        .RequirePermission(IdentityPermissions.Roles.View)
        .WithDescription("Returns every permission registered in the host.")
        .Produces<IReadOnlyList<PermissionCatalogEntryDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}

public sealed class GetPermissionCatalogQueryHandler()
    : IQueryHandler<GetPermissionCatalogQuery, IReadOnlyList<PermissionCatalogEntryDto>>
{
    public ValueTask<IReadOnlyList<PermissionCatalogEntryDto>> Handle(
        GetPermissionCatalogQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Matches the same root-vs-admin rule used by RolePermissionSyncer so the catalog the
        // SPA edits agrees with the set the syncer would push into a tenant's role claims.
        //var source = PermissionConstants.Admin; //
        var source = PermissionConstants.Admin.Concat(PermissionConstants.Root).DistinctBy(p => p.Name);

        IReadOnlyList<PermissionCatalogEntryDto> result =
        [
            .. source.Select(p => new PermissionCatalogEntryDto(
                p.Name,
                p.Description,
                p.Resource,
                p.Action,
                p.IsBasic,
                p.IsRoot))
        ];

        return ValueTask.FromResult(result);
    }
}