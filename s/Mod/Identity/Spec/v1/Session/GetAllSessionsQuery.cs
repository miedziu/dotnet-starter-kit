using FSH.Framework.Shared.Persistence;
using Mediator;

namespace FSH.Mod.Identity.Spec.v1.Session;

public sealed class GetAllSessionsQuery : IPagedQuery, IQuery<PagedResponse<UserSessionDto>>
{
    public int? PageNumber { get; set; }

    public int? PageSize { get; set; }

    public string? Sort { get; set; }

    public string? Search { get; set; }

    public bool? IncludeInactive { get; set; }
}