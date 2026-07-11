using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Users.RegisterUserStep1;
using Mediator;

namespace FSH.Modules.Identity.Features.v1.Users.RegisterUserStep1;

public sealed class RegisterUserStep1CommandHandler(
    IUserRegistrationService registrationService) : ICommandHandler<RegisterUserStep1Command, RegisterUserStep1Response>
{
    public async ValueTask<RegisterUserStep1Response> Handle(RegisterUserStep1Command command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        string userId = await registrationService.RegisterStep1Async(
            command.Email,
            command.Password,
            command.ConfirmPassword,
            command.Origin ?? string.Empty,
            command.ReferralUsernames,
            cancellationToken).ConfigureAwait(false);

        return new RegisterUserStep1Response(userId, "User registered successfully. Please check your email to confirm your account.");
    }
}