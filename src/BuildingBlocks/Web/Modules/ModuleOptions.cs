namespace FSH.Framework.Web.Modules;

/// <summary>
/// Configuration for enabling/disabling backend modules at runtime.
/// An empty or missing EnabledModules list means all modules are enabled.
/// </summary>
public class ModuleOptions
{
    public IReadOnlyList<string> EnabledModules { get; init; } = Array.Empty<string>();

    public bool IsModuleEnabled(string moduleName) =>
        EnabledModules.Count == 0 || EnabledModules.Contains(moduleName, StringComparer.OrdinalIgnoreCase);
}