using Mediator;

namespace FSH.Mod.Identity.Spec.v1.User;

/// <summary>
/// Administratively confirms a user's email (no confirmation token). Gated by
/// <c>Permissions.Users.ConfirmEmail</c> at the endpoint.
/// </summary>
public sealed record AdminConfirmEmailCommand(string UserId) : ICommand<Unit>;