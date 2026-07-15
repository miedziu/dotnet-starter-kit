using FSH.Modules.Chat.Contracts.v1.Dtos;
using Mediator;
using System.Collections.ObjectModel;

namespace FSH.Modules.Chat.Contracts.v1.Queries;

public sealed record ListMyChannelsQuery(int Page = 1, int PageSize = 50)
    : IQuery<ReadOnlyCollection<ChannelDto>>;