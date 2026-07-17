using FluentValidation;
using FSH.Framework.Web.Origin;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Users;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace FSH.Modules.Identity.Features.v1.Users;

public static class ForgotPasswordEndpoint
{
    internal static RouteHandlerBuilder MapForgotPasswordEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/forgot-password", async (
            HttpRequest request,
            [FromBody] ForgotPasswordCommand command,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return TypedResults.Ok(result);
        })
        .WithName("RequestPasswordReset")
        .WithSummary("Request password reset")
        .WithDescription("Generate a password reset token and send it via email.")
        .AllowAnonymous()
        .Produces(StatusCodes.Status200OK);
    }
}

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(p => p.Email).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .EmailAddress();
    }
}

public sealed class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, string>
{
    private readonly IUserService _userService;
    private readonly IOptions<OriginOptions> _originOptions;

    public ForgotPasswordCommandHandler(IUserService userService, IOptions<OriginOptions> originOptions)
    {
        _userService = userService;
        _originOptions = originOptions;
    }

    public async ValueTask<string> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var origin = _originOptions.Value?.OriginUrl?.ToString();
        if (string.IsNullOrWhiteSpace(origin))
        {
            throw new InvalidOperationException("Origin URL is not configured.");
        }

        await _userService.ForgotPasswordAsync(command.Email, origin, cancellationToken).ConfigureAwait(false);

        return "Password reset email sent.";
    }
}