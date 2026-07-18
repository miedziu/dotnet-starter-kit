using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Mod.Identity.Domain;
using FSH.Mod.Identity.Spec.v1.TwoFactor;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;

namespace FSH.Mod.Identity.Features.v1.TwoFactor;

public static class VerifyEnrollTwoFactorEndpoint
{
    internal static RouteHandlerBuilder MapVerifyEnrollTwoFactorEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/2fa/verify",
                async (VerifyEnrollTwoFactorCommand command, IMediator mediator, CancellationToken ct) =>
                    TypedResults.Ok(new { success = await mediator.Send(command, ct) }))
            .WithName("VerifyEnrollTwoFactor")
            .WithSummary("Confirm TOTP enrollment")
            .WithDescription("Verifies the 6-digit code from the authenticator app. On success, 2FA is enabled and subsequent logins must include a code.")
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }
}

public sealed class VerifyEnrollTwoFactorCommandValidator : AbstractValidator<VerifyEnrollTwoFactorCommand>
{
    public VerifyEnrollTwoFactorCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(10); // allow spaces; handler strips them
    }
}

public sealed class VerifyEnrollTwoFactorCommandHandler
    : ICommandHandler<VerifyEnrollTwoFactorCommand, bool>
{
    private readonly UserManager<FshUser> _userManager;
    private readonly ICurrentUser _currentUser;

    public VerifyEnrollTwoFactorCommandHandler(UserManager<FshUser> userManager, ICurrentUser currentUser)
    {
        _userManager = userManager;
        _currentUser = currentUser;
    }

    public async ValueTask<bool> Handle(
        VerifyEnrollTwoFactorCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!_currentUser.IsAuthenticated())
        {
            throw new UnauthorizedException();
        }

        var userId = _currentUser.GetUserId().ToString();
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException($"User {userId} not found.");

        var sanitized = command.Code.Replace(" ", string.Empty, StringComparison.Ordinal);
        var valid = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            sanitized);

        if (!valid)
        {
            throw new CustomException(
                "The authenticator code is invalid.",
                errors: null,
                System.Net.HttpStatusCode.BadRequest);
        }

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        return true;
    }
}