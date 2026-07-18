using Mediator;
using System.Collections.ObjectModel;

namespace FSH.Mod.Chat.Spec.v1.Message;

/// <summary>
/// All currently-pinned messages in a channel, ordered by PinnedAtUtc desc.
/// </summary>
public sealed record GetPinnedMessagesQuery(Guid ChannelId)
    : IQuery<ReadOnlyCollection<MessageDto>>;