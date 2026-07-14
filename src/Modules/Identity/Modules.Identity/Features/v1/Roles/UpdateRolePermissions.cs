using FluentValidation;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Identity.Contracts.Authorization;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Roles.UpdatePermissions;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Identity.Features.v1.Roles;

public static class UpdateRolePermissionsEndpoint
{
    public static RouteHandlerBuilder MapUpdateRolePermissionsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/{id}/permissions", Handler)
        .WithName("UpdateRolePermissions")
        .WithSummary("Update role permissions")
        .RequirePermission(IdentityPermissions.Roles.Update)
        .WithDescription("Replace the set of permissions assigned to a role.")
        .Produces<string>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<string>, BadRequest>> Handler(
        string id,
        [FromBody] UpdatePermissionsCommand request,
        IMediator mediator,
        CancellationToken ct)
    {
        if (id != request.RoleId)
        {
            return TypedResults.BadRequest();
        }

        var response = await mediator.Send(request, ct);
        return TypedResults.Ok(response);
    }
}

public sealed class UpdatePermissionsCommandValidator : AbstractValidator<UpdatePermissionsCommand>
{
    public UpdatePermissionsCommandValidator()
    {
        RuleFor(r => r.RoleId)
            .NotEmpty();
        RuleFor(r => r.Permissions)
            .NotNull();
    }
}

public sealed class UpdatePermissionsCommandHandler : ICommandHandler<UpdatePermissionsCommand, string>
{
    private readonly IRoleService _roleService;

    public UpdatePermissionsCommandHandler(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public async ValueTask<string> Handle(UpdatePermissionsCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        return await _roleService.UpdatePermissionsAsync(command.RoleId, command.Permissions, cancellationToken).ConfigureAwait(false);
    }
}