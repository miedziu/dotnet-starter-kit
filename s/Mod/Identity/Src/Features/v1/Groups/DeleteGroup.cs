using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mod.Identity.Data;
using FSH.Mod.Identity.Spec.Services;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mod.Identity.Features.v1.Groups;

public static class DeleteGroupEndpoint
{
    public static RouteHandlerBuilder MapDeleteGroupEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/groups/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new DeleteGroupCommand(id), ct);
            return TypedResults.NoContent();
        })
        .WithName("DeleteGroup")
        .WithSummary("Delete a group")
        .RequirePermission(IdentityPermissions.Groups.Delete)
        .WithDescription("Soft delete a group. System groups cannot be deleted.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}

public sealed class DeleteGroupCommandValidator : AbstractValidator<DeleteGroupCommand>
{
    public DeleteGroupCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Group ID is required.");
    }
}

public sealed class DeleteGroupCommandHandler : ICommandHandler<DeleteGroupCommand, Unit>
{
    private readonly IdentityDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IUserPermissionService _userPermissionService;

    public DeleteGroupCommandHandler(IdentityDbContext dbContext, ICurrentUser currentUser, IUserPermissionService userPermissionService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _userPermissionService = userPermissionService;
    }

    public async ValueTask<Unit> Handle(DeleteGroupCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var group = await _dbContext.Groups
            .FirstOrDefaultAsync(g => g.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException($"Group with ID '{command.Id}' not found.");

        if (group.IsSystemGroup)
        {
            throw new ForbiddenException("System groups cannot be deleted.");
        }

        // Snapshot members before delete; soft-delete flips IsDeleted but membership rows
        // persist, so capture first for clarity.
        var memberIntIds = await _dbContext.UserGroups
            .Where(ug => ug.GroupId == command.Id)
            .Select(ug => ug.UserId)
            .ToListAsync(cancellationToken);

        // Soft delete via domain method
        group.Delete(_currentUser.GetIntUserId());

        await _dbContext.SaveChangesAsync(cancellationToken);

        // A deleted group can no longer contribute its roles to members' effective
        // permission sets — flush each member's cached entry.
        foreach (var intId in memberIntIds)
        {
            // Fetch the user by IntId and get the string Id
            var userId = await _dbContext.Users
                .Where(u => u.IntId == intId)
                .Select(u => u.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (userId != null)
            {
                await _userPermissionService.InvalidatePermissionCacheAsync(userId, cancellationToken).ConfigureAwait(false);
            }
        }

        return Unit.Value;
    }
}