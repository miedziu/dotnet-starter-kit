using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Identity.Contracts.Authorization;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Dtos;
using FSH.Modules.Identity.Contracts.v1.Sessions;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Identity.Features.v1.Sessions;

public static class GetUserSessionsEndpoint
{
    internal static RouteHandlerBuilder MapGetUserSessionsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/users/{userId:guid}/sessions", async (Guid userId, CancellationToken ct, IMediator mediator) =>
            TypedResults.Ok(await mediator.Send(new GetUserSessionsQuery(userId), ct)))
        .WithName("GetUserSessions")
        .WithSummary("Get user's sessions (Admin)")
        .RequirePermission(IdentityPermissions.Sessions.ViewAll)
        .WithDescription("Retrieve all active sessions for a specific user. Requires admin permission.")
        .Produces<IEnumerable<UserSessionDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}

public sealed class GetUserSessionsQueryHandler : IQueryHandler<GetUserSessionsQuery, List<UserSessionDto>>
{
    private readonly ISessionService _sessionService;

    public GetUserSessionsQueryHandler(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public async ValueTask<List<UserSessionDto>> Handle(GetUserSessionsQuery query, CancellationToken cancellationToken)
    {
        return await _sessionService.GetUserSessionsForAdminAsync(query.UserId.ToString(), cancellationToken);
    }
}