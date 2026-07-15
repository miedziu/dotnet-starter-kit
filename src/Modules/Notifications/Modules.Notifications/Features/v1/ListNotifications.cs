using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Notifications.Contracts.Authorization;
using FSH.Modules.Notifications.Contracts.v1;
using FSH.Modules.Notifications.Contracts.v1.Dtos;
using FSH.Modules.Notifications.Data;
using FSH.Modules.Notifications.Features.v1.Internal;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace FSH.Modules.Notifications.Features.v1;

public static class ListNotificationsEndpoint
{
    internal static RouteHandlerBuilder MapListNotificationsEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapGet("/",
                async (bool? unreadOnly, int? page, int? pageSize, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(
                        new ListNotificationsQuery(unreadOnly ?? false, page ?? 1, pageSize ?? 50),
                        ct)))
            .WithName("ListNotifications")
            .WithSummary("List the caller's notifications (newest first)")
            .RequirePermission(NotificationPermissions.Inbox.View);
}

public sealed class ListNotificationsQueryValidator : AbstractValidator<ListNotificationsQuery>
{
    public ListNotificationsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}

public sealed class ListNotificationsQueryHandler(
    NotificationsDbContext db,
    ICurrentUser currentUser)
    : IQueryHandler<ListNotificationsQuery, ReadOnlyCollection<NotificationDto>>
{
    public async ValueTask<ReadOnlyCollection<NotificationDto>> Handle(ListNotificationsQuery q, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(q);
        var userId = currentUser.GetUserId();
        if (userId == Guid.Empty) throw new UnauthorizedException("no current user");
        var currentUserId = userId.ToString();

        int page = Math.Max(1, q.Page);
        int pageSize = Math.Clamp(q.PageSize, 1, 200);

        var query = db.Notifications.AsNoTracking()
            .Where(n => n.UserId == currentUserId);

        if (q.UnreadOnly)
        {
            query = query.Where(n => n.ReadAtUtc == null);
        }

        var rows = await query
            .OrderByDescending(n => n.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(n => n.ToDto()).ToList().AsReadOnly();
    }
}