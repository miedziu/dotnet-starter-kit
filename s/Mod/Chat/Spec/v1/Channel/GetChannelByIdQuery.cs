using Mediator;

namespace FSH.Mod.Chat.Spec.v1.Channel;

public sealed record GetChannelByIdQuery(Guid ChannelId) : IQuery<ChannelDto>;