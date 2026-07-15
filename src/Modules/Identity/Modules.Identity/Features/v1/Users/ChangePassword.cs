using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Users;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Identity.Features.v1.Users;

public static class ChangePasswordEndpoint
{
    internal static RouteHandlerBuilder MapChangePasswordEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/change-password", async (
            [FromBody] ChangePasswordCommand command,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return TypedResults.Ok(result);
        })
        .WithName("ChangePassword")
        .WithSummary("Change password")
        .WithDescription("Change the current user's password.")
        .RequireAuthorization()
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

public sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    private readonly IPasswordHistoryService _passwordHistoryService;
    private readonly ICurrentUser _currentUser;

    public ChangePasswordValidator(
        IPasswordHistoryService passwordHistoryService,
        ICurrentUser currentUser)
    {
        _passwordHistoryService = passwordHistoryService;
        _currentUser = currentUser;

        RuleFor(p => p.Password)
            .NotEmpty()
            .WithMessage("Current password is required.");

        RuleFor(p => p.NewPassword)
            .NotEmpty()
            .WithMessage("New password is required.")
            .NotEqual(p => p.Password)
            .WithMessage("New password must be different from the current password.")
            .MustAsync(NotBeInPasswordHistoryAsync)
            .WithMessage("This password has been used recently. Please choose a different password.");

        RuleFor(p => p.ConfirmNewPassword)
            .Equal(p => p.NewPassword)
            .WithMessage("Passwords do not match.");
    }

    private async Task<bool> NotBeInPasswordHistoryAsync(string newPassword, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated())
        {
            return true; // Let other validation handle unauthorized access
        }

        var userId = _currentUser.GetUserId().ToString();

        // Check if password is in history
        var isInHistory = await _passwordHistoryService.IsPasswordInHistoryAsync(userId, newPassword, ct);
        return !isInHistory; // Return true if NOT in history (validation passes)
    }
}

public sealed class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand, string>
{
    private readonly IUserService _userService;
    private readonly ICurrentUser _currentUser;

    public ChangePasswordCommandHandler(IUserService userService, ICurrentUser currentUser)
    {
        _userService = userService;
        _currentUser = currentUser;
    }

    public async ValueTask<string> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!_currentUser.IsAuthenticated())
        {
            throw new InvalidOperationException("User is not authenticated.");
        }

        var userId = _currentUser.GetUserId().ToString();

        await _userService.ChangePasswordAsync(command.Password, command.NewPassword, command.ConfirmNewPassword, userId, cancellationToken).ConfigureAwait(false);

        return "password reset email sent";
    }
}