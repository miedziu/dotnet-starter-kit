using FSH.Framework.Shared.Persistence;
using Mediator;

namespace FSH.Mod.Identity.Spec.v1.User;

public sealed class SearchUsersQuery : IPagedQuery, IQuery<PagedResponse<UserDto>>
{
    public int? PageNumber { get; set; }

    public int? PageSize { get; set; }

    public string? Sort { get; set; }

    public string? Search { get; set; }

    public bool? IsActive { get; set; }

    public bool? EmailConfirmed { get; set; }

    public string? RoleId { get; set; }
}