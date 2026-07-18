using Mediator;

namespace FSH.Mods.Identity.Spec.v1.User;

public class RegisterUserStep2Command : ICommand<RegisterUserStep2Response>
{
    public string UserId { get; set; } = default!;
    public short? DistrictId { get; set; }
    public short? CommuneId { get; set; }
}