using FSH.Framework.Web.Realtime;
using FSH.Mods.Chat.Data;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mods.Chat.Services;

/// <summary>
/// Chat module's adapter that satisfies the realtime hub's membership probe. Scoped — the
/// hub instantiates one per call.
/// </summary>
public sealed class ChannelMembershipChecker(ChatDbContext db) : IChannelMembershipChecker
{
    public async ValueTask<bool> IsMemberAsync(Guid channelId, string userId, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(userId)) return false;
        return await db.Channels
            .AnyAsync(c => c.Id == channelId && c.Members.Any(m => m.UserId == userId), ct)
            .ConfigureAwait(false);
    }
}