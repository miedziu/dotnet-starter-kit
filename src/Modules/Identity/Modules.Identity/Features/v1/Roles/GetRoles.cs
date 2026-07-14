using FluentValidation;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Shared.Persistence;
using FSH.Modules.Identity.Contracts.Authorization;
using FSH.Modules.Identity.Contracts.DTOs;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Roles.GetRoles;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Identity.Features.v1.Roles;

public static class GetRolesEndpoint
{
    public static RouteHandlerBuilder MapGetRolesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/roles",
            async ([AsParameters] GetRolesQuery query, IMediator mediator, CancellationToken ct) =>
                TypedResults.Ok(await mediator.Send(query, ct)))
        .WithName("ListRoles")
        .WithSummary("List roles (paged)")
        .RequirePermission(IdentityPermissions.Roles.View)
        .Produces<PagedResponse<RoleDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .WithDescription("Retrieve roles. Pageable via PageNumber/PageSize; filterable via Search (case-insensitive substring against name + description).");
    }
}

public sealed class GetRolesQueryHandler : IQueryHandler<GetRolesQuery, PagedResponse<RoleDto>>
{
    private readonly IRoleService _roleService;

    public GetRolesQueryHandler(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public async ValueTask<PagedResponse<RoleDto>> Handle(GetRolesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        return await _roleService.GetRolesAsync(
            query.PageNumber ?? 1,
            query.PageSize ?? 20,
            query.Search,
            cancellationToken).ConfigureAwait(false);
    }
}

public sealed class GetRolesQueryValidator : AbstractValidator<GetRolesQuery>
{
    public GetRolesQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Page size must be greater than or equal to 1.");
    }
}