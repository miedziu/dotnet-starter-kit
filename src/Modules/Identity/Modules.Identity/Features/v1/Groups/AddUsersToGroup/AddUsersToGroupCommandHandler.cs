using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Groups.AddUsersToGroup;
using FSH.Modules.Identity.Data;
using FSH.Modules.Identity.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Identity.Features.v1.Groups.AddUsersToGroup;

public sealed class AddUsersToGroupCommandHandler : ICommandHandler<AddUsersToGroupCommand, AddUsersToGroupResponse>
{
    private readonly IdentityDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IUserPermissionService _userPermissionService;

    public AddUsersToGroupCommandHandler(IdentityDbContext dbContext, ICurrentUser currentUser, IUserPermissionService userPermissionService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _userPermissionService = userPermissionService;
    }

    public async ValueTask<AddUsersToGroupResponse> Handle(AddUsersToGroupCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Validate group exists
        var groupExists = await _dbContext.Groups
            .AnyAsync(g => g.Id == command.GroupId, cancellationToken);

        if (!groupExists)
        {
            throw new NotFoundException($"Group with ID '{command.GroupId}' not found.");
        }

        // Get IntIds for the users (since UserGroup.UserId is now int)
        var userIntIds = await _dbContext.Users
            .Where(u => command.UserIds.Contains(u.Id))
            .Select(u => new { u.Id, u.IntId })
            .ToDictionaryAsync(x => x.Id, x => x.IntId, cancellationToken);

        var invalidUserIds = command.UserIds.Except(userIntIds.Keys).ToList();
        if (invalidUserIds.Count > 0)
        {
            throw new NotFoundException($"Users not found: {string.Join(", ", invalidUserIds)}");
        }

        // Get existing memberships (compare by IntId)
        var existingIntIds = await _dbContext.UserGroups
            .Where(ug => ug.GroupId == command.GroupId && userIntIds.Values.Contains(ug.UserId))
            .Select(ug => ug.UserId)
            .ToHashSetAsync(cancellationToken);

        var alreadyMemberUserIds = userIntIds
            .Where(kvp => existingIntIds.Contains(kvp.Value))
            .Select(kvp => kvp.Key)
            .ToList();

        var usersToAdd = userIntIds
            .Where(kvp => !existingIntIds.Contains(kvp.Value))
            .ToList();

        // Add new memberships using IntId
        var currentUserId = _currentUser.GetUserId().ToString();
        foreach (var userIntId in usersToAdd)
        {
            _dbContext.UserGroups.Add(UserGroup.Create(userIntId.Value, command.GroupId, currentUserId));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Joining a group can grant new roles (via GroupRoles) feeding JWT claims; invalidate
        // each newly-added user's cached permission set so their next request reflects it.
        foreach (var userId in usersToAdd.Select(x => x.Key))
        {
            await _userPermissionService.InvalidatePermissionCacheAsync(userId, cancellationToken).ConfigureAwait(false);
        }

        return new AddUsersToGroupResponse(usersToAdd.Count, alreadyMemberUserIds);
    }
}