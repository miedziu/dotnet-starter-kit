using FluentValidation;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mod.Identity.Spec.Services;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FSH.Mod.Identity.Features.v1.Roles;

public static class CreateOrUpdateRoleEndpoint
{
    public static RouteHandlerBuilder MapCreateOrUpdateRoleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/roles", async (IMediator mediator, [FromBody] UpsertRoleCommand request, CancellationToken ct) =>
            TypedResults.Ok(await mediator.Send(request, ct)))
        .WithName("CreateOrUpdateRole")
        .WithSummary("Create or update role")
        .RequirePermission(IdentityPermissions.Roles.Create)
        .WithDescription("Create a new role or update an existing role's name and description.")
        .Produces<RoleDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}

public sealed class UpsertRoleCommandValidator : AbstractValidator<UpsertRoleCommand>
{
    public UpsertRoleCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Role name is required.");
    }
}

public sealed class UpsertRoleCommandHandler : ICommandHandler<UpsertRoleCommand, RoleDto>
{
    private readonly IRoleService _roleService;

    public UpsertRoleCommandHandler(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public async ValueTask<RoleDto> Handle(UpsertRoleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        return await _roleService.CreateOrUpdateRoleAsync(command.Id, command.Name, command.Description ?? string.Empty, cancellationToken)
            .ConfigureAwait(false);
    }
}