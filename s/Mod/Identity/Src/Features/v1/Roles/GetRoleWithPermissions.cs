using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mod.Identity.Spec.Services;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Mod.Identity.Features.v1.Roles;

public static class GetRolePermissionsEndpoint
{
    public static RouteHandlerBuilder MapGetRolePermissionsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/{id:guid}/permissions", async (string id, IMediator mediator, CancellationToken ct) =>
            TypedResults.Ok(await mediator.Send(new GetRoleWithPermissionsQuery(id), ct)))
        .WithName("GetRolePermissions")
        .WithSummary("Get role permissions")
        .RequirePermission(IdentityPermissions.Roles.View)
        .WithDescription("Retrieve a role along with its assigned permissions.")
        .Produces<RoleDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}

public sealed class GetRoleWithPermissionsQueryHandler : IQueryHandler<GetRoleWithPermissionsQuery, RoleDto>
{
    private readonly IRoleService _roleService;

    public GetRoleWithPermissionsQueryHandler(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public async ValueTask<RoleDto> Handle(GetRoleWithPermissionsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        return await _roleService.GetWithPermissionsAsync(query.Id, cancellationToken).ConfigureAwait(false);
    }
}