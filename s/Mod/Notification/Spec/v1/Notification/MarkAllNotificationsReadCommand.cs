using Mediator;

namespace FSH.Mod.Notification.Spec.v1.Notification;

/// <summary>Mark every unread notification for the caller as read. Returns the count updated.</summary>
public sealed record MarkAllNotificationsReadCommand : ICommand<int>;