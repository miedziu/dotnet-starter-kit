using FSH.Mod.Identity.Data;
using FSH.Mod.Identity.Spec.Services;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mod.Identity.Services;

public sealed class GroupRoleService : IGroupRoleService
{
    private readonly IdentityDbContext _dbContext;

    public GroupRoleService(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<string>> GetUserGroupRolesAsync(string userId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(userId);

        // Fetch IntId from the user (since UserGroup.UserId is now int)
        var userIntId = await _dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => u.IntId)
            .FirstOrDefaultAsync(ct);

        // Get all group IDs the user belongs to
        var userGroupIds = await _dbContext.UserGroups
            .Where(ug => ug.UserId == userIntId)
            .Select(ug => ug.GroupId)
            .ToListAsync(ct);

        if (userGroupIds.Count == 0)
        {
            return [];
        }

        // Get all distinct role names from those groups
        var groupRoles = await _dbContext.GroupRoles
            .Where(gr => userGroupIds.Contains(gr.GroupId))
            .Select(gr => gr.Role!.Name!)
            .Distinct()
            .ToListAsync(ct);

        return groupRoles;
    }
}