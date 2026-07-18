using Mediator;

namespace FSH.Mods.Chat.Spec.v1.Channel;

public sealed record GetChannelByIdQuery(Guid ChannelId) : IQuery<ChannelDto>;