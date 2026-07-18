using FSH.Framework.Core.Context;

namespace FSH.Mods.Identity.Spec.Services;

/// <summary>
/// Service interface for managing the current user context.
/// Combines user identity access with initialization capabilities.
/// </summary>
public interface ICurrentUserService : ICurrentUser, ICurrentUserInitializer
{
}