using FSH.Framework.Shared.Constants;
using FSH.Modules.Identity.Contracts.DTOs;
using FSH.Modules.Identity.Contracts.v1.Permissions.GetPermissionCatalog;
using Mediator;

namespace FSH.Modules.Identity.Features.v1.Permissions.GetPermissionCatalog;

public sealed class GetPermissionCatalogQueryHandler()
    : IQueryHandler<GetPermissionCatalogQuery, IReadOnlyList<PermissionCatalogEntryDto>>
{
    public ValueTask<IReadOnlyList<PermissionCatalogEntryDto>> Handle(
        GetPermissionCatalogQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Matches the same root-vs-admin rule used by RolePermissionSyncer so the catalog the
        // SPA edits agrees with the set the syncer would push into a tenant's role claims.
        //var source = PermissionConstants.Admin; //
        var source = PermissionConstants.Admin.Concat(PermissionConstants.Root).DistinctBy(p => p.Name);

        IReadOnlyList<PermissionCatalogEntryDto> result =
        [
            .. source.Select(p => new PermissionCatalogEntryDto(
                p.Name,
                p.Description,
                p.Resource,
                p.Action,
                p.IsBasic,
                p.IsRoot))
        ];

        return ValueTask.FromResult(result);
    }
}