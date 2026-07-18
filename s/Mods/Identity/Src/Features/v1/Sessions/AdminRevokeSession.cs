using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mods.Identity.Spec;
using FSH.Mods.Identity.Spec.Services;
using FSH.Mods.Identity.Spec.v1.Session;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace FSH.Mods.Identity.Features.v1.Sessions;

public static class AdminRevokeSessionEndpoint
{
    internal static RouteHandlerBuilder MapAdminRevokeSessionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/users/{userId:guid}/sessions/{sessionId:guid}", Handler)
        .WithName("AdminRevokeSession")
        .WithSummary("Revoke a user's session (Admin)")
        .RequirePermission(IdentityPermissions.Sessions.RevokeAll)
        .WithDescription("Revoke a specific session for a user. Requires admin permission.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<NoContent, NotFound>> Handler(
        Guid userId,
        Guid sessionId,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new AdminRevokeSessionCommand(userId, sessionId), ct);
        return result ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}

public sealed class AdminRevokeSessionCommandValidator : AbstractValidator<AdminRevokeSessionCommand>
{
    public AdminRevokeSessionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.")
            .When(x => x.Reason is not null);
    }
}

public sealed class AdminRevokeSessionCommandHandler : ICommandHandler<AdminRevokeSessionCommand, bool>
{
    private readonly ISessionService _sessionService;
    private readonly ICurrentUser _currentUser;

    public AdminRevokeSessionCommandHandler(ISessionService sessionService, ICurrentUser currentUser)
    {
        _sessionService = sessionService;
        _currentUser = currentUser;
    }

    public async ValueTask<bool> Handle(AdminRevokeSessionCommand command, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.GetUserId().ToString();

        // Get the session to verify it belongs to the specified user
        var session = await _sessionService.GetSessionAsync(command.SessionId, cancellationToken);
        if (session is null || session.UserId != command.UserId.ToString())
        {
            return false;
        }

        // Use the admin revocation method (doesn't check ownership)
        return await _sessionService.RevokeSessionForAdminAsync(
            command.SessionId,
            adminId,
            command.Reason ?? "Revoked by administrator",
            cancellationToken);
    }
}