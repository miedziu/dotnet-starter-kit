using FluentValidation;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Users;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Identity.Features.v1.Users;

public static class RegisterUserStep1Endpoint
{
    internal static RouteHandlerBuilder MapRegisterUserStep1Endpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/register/step1", async (RegisterUserStep1Command command,
            HttpContext context,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return TypedResults.Created($"/api/v1/identity/register/step2", result);
        })
        .WithName("RegisterUserStep1")
        .WithSummary("Register user step 1")
        .AllowAnonymous()
        .WithDescription("First step of multi-step registration: validates email availability and creates user account.")
        .Produces<RegisterUserStep1Response>(StatusCodes.Status202Accepted)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

public sealed class RegisterUserStep1CommandValidator : AbstractValidator<RegisterUserStep1Command>
{
    public RegisterUserStep1CommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Password confirmation is required.")
            .Equal(x => x.Password).WithMessage("Passwords do not match.");
    }
}

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