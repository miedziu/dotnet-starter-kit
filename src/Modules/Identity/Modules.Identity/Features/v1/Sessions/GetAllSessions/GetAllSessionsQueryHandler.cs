using FSH.Framework.Shared.Persistence;
using FSH.Modules.Identity.Contracts.DTOs;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Sessions.GetAllSessions;
using Mediator;

namespace FSH.Modules.Identity.Features.v1.Sessions.GetAllSessions;

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