using FSH.Modules.Chat.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Chat.Contracts.v1.Queries;

public sealed record GetChannelByIdQuery(Guid ChannelId) : IQuery<ChannelDto>;