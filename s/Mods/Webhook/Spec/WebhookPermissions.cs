using FSH.Framework.Shared.Identity;

namespace FSH.Mods.Webhook.Spec;

public static class WebhookPermissions
{
    public static class Subscriptions
    {
        public const string Resource = "Webhook";
        public const string View = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Delete = $"Permissions.{Resource}.Delete";
        public const string Test = $"Permissions.{Resource}.Test";
    }

    public static IReadOnlyList<FshPermission> All { get; } =
    [
        new("View Webhook",    ActionConstants.View,   Subscriptions.Resource, IsBasic: true),
        new("Create Webhook",  ActionConstants.Create, Subscriptions.Resource),
        new("Delete Webhook",  ActionConstants.Delete, Subscriptions.Resource),
        new("Test Webhook",    ActionConstants.Test,   Subscriptions.Resource),
    ];
}