using Mediator;

namespace FSH.Mod.Chat.Spec.v1.Member;

public sealed record RemoveChannelMemberCommand(
    Guid ChannelId,
    string UserId) : ICommand<Unit>;