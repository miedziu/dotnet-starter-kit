namespace FSH.Mod.Notification.Features.v1.Internal;

internal static class NotificationMappers
{
    public static NotificationDto ToDto(this Notification n) =>
        new(n.Id, n.Type, n.Title, n.Body, n.Link, n.Source, n.MetadataJson, n.ReadAtUtc, n.CreatedAtUtc);
}