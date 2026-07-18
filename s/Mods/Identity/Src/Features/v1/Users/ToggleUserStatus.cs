using FluentValidation;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mods.Identity.Spec;
using FSH.Mods.Identity.Spec.Services;
using FSH.Mods.Identity.Spec.v1.User;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FSH.Mods.Identity.Features.v1.Users;

public static class ToggleUserStatusEndpoint
{
    internal static RouteHandlerBuilder MapToggleUserStatusEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPatch("/users/{id:guid}", Handler)
        .WithName("ToggleUserStatus")
        .WithSummary("Toggle user status")
        .RequirePermission(IdentityPermissions.Users.Update)
        .WithDescription("Activate or deactivate a user account.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<NoContent, BadRequest>> Handler(
        string id,
        [FromBody] ToggleUserStatusCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            command.UserId = id;
        }

        if (!string.Equals(id, command.UserId, StringComparison.Ordinal))
        {
            return TypedResults.BadRequest();
        }

        await mediator.Send(command, ct);
        return TypedResults.NoContent();
    }
}

public sealed class ToggleUserStatusCommandValidator : AbstractValidator<ToggleUserStatusCommand>
{
    public ToggleUserStatusCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}

public sealed class ToggleUserStatusCommandHandler : ICommandHandler<ToggleUserStatusCommand, Unit>
{
    private readonly IUserService _userService;

    public ToggleUserStatusCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async ValueTask<Unit> Handle(ToggleUserStatusCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            throw new ArgumentException("UserId must be provided.", nameof(command.UserId));
        }

        await _userService.ToggleStatusAsync(command.ActivateUser, command.UserId, cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}