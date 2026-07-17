using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Identity.Contracts.Authorization;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Dtos;
using FSH.Modules.Identity.Contracts.v1.Users;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Identity.Features.v1.Users;

public static class GetUserByIdEndpoint
{
    internal static RouteHandlerBuilder MapGetUserByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/users/{id:guid}", async (string id, IMediator mediator, CancellationToken ct) =>
            TypedResults.Ok(await mediator.Send(new GetUserQuery(id), ct)))
        .WithName("GetUser")
        .WithSummary("Get user by ID")
        .RequirePermission(IdentityPermissions.Users.View)
        .WithDescription("Retrieve a user's profile details by unique user identifier.")
        .Produces<UserDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}

public sealed class GetUserByIdQueryHandler : IQueryHandler<GetUserQuery, UserDto>
{
    private readonly IUserService _userService;

    public GetUserByIdQueryHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async ValueTask<UserDto> Handle(GetUserQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        return await _userService.GetAsync(query.Id, cancellationToken).ConfigureAwait(false);
    }
}