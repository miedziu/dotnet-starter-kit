using FSH.Framework.Shared.Identity;

namespace FSH.Mods.Notification.Spec;

/// <summary>
/// Permission constants for the Notification module. Permissions follow the
/// <c>Permissions.{Resource}.{Action}</c> shape per framework convention.
/// </summary>
public static class NotificationPermissions
{
    public static class Inbox
    {
        public const string Resource = "Notification.Inbox";
        public const string View = $"Permissions.{Resource}.View";
        public const string MarkRead = $"Permissions.{Resource}.MarkRead";
    }

    public static IReadOnlyList<FshPermission> All { get; } =
    [
        new("View Notifications",      ActionConstants.View, Inbox.Resource, IsBasic: true),
        new("Mark Notifications Read", ActionConstants.MarkRead, Inbox.Resource, IsBasic: true),
    ];
}