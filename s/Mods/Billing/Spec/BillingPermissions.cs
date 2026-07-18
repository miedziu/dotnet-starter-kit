using FSH.Framework.Shared.Identity;

namespace FSH.Mods.Billing.Spec;

public static class BillingPermissions
{
    public const string Resource = "Billing";
    public const string View = $"Permissions.{Resource}.View";
    public const string Manage = $"Permissions.{Resource}.Manage";

    public static IReadOnlyList<FshPermission> All { get; } =
    [
        new("View Billing",   ActionConstants.View, Resource, IsBasic: true),
        new("Manage Billing", ActionConstants.Manage, Resource),
    ];
}