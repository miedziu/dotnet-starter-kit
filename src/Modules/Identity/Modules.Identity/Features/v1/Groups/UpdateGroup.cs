using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Identity.Contracts.Authorization;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Dtos;
using FSH.Modules.Identity.Contracts.v1.Groups;
using FSH.Modules.Identity.Data;
using FSH.Modules.Identity.Domain;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Identity.Features.v1.Groups;

public sealed record UpdateGroupRequest(
    string Name,
    string? Description,
    bool IsDefault,
    IReadOnlyList<string>? RoleIds);

public static class UpdateGroupEndpoint
{
    public static RouteHandlerBuilder MapUpdateGroupEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/groups/{id:guid}", async (Guid id, IMediator mediator, [FromBody] UpdateGroupRequest request, CancellationToken ct) =>
            TypedResults.Ok(await mediator.Send(new UpdateGroupCommand(id, request.Name, request.Description, request.IsDefault, request.RoleIds), ct)))
        .WithName("UpdateGroup")
        .WithSummary("Update a group")
        .RequirePermission(IdentityPermissions.Groups.Update)
        .WithDescription("Update a group's name, description, default status, and role assignments.")
        .Produces<GroupDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

public sealed class UpdateGroupCommandValidator : AbstractValidator<UpdateGroupCommand>
{
    public UpdateGroupCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Group ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Group name is required.")
            .MaximumLength(256).WithMessage("Group name must not exceed 256 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1024).WithMessage("Description must not exceed 1024 characters.");
    }
}

public sealed class UpdateGroupCommandHandler : ICommandHandler<UpdateGroupCommand, GroupDto>
{
    private readonly IdentityDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IUserPermissionService _userPermissionService;

    public UpdateGroupCommandHandler(IdentityDbContext dbContext, ICurrentUser currentUser, IUserPermissionService userPermissionService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _userPermissionService = userPermissionService;
    }

    public async ValueTask<GroupDto> Handle(UpdateGroupCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var group = await GetGroupAsync(command.Id, cancellationToken);

        // System groups are framework-managed — name, description, default flag, and role
        // assignments are all part of the seed contract that the startup syncer relies on.
        if (group.IsSystemGroup)
        {
            throw new ForbiddenException("System groups cannot be modified.");
        }

        await ValidateUniqueNameAsync(command.Id, command.Name, cancellationToken);
        await ValidateRoleIdsAsync(command.RoleIds, cancellationToken);

        var userId = _currentUser.GetIntUserId();
        group.Update(command.Name, command.Description, userId);
        group.SetAsDefault(command.IsDefault, userId);

        var currentRoleIdsBefore = group.GroupRoles.Select(gr => gr.RoleId).ToHashSet();
        var newRoleIds = UpdateRoleAssignments(group, command.RoleIds);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // If the set of group→role assignments actually changed, every member's
        // effective permission set may have shifted — invalidate each.
        if (!currentRoleIdsBefore.SetEquals(newRoleIds))
        {
            var memberIntIds = await _dbContext.UserGroups
                .Where(ug => ug.GroupId == command.Id)
                .Select(ug => ug.UserId)
                .ToListAsync(cancellationToken);

            // Fetch string user IDs from IntIds
            var memberUserIds = await _dbContext.Users
                .Where(u => memberIntIds.Contains(u.IntId))
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);

            foreach (var memberId in memberUserIds)
            {
                await _userPermissionService.InvalidatePermissionCacheAsync(memberId, cancellationToken).ConfigureAwait(false);
            }
        }

        return await BuildResponseAsync(group, newRoleIds, cancellationToken);
    }

    private async Task<Group> GetGroupAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Groups
            .Include(g => g.GroupRoles)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Group with ID '{id}' not found.");
    }

    private async Task ValidateUniqueNameAsync(Guid excludeId, string name, CancellationToken cancellationToken)
    {
        var nameExists = await _dbContext.Groups
            .AnyAsync(g => g.Name == name && g.Id != excludeId, cancellationToken);

        if (nameExists)
        {
            throw new CustomException($"Group with name '{name}' already exists.", (IEnumerable<string>?)null, System.Net.HttpStatusCode.Conflict);
        }
    }

    private async Task ValidateRoleIdsAsync(IReadOnlyList<string>? roleIds, CancellationToken cancellationToken)
    {
        if (roleIds is not { Count: > 0 })
        {
            return;
        }

        var existingRoleIds = await _dbContext.Roles
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        var invalidRoleIds = roleIds.Except(existingRoleIds).ToList();
        if (invalidRoleIds.Count > 0)
        {
            throw new NotFoundException($"Roles not found: {string.Join(", ", invalidRoleIds)}");
        }
    }

    private static HashSet<string> UpdateRoleAssignments(Group group, IReadOnlyList<string>? roleIds)
    {
        var currentRoleIds = group.GroupRoles.Select(gr => gr.RoleId).ToHashSet();
        var newRoleIds = roleIds?.ToHashSet() ?? [];

        var rolesToRemove = group.GroupRoles.Where(gr => !newRoleIds.Contains(gr.RoleId)).ToList();
        foreach (var role in rolesToRemove)
        {
            group.GroupRoles.Remove(role);
        }

        foreach (var roleId in newRoleIds.Where(id => !currentRoleIds.Contains(id)))
        {
            group.GroupRoles.Add(GroupRole.Create(group.Id, roleId));
        }

        return newRoleIds;
    }

    private async Task<GroupDto> BuildResponseAsync(Group group, HashSet<string> roleIds, CancellationToken cancellationToken)
    {
        var memberCount = await _dbContext.UserGroups
            .AsNoTracking()
            .CountAsync(ug => ug.GroupId == group.Id, cancellationToken);

        var roleNames = roleIds.Count > 0
            ? await _dbContext.Roles
                .Where(r => roleIds.Contains(r.Id))
                .Select(r => r.Name!)
                .ToListAsync(cancellationToken)
            : [];

        return new GroupDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            IsDefault = group.IsDefault,
            IsSystemGroup = group.IsSystemGroup,
            MemberCount = memberCount,
            RoleIds = roleIds.ToList().AsReadOnly(),
            RoleNames = roleNames.AsReadOnly(),
            CreatedAt = group.CreatedAt
        };
    }
}