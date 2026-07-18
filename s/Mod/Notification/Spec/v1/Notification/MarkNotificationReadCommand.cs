using Mediator;

namespace FSH.Mod.Notification.Spec.v1.Notification;

public sealed record MarkNotificationReadCommand(Guid NotificationId) : ICommand<Unit>;