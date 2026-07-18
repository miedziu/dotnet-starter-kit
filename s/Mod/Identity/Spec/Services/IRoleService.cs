using FSH.Framework.Shared.Persistence;

namespace FSH.Mod.Identity.Spec.Services;

public interface IRoleService
{
    Task<PagedResponse<RoleDto>> GetRolesAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? search = null,
        CancellationToken ct = default);
    Task<RoleDto?> GetRoleAsync(string id, CancellationToken ct = default);
    Task<RoleDto> CreateOrUpdateRoleAsync(string roleId, string name, string description, CancellationToken ct = default);
    Task DeleteRoleAsync(string id, CancellationToken ct = default);
    Task<RoleDto> GetWithPermissionsAsync(string id, CancellationToken ct = default);
    Task<string> UpdatePermissionsAsync(string roleId, List<string> permissions, CancellationToken ct = default);
}