using FluentValidation;
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

public static class RemoveUserFromGroupEndpoint
{
    public static RouteHandlerBuilder MapRemoveUserFromGroupEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/groups/{groupId:guid}/members/{userId}", async (Guid groupId, string userId, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new RemoveUserFromGroupCommand(groupId, userId), ct);
            return TypedResults.NoContent();
        })
        .WithName("RemoveUserFromGroup")
        .WithSummary("Remove a user from a group")
        .RequirePermission(IdentityPermissions.Groups.ManageMembers)
        .WithDescription("Remove a specific user from a group.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}

public sealed class RemoveUserFromGroupCommandValidator : AbstractValidator<RemoveUserFromGroupCommand>
{
    public RemoveUserFromGroupCommandValidator()
    {
        RuleFor(x => x.GroupId)
            .NotEmpty().WithMessage("Group ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}

public sealed class RemoveUserFromGroupCommandHandler : ICommandHandler<RemoveUserFromGroupCommand, Unit>
{
    private readonly IdentityDbContext _dbContext;
    private readonly IUserPermissionService _userPermissionService;

    public RemoveUserFromGroupCommandHandler(IdentityDbContext dbContext, IUserPermissionService userPermissionService)
    {
        _dbContext = dbContext;
        _userPermissionService = userPermissionService;
    }

    public async ValueTask<Unit> Handle(RemoveUserFromGroupCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Fetch IntId from the user (since UserGroup.UserId is now int)
        var userIntId = await _dbContext.Users
            .Where(u => u.Id == command.UserId)
            .Select(u => u.IntId)
            .FirstOrDefaultAsync(cancellationToken);

        var membership = await _dbContext.UserGroups
            .Include(ug => ug.Group)
            .FirstOrDefaultAsync(ug => ug.GroupId == command.GroupId && ug.UserId == userIntId, cancellationToken);

        if (membership is null)
        {
            throw new NotFoundException($"User '{command.UserId}' is not a member of group '{command.GroupId}'.");
        }

        // Default groups (e.g. seeded "All Users") require every tenant user to be a member, so
        // removing one breaks that invariant and leaves later registrants in a half-populated group.
        if (membership.Group is not null && membership.Group.IsDefault)
        {
            throw new ForbiddenException("Users cannot be removed from a default group.");
        }

        _dbContext.UserGroups.Remove(membership);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Leaving a group may revoke roles the user only held through this group —
        // invalidate so the cached permission set is rebuilt on next request.
        await _userPermissionService.InvalidatePermissionCacheAsync(command.UserId, cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}