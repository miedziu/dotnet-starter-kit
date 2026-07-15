using FluentValidation;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Users;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Identity.Features.v1.Users;

public static class RegisterUserStep3Endpoint
{
    internal static RouteHandlerBuilder MapRegisterUserStep3Endpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/register/step3", async (RegisterUserStep3Command command,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return TypedResults.Ok(result);
        })
        .WithName("RegisterUserStep3")
        .WithSummary("Register user - Step 3 (profile)")
        .WithDescription("Complete user profile with first name, last name, and username. Requires authentication.")
        .Produces<RegisterUserStep3Response>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

public sealed class RegisterUserStep3Validator : AbstractValidator<RegisterUserStep3Command>
{
    public RegisterUserStep3Validator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters.")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters.");
    }
}

public sealed class RegisterUserStep3CommandHandler(
    IUserRegistrationService registrationService) : ICommandHandler<RegisterUserStep3Command, RegisterUserStep3Response>
{
    public async ValueTask<RegisterUserStep3Response> Handle(RegisterUserStep3Command command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        await registrationService.UpdateUserProfileAsync(
            command.UserId,
            command.FirstName,
            command.LastName,
            command.UserName,
            cancellationToken).ConfigureAwait(false);

        return new RegisterUserStep3Response(command.UserId, "Profile completed successfully.");
    }
}