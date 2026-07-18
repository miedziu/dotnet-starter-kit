using Mediator;

namespace FSH.Mods.Notification.Spec.v1.Notifications;

/// <summary>Bell badge count — number of caller's unread notifications.</summary>
public sealed record GetUnreadCountQuery : IQuery<int>;