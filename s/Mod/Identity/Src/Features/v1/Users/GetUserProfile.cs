using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Claims;
using FSH.Mod.Identity.Spec.Services;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace FSH.Mod.Identity.Features.v1.Users;

public static class GetUserProfileEndpoint
{
    internal static RouteHandlerBuilder MapGetMeEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/profile", async (ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            if (user.GetUserId() is not { } userId || string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException();
            }

            return TypedResults.Ok(await mediator.Send(new GetCurrentUserProfileQuery(userId), ct));
        })
        .WithName("GetCurrentUserProfile")
        .WithSummary("Get current user profile")
        .WithDescription("Retrieve the authenticated user's profile from the access token.")
        .RequireAuthorization()
        .Produces<UserDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}

public sealed class GetCurrentUserProfileQueryHandler : IQueryHandler<GetCurrentUserProfileQuery, UserDto>
{
    private readonly IUserService _userService;

    public GetCurrentUserProfileQueryHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async ValueTask<UserDto> Handle(GetCurrentUserProfileQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        return await _userService.GetAsync(query.UserId, cancellationToken).ConfigureAwait(false);
    }
}