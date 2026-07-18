using Mediator;

namespace FSH.Mod.Identity.Spec.v1.User;

public class ChangePasswordCommand : ICommand<string>
{
    /// <summary>The user's current password.</summary>
    public string Password { get; init; } = default!;

    /// <summary>The new password the user wants to set.</summary>
    public string NewPassword { get; init; } = default!;

    /// <summary>Confirmation of the new password.</summary>
    public string ConfirmNewPassword { get; init; } = default!;
}