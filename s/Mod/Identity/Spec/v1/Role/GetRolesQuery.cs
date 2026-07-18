using FSH.Framework.Shared.Persistence;
using Mediator;

namespace FSH.Mod.Identity.Spec.v1.Role;

public sealed class GetRolesQuery : IPagedQuery, IQuery<PagedResponse<RoleDto>>
{
    public int? PageNumber { get; set; }

    public int? PageSize { get; set; }

    public string? Sort { get; set; }

    /// <summary>Case-insensitive substring match against role name + description.</summary>
    public string? Search { get; set; }
}