using Mediator;

namespace FSH.Mod.Chat.Spec.v1.Channel;

public sealed record RestoreChannelCommand(Guid ChannelId) : ICommand<Unit>;