using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Users.RegisterUserStep2;
using Mediator;

namespace FSH.Modules.Identity.Features.v1.Users.RegisterUserStep2;

public sealed class RegisterUserStep2CommandHandler(
    IUserRegistrationService registrationService) : ICommandHandler<RegisterUserStep2Command, RegisterUserStep2Response>
{
    public async ValueTask<RegisterUserStep2Response> Handle(RegisterUserStep2Command command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        bool requiresProfileCompletion = await registrationService.UpdateUserAddressAsync(
            command.UserId,
            command.DistrictId,
            command.CommuneId,
            cancellationToken).ConfigureAwait(false);

        return new RegisterUserStep2Response(command.UserId, requiresProfileCompletion);
    }
}