using FluentValidation;
using FSH.Mod.Identity.Spec.Services;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Mod.Identity.Features.v1.Users;

public static class RegisterUserStep2Endpoint
{
    internal static RouteHandlerBuilder MapRegisterUserStep2Endpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/register/step2", async (RegisterUserStep2Command command,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .WithName("RegisterUserStep2")
        .WithSummary("Register user - Step 2 (address)")
        .WithDescription("Update user address information. Requires authentication.")
        .Produces<RegisterUserStep2Response>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

public sealed class RegisterUserStep2Validator : AbstractValidator<RegisterUserStep2Command>
{
    public RegisterUserStep2Validator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.DistrictId)
            .Must(v => !v.HasValue || v.Value > 0).WithMessage("District ID must be greater than 0 if provided.")
            .When(x => x.DistrictId.HasValue);

        RuleFor(x => x.CommuneId)
            .Must(v => !v.HasValue || v.Value > 0).WithMessage("Commune ID must be greater than 0 if provided.")
            .When(x => x.CommuneId.HasValue);
    }
}

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