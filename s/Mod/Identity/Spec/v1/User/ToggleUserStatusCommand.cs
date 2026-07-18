using Mediator;

namespace FSH.Mod.Identity.Spec.v1.User;

public class ToggleUserStatusCommand : ICommand<Unit>
{
    public bool ActivateUser { get; set; }
    public string? UserId { get; set; }
}