using Mediator;

namespace FSH.Mods.Chat.Spec.v1.Channel;

public sealed record UpdateChannelCommand(
    Guid ChannelId,
    string Name,
    string? Description,
    bool IsPrivate) : ICommand<Unit>;