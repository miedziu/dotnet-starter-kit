using FSH.Framework.Core.Context;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Identity.Contracts.Authorization;
using FSH.Modules.Identity.Contracts.DTOs;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Sessions.GetMySessions;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Identity.Features.v1.Sessions;

public static class GetMySessionsEndpoint
{
    internal static RouteHandlerBuilder MapGetMySessionsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/sessions/me", async (CancellationToken ct, IMediator mediator) =>
            TypedResults.Ok(await mediator.Send(new GetMySessionsQuery(), ct)))
        .WithName("GetMySessions")
        .WithSummary("Get current user's sessions")
        .RequirePermission(IdentityPermissions.Sessions.View)
        .WithDescription("Retrieve all active sessions for the currently authenticated user.")
        .Produces<IEnumerable<UserSessionDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}

public sealed class GetMySessionsQueryHandler : IQueryHandler<GetMySessionsQuery, List<UserSessionDto>>
{
    private readonly ISessionService _sessionService;
    private readonly ICurrentUser _currentUser;

    public GetMySessionsQueryHandler(ISessionService sessionService, ICurrentUser currentUser)
    {
        _sessionService = sessionService;
        _currentUser = currentUser;
    }

    public async ValueTask<List<UserSessionDto>> Handle(GetMySessionsQuery query, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId().ToString();
        return await _sessionService.GetUserSessionsAsync(userId, cancellationToken);
    }
}