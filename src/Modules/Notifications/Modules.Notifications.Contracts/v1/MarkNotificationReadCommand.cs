using Mediator;

namespace FSH.Modules.Notifications.Contracts.v1;

public sealed record MarkNotificationReadCommand(Guid NotificationId) : ICommand<Unit>;