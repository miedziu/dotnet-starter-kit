namespace FSH.Framework.Web.Modules;

/// <summary>
/// Configuration for enabling/disabling backend modules at runtime.
/// An empty or missing list means all modules are enabled.
/// </summary>
public sealed class ModuleOptions
{
    public IReadOnlyList<string> DisabledModules { get; init; } = Array.Empty<string>();

    public bool IsModuleEnabled(string moduleName) =>
        DisabledModules.Count != 0 && !DisabledModules.Contains(moduleName, StringComparer.OrdinalIgnoreCase);
}