using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Users.RegisterUserStep3;
using Mediator;

namespace FSH.Modules.Identity.Features.v1.Users.RegisterUserStep3;

public sealed class RegisterUserStep3CommandHandler(
    IUserRegistrationService registrationService) : ICommandHandler<RegisterUserStep3Command, RegisterUserStep3Response>
{
    public async ValueTask<RegisterUserStep3Response> Handle(RegisterUserStep3Command command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var result = await registrationService.UpdateUserProfileAsync(
            command.UserId,
            command.FirstName,
            command.LastName,
            command.UserName,
            cancellationToken).ConfigureAwait(false);

        return new RegisterUserStep3Response(command.UserId, result.UserName, "Profile completed successfully.");
    }
}