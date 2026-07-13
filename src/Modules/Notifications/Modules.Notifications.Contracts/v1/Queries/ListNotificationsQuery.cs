using FSH.Modules.Notifications.Contracts.v1.DTOs;
using Mediator;
using System.Collections.ObjectModel;

namespace FSH.Modules.Notifications.Contracts.v1.Queries;

/// <summary>
/// Inbox list scoped to the caller. <paramref name="UnreadOnly"/> filters to <c>ReadAtUtc IS NULL</c>;
/// otherwise the full mix is returned, newest first.
/// </summary>
public sealed record ListNotificationsQuery(bool UnreadOnly = false, int Page = 1, int PageSize = 50)
    : IQuery<ReadOnlyCollection<NotificationDto>>;
