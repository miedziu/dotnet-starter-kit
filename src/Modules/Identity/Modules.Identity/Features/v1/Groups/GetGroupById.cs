using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Identity.Contracts.Authorization;
using FSH.Modules.Identity.Contracts.DTOs;
using FSH.Modules.Identity.Contracts.v1.Groups.GetGroupById;
using FSH.Modules.Identity.Data;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Identity.Features.v1.Groups;

public static class GetGroupByIdEndpoint
{
    public static RouteHandlerBuilder MapGetGroupByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/groups/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
            TypedResults.Ok(await mediator.Send(new GetGroupByIdQuery(id), ct)))
        .WithName("GetGroupById")
        .WithSummary("Get group by ID")
        .RequirePermission(IdentityPermissions.Groups.View)
        .WithDescription("Retrieve a specific group by its ID including roles and member count.")
        .Produces<GroupDto>(StatusCodes.Status200OK);
    }
}

public sealed class GetGroupByIdQueryHandler : IQueryHandler<GetGroupByIdQuery, GroupDto>
{
    private readonly IdentityDbContext _dbContext;

    public GetGroupByIdQueryHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<GroupDto> Handle(GetGroupByIdQuery query, CancellationToken cancellationToken)
    {
        var group = await _dbContext.Groups
            .AsNoTracking()
            .Include(g => g.GroupRoles)
            .FirstOrDefaultAsync(g => g.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException($"Group with ID '{query.Id}' not found.");

        var memberCount = await _dbContext.UserGroups
            .AsNoTracking()
            .CountAsync(ug => ug.GroupId == group.Id, cancellationToken);

        var roleIds = group.GroupRoles.Select(gr => gr.RoleId).ToList();
        var roleNames = roleIds.Count > 0
            ? await _dbContext.Roles
                .AsNoTracking()
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
            RoleIds = roleIds.AsReadOnly(),
            RoleNames = roleNames.AsReadOnly(),
            CreatedAt = group.CreatedAt
        };
    }
}