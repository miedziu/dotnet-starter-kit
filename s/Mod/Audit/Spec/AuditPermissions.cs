using FSH.Framework.Shared.Constants;

namespace FSH.Mod.Audit.Spec;

public static class AuditPermissions
{
    public static class AuditTrails
    {
        public const string Resource = nameof(AuditTrails);
        public const string View = $"Permissions.{Resource}.View";
    }

    public static IReadOnlyList<FshPermission> All { get; } =
    [
        new("View Audit Trails", ActionConstants.View, AuditTrails.Resource, IsBasic: true),
    ];
}