using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Identity.Contracts.Authorization;
using FSH.Modules.Identity.Contracts.DTOs;
using FSH.Modules.Identity.Contracts.v1.Groups.GetGroupMembers;
using FSH.Modules.Identity.Data;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Identity.Features.v1.Groups;

public static class GetGroupMembersEndpoint
{
    public static RouteHandlerBuilder MapGetGroupMembersEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/groups/{groupId:guid}/members", async (Guid groupId, IMediator mediator, CancellationToken ct) =>
            TypedResults.Ok(await mediator.Send(new GetGroupMembersQuery(groupId), ct)))
        .WithName("GetGroupMembers")
        .WithSummary("Get members of a group")
        .RequirePermission(IdentityPermissions.Groups.View)
        .WithDescription("Retrieve all users that belong to a specific group.")
        .Produces<IEnumerable<GroupMemberDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    }
}

public sealed class GetGroupMembersQueryHandler : IQueryHandler<GetGroupMembersQuery, IEnumerable<GroupMemberDto>>
{
    private readonly IdentityDbContext _dbContext;

    public GetGroupMembersQueryHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<IEnumerable<GroupMemberDto>> Handle(GetGroupMembersQuery query, CancellationToken cancellationToken)
    {
        // Validate group exists
        var groupExists = await _dbContext.Groups
            .AsNoTracking()
            .AnyAsync(g => g.Id == query.GroupId, cancellationToken);

        if (!groupExists)
        {
            throw new NotFoundException($"Group with ID '{query.GroupId}' not found.");
        }

        // Get memberships with user info
        var memberships = await _dbContext.UserGroups
            .AsNoTracking()
            .Where(ug => ug.GroupId == query.GroupId)
            .Join(
                _dbContext.Users,
                ug => ug.UserId,
                u => u.IntId,
                (ug, u) => new GroupMemberDto
                {
                    UserId = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    AddedAt = ug.AddedAt,
                })
            .OrderBy(m => m.UserName)
            .ToListAsync(cancellationToken);

        return memberships;
    }
}