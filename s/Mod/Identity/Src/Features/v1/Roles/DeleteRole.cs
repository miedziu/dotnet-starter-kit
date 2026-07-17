using FluentValidation;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Identity.Contracts.Authorization;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Roles;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Identity.Features.v1.Roles;

public static class DeleteRoleEndpoint
{
    public static RouteHandlerBuilder MapDeleteRoleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/roles/{id:guid}", async (string id, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new DeleteRoleCommand(id), ct);
            return TypedResults.NoContent();
        })
        .WithName("DeleteRole")
        .WithSummary("Delete role by ID")
        .RequirePermission(IdentityPermissions.Roles.Delete)
        .WithDescription("Remove an existing role by its unique identifier.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}

public sealed class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Role ID is required.");
    }
}

public sealed class DeleteRoleCommandHandler : ICommandHandler<DeleteRoleCommand, Unit>
{
    private readonly IRoleService _roleService;

    public DeleteRoleCommandHandler(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public async ValueTask<Unit> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        await _roleService.DeleteRoleAsync(command.Id, cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}