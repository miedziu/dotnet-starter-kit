using Mediator;

namespace FSH.Mods.Notification.Spec.v1.Notifications;

public sealed record MarkNotificationReadCommand(Guid NotificationId) : ICommand<Unit>;