using Mediator;

namespace FSH.Mod.Notification.Spec.v1.Notification;

/// <summary>Bell badge count — number of caller's unread notifications.</summary>
public sealed record GetUnreadCountQuery : IQuery<int>;