using FSH.Mods.Notification.Spec.v1;

namespace FSH.Mods.Notification.Features.v1.Internal;

internal static class NotificationMappers
{
    public static NotificationDto ToDto(this Domain.Notification n) =>
        new(n.Id, n.Type, n.Title, n.Body, n.Link, n.Source, n.MetadataJson, n.ReadAtUtc, n.CreatedAtUtc);
}