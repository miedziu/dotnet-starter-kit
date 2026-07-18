using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mod.Identity.Spec.Services;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Mod.Identity.Features.v1.Sessions;

public static class AdminRevokeAllSessionsEndpoint
{
    internal static RouteHandlerBuilder MapAdminRevokeAllSessionsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/users/{userId:guid}/sessions/revoke-all", async (Guid userId, AdminRevokeAllSessionsCommand? command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command ?? new AdminRevokeAllSessionsCommand(userId), ct);
            return TypedResults.Ok(new { RevokedCount = result });
        })
        .WithName("AdminRevokeAllSessions")
        .WithSummary("Revoke all user's sessions (Admin)")
        .RequirePermission(IdentityPermissions.Sessions.RevokeAll)
        .WithDescription("Revoke all sessions for a specific user. Requires admin permission.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}

public sealed class AdminRevokeAllSessionsCommandValidator : AbstractValidator<AdminRevokeAllSessionsCommand>
{
    public AdminRevokeAllSessionsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.")
            .When(x => x.Reason is not null);
    }
}

public sealed class AdminRevokeAllSessionsCommandHandler : ICommandHandler<AdminRevokeAllSessionsCommand, int>
{
    private readonly ISessionService _sessionService;
    private readonly ICurrentUser _currentUser;

    public AdminRevokeAllSessionsCommandHandler(ISessionService sessionService, ICurrentUser currentUser)
    {
        _sessionService = sessionService;
        _currentUser = currentUser;
    }

    public async ValueTask<int> Handle(AdminRevokeAllSessionsCommand command, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.GetUserId().ToString();
        return await _sessionService.RevokeAllSessionsForAdminAsync(
            command.UserId.ToString(),
            adminId,
            command.Reason ?? "Revoked by administrator",
            cancellationToken);
    }
}