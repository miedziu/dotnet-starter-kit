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

public static class GetUnreadCountEndpoint
{
    internal static RouteHandlerBuilder MapGetUnreadCountEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapGet("/unread-count",
                async (IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new GetUnreadCountQuery(), ct)))
            .WithName("GetUnreadNotificationCount")
            .WithSummary("Count of caller's unread notifications (bell badge)")
            .RequirePermission(NotificationPermissions.Inbox.View);
}

public sealed class GetUnreadCountQueryHandler(
    NotificationsDbContext db,
    ICurrentUser currentUser)
    : IQueryHandler<GetUnreadCountQuery, int>
{
    public async ValueTask<int> Handle(GetUnreadCountQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var userId = currentUser.GetUserId();
        if (userId == Guid.Empty) throw new UnauthorizedException("no current user");
        var currentUserId = userId.ToString();

        return await db.Notifications.AsNoTracking()
            .CountAsync(n => n.UserId == currentUserId && n.ReadAtUtc == null, cancellationToken)
            .ConfigureAwait(false);
    }
}