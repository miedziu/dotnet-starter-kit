using Mediator;

namespace FSH.Mod.Identity.Spec.v1.User;

public class RegisterUserStep3Command : ICommand<RegisterUserStep3Response>
{
    public string UserId { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string UserName { get; set; } = default!;
}