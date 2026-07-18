using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mods.Identity.Data;
using FSH.Mods.Identity.Spec;
using FSH.Mods.Identity.Spec.v1;
using FSH.Mods.Identity.Spec.v1.Group;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mods.Identity.Features.v1.Groups;

public static class GetGroupsEndpoint
{
    public static RouteHandlerBuilder MapGetGroupsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/groups", async (IMediator mediator, string? search, CancellationToken ct) =>
            TypedResults.Ok(await mediator.Send(new GetGroupsQuery(search), ct)))
        .WithName("ListGroups")
        .WithSummary("List all groups")
        .RequirePermission(IdentityPermissions.Groups.View)
        .WithDescription("Retrieve all groups with optional search filter.")
        .Produces<IEnumerable<GroupDto>>(StatusCodes.Status200OK);
    }
}

public sealed class GetGroupsQueryHandler : IQueryHandler<GetGroupsQuery, IEnumerable<GroupDto>>
{
    private readonly IdentityDbContext _dbContext;

    public GetGroupsQueryHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<IEnumerable<GroupDto>> Handle(GetGroupsQuery query, CancellationToken cancellationToken)
    {
        var groupsQuery = _dbContext.Groups
            .AsNoTracking()
            .Include(g => g.GroupRoles)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.ToLowerInvariant();
            groupsQuery = groupsQuery.Where(g =>
                g.Name.ToLower().Contains(searchTerm) ||
                (g.Description != null && g.Description.ToLower().Contains(searchTerm)));
        }

        var groups = await groupsQuery
            .OrderBy(g => g.Name)
            .ToListAsync(cancellationToken);

        // Get member counts in one query
        var groupIds = groups.Select(g => g.Id).ToList();
        var memberCounts = await _dbContext.UserGroups
            .AsNoTracking()
            .Where(ug => groupIds.Contains(ug.GroupId))
            .GroupBy(ug => ug.GroupId)
            .Select(g => new { GroupId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.GroupId, x => x.Count, cancellationToken);

        // Get all role IDs from groups
        var allRoleIds = groups
            .SelectMany(g => g.GroupRoles.Select(gr => gr.RoleId))
            .Distinct()
            .ToList();

        var roleNames = await _dbContext.Roles
            .AsNoTracking()
            .Where(r => allRoleIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => r.Name!, cancellationToken);

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