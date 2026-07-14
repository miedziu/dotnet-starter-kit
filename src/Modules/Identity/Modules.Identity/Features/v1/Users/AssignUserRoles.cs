using FluentValidation;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Identity.Contracts.Authorization;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Users.AssignUserRoles;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Identity.Features.v1.Users;

public static class AssignUserRolesEndpoint
{
    internal static RouteHandlerBuilder MapAssignUserRolesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/users/{id:guid}/roles", Handler)
        .WithName("AssignUserRoles")
        .WithSummary("Assign roles to user")
        .WithDescription("Assign one or more roles to a user.")
        .RequirePermission(IdentityPermissions.Users.ManageRoles)
        .Produces<string>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<Ok<string>, BadRequest>> Handler(
        string id,
        AssignUserRolesCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        if (!string.Equals(id, command.UserId, StringComparison.Ordinal))
        {
            return TypedResults.BadRequest();
        }

        var result = await mediator.Send(command, ct);
        return TypedResults.Ok(result);
    }
}

public sealed class AssignUserRolesCommandValidator : AbstractValidator<AssignUserRolesCommand>
{
    public AssignUserRolesCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.UserRoles)
            .NotNull().WithMessage("User roles list is required.");
    }
}

public sealed class AssignUserRolesCommandHandler(IUserService userService)
    : ICommandHandler<AssignUserRolesCommand, string>
{
    public async ValueTask<string> Handle(AssignUserRolesCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        return await userService.AssignRolesAsync(command.UserId, command.UserRoles, cancellationToken);
    }
}