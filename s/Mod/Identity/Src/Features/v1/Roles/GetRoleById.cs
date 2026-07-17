using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Identity.Contracts.Authorization;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Dtos;
using FSH.Modules.Identity.Contracts.v1.Roles;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Identity.Features.v1.Roles;

public static class GetRoleByIdEndpoint
{
    public static RouteHandlerBuilder MapGetRoleByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/roles/{id:guid}", async (string id, IMediator mediator, CancellationToken ct) =>
            TypedResults.Ok(await mediator.Send(new GetRoleQuery(id), ct)))
        .WithName("GetRole")
        .WithSummary("Get role by ID")
        .RequirePermission(IdentityPermissions.Roles.View)
        .WithDescription("Retrieve details of a specific role by its unique identifier.")
        .Produces<RoleDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}

public sealed class GetRoleByIdQueryHandler : IQueryHandler<GetRoleQuery, RoleDto?>
{
    private readonly IRoleService _roleService;

    public GetRoleByIdQueryHandler(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public async ValueTask<RoleDto?> Handle(GetRoleQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        return await _roleService.GetRoleAsync(query.Id, cancellationToken).ConfigureAwait(false);
    }
}