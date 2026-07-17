using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Users;

public class ForgotPasswordCommand : ICommand<string>
{
    public string Email { get; set; } = default!;
}