using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Users;

public class RegisterUserStep3Command : ICommand<RegisterUserStep3Response>
{
    public string UserId { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string UserName { get; set; } = default!;
}