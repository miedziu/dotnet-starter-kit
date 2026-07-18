using Mediator;

namespace FSH.Mods.Chat.Spec.v1.Member;

public sealed record AddChannelMembersCommand(
    Guid ChannelId,
    IReadOnlyList<string> UserIds) : ICommand<Unit>;