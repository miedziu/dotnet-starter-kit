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

public static class GetUsersListEndpoint
{
    internal static RouteHandlerBuilder MapGetUsersListEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/users", async (CancellationToken ct, IMediator mediator) =>
            TypedResults.Ok(await mediator.Send(new GetUsersQuery(), ct)))
        .WithName("ListUsers")
        .WithSummary("List users")
        .RequirePermission(IdentityPermissions.Users.View)
        .WithDescription("Retrieve a list of users.")
        .Produces<IEnumerable<UserDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}

public sealed class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, List<UserDto>>
{
    private readonly IUserService _userService;

    public GetUsersQueryHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async ValueTask<List<UserDto>> Handle(GetUsersQuery query, CancellationToken cancellationToken)
    {
        return await _userService.GetListAsync(cancellationToken).ConfigureAwait(false);
    }
}