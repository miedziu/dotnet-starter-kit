using Mediator;

namespace FSH.Mod.Chat.Spec.v1.Member;

public sealed record AddChannelMembersCommand(
    Guid ChannelId,
    IReadOnlyList<string> UserIds) : ICommand<Unit>;