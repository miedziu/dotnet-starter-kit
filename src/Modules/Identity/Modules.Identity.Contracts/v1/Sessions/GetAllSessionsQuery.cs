using FSH.Framework.Shared.Persistence;
using FSH.Modules.Identity.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Sessions;

public sealed class GetAllSessionsQuery : IPagedQuery, IQuery<PagedResponse<UserSessionDto>>
{
    public int? PageNumber { get; set; }

    public int? PageSize { get; set; }

    public string? Sort { get; set; }

    public string? Search { get; set; }

    public bool? IncludeInactive { get; set; }
}