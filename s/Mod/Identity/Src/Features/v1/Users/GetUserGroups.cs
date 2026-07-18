using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mod.Identity.Data;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mod.Identity.Features.v1.Users;

public static class GetUserGroupsEndpoint
{
    public static RouteHandlerBuilder MapGetUserGroupsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/users/{userId}/groups", async (string userId, IMediator mediator, CancellationToken ct) =>
            TypedResults.Ok(await mediator.Send(new GetUserGroupsQuery(userId), ct)))
        .WithName("GetUserGroups")
        .WithSummary("Get groups for a user")
        .RequirePermission(IdentityPermissions.Groups.View)
        .WithDescription("Retrieve all groups that a specific user belongs to.")
        .Produces<IEnumerable<GroupDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}

public sealed class GetUserGroupsQueryHandler : IQueryHandler<GetUserGroupsQuery, IEnumerable<GroupDto>>
{
    private readonly IdentityDbContext _dbContext;

    public GetUserGroupsQueryHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<IEnumerable<GroupDto>> Handle(GetUserGroupsQuery query, CancellationToken cancellationToken)
    {
        // Validate user exists
        var userExists = await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == query.UserId, cancellationToken);

        if (!userExists)
        {
            throw new NotFoundException($"User with ID '{query.UserId}' not found.");
        }

        // Get user's groups (using IntId since UserGroup.UserId is now int)
        var userIntId = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == query.UserId)
            .Select(u => u.IntId)
            .FirstOrDefaultAsync(cancellationToken);

        var groupIds = await _dbContext.UserGroups
            .AsNoTracking()
            .Where(ug => ug.UserId == userIntId)
            .Select(ug => ug.GroupId)
            .ToListAsync(cancellationToken);

        if (groupIds.Count == 0)
        {
            return [];
        }

        var groups = await _dbContext.Groups
            .AsNoTracking()
            .Include(g => g.GroupRoles)
            .Where(g => groupIds.Contains(g.Id))
            .ToListAsync(cancellationToken);

        // Get member counts
        var memberCounts = await _dbContext.UserGroups
            .AsNoTracking()
            .Where(ug => groupIds.Contains(ug.GroupId))
            .GroupBy(ug => ug.GroupId)
            .Select(g => new { GroupId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.GroupId, x => x.Count, cancellationToken);

        // Get role names
        var allRoleIds = groups
            .SelectMany(g => g.GroupRoles.Select(gr => gr.RoleId))
            .Distinct()
            .ToList();

        var roleNames = allRoleIds.Count > 0
            ? await _dbContext.Roles
                .AsNoTracking()
                .Where(r => allRoleIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, r => r.Name!, cancellationToken)
            : new Dictionary<string, string>();

        return groups.Select(g => new GroupDto
        {
            Id = g.Id,
            Name = g.Name,
            Description = g.Description,
            IsDefault = g.IsDefault,
            IsSystemGroup = g.IsSystemGroup,
            MemberCount = memberCounts.GetValueOrDefault(g.Id, 0),
            RoleIds = g.GroupRoles.Select(gr => gr.RoleId).ToList().AsReadOnly(),
            RoleNames = g.GroupRoles
                .Select(gr => roleNames.GetValueOrDefault(gr.RoleId, gr.RoleId))
                .ToList()
                .AsReadOnly(),
            CreatedAt = g.CreatedAt
        });
    }
}