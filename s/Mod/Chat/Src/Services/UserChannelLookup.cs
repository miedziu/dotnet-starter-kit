using FSH.Framework.Web.Realtime;
using FSH.Mod.Chat.Data;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mod.Chat.Services;

/// <summary>
/// Returns the channel ids the user is currently a member of. AppHub calls this on
/// <c>OnConnectedAsync</c> to pre-join the connection to every channel group at once.
/// </summary>
public sealed class UserChannelLookup(ChatDbContext db) : IUserChannelLookup
{
    public async ValueTask<IReadOnlyList<Guid>> ListMyChannelIdsAsync(string userId, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(userId)) return [];
        return await db.Channels
            .Where(c => c.Members.Any(m => m.UserId == userId))
            .Select(c => c.Id)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }
}