using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace FSH.Framework.Web.Mod;

/// <summary>
/// Helper for filtering and registering modules based on runtime configuration.
/// Used by both API and DbMigrator to ensure consistent module loading.
/// </summary>
public static class ModuleRegistrationHelper
{
    /// <summary>
    /// Gets filtered module assemblies based on enabled modules configuration.
    /// Used to selectively load module services and DbInitializers at runtime.
    /// </summary>
    /// <param name="configuration">The configuration to read ModuleOptions from.</param>
    /// <param name="allModules">All available modules as (name, contractsType, runtimeType) tuples.</param>
    /// <returns>Module assemblies for enabled modules only.</returns>
    public static Assembly[] GetFilteredModuleAssemblies(
        IConfiguration configuration,
        params (string name, Type contractsType, Type runtimeType)[] allModules)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var moduleOptions = configuration.GetSection("ModuleOptions").Get<ModuleOptions>() ?? new();

        var enabledModules = allModules
            .Where(m => moduleOptions.IsModuleEnabled(m.name))
            .ToArray();

        return enabledModules
            .Select(m => m.runtimeType.Assembly)
            .ToArray();
    }
}