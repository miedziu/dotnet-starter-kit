using Mediator;

namespace FSH.Mod.Chat.Spec.v1.Channel;

public sealed record ArchiveChannelCommand(Guid ChannelId) : ICommand<Unit>;