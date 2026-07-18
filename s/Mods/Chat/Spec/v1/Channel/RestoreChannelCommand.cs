using Mediator;

namespace FSH.Mods.Chat.Spec.v1.Channel;

public sealed record RestoreChannelCommand(Guid ChannelId) : ICommand<Unit>;