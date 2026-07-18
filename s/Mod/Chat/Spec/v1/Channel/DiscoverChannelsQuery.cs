using Mediator;
using System.Collections.ObjectModel;

namespace FSH.Mod.Chat.Spec.v1.Channel;

public sealed record DiscoverChannelsQuery(string? Search, int Page = 1, int PageSize = 50)
    : IQuery<ReadOnlyCollection<ChannelDto>>;