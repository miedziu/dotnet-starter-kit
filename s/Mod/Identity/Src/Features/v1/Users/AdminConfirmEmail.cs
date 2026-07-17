using FluentValidation;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Identity.Contracts.Authorization;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Users;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Identity.Features.v1.Users;

public static class AdminConfirmEmailEndpoint
{
    internal static RouteHandlerBuilder MapAdminConfirmEmailEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/users/{id:guid}/confirm-email", Handler)
        .WithName("AdminConfirmEmail")
        .WithSummary("Confirm a user's email (admin)")
        .RequirePermission(IdentityPermissions.Users.ConfirmEmail)
        .WithDescription("Marks another user's email address as confirmed without a confirmation token.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<NoContent> Handler(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        await mediator.Send(new AdminConfirmEmailCommand(id.ToString()), ct);
        return TypedResults.NoContent();
    }
}

public sealed class AdminConfirmEmailCommandValidator : AbstractValidator<AdminConfirmEmailCommand>
{
    public AdminConfirmEmailCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}

public sealed class AdminConfirmEmailCommandHandler(
    IUserService userService) : ICommandHandler<AdminConfirmEmailCommand, Unit>
{
    public async ValueTask<Unit> Handle(AdminConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        await userService.AdminConfirmEmailAsync(command.UserId, cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}