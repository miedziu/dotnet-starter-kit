using Mediator;

namespace FSH.Mods.Identity.Spec.v1.User;

public class ResetPasswordCommand : ICommand<string>
{
    public string Email { get; set; } = default!;

    public string Password { get; set; } = default!;

    public string Token { get; set; } = default!;
}