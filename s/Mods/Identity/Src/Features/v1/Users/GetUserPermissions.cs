using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Claims;
using FSH.Mods.Identity.Spec.Services;
using FSH.Mods.Identity.Spec.v1.User;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace FSH.Mods.Identity.Features.v1.Users;

public static class GetUserPermissionsEndpoint
{
    // No RequirePermission on purpose: returns the *caller's* own permissions (the SPA needs them to render
    // gated routes); gating behind Users.View would lock out non-user-managing roles. Fallback policy → 401.
    internal static RouteHandlerBuilder MapGetCurrentUserPermissionsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/permissions", async (ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            if (user.GetUserId() is not { } userId || string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException();
            }

            return TypedResults.Ok(await mediator.Send(new GetCurrentUserPermissionsQuery(userId), ct));
        })
        .WithName("GetCurrentUserPermissions")
        .WithSummary("Get current user permissions")
        .WithDescription("Retrieve permissions for the authenticated user. Requires authentication only — every signed-in user can read their own grants.")
        .Produces<IEnumerable<string>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}

public sealed class GetCurrentUserPermissionsQueryHandler : IQueryHandler<GetCurrentUserPermissionsQuery, List<string>?>
{
    private readonly IUserService _userService;

    public GetCurrentUserPermissionsQueryHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async ValueTask<List<string>?> Handle(GetCurrentUserPermissionsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        return await _userService.GetPermissionsAsync(query.UserId, cancellationToken).ConfigureAwait(false);
    }
}