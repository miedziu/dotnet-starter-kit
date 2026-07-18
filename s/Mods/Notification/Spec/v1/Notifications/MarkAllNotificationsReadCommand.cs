using Mediator;

namespace FSH.Mods.Notification.Spec.v1.Notifications;

/// <summary>Mark every unread notification for the caller as read. Returns the count updated.</summary>
public sealed record MarkAllNotificationsReadCommand : ICommand<int>;