using FSH.Framework.Eventing.Abstractions;
using FSH.Mods.Identity.Domain.Events;
using FSH.Mods.Identity.Spec.Events;
using Mediator;
using Microsoft.Extensions.Logging;

namespace FSH.Mods.Identity.Events;

/// <summary>
/// Handles the UserRegisteredEvent domain event by publishing an integration event
/// so other modules can react to new user registrations.
/// </summary>
public sealed class UserRegisteredHandler(
    IEventBus eventBus,
    ILogger<UserRegisteredHandler> logger)
    : INotificationHandler<UserRegisteredEvent>
{
    public async ValueTask Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (logger.IsEnabled(LogLevel.Information))
        {
            // PII minimization: log the pseudonymous UserId only, not the email address.
            logger.LogInformation(
                "User registered: {UserId} ",
                notification.UserId);
        }

        var integrationEvent = new UserRegisteredIntegrationEvent(
            Id: notification.EventId,
            OccurredAt: notification.OccurredAt.UtcDateTime,
            CorrelationId: notification.CorrelationId ?? notification.EventId.ToString(),
            Source: nameof(UserRegisteredHandler),
            UserId: notification.UserId,
            Email: notification.Email,
            FirstName: notification.FirstName ?? string.Empty,
            LastName: notification.LastName ?? string.Empty);

        await eventBus.PublishAsync(integrationEvent, cancellationToken).ConfigureAwait(false);
    }
}