using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Users.RegisterUserStep2;

public class RegisterUserStep2Command : ICommand<RegisterUserStep2Response>
{
    public string UserId { get; set; } = default!;
    public short? DistrictId { get; set; }
    public short? CommuneId { get; set; }
}