using FSH.Framework.Shared.Persistence;
using FSH.Modules.Identity.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Roles;

public sealed class GetRolesQuery : IPagedQuery, IQuery<PagedResponse<RoleDto>>
{
    public int? PageNumber { get; set; }

    public int? PageSize { get; set; }

    public string? Sort { get; set; }

    /// <summary>Case-insensitive substring match against role name + description.</summary>
    public string? Search { get; set; }
}