using FluentValidation;
using FSH.Mod.Identity.Spec.Services;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FSH.Mod.Identity.Features.v1.Users;

public static class ResetPasswordEndpoint
{
    internal static RouteHandlerBuilder MapResetPasswordEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/reset-password",
            async ([FromBody] ResetPasswordCommand command,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return TypedResults.Ok(result);
        })
        .WithName("ResetPassword")
        .WithSummary("Reset password")
        .WithDescription("Reset the user's password using the provided verification token.")
        .AllowAnonymous()
        .Produces(StatusCodes.Status200OK);
    }
}

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.Token).NotEmpty();
    }
}

public sealed class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand, string>
{
    private readonly IUserService _userService;

    public ResetPasswordCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async ValueTask<string> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        await _userService.ResetPasswordAsync(command.Email, command.Password, command.Token, cancellationToken).ConfigureAwait(false);

        return "Password has been reset.";
    }
}