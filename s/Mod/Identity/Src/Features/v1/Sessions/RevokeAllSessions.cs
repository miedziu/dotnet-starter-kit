using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Identity.Contracts.Authorization;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Sessions;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Identity.Features.v1.Sessions;

public static class RevokeAllSessionsEndpoint
{
    internal static RouteHandlerBuilder MapRevokeAllSessionsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/sessions/revoke-all", async (RevokeAllSessionsCommand? command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command ?? new RevokeAllSessionsCommand(), ct);
            return TypedResults.Ok(new { RevokedCount = result });
        })
        .WithName("RevokeAllSessions")
        .WithSummary("Revoke all sessions")
        .RequirePermission(IdentityPermissions.Sessions.Revoke)
        .WithDescription("Revoke all sessions for the currently authenticated user except the current one.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}

public sealed class RevokeAllSessionsCommandValidator : AbstractValidator<RevokeAllSessionsCommand>
{
    public RevokeAllSessionsCommandValidator()
    {
        // ExceptSessionId is optional - no validation required
        // This validator exists for consistency and potential future validation rules
    }
}

public sealed class RevokeAllSessionsCommandHandler : ICommandHandler<RevokeAllSessionsCommand, int>
{
    private readonly ISessionService _sessionService;
    private readonly ICurrentUser _currentUser;

    public RevokeAllSessionsCommandHandler(ISessionService sessionService, ICurrentUser currentUser)
    {
        _sessionService = sessionService;
        _currentUser = currentUser;
    }

    public async ValueTask<int> Handle(RevokeAllSessionsCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId().ToString();
        return await _sessionService.RevokeAllSessionsAsync(
            userId,
            userId,
            command.ExceptSessionId,
            "User requested logout from all devices",
            cancellationToken);
    }
}