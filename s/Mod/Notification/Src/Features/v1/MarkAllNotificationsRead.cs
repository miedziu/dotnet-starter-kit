using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Notifications.Contracts.Authorization;
using FSH.Modules.Notifications.Contracts.v1;
using FSH.Modules.Notifications.Data;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Notifications.Features.v1;

public static class MarkAllNotificationsReadEndpoint
{
    internal static RouteHandlerBuilder MapMarkAllNotificationsReadEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapPost("/read-all",
                async (IMediator mediator, CancellationToken ct) =>
                    Results.Ok(new { updated = await mediator.Send(new MarkAllNotificationsReadCommand(), ct) }))
            .WithName("MarkAllNotificationsRead")
            .WithSummary("Mark every unread notification for the caller as read; returns the count updated")
            .RequirePermission(NotificationPermissions.Inbox.MarkRead);
}

public sealed class MarkAllNotificationsReadCommandHandler(
    NotificationsDbContext db,
    ICurrentUser currentUser)
    : ICommandHandler<MarkAllNotificationsReadCommand, int>
{
    public async ValueTask<int> Handle(MarkAllNotificationsReadCommand cmd, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        var userId = currentUser.GetUserId();
        if (userId == Guid.Empty) throw new UnauthorizedException("no current user");
        var currentUserId = userId.ToString();

        var now = DateTime.UtcNow;
        // Single bulk UPDATE — no row materialization, no domain events fired (none for this aggregate).
        var updated = await db.Notifications
            .Where(n => n.UserId == currentUserId && n.ReadAtUtc == null)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.ReadAtUtc, now), cancellationToken)
            .ConfigureAwait(false);

        return updated;
    }
}