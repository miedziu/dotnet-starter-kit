using Mediator;

namespace FSH.Mod.Chat.Spec.v1.Channel;

public sealed record UpdateChannelCommand(
    Guid ChannelId,
    string Name,
    string? Description,
    bool IsPrivate) : ICommand<Unit>;