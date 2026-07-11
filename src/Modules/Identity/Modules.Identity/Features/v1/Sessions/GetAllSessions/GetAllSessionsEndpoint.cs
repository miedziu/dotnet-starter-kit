using FSH.Modules.Identity.Contracts.Authorization;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Identity.Contracts.DTOs;
using FSH.Modules.Identity.Contracts.v1.Sessions.GetAllSessions;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Identity.Features.v1.Sessions.GetAllSessions;

public static class GetAllSessionsEndpoint
{
    internal static RouteHandlerBuilder MapGetAllSessionsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/sessions", async (CancellationToken cancellationToken, IMediator mediator) =>
            TypedResults.Ok(await mediator.Send(new GetAllSessionsQuery(), cancellationToken)))
        .WithName("GetAllSessions")
        .WithSummary("Get all sessions (Admin)")
        .RequirePermission(IdentityPermissions.Sessions.ViewAll)
        .WithDescription("Returns paged sessions across the application, filterable by active state and a free-text search across user name, email, and IP address.")
        .Produces<IEnumerable<UserSessionDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}