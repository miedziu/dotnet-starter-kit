using FluentValidation;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Users;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Identity.Features.v1.Users;

public static class RegisterUserEndpoint
{
    internal static RouteHandlerBuilder MapRegisterUserEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/register", async (RegisterUserCommand command,
            HttpContext context,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var origin = $"{context.Request.Scheme}://{context.Request.Host.Value}{context.Request.PathBase.Value}";
            command.Origin = origin;
            var result = await mediator.Send(command, ct);
            return TypedResults.Created($"/api/v1/identity/users/{result.UserId}", result);
        })
        .WithName("RegisterUser")
        .WithSummary("Register user")
        .AllowAnonymous()
        .WithIdempotency()
        .WithDescription("Create a new user account.")
        .Produces<RegisterUserResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters.")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Password confirmation is required.")
            .Equal(x => x.Password).WithMessage("Passwords do not match.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
            .When(x => x.PhoneNumber is not null);

        RuleFor(x => x.ReferralUsernames)
            .Must(usernames => usernames is null || usernames.All(username => !string.IsNullOrWhiteSpace(username)))
            .WithMessage("Referral usernames must not contain empty or whitespace values.")
            .When(x => x.ReferralUsernames is not null && x.ReferralUsernames.Length > 0);
    }
}

public sealed class RegisterUserCommandHandler(
    IUserRegistrationService registrationService) : ICommandHandler<RegisterUserCommand, RegisterUserResponse>
{
    public async ValueTask<RegisterUserResponse> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        string userId = await registrationService.RegisterAsync(
            command.FirstName,
            command.LastName,
            command.Email,
            command.UserName,
            command.Password,
            command.ConfirmPassword,
            command.PhoneNumber ?? string.Empty,
            command.Origin ?? string.Empty,
            command.ReferralUsernames,
            cancellationToken).ConfigureAwait(false);

        return new RegisterUserResponse(userId);
    }
}