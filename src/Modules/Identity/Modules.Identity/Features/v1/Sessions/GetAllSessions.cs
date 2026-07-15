using FluentValidation;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Shared.Persistence;
using FSH.Framework.Web.Validation;
using FSH.Modules.Identity.Contracts.Authorization;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Dtos;
using FSH.Modules.Identity.Contracts.v1.Sessions;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Identity.Features.v1.Sessions;

public static class GetAllSessionsEndpoint
{
    internal static RouteHandlerBuilder MapGetAllSessionsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/sessions", async (CancellationToken ct, IMediator mediator) =>
            TypedResults.Ok(await mediator.Send(new GetAllSessionsQuery(), ct)))
        .WithName("GetAllSessions")
        .WithSummary("Get all sessions (Admin)")
        .RequirePermission(IdentityPermissions.Sessions.ViewAll)
        .WithDescription("Returns paged sessions across the application, filterable by active state and a free-text search across user name, email, and IP address.")
        .Produces<IEnumerable<UserSessionDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}

public sealed class GetAllSessionsValidator : AbstractValidator<GetAllSessionsQuery>
{
    public GetAllSessionsValidator()
    {
        Include(new PagedQueryValidator<GetAllSessionsQuery>());
    }
}

public sealed class GetAllSessionsQueryHandler : IQueryHandler<GetAllSessionsQuery, PagedResponse<UserSessionDto>>
{
    private readonly ISessionService _sessionService;

    public GetAllSessionsQueryHandler(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public async ValueTask<PagedResponse<UserSessionDto>> Handle(GetAllSessionsQuery query, CancellationToken cancellationToken)
    {
        int pageNumber = query.PageNumber ?? 1;
        int pageSize = query.PageSize ?? 10;
        int skip = (pageNumber - 1) * pageSize;

        var result = await _sessionService.GetAllSessionsAsync(
            query.IncludeInactive ?? false,
            query.Search,
            skip,
            pageSize,
            cancellationToken).ConfigureAwait(false);

        return new PagedResponse<UserSessionDto>
        {
            Items = result.Items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = result.TotalCount,
            TotalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize)
        };
    }
}