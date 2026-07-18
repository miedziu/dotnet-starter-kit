using Mediator;

namespace FSH.Mod.Identity.Spec.v1.User;

public class ForgotPasswordCommand : ICommand<string>
{
    public string Email { get; set; } = default!;
}