using FSH.Framework.Shared.Identity;

namespace FSH.Mods.File.Spec;

/// <summary>
/// Permission constants + the registry entry consumed by <c>FileModule.ConfigureServices</c>.
/// Permission names follow the <c>Permissions.{Resource}.{Action}</c> shape per the framework
/// convention (see <see cref="FshPermission.NameFor"/>).
/// </summary>
public static class FilePermissions
{
    public const string Resource = "File";

    public const string Upload = $"Permissions.{Resource}.Upload";
    public const string DeleteOwn = $"Permissions.{Resource}.DeleteOwn";
    public const string DeleteAny = $"Permissions.{Resource}.DeleteAny";
    public const string ViewTrash = $"Permissions.{Resource}.ViewTrash";
    public const string Restore = $"Permissions.{Resource}.Restore";

    public static IReadOnlyList<FshPermission> All { get; } =
    [
        new("Upload File",     ActionConstants.Upload,    Resource, IsBasic: true),
        new("Delete Own File", ActionConstants.DeleteOwn, Resource, IsBasic: true),
        new("Delete Any File",  ActionConstants.DeleteAny, Resource),
        new("View File Trash", ActionConstants.ViewTrash, Resource),
        new("Restore File",    ActionConstants.Restore,   Resource),
    ];
}