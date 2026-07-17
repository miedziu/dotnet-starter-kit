using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Identity.Contracts.Authorization;
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

public static class CreateGroupEndpoint
{
    public static RouteHandlerBuilder MapCreateGroupEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/groups", async (IMediator mediator, [FromBody] CreateGroupCommand request, CancellationToken ct) =>
        {
            var result = await mediator.Send(request, ct);
            return TypedResults.Created($"/api/v1/groups/{result.Id}", result);
        })
        .WithName("CreateGroup")
        .WithSummary("Create a new group")
        .RequirePermission(IdentityPermissions.Groups.Create)
        .WithDescription("Create a new group with optional role assignments.")
        .Produces<GroupDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

public sealed class CreateGroupCommandValidator : AbstractValidator<CreateGroupCommand>
{
    public CreateGroupCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Group name is required.")
            .MaximumLength(256).WithMessage("Group name must not exceed 256 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1024).WithMessage("Description must not exceed 1024 characters.");
    }
}

public sealed class CreateGroupCommandHandler : ICommandHandler<CreateGroupCommand, GroupDto>
{
    private readonly IdentityDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public CreateGroupCommandHandler(IdentityDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<GroupDto> Handle(CreateGroupCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Validate name is unique within tenant
        var nameExists = await _dbContext.Groups
            .AnyAsync(g => g.Name == command.Name, cancellationToken);

        if (nameExists)
        {
            throw new CustomException($"Group with name '{command.Name}' already exists.", (IEnumerable<string>?)null, System.Net.HttpStatusCode.Conflict);
        }

        // Validate role IDs exist — fetch Id+Name in a single query to avoid a second roundtrip later
        List<(string Id, string Name)> resolvedRoles = [];
        if (command.RoleIds is { Count: > 0 })
        {
            var rawRoles = await _dbContext.Roles
                .Where(r => command.RoleIds.Contains(r.Id))
                .Select(r => new { r.Id, r.Name })
                .ToListAsync(cancellationToken);
            resolvedRoles = rawRoles.Select(r => (r.Id, r.Name!)).ToList();

            var invalidRoleIds = command.RoleIds.Except(resolvedRoles.Select(r => r.Id)).ToList();
            if (invalidRoleIds.Count > 0)
            {
                throw new NotFoundException($"Roles not found: {string.Join(", ", invalidRoleIds)}");
            }
        }

        var group = Group.Create(
            name: command.Name,
            description: command.Description,
            isDefault: command.IsDefault,
            isSystemGroup: false,
            createdBy: _currentUser.GetUserId().ToString());

        // Add role assignments
        foreach (var role in resolvedRoles)
        {
            _dbContext.GroupRoles.Add(GroupRole.Create(group.Id, role.Item1));
        }

        _dbContext.Groups.Add(group);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new GroupDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            IsDefault = group.IsDefault,
            IsSystemGroup = group.IsSystemGroup,
            MemberCount = 0,
            RoleIds = resolvedRoles.Select(r => r.Id).ToList().AsReadOnly(),
            RoleNames = resolvedRoles.Select(r => r.Name).ToList().AsReadOnly(),
            CreatedAt = group.CreatedAt
        };
    }
}