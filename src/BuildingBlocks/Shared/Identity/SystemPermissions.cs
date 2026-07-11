namespace FSH.Framework.Shared.Constants;

/// <summary>
/// Cross-cutting platform permissions that don't belong to a specific business module.
/// Registered automatically during <c>AddHeroPlatform</c>.
/// </summary>
public static class SystemPermissions
{
    public static class Hangfire
    {
        public const string Resource = nameof(Hangfire);
        public const string View = $"Permissions.{Resource}.View";
    }

    public static class Dashboard
    {
        public const string Resource = nameof(Dashboard);
        public const string View = $"Permissions.{Resource}.View";
    }

    public static class Platform
    {
        public const string Plans = $"{nameof(Platform)}.Plans";
        public const string Subscriptions = $"{nameof(Platform)}.Subscriptions";
        public const string Invoices = $"{nameof(Platform)}.Invoices";
        public const string Webhooks = $"{nameof(Platform)}.Webhooks";
        public const string Audits = $"{nameof(Platform)}.Audits";
        public const string Users = $"{nameof(Platform)}.Users";
    }

    public static IReadOnlyList<FshPermission> All { get; } =
    [
        new("View Hangfire",  ActionConstants.View, Hangfire.Resource,  IsBasic: true),
        new("View Dashboard", ActionConstants.View, Dashboard.Resource, IsBasic: true),
        new("Manage Plans",              "Manage",               Platform.Plans,         IsRoot: true),
        new("Manage Subscriptions",      "Manage",               Platform.Subscriptions, IsRoot: true),
        new("Admin All Invoices",        "Admin",                Platform.Invoices,      IsRoot: true),
        new("Admin All Webhooks",        "Admin",                Platform.Webhooks,      IsRoot: true),
    ];
}
