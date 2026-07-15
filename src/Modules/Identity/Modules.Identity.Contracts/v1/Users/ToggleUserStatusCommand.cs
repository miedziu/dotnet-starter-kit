using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Users;

public class ToggleUserStatusCommand : ICommand<Unit>
{
    public bool ActivateUser { get; set; }
    public string? UserId { get; set; }
}