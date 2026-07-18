using Mediator;

namespace FSH.Mod.Chat.Spec.v1.Channel;

public sealed record MarkChannelReadCommand(Guid ChannelId, Guid MessageId) : ICommand<Unit>;