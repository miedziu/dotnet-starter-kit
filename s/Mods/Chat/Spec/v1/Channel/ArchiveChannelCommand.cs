using Mediator;

namespace FSH.Mods.Chat.Spec.v1.Channel;

public sealed record ArchiveChannelCommand(Guid ChannelId) : ICommand<Unit>;