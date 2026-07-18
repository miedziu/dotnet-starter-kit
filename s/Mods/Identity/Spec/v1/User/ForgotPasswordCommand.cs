using Mediator;

namespace FSH.Mods.Identity.Spec.v1.User;

public class ForgotPasswordCommand : ICommand<string>
{
    public string Email { get; set; } = default!;
}